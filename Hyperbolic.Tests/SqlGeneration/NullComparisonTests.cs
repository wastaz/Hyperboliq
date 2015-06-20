using Xunit;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "NullComparisons")]
    public class NullComparisonTests
    {
        [Fact]
        public void ItCanGenerateCorrectSqlForAComparisonWithNull()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(S.Col<Person>("Age"), BinaryOperation.Equal, S.Null())));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NULL", result);
        }

        [Fact]
        public void ItCanGenerateCorrectSqlForAnInvertedComparisonWithNull()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(S.Col<Person>("Age"), BinaryOperation.NotEqual, S.Null())));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NOT NULL", result);
        }

        [Fact]
        public void ItCanSupportFlippingOrdersForComparisonWithNull()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(S.Null(), BinaryOperation.Equal, S.Col<Person>("Age"))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NULL", result);
        }

        [Fact]
        public void ItCanSupportFlippingOrdersForInvertedComparisonWithNull()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(S.Null(), BinaryOperation.NotEqual, S.Col<Person>("Age"))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NOT NULL", result);
        }
    }
}
