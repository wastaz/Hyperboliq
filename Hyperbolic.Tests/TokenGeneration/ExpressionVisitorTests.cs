using System;
using System.Linq.Expressions;
using Microsoft.FSharp.Collections;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.ExpressionVisitor;
using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using Microsoft.FSharp.Core;

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
            var tableRef = TableReferenceFromType<Person>();
            var ev = Visit(
                func,
                new[] { tableRef }.ToContext()
                );

            var expected = OptionModule.OfObj(Col<Person>("Age"));

            Assert.Equal(expected, ev);
        }

        [Fact]
        public void ItCanVisitAMultipleColumnSelector()
        {
            Expression<Func<Person, object>> func = (Person p) => new { p.Age, p.Name };
            var tableRef = TableReferenceFromType<Person>();
            var ev = Visit(
                func,
                new[] { tableRef }.ToContext()
                );

            var expected =
                OptionModule.OfObj(
                    ValueNode.NewValueList(
                        ListModule.OfArray(new[] { Col<Person>("Age"), Col<Person>("Name") })));
            Assert.Equal(expected, ev);
        }

        [Fact]
        public void ItCanVisitASimpleBinaryExpression()
        {
            Expression<Func<Person, bool>> func = (Person p) => p.Age > 42;
            var tableRef = TableReferenceFromType<Person>();
            var ev = Visit(
                func,
                new[] { tableRef }.ToContext()
                );

            var expected =
                OptionModule.OfObj(
                    BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)));
            Assert.Equal(expected, ev);
        }

        [Fact]
        public void ItCanVisitASimpleBinaryExpressionComparingTwoInstancesOfTheSameType()
        {
            Expression<Func<Person, Person, bool>> func = (Person p1, Person p2) => p1.Age < p2.Age;
            var tableRefs = new[]
            {
                NamedTableReferenceFromType<Person>("p1"),
                NamedTableReferenceFromType<Person>("p2"),
            };
            var ev = Visit(func, tableRefs.ToContext());

            var expected =
                OptionModule.OfObj(
                    BinExp(Col(tableRefs[0], "Age"), BinaryOperation.LessThan, Col(tableRefs[1], "Age")));
            Assert.Equal(expected, ev);
        }

        [Fact]
        public void ItCanVisitABinaryExpressionWithAndAndOrs()
        {
            Expression<Func<Person, bool>> func = (Person p) => p.Age > 42 && (p.Name == "Kalle" || p.Name == "Anna");
            var tableRefs = new[]
            {
                TableReferenceFromType<Person>(),
            };
            var ev = Visit(func, tableRefs.ToContext());

            var expected =
                OptionModule.OfObj(
                    BinExp(
                        BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)),
                        BinaryOperation.And,
                        BinExp(
                            BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Kalle'")),
                            BinaryOperation.Or,
                            BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Anna'")))));

            Assert.Equal(expected, ev);
        }

        [Fact]
        public void ItShouldBeAbleToVisitParameters()
        {
            var ageParam = new ExpressionParameter<int>("ageparam");
            Expression<Func<Person, bool>> func = (Person p) => p.Age != ageParam;
            var tableRefs = new[]
            {
                TableReferenceFromType<Person>(),
            };
            var ev = Visit(func, tableRefs.ToContext());

            var expected =
                OptionModule.OfObj(BinExp(Col<Person>("Age"), BinaryOperation.NotEqual, Param("ageparam")));
            Assert.Equal(expected, ev);
        }
    }
}