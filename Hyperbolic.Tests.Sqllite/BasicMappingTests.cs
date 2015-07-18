using Xunit;
using Hyperboliq.Tests.Model;
using System.Linq;
using Hyperboliq.Domain;
using Hyperboliq.Dialects;

namespace Hyperboliq.Tests.Sqllite
{
    [Trait("Sqlite", "Mapping")]
    public class BasicMappingTests
    {
        [Fact]
        public void ItShouldReturnTheNumberOfAffectedRowsWhenExecutingANonQuery()
        {
            var factory = new HyperboliqConnectionFactory(SqlLite.Dialect, ":memory:");
            using (var con = factory.OpenDbConnection())
            {
                const string createTable = "CREATE TABLE Person (Id INT, Name VARCHAR(50), Age INT, LivesAtHouseId INT, ParentId INT)";
                var cmd1 = con.AsIDbConnection().CreateCommand();
                cmd1.CommandText = createTable;
                cmd1.ExecuteNonQuery();

                var insertQuery = Insert.Into<Person>().AllColumns.Values(
                    new Person { Id = 1, Name = "Kalle", Age = 42 },
                    new Person { Id = 2, Name = "Pelle", Age = 18 },
                    new Person { Id = 3, Name = "Gustav", Age = 45 });
                int affected = con.ExecuteNonQuery(insertQuery);

                Assert.Equal(3, affected);
            }
        }

        [Fact]
        public void ItShouldBeAbleToMapTheResultsOfASimpleQuery()
        {
            var factory = new HyperboliqConnectionFactory(SqlLite.Dialect, ":memory:");
            using (var con = factory.OpenDbConnection())
            {
                const string createTable = "CREATE TABLE Person (Id INT, Name VARCHAR(50), Age INT, LivesAtHouseId INT, ParentId INT)";
                var cmd1 = con.AsIDbConnection().CreateCommand();
                cmd1.CommandText = createTable;
                cmd1.ExecuteNonQuery();

                var insertQuery = Insert.Into<Person>().AllColumns.Values(new Person { Id = 1, Name = "Kalle", Age = 42 });
                con.ExecuteNonQuery(insertQuery);

                var selectQuery = Select.Star<Person>().From<Person>();
                var persons = con.Query<Person>(selectQuery);

                Assert.Equal(1, persons.Count());
                var person = persons.First();
                Assert.Equal("Kalle", person.Name);
                Assert.Equal(1, person.Id);
                Assert.Equal(42, person.Age);
            }
        }

        [Fact]
        public void ItShouldBeAbleToMapTheResultsOfAScalarQuery()
        {
            var factory = new HyperboliqConnectionFactory(SqlLite.Dialect, ":memory:");
            using (var con = factory.OpenDbConnection())
            {
                const string createTable = "CREATE TABLE Person (Id INT, Name VARCHAR(50), Age INT, LivesAtHouseId INT, ParentId INT)";
                var cmd1 = con.AsIDbConnection().CreateCommand();
                cmd1.CommandText = createTable;
                cmd1.ExecuteNonQuery();

                var insertQuery = Insert.Into<Person>().AllColumns.Values(
                    new Person { Id = 1, Name = "Kalle", Age = 42 }, 
                    new Person { Id = 2, Name = "Pelle", Age = 18 },
                    new Person { Id = 3, Name = "Gustav", Age = 45 });
                con.ExecuteNonQuery(insertQuery);

                var selectQuery = Select.Column<Person>(p => new { Count = Sql.Count(), }).From<Person>();
                var result = (long)con.ExecuteScalar(selectQuery);

                Assert.Equal(3, result);
            }
        }


        public class AliasPerson
        {
            [Alias("Name")]
            public string AliasName { get; set; }

            public int Id { get; set; }

            [Alias("Age")]
            public int AliasAge { get; set; }
        }

        [Fact]
        public void ItShouldBeAbleToMapAClassByUsingAliases()
        {
            var factory = new HyperboliqConnectionFactory(SqlLite.Dialect, ":memory:");
            using (var con = factory.OpenDbConnection())
            {
                const string createTable = "CREATE TABLE Person (Id INT, Name VARCHAR(50), Age INT, LivesAtHouseId INT, ParentId INT)";
                var cmd1 = con.AsIDbConnection().CreateCommand();
                cmd1.CommandText = createTable;
                cmd1.ExecuteNonQuery();

                var insertQuery = Insert.Into<Person>().AllColumns.Values(new Person { Id = 1, Name = "Kalle", Age = 42 });
                con.ExecuteNonQuery(insertQuery);

                var selectQuery = Select.Star<Person>().From<Person>();
                var persons = con.Query<AliasPerson>(selectQuery);

                Assert.Equal(1, persons.Count());
                var person = persons.First();
                Assert.Equal(1, person.Id);
                Assert.Equal("Kalle", person.AliasName);
                Assert.Equal(42, person.AliasAge);
            }
        }

