using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Hyperboliq.Tests.Model;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Domain;

namespace Hyperboliq.Tests.TokenGeneration
{
    public class PersonLite
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class CommonTableExpressionTests
    {
        [Fact]
        public void ItShouldBePossibleToSelectFromACommonTableExpression() 
        {
            /*
            var cte = new Stream.CommonTableDefinition<PersonLite>(
                Select.Column<Person>(p => new { p.Name, p.Age })
                      .From<Person>()
                      .Where<Person>(p => p.Age > 15),
                Types.NamedTableReferenceFromType<PersonLite>("PersonLiteCteRef")
            );
            */
            var expr =
                With.Table<PersonLite>(
                        Select.Column<Person>(p => new { p.Name, p.Age })
                              .From<Person>()
                              .Where<Person>(p => p.Age > 15))
                    .Query(Select.Column<PersonLite>(p => p.Name)
                                 .From<PersonLite>()
                                 .Where<PersonLite>(p => p.Age == 42));
            var result = expr.ToSqlExpression();

            var expected =
                S.SelectNode(
                    
                );

            //var expectedSql =
                //"WITH PersonCte AS (SELECT PersonRef.Name, PersonRef.Age FROM Person PersonRef WHERE PersonRef.Age > 15) SELECT PersonLiteRef.Name FROM PersonCte PersonLiteRef WHERE PersonLifeRef = 42";
        }
    }
}
