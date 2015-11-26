using NUnit.Framework;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using Hyperboliq.Tests.TokenGeneration;
using JoinType = Hyperboliq.Domain.AST.JoinType;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;
using SqlExpression = Hyperboliq.Domain.AST.SqlExpression;
using S = Hyperboliq.Tests.SqlStreamExtensions;


namespace Hyperboliq.Tests.SqlGeneration
{
    [TestFixture]
    public class SqlGeneration_SimpleJoinTests
    {
        private SqlExpression GetExpectedJoinStream(JoinType joinKeyword)
        {
            return
                S.SelectNode(
                    S.Select(S.Star<Person>(), S.Star<Car>()),
                    S.From<Person>(
                        S.Join<Person, Car>(joinKeyword, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))
                        ));
        }

        [Test]
        public void ItShouldBeAbleToSqlifyASimpleInnerJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.InnerJoin);
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef INNER JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId"));
        }

        [Test]
        public void ItShouldBeAbleToSqlifyASimpleLeftJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.LeftJoin);
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef LEFT JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId"));
        }

        [Test]
        public void ItShouldBeAbleToSqlifyASimpleRightJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.RightJoin);
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef RIGHT JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId"));
        }

        [Test]
        public void ItShouldBeAbleToSqlifyASimpleFullJoin()
        {
            var stream = GetExpectedJoinStream(JoinType.FullJoin);
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo(@"SELECT PersonRef.*, CarRef.* FROM Person PersonRef FULL JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId"));
        }

        [Test]
        public void ItShouldBeAbleToSqlifyMultipleTableJoins()
        {
            var stream =
               S.SelectNode(
                   S.Select(S.Star<Person>(), S.Star<Car>(), S.Star<House>()),
                   S.From<House>(
                       S.Join<House, Person>(JoinType.InnerJoin, S.BinExp(S.Col<House>("Id"), BinaryOperation.Equal, S.Col<Person>("LivesAtHouseId"))),
                       S.Join<Person, Car>(JoinType.LeftJoin, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(
                result,
                Is.EqualTo(
                    @"SELECT PersonRef.*, CarRef.*, HouseRef.* " +
                    "FROM House HouseRef " +
                    "INNER JOIN Person PersonRef ON HouseRef.Id = PersonRef.LivesAtHouseId " +
                    "LEFT JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId"));
        }
    }
}
