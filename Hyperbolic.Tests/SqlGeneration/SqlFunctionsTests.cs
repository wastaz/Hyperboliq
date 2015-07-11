using Hyperboliq.Tests.Model;
using Hyperboliq.Domain;
using Hyperboliq.Dialects;
using Xunit;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;

namespace Hyperboliq.Tests.SqlGeneration
{
    public class SqlGeneration_SqlFunctionsTests
    {

        [Fact]
        public void ItCanGenerateSqlForToUpper()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Func(Stream.FunctionType.Upper, new[] { S.Col<Person>("Name") })),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(
                            S.Func(Stream.FunctionType.Upper, new[] { S.Col<Person>("Name") }),
                            BinaryOperation.Equal,
                            S.Const("'KALLE'"))
                    ));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            string expected = "SELECT UPPER(PersonRef.Name) FROM Person PersonRef WHERE UPPER(PersonRef.Name) = 'KALLE'";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItCanGenerateSqlForToLower()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Func(Stream.FunctionType.Lower, new[] { S.Col<Person>("Name") })),
                    S.From<Person>(),
                    S.Where(
                        S.BinExp(
                            S.Func(Stream.FunctionType.Lower, new[] { S.Col<Person>("Name") }),
                            BinaryOperation.Equal,
                            S.Const("'kalle'"))
                    ));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);

            string expected = "SELECT LOWER(PersonRef.Name) FROM Person PersonRef WHERE LOWER(PersonRef.Name) = 'kalle'";
            Assert.Equal(expected, result);
        }
    }
}
