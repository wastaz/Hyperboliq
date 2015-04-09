using System;
using System.Linq.Expressions;
using Microsoft.FSharp.Collections;
using static Hyperboliq.Domain.Stream;

namespace Hyperboliq
{
    public class Having : ISqlStreamTransformable
    {
        private SelectExpression expr;

        internal Having(SelectExpression expr)
        {
            this.expr = expr;
            this.expr.CurrentCompositeExpressionMode = SelectExpression.CompositeExpressionMode.Having;
        }

        public Having And<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr.And(predicate);
            return this;
        }

        public Having And<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr.And(predicate);
            return this;
        }

        public Having Or<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            expr.Or(predicate);
            return this;
        }

        public Having Or<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr.Or(predicate);
            return this;
        }

        public FSharpList<SqlNode> ToSqlStream() { return expr.ToSqlStream(); }
    }

}
