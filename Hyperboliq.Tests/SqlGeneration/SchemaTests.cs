using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Hyperboliq;
using Hyperboliq.Dialects;
using Hyperboliq.Domain;
using S = Hyperboliq.Tests.SqlStreamExtensions;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;

namespace Hyperboliq.Tests.SqlGeneration
{
    [Schema("HumanResources")]
    public class Employee
    {
        public string Name { get; set; }
        public int Salary { get; set; }
    }

    [TestFixture]
    public class SqlGeneration_SchemaTests
    {
        [Test]
        public void ItShouldIncludeTheSchemaInTheGeneratedSql()
        {
            var stream =
                S.SelectNode(
                    S.Select(S.Star<Employee>()),
                    S.From<Employee>(),
                    S.Where(S.BinExp(S.Col<Employee>("Salary"), BinaryOperation.GreaterThan, S.Const(25000))));

            var result = SqlGen.SqlifyExpression(AnsiSql.Dialect, stream);
            Assert.That(result, Is.EqualTo(@"SELECT EmployeeRef.* FROM HumanResources.Employee EmployeeRef WHERE EmployeeRef.Salary > 25000"));
        }
    }
}
