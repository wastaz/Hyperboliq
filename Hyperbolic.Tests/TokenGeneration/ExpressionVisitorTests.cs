using System;
using System.Linq.Expressions;
using Microsoft.FSharp.Collections;
using NUnit.Framework;
using Hyperboliq.Tests;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;
using ValueNode = Hyperboliq.Domain.AST.ValueNode;
using System.Collections;
using Hyperboliq.Tests.TokenGeneration;

namespace Hyperboliq.Domain.Tests
{
    public static class Extensions
    {
        public static FSharpList<ITableReference> ToContext(this ITableReference[] refs)
        {
            return ListModule.OfArray(refs);
        }
    }

    [TestFixture]
    public class TokenGeneration_ExpressionVisitorTests
    {
        [Test]
        public void ItCanVisitASimpleColumnSelector()
        {
            Expression<Func<Person, int>> func = (Person p) => p.Age;
            var tableRef = Types.TableReferenceFromType<Person>();
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                new[] { tableRef }.ToContext()
                );

            var expected = S.Col<Person>("Age").ToOption();

            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItCanVisitAMultipleColumnSelector()
        {
            Expression<Func<Person, object>> func = (Person p) => new { p.Age, p.Name };
            var tableRef = Types.TableReferenceFromType<Person>();
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                new[] { tableRef }.ToContext()
                );

            var expected =
                ValueNode.NewValueList(
                    ListModule.OfArray(new[] { S.Col<Person>("Age"), S.Col<Person>("Name") })).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItCanVisitASimpleBinaryExpression()
        {
            Expression<Func<Person, bool>> func = (Person p) => p.Age > 42;
            var tableRef = Types.TableReferenceFromType<Person>();
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                new[] { tableRef }.ToContext()
                );

