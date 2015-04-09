using System;
using System.Linq.Expressions;

namespace Hyperboliq.FluentApi
{
    public class InsertInto<TTable>
    {
        private readonly InsertExpression<TTable> expr = new InsertExpression<TTable>();

        public InsertValues<TTable> AllColumns
        {
            get { return new InsertValues<TTable>(expr.AllColumns()); }
        }

        public InsertValues<TTable> Columns(Expression<Func<TTable, object>> selector) =>
            new InsertValues<TTable>(expr.Columns(selector));
    }


    public static class Insert
    {
        public static InsertInto<TTable> Into<TTable>()
        {
            return new InsertInto<TTable>();
        }
    }
}
