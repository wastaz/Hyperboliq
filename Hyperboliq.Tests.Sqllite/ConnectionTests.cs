using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Hyperboliq;
using Hyperboliq.Dialects;

namespace Hyperboliq.Tests.Sqllite
{
    [TestFixture]
    public class ConnectionTests
    {
        [Test]
        [Explicit]
        public void ItCanConnectToSqlLite()
        {
            var dbFactory = new HyperboliqConnectionFactory(Dialects.SqlLite.Dialect, "Data source=:memory:");
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
