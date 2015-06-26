using Hyperboliq.Tests.Model;
using Xunit;
using Hyperboliq.Dialects;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Domain;
using AggregateType = Hyperboliq.Domain.Stream.AggregateType;
using ValueNode = Hyperboliq.Domain.Stream.ValueNode;

namespace Hyperboliq.Tests.SqlGeneration
{
    
    [Trait("SqlGeneration", "Select")]
    public class SqlGeneration_SimpleSelectTests
    {
        [Fact]
        public void ItShouldBePossibleToSqlifyASimpleSelect()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>());

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef", result);
        }

        [Fact]
        public void ItShouldBePossibleToSqlifyASelectWithManyColumns()
        {
            var stream =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.Col<Person>("Age"),
                        S.Col<Person>("Id")),
                    S.From<Person>());

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.Name, PersonRef.Age, PersonRef.Id FROM Person PersonRef", result);
        }

        [Fact]
        public void ItShouldBePossibleToSelectWithColumnAliases()
        {
            var stream =
               S.SelectNode(
                   S.Select(
                       S.AliasedCol<Person>(
                           AggregateType.Count,
                           ValueNode.NullValue,
                           "NumberOfPersons")),
                   S.From<Person>());
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT COUNT(*) AS NumberOfPersons FROM Person PersonRef", result);
        }

        [Fact]
        public void ItShouldBePossibleToMixAliasedAndUnAliasedColumns()
        {
            var stream =
                S.SelectNode(
                    S.Select(
                        S.Col<Person>("Name"),
                        S.AliasedCol<Person>(
                            AggregateType.Max,
                            S.Col<Person>("Age"),
                            "MaxAge"),
                        S.AliasedCol<Person>(
                            AggregateType.Min,
                            S.Col<Person>("Age"),
                            "MinAge")),
                    S.From<Person>(),
                    groupBy: S.GroupBy(S.Col<Person>("Name")));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.Name, MAX(PersonRef.Age) AS MaxAge, MIN(PersonRef.Age) AS MinAge FROM Person PersonRef GROUP BY PersonRef.Name", result);
        }
    }
    
}
