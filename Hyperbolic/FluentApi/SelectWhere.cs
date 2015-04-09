using Microsoft.FSharp.Collections;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;

namespace Hyperboliq
{
    public class SelectWhere : ISqlQuery, ISqlStreamTransformable
    {
        private SelectExpression expr;
        internal SelectWhere(SelectExpression expr) { this.expr = expr; }

        public SelectWhere And<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr.And(predicate);
            return this;
        }

        public SelectWhere And<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr.And(predicate);
            return this;
        }

        public SelectWhere Or<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr.Or(predicate);
            return this;
        }

        public SelectWhere Or<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr.Or(predicate);
            return this;
        }

        public OrderBy OrderBy<TTableType>(Expression<Func<TTableType, object>> orderExpr, Direction direction = null, NullsOrdering nullsOrdering = null)
        {
            return new OrderBy(expr).ThenBy(orderExpr, direction, nullsOrdering);
        }

        public FSharpList<SqlNode> ToSqlStream() => expr.ToSqlStream();
        public string ToSql(ISqlDialect dialect) => expr.ToSql(dialect);
    }

}
