using Xunit;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Tests.SqlStreamExtensions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;

namespace Hyperboliq.Tests
{
    [Trait("TokenGeneration", "Joins")]
    public class TokenGeneration_AdvancedJoinTests
    {
        [Fact]
        public void ItShouldBePossibleToJoinATableToItself()
        {
            var child = NamedTableReferenceFromType<Person>("child");
            var parent = NamedTableReferenceFromType<Person>("parent");

            var expr = Select.Column(child, p => new { p.Name })
                             .Column(parent, p => new { p.Name })
                             .From(child)
                             .InnerJoin(child, parent, (p1, p2) => p1.ParentId == p2.Id);
            var result = expr.ToSqlExpression();

            var expected =
                SelectNode(
                    Select(Col(parent, "Name"), Col(child, "Name")),
                    From(
                        child,
                        Join(child, parent, JoinType.InnerJoin, BinExp(Col(child, "ParentId"), BinaryOperation.Equal, Col(parent, "Id")))));
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
                SelectNode(
                    Select(Col<Car>("*")),
                    From<House>(
                        Join<House, Person, Car>(
                            JoinType.InnerJoin, 
                            BinExp(
                                BinExp(Col<Car>("Brand"), BinaryOperation.Equal, Col<House>("Address")),
                                BinaryOperation.And,
                                BinExp(Col<Person>("LivesAtHouseId"), BinaryOperation.Equal, Col<House>("Id")))),
                        Join<House, Person>(
                            JoinType.InnerJoin,
                            BinExp(Col<House>("Id"), BinaryOperation.Equal, Col<Person>("LivesAtHouseId")))));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ItShouldBePossibleToInnerJoinATableOnItselfSeveralTimes()
        {
            var child = NamedTableReferenceFromType<Person>("child");
            var parent = NamedTableReferenceFromType<Person>("parent");
            var grandparent = NamedTableReferenceFromType<Person>("grandparent");
            var expr = Select.Column(grandparent, gp => new { gp.Name, gp.Age })
                             .From(child)
                             .InnerJoin(child, parent, (c, p) => c.ParentId == p.Id)
                             .InnerJoin(child, parent, grandparent, (c, p, gp) => p.ParentId == gp.Id && c.LivesAtHouseId == gp.LivesAtHouseId);

            var expected =
                SelectNode(
                    Select(
                        Col(grandparent, "Name"),
                        Col(grandparent, "Age")),
                    From(
                        child,
                        Join(child, parent, JoinType.InnerJoin, BinExp(Col(child, "ParentId"), BinaryOperation.Equal, Col(parent, "Id"))),
                        Join(
                            child, 
                            parent, 
                            grandparent, 
                            JoinType.InnerJoin, 
                            BinExp(
                                BinExp(Col(parent, "ParentId"), BinaryOperation.Equal, Col(grandparent, "Id")),
                                BinaryOperation.And,
                                BinExp(Col(child, "LivesAtHouseId"), BinaryOperation.Equal, Col(grandparent, "LivesAtHouseId"))))));
        }
    }
}
