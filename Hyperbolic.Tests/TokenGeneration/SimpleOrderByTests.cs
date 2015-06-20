using Xunit;
using Hyperboliq.Tests.Model;
using Direction = Hyperboliq.Domain.Stream.Direction;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;
using JoinType = Hyperboliq.Domain.Stream.JoinType;
using S = Hyperboliq.Tests.SqlStreamExtensions;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "OrderBy")]
    public class TokenGeneration_SimpleOrderByTests
    {
        [Fact]
        public void ItShouldBePossibleToOrderAscendingByAColumn()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Ascending);
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    orderBy: S.OrderBy(S.OrderClause(S.Col<Person>("Age"), Direction.Ascending)));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderDescendingByAColumn()
        {
            var expr = Select.Star<Person>()
                             .From<Person>()
                             .OrderBy<Person>(p => p.Age, Direction.Descending);
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(S.Col<Person>("*")),
                    S.From<Person>(),
                    orderBy: S.OrderBy(S.OrderClause(S.Col<Person>("Age"), Direction.Descending)));

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToOrderBySeveralColumns()
        {
            var expr = Select.Star<Person>().Star<Car>()
                             .From<Person>()
                             .InnerJoin<Person, Car>((p, c) => p.Id == c.DriverId)
                             .OrderBy<Person>(p => p.Age, Direction.Ascending)
                             .ThenBy<Car>(c => c.Brand, Direction.Descending);
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(S.Col<Car>("*"), S.Col<Person>("*")),
                    S.From<Person>(
                        S.Join<Person, Car>(JoinType.InnerJoin, S.BinExp(S.Col<Person>("Id"), BinaryOperation.Equal, S.Col<Car>("DriverId")))),
                    orderBy: S.OrderBy(
                        S.OrderClause(S.Col<Car>("Brand"), Direction.Descending),
                        S.OrderClause(S.Col<Person>("Age"), Direction.Ascending))
                    );

            Assert.Equal(expected, result);
        }
    }
}
