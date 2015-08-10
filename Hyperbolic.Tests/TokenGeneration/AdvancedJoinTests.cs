using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using JoinType = Hyperboliq.Domain.AST.JoinType;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "Joins")]
    public class TokenGeneration_AdvancedJoinTests
    {
        [Fact]
        public void ItShouldBePossibleToJoinATableToItself()
        {
            var child = Table<Person>.WithReferenceName("child");
            var parent = Table<Person>.WithReferenceName("parent");
            
            var expr = Select.Column(child, p => new { p.Name })
                             .Column(parent, p => new { p.Name })
                             .From(child)
                             .InnerJoin(child, parent, (p1, p2) => p1.ParentId == p2.Id);
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(S.Col(parent, "Name"), S.Col(child, "Name")),
                    S.From(
                        child,
                        S.Join(child, parent, JoinType.InnerJoin, S.BinExp(S.Col(child, "ParentId"), BinaryOperation.Equal, S.Col(parent, "Id")))));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToInnerJoinATableOnSeveralOtherTables()
        {
            var expr = Select.Star<Car>()
                             .From<House>()
                             .InnerJoin<House, Person>((h, p) => h.Id == p.LivesAtHouseId)
                             .InnerJoin<House, Person, Car>((h, p, c) => c.Brand == h.Address && p.LivesAtHouseId == h.Id);
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(S.Star<Car>()),
                    S.From<House>(
                        S.Join<House, Person, Car>(
                            JoinType.InnerJoin, 
                            S.BinExp(
                                S.BinExp(S.Col<Car>("Brand"), BinaryOperation.Equal, S.Col<House>("Address")),
                                BinaryOperation.And,
                                S.BinExp(S.Col<Person>("LivesAtHouseId"), BinaryOperation.Equal, S.Col<House>("Id")))),
                        S.Join<House, Person>(
                            JoinType.InnerJoin,
                            S.BinExp(S.Col<House>("Id"), BinaryOperation.Equal, S.Col<Person>("LivesAtHouseId")))));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToInnerJoinATableOnItselfSeveralTimes()
        {
            var child = Table<Person>.WithReferenceName("child");
            var parent = Table<Person>.WithReferenceName("parent");
            var grandparent = Table<Person>.WithReferenceName("grandparent");

            var expr = Select.Column(grandparent, gp => new { gp.Name, gp.Age })
                             .From(child)
                             .InnerJoin(child, parent, (c, p) => c.ParentId == p.Id)
                             .InnerJoin(child, parent, grandparent, (c, p, gp) => p.ParentId == gp.Id && c.LivesAtHouseId == gp.LivesAtHouseId);
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    S.Select(
                        S.Col(grandparent, "Name"),
                        S.Col(grandparent, "Age")),
                    S.From(
                        child,
                        S.Join(
                            child, 
                            parent, 
                            grandparent, 
                            JoinType.InnerJoin, 
                            S.BinExp(
                                S.BinExp(S.Col(parent, "ParentId"), BinaryOperation.Equal, S.Col(grandparent, "Id")),
                                BinaryOperation.And,
                                S.BinExp(S.Col(child, "LivesAtHouseId"), BinaryOperation.Equal, S.Col(grandparent, "LivesAtHouseId")))),
                        S.Join(child, parent, JoinType.InnerJoin, S.BinExp(S.Col(child, "ParentId"), BinaryOperation.Equal, S.Col(parent, "Id")))));
            Assert.Equal(expected, result);
        }
    }
}
