using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Hyperboliq;
using Hyperboliq.Dialects;

namespace Hyperboliq.Tests.SqlServer
{
    [TestFixture]
    public class ConnectionTests
    {
        [Test]
        [Explicit]
        public void ItCanConnectToSqlServer()
        {
            var dbFactory = new HyperboliqConnectionFactory(Dialects.SqlServer.Dialect, "Server=.\\SQLEXPRESS;Initial Catalog=AdventureWorks;Integrated Security=SSPI;");
            try
            {
                var connection = dbFactory.OpenDbConnection();
                var con = (IDisposable)connection;
                con.Dispose();
            }
            catch
            {
                Assert.Fail();
            }
        }
    }
}
