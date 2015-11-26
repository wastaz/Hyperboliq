using NUnit.Framework;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;

namespace Hyperboliq.Tests.SqlGeneration
{
    [TestFixture]
    public class NullComparisonTests
    {
        [Test]
        public void ItCanGenerateCorrectSqlForAComparisonWithNull()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(S.Col<Person>("Age"), BinaryOperation.Equal, S.Null())));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.That(result, Is.EqualTo("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NULL"));
        }

        [Test]
        public void ItCanGenerateCorrectSqlForAnInvertedComparisonWithNull()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(S.Col<Person>("Age"), BinaryOperation.NotEqual, S.Null())));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.That(result, Is.EqualTo("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NOT NULL"));
        }

        [Test]
        public void ItCanSupportFlippingOrdersForComparisonWithNull()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(S.Null(), BinaryOperation.Equal, S.Col<Person>("Age"))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.That(result, Is.EqualTo("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NULL"));
        }

        [Test]
        public void ItCanSupportFlippingOrdersForInvertedComparisonWithNull()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(S.Null(), BinaryOperation.NotEqual, S.Col<Person>("Age"))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            Assert.That(result, Is.EqualTo("SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age IS NOT NULL"));
        }
    }
}
