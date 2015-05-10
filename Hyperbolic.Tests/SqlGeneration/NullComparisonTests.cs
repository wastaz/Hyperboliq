using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.SqlGen;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "NullComparisons")]
    public class NullComparisonTests
    {
        [Fact]
        public void ItCanGenerateCorrectSqlForAComparisonWithNull()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(
                        BinExp(Col<Person>("Age"), BinaryOperation.Equal, Null())));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NULL", result);
        }

        [Fact]
        public void ItCanGenerateCorrectSqlForAnInvertedComparisonWithNull()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(
                        BinExp(Col<Person>("Age"), BinaryOperation.NotEqual, Null())));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NOT NULL", result);
        }

        [Fact]
        public void ItCanSupportFlippingOrdersForComparisonWithNull()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(
                        BinExp(Null(), BinaryOperation.Equal, Col<Person>("Age"))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NULL", result);
        }

        [Fact]
        public void ItCanSupportFlippingOrdersForInvertedComparisonWithNull()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(
                        BinExp(Null(), BinaryOperation.NotEqual, Col<Person>("Age"))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.Equal("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NOT NULL", result);
        }
    }
}
