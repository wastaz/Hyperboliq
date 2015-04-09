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

            var expr = new SelectExpression().Select(child, p => new { p.Name })
                                          .Select(parent, p => new { p.Name })
                                          .From(child)
                                          .InnerJoin(child, parent, (p1, p2) => p1.ParentId == p2.Id);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col(child, "Name"),
                    Col(parent, "Name"),
                    Kw(KeywordNode.From),
                    Tbl(child),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl(parent),
                    Kw(KeywordNode.On),
                    BinExp(Col(child, "ParentId"), BinaryOperation.Equal, Col(parent, "Id"))
                    );
            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBePossibleToInnerJoinATableOnSeveralOtherTables()
        {
            var expr = new SelectExpression().SelectAll<Car>()
                .From<House>()
                .InnerJoin<House, Person>((h, p) => h.Id == p.LivesAtHouseId)
                .InnerJoin<House, Person, Car>((h, p, c) => c.Brand == h.Address && p.LivesAtHouseId == h.Id);
            var result = expr.ToSqlStream();

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col<Car>("*"),
                    Kw(KeywordNode.From),
                    Tbl<House>(),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl<Person>(),
                    Kw(KeywordNode.On),
                    BinExp(Col<House>("Id"), BinaryOperation.Equal, Col<Person>("LivesAtHouseId")),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl<Car>(),
                    Kw(KeywordNode.On),
                    BinExp(
                        BinExp(Col<Car>("Brand"), BinaryOperation.Equal, Col<House>("Address")),
                        BinaryOperation.And,
                        BinExp(Col<Person>("LivesAtHouseId"), BinaryOperation.Equal, Col<House>("Id")))
                    );
            result.ShouldEqual(expected);
        }

        [Fact]
        public void ItShouldBePossibleToInnerJoinATableOnItselfSeveralTimes()
        {
            var child = NamedTableReferenceFromType<Person>("child");
            var parent = NamedTableReferenceFromType<Person>("parent");
            var grandparent = NamedTableReferenceFromType<Person>("grandparent");
            var expr = new SelectExpression().Select(grandparent, gp => new { gp.Name, gp.Age })
                .From(child)
                .InnerJoin(child, parent, (c, p) => c.ParentId == p.Id)
                .InnerJoin(child, parent, grandparent, (c, p, gp) => p.ParentId == gp.Id && c.LivesAtHouseId == gp.LivesAtHouseId);

            var expected =
                StreamFrom(
                    Kw(KeywordNode.Select),
                    Col(grandparent, "Name"),
                    Col(grandparent, "Age"),
                    Kw(KeywordNode.From),
                    Tbl(child),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl(parent),
                    Kw(KeywordNode.On),
                    BinExp(Col(child, "ParentId"), BinaryOperation.Equal, Col(parent, "Id")),
                    Kw(KeywordNode.NewJoin(JoinType.InnerJoin)),
                    Tbl(grandparent),
                    Kw(KeywordNode.On),
                    BinExp(
                        BinExp(Col(parent, "ParentId"), BinaryOperation.Equal, Col(grandparent, "Id")),
                        BinaryOperation.And,
                        BinExp(Col(child, "LivesAtHouseId"), BinaryOperation.Equal, Col(grandparent, "LivesAtHouseId"))));
                    
        }
    }
}