        [Fact]
        public void ItShouldBeAbleToDoDynamicMapping()
        {
            var factory = new HyperboliqConnectionFactory(SqlLite.Dialect, ":memory:");
            using (var con = factory.OpenDbConnection())
            {
                const string createTable = "CREATE TABLE Person (Id INT, Name VARCHAR(50), Age INT, LivesAtHouseId INT, ParentId INT)";
                var cmd1 = con.AsIDbConnection().CreateCommand();
                cmd1.CommandText = createTable;
                cmd1.ExecuteNonQuery();

                var insertQuery = Insert.Into<Person>().AllColumns.Values(new Person { Id = 1, Name = "Kalle", Age = 42 });
                con.ExecuteNonQuery(insertQuery);

                var selectQuery = Select.Star<Person>().From<Person>();
                var persons = con.DynamicQuery(selectQuery);

                Assert.Equal(1, persons.Count());
                dynamic person = persons.First();
                Assert.Equal(person.Id, 1);
                Assert.Equal(person.Name, "Kalle");
                Assert.Equal(person.Age, 42);
            }
        }

        [Fact]
        public void ItShouldBePossibleToRunAQueryWithParameters()
        {
            var factory = new HyperboliqConnectionFactory(SqlLite.Dialect, ":memory:");
            using (var con = factory.OpenDbConnection())
            {
                const string createTable = "CREATE TABLE Person (Id INT, Name VARCHAR(50), Age INT, LivesAtHouseId INT, ParentId INT)";
                var cmd1 = con.AsIDbConnection().CreateCommand();
                cmd1.CommandText = createTable;
                cmd1.ExecuteNonQuery();

                var insertQuery = Insert.Into<Person>().AllColumns.Values(new Person { Id = 1, Name = "Kalle", Age = 42 });
                con.ExecuteNonQuery(insertQuery);

                var nameParam = new ExpressionParameter<string>("name");
                var selectQuery = Select.Star<Person>().From<Person>().Where<Person>(p => p.Name == nameParam);
                nameParam.SetValue("Kalle");
                var persons = con.Query<Person>(selectQuery, nameParam);

                Assert.Equal(1, persons.Count());
                var person = persons.First();
                Assert.Equal(1, person.Id);
                Assert.Equal("Kalle", person.Name);
                Assert.Equal(42, person.Age);
            }
        }

        public class MapAliasTestResultSet
        {
            public int Age { get; set; }
            public long Count { get; set; }
        }

        [Fact]
        public void ItShouldBePossibleToMapAliasedColumns()
        {
            var factory = new HyperboliqConnectionFactory(SqlLite.Dialect, ":memory:");
            using (var con = factory.OpenDbConnection())
            {
                const string createTable = "CREATE TABLE Person (Id INT, Name VARCHAR(50), Age INT, LivesAtHouseId INT, ParentId INT)";
                var cmd1 = con.AsIDbConnection().CreateCommand();
                cmd1.CommandText = createTable;
                cmd1.ExecuteNonQuery();

                var insertQuery = Insert.Into<Person>().AllColumns.Values(
                    new Person { Id = 1, Name = "Kalle", Age = 42 },
                    new Person { Id = 2, Name = "Pelle", Age = 42 },
                    new Person { Id = 3, Name = "Putte", Age = 45 });
                con.ExecuteNonQuery(insertQuery);

                var selectQuery = Select.Column<Person>(p => new { p.Age, Count = Sql.Count() }).From<Person>().GroupBy<Person>(p => p.Age);
                var result = con.Query<MapAliasTestResultSet>(selectQuery);

                Assert.Equal(2, result.Count());

                var first = result.ElementAt(0);
                Assert.Equal(42, first.Age);
                Assert.Equal(2, first.Count);

                var second = result.ElementAt(1);
                Assert.Equal(45, second.Age);
                Assert.Equal(1, second.Count);
            }
        }

        [Fact]
        public void ItShouldBePossibleToMapUnions()
        {
            var factory = new HyperboliqConnectionFactory(SqlLite.Dialect, ":memory:");
            using (var con = factory.OpenDbConnection())
            {
                const string createTable = "CREATE TABLE Person (Id INT, Name VARCHAR(50), Age INT, LivesAtHouseId INT, ParentId INT)";
                var cmd1 = con.AsIDbConnection().CreateCommand();
                cmd1.CommandText = createTable;
                cmd1.ExecuteNonQuery();

                var insertQuery = Insert.Into<Person>().AllColumns.Values(
                    new Person { Id = 1, Name = "Kalle", Age = 42 },
                    new Person { Id = 2, Name = "Pelle", Age = 42 },
                    new Person { Id = 3, Name = "Putte", Age = 45 });
                con.ExecuteNonQuery(insertQuery);

                var selectQuery =
                    SetOperations.Union(
                        Select.Star<Person>().From<Person>().Where<Person>(p => p.Age > 42),
                        Select.Star<Person>().From<Person>().Where<Person>(p => p.Name == "Kalle")
                    );
                var result = con.Query<Person>(selectQuery);

                Assert.Equal(2, result.Count());

                Assert.True(result.Any(p => p.Id == 1 && p.Name == "Kalle" && p.Age == 42));
                Assert.True(result.Any(p => p.Id == 3 && p.Name == "Putte" && p.Age == 45));
            }
        }
    }
}
