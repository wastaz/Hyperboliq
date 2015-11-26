using NUnit.Framework;
using System.Linq;
using Hyperboliq.Domain;
using Hyperboliq.Dialects;
using Hyperboliq.Tests.TokenGeneration;

namespace Hyperboliq.Tests.Sqllite
{
    [TestFixture]
    public class BasicMappingTests
    {
        [Test]
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

                Assert.That(affected, Is.EqualTo(3));
            }
        }

        [Test]
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

                Assert.That(persons, Has.Count.EqualTo(1));
                var person = persons.First();
                Assert.That(person.Name, Is.EqualTo("Kalle"));
                Assert.That(person.Id, Is.EqualTo(1));
                Assert.That(person.Age, Is.EqualTo(42));
            }
        }

        [Test]
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

                Assert.That(result, Is.EqualTo(3));
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

        [Test]
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

                Assert.That(persons, Has.Count.EqualTo(1));
                var person = persons.First();
                Assert.That(person.Id, Is.EqualTo(1));
                Assert.That(person.AliasName, Is.EqualTo("Kalle"));
                Assert.That(person.AliasAge, Is.EqualTo(42));
            }
        }

        [Test]
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

                Assert.That(persons, Has.Count.EqualTo(1));
                dynamic person = persons.First();
                Assert.That(person.Id, Is.EqualTo(1));
                Assert.That(person.Name, Is.EqualTo("Kalle"));
                Assert.That(person.Age, Is.EqualTo(42));
            }
        }

        [Test]
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

                Assert.That(persons, Has.Count.EqualTo(1));
                var person = persons.First();
                Assert.That(person.Id, Is.EqualTo(1));
                Assert.That(person.Name, Is.EqualTo("Kalle"));
                Assert.That(person.Age, Is.EqualTo(42));
            }
        }

        public class MapAliasTestResultSet
        {
            public int Age { get; set; }
            public long Count { get; set; }
        }

        [Test]
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

                Assert.That(result, Has.Count.EqualTo(2));

                var first = result.ElementAt(0);
                Assert.That(first.Age, Is.EqualTo(42));
                Assert.That(first.Count, Is.EqualTo(2));

                var second = result.ElementAt(1);
                Assert.That(second.Age, Is.EqualTo(45));
                Assert.That(second.Count, Is.EqualTo(1));
            }
        }

        [Test]
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

                Assert.That(result, Has.Count.EqualTo(2));

                Assert.True(result.Any(p => p.Id == 1 && p.Name == "Kalle" && p.Age == 42));
                Assert.True(result.Any(p => p.Id == 3 && p.Name == "Putte" && p.Age == 45));
            }
        }
    }
}