            var expected =
                S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42)).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItCanVisitASimpleBinaryExpressionComparingTwoInstancesOfTheSameType()
        {
            Expression<Func<Person, Person, bool>> func = (Person p1, Person p2) => p1.Age < p2.Age;
            var tableRefs = new[]
            {
                Types.NamedTableReferenceFromType<Person>("p1"),
                Types.NamedTableReferenceFromType<Person>("p2"),
            };
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                tableRefs.ToContext());

            var expected =
                S.BinExp(S.Col(tableRefs[0], "Age"), BinaryOperation.LessThan, S.Col(tableRefs[1], "Age")).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItCanVisitABinaryExpressionWithAndAndOrs()
        {
            Expression<Func<Person, bool>> func = (Person p) => p.Age > 42 && (p.Name == "Kalle" || p.Name == "Anna");
            var tableRefs = new[]
            {
                Types.TableReferenceFromType<Person>(),
            };
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                tableRefs.ToContext());

            var expected =
                S.BinExp(
                    S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42)),
                    BinaryOperation.And,
                    S.BinExp(
                        S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Kalle'")),
                        BinaryOperation.Or,
                        S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Anna'")))).ToOption();

            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBeAbleToVisitParameters()
        {
            var ageParam = new ExpressionParameter<int>("ageparam");
            Expression<Func<Person, bool>> func = (Person p) => p.Age != ageParam;
            var tableRefs = new[]
            {
                Types.TableReferenceFromType<Person>(),
            };
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                tableRefs.ToContext());

            var expected =
                S.BinExp(S.Col<Person>("Age"), BinaryOperation.NotEqual, S.Param("ageparam")).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBeAbleToGenerateAliasedColumnTokens()
        {
            Expression<Func<Person, object>> func = (Person p) => new { p.Name, AgeOfPerson = p.Age, Count = Sql.Count() };
            var tableRefs = new[]
            {
                Types.TableReferenceFromType<Person>(),
            };
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                tableRefs.ToContext());

            var expected =
                ValueNode.NewValueList(
                    ListModule.OfArray(new[] {
                        S.Col<Person>("Name"),
                        S.AliasedCol<Person>("Age", "AgeOfPerson"),
                        S.AliasedCol<Person>(AST.AggregateType.Count, S.Null(), "Count")
                    })).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBeAbleToGenerateAliasesForConstants()
        {
            Expression<Func<Person, object>> func = (Person p) => new { FavoriteNumber = 42 };
            var tableRefs = new[]
            {
                Types.TableReferenceFromType<Person>(),
            };
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                tableRefs.ToContext());

            var expected =
                ValueNode.NewValueList(
                    ListModule.OfArray(new[] {
                        S.AliasedCol(S.Const(42), "FavoriteNumber"),
                    })).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldBeAbleToGenerateAliasesForBinaryExpressions()
        {
            Expression<Func<Person, object>> func = (Person p) => new { AgePlusTen = p.Age + 10 };
            var tableRefs = new[]
            {
                Types.TableReferenceFromType<Person>(),
            };
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                tableRefs.ToContext());

            var expected =
                ValueNode.NewValueList(
                    ListModule.OfArray(new[] {
                        S.AliasedCol(S.BinExp(S.Col<Person>("Age"), BinaryOperation.Add, S.Const(10)), "AgePlusTen"),
                    })).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItShouldNotGenerateAliasColumnsForColumnsBeingAliasedToItsOrdinaryName()
        {
            Expression<Func<Person, object>> func = (Person p) => new { Name = p.Name, Age = p.Age };
            var tableRefs = new[]
            {
                Types.TableReferenceFromType<Person>(),
            };
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                tableRefs.ToContext());
            var expected =
                ValueNode.NewValueList(
                    ListModule.OfArray(new[] {
                        S.Col<Person>("Name"),
                        S.Col<Person>("Age"),
                    })).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItCanParseToUpper()
        {
            Expression<Func<Person, object>> func = (Person p) => p.Name.ToUpper() == "KALLE";
            var tableRefs = new[]
            {
                Types.TableReferenceFromType<Person>(),
            };
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                tableRefs.ToContext());
            var expected =
                S.BinExp(
                    S.Func(AST.FunctionType.Upper, new[] { S.Col<Person>("Name") }),
                    BinaryOperation.Equal,
                    S.Const("'KALLE'")).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItCanParseToLower()
        {
            Expression<Func<Person, object>> func = (Person p) => p.Name.ToLower() == "kalle";
            var tableRefs = new[]
            {
                Types.TableReferenceFromType<Person>(),
            };
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                tableRefs.ToContext());
            var expected =
                S.BinExp(
                    S.Func(AST.FunctionType.Lower, new[] { S.Col<Person>("Name") }),
                    BinaryOperation.Equal,
                    S.Const("'kalle'")).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }

        [Test]
        public void ItParsesStringAdditionAsConcat()
        {
            Expression<Func<Person, Car, object>> func = (Person p, Car c) => (p.Name + c.Brand) == "KalleSaab";
            var tableRefs = new []
            {
                (ITableReference)Types.TableReferenceFromType<Person>(),
                (ITableReference)Types.TableReferenceFromType<Car>(),
            };
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(func),
                tableRefs.ToContext());
            var expected =
                S.BinExp(
                    S.Func(AST.FunctionType.Concat, new[] { S.Col<Person>("Name"), S.Col<Car>("Brand"), }),
                    BinaryOperation.Equal,
                    S.Const("'KalleSaab'")).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }

        public static object[] ConcatTestData =
        {
            (Expression<Func<Person, Car, object>>)((Person p, Car c) => (p.Name + c.Brand + p.Name + c.Brand) == "KalleSaab"),
            (Expression<Func<Person, Car, object>>)((Person p, Car c) => (p.Name + c.Brand + (p.Name + c.Brand)) == "KalleSaab"),
            (Expression<Func<Person, Car, object>>)((Person p, Car c) => (p.Name + (c.Brand + p.Name + c.Brand)) == "KalleSaab"),
        };

        [Test, TestCaseSource("ConcatTestData")]
        public void ItParsesSeveralStringAdditionsIntoASingleConcat(Expression<Func<Person, Car, object>> testcase)
        {
            var tableRefs = new[]
            {
                (ITableReference)Types.TableReferenceFromType<Person>(),
                (ITableReference)Types.TableReferenceFromType<Car>(),
            };
            var ev = ExpressionVisitor.Visit(
                ExpressionVisitor.VisitableExpression.NewLinqExpression(testcase),
                tableRefs.ToContext());
            var expected =
                S.BinExp(
                    S.Func(AST.FunctionType.Concat, new[] { S.Col<Person>("Name"), S.Col<Car>("Brand"), S.Col<Person>("Name"), S.Col<Car>("Brand"), }),
                    BinaryOperation.Equal,
                    S.Const("'KalleSaab'")).ToOption();
            Assert.That(ev, Is.EqualTo(expected));
        }
    }
}