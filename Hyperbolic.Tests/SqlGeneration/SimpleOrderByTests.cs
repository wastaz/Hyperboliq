using Xunit;
using Hyperboliq.Domain;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using JoinType = Hyperboliq.Domain.Stream.JoinType;
using Direction = Hyperboliq.Domain.Stream.Direction;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;
using Hyperboliq.Tests.Model;
using Hyperboliq.Dialects;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Trait("SqlGeneration", "OrderBy")]
    public class SqlGeneration_SimpleOrderByTests
    {
        [Fact]
        public void ItShouldBePossibleToOrderAscendingByAColumn()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    orderBy: S.OrderBy(
                        S.OrderClause(S.Col<Person>("Age"), Direction.Ascending))
                    );
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age ASC", result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderDescendingByAColumn()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>()),
                    S.From<Person>(),
                    orderBy: S.OrderBy(S.OrderClause(S.Col<Person>("Age"), Direction.Descending)));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(@"SELECT PersonRef.* FROM Person PersonRef ORDER BY PersonRef.Age DESC", result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderBySeveralColumns()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Person>(), S.Star<Car>()),
                    S.From<Person>(
                        S.Join<Person, Car>(JoinType.InnerJoin, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))),
                    orderBy: 
                        S.OrderBy(
                            S.OrderClause(S.Col<Person>("Age"), Direction.Ascending),
                            S.OrderClause(S.Col<Car>("Brand"), Direction.Descending)));
            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.Equal(
                @"SELECT PersonRef.*, CarRef.* FROM Person PersonRef " +
                "INNER JOIN Car CarRef ON PersonRef.Id = CarRef.DriverId " +
                "ORDER BY PersonRef.Age ASC, CarRef.Brand DESC", result);
        }
    }
}
