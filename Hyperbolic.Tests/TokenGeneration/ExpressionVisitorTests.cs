using System;
using System.Linq.Expressions;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using Hyperboliq.Tests.Model;
using Hyperboliq.Domain;
using Xunit;
using Hyperboliq.Tests;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;
using ValueNode = Hyperboliq.Domain.Stream.ValueNode;


namespace Hyperboliq.Domain.Tests
{
    public static class Extensions
    {
        public static FSharpList<ITableReference> ToContext(this ITableReference[] refs)
        {
            return ListModule.OfArray(refs);
        }
    }

    [Trait("TokenGeneration", "ExpressionVisitor")]
    public class TokenGeneration_ExpressionVisitorTests
    {
        [Fact]
        public void ItCanVisitASimpleColumnSelector()
        {
            Expression<Func<Person, int>> func = (Person p) => p.Age;
            var tableRef = Types.TableReferenceFromType<Person>();
            var ev = ExpressionVisitor.Visit(
                func,
                new[] { tableRef }.ToContext()
                );

            var expected = S.Col<Person>("Age").ToOption();

            Assert.Equal(expected, ev);
        }

        [Fact]
        public void ItCanVisitAMultipleColumnSelector()
        {
            Expression<Func<Person, object>> func = (Person p) => new { p.Age, p.Name };
            var tableRef = Types.TableReferenceFromType<Person>();
            var ev = ExpressionVisitor.Visit(
                func,
                new[] { tableRef }.ToContext()
                );

            var expected =
                ValueNode.NewValueList(
                    ListModule.OfArray(new[] { S.Col<Person>("Age"), S.Col<Person>("Name") })).ToOption();
            Assert.Equal(expected, ev);
        }

        [Fact]
        public void ItCanVisitASimpleBinaryExpression()
        {
            Expression<Func<Person, bool>> func = (Person p) => p.Age > 42;
            var tableRef = Types.TableReferenceFromType<Person>();
            var ev = ExpressionVisitor.Visit(
                func,
                new[] { tableRef }.ToContext()
                );

            var expected =
                S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42)).ToOption();
            Assert.Equal(expected, ev);
        }

        [Fact]
        public void ItCanVisitASimpleBinaryExpressionComparingTwoInstancesOfTheSameType()
        {
            Expression<Func<Person, Person, bool>> func = (Person p1, Person p2) => p1.Age < p2.Age;
            var tableRefs = new[]
            {
                Types.NamedTableReferenceFromType<Person>("p1"),
                Types.NamedTableReferenceFromType<Person>("p2"),
            };
            var ev = ExpressionVisitor.Visit(func, tableRefs.ToContext());

            var expected =
                S.BinExp(S.Col(tableRefs[0], "Age"), BinaryOperation.LessThan, S.Col(tableRefs[1], "Age")).ToOption();
            Assert.Equal(expected, ev);
        }

        [Fact]
        public void ItCanVisitABinaryExpressionWithAndAndOrs()
        {
            Expression<Func<Person, bool>> func = (Person p) => p.Age > 42 && (p.Name == "Kalle" || p.Name == "Anna");
            var tableRefs = new[]
            {
                Types.TableReferenceFromType<Person>(),
            };
            var ev = ExpressionVisitor.Visit(func, tableRefs.ToContext());

            var expected =
                S.BinExp(
                    S.BinExp(S.Col<Person>("Age"), BinaryOperation.GreaterThan, S.Const(42)),
                    BinaryOperation.And,
                    S.BinExp(
                        S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Kalle'")),
                        BinaryOperation.Or,
                        S.BinExp(S.Col<Person>("Name"), BinaryOperation.Equal, S.Const("'Anna'")))).ToOption();

            Assert.Equal(expected, ev);
        }

        [Fact]
        public void ItShouldBeAbleToVisitParameters()
        {
            var ageParam = new ExpressionParameter<int>("ageparam");
            Expression<Func<Person, bool>> func = (Person p) => p.Age != ageParam;
            var tableRefs = new[]
            {
                Types.TableReferenceFromType<Person>(),
            };
            var ev = ExpressionVisitor.Visit(func, tableRefs.ToContext());

            var expected =
                S.BinExp(S.Col<Person>("Age"), BinaryOperation.NotEqual, S.Param("ageparam")).ToOption();
            Assert.Equal(expected, ev);
        }

        [Fact]
        public void ItShouldBeAbleToGenerateAliasedColumnTokens()
        {
            Expression<Func<Person, object>> func = (Person p) => new { p.Name, AgeOfPerson = p.Age, Count = Sql.Count() };
            var tableRefs = new[]
            {
                Types.TableReferenceFromType<Person>(),
            };
            var ev = ExpressionVisitor.Visit(func, tableRefs.ToContext());

            var expected =
                ValueNode.NewValueList(
                    ListModule.OfArray(new[] {
                        S.Col<Person>("Name"),
                        S.AliasedCol<Person>("Age", "AgeOfPerson"),
                        S.AliasedCol<Person>(Stream.AggregateType.Count, S.Null(), "Count")
                    })).ToOption();
            Assert.Equal(expected, ev);
        }
    }
}