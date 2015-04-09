using System;
using Xunit;
using FluentAssertions;
using Hyperboliq;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGenerator;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "Parameters")]
    public class SqlGeneration_ParameterizedQueryTests
    {
        [Fact]
        public void ItShouldBeAbleToParameterizeAQuery()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.Where),
                    BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Param("age"))
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);

            result.Should().Be("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > @age");
        }

        [Fact]
        public void ItShouldBePossibleToUseTheParameterInMoreComplexExpressions()
        {
            var stream =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Person>("*"),
                    Kw(KeywordNode.From),
                    Tbl<Person>(),
                    Kw(KeywordNode.Where),
                    BinExp(
                        BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Param("age")),
                        BinaryOperation.And,
                        BinExp(Param("age"), BinaryOperation.LessThan, Const(90))
                        ),
                    Kw(KeywordNode.Or),
                    BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Param("age"))
                    );
            var result = SqlifySeq(AnsiSql.Dialect, stream);

            result.Should().Be(
                @"SELECT PersonRef.* " +
                "FROM Person PersonRef " +
                "WHERE PersonRef.Age > @age "+
                "AND @age < 90 " +
                "OR PersonRef.Age < @age");
        }
    }
}
