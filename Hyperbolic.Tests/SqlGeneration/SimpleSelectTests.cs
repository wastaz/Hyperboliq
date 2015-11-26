using NUnit.Framework;
using Hyperboliq.Dialects;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using AggregateType = Hyperboliq.Domain.AST.AggregateType;
using ValueNode = Hyperboliq.Domain.AST.ValueNode;

namespace Hyperboliq.Tests.SqlGeneration
{
    
    [TestFixture]
    public class SqlGeneration_SimpleSelectTests
    {
        [Test]
        public void ItShouldBePossibleToSqlifyASimpleSelect()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>());

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo(@"SELECT PersonRef.* FROM Person PersonRef"));
        }

        [Test]
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
            Assert.That(result, Is.EqualTo(@"SELECT PersonRef.Name, PersonRef.Age, PersonRef.Id FROM Person PersonRef"));
        }

        [Test]
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
            Assert.That(result, Is.EqualTo(@"SELECT COUNT(*) AS NumberOfPersons FROM Person PersonRef"));
        }

        [Test]
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
            Assert.That(result, Is.EqualTo(@"SELECT PersonRef.Name, MAX(PersonRef.Age) AS MaxAge, MIN(PersonRef.Age) AS MinAge FROM Person PersonRef GROUP BY PersonRef.Name"));
        }
    }
    
}
