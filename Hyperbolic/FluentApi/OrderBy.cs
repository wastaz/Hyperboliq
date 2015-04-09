using Microsoft.FSharp.Collections;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;

namespace Hyperboliq
{
    public class OrderBy : ISqlStreamTransformable
    {
        private SelectExpression expr;

        internal OrderBy(SelectExpression expr) { this.expr = expr; }

        public OrderBy ThenBy<TTableType>(Expression<Func<TTableType, object>> orderExpr, Direction direction = null, NullsOrdering nullsOrdering = null)
        {
            expr.OrderBy(orderExpr, direction ?? Direction.Ascending, nullsOrdering ?? NullsOrdering.NullsUndefined);
            return this;
        }

        public FSharpList<SqlNode> ToSqlStream() { return expr.ToSqlStream(); }
    }
}
