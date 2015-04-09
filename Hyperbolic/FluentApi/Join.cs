using Microsoft.FSharp.Collections;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;

namespace Hyperboliq
{
    public class Join : ISqlStreamTransformable
    {
        private SelectExpression expr;
        internal Join(SelectExpression expr)
        {
            this.expr = expr;
        }

        public Join InnerJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr)
        {
            expr.InnerJoin(joinExpr);
            return this;
        }

        public Join InnerJoin<TSource1, TSource2, TTarget>(Expression<Func<TSource1, TSource2, TTarget, bool>> joinExpr)
        {
            expr.InnerJoin(joinExpr);
            return this;
        }

        public Join LeftJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr)
        {
            expr.LeftJoin(joinExpr);
            return this;
        }

        public Join RightJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr)
        {
            expr.RightJoin(joinExpr);
            return this;
        }

        public Join FullJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr)
        {
            expr.FullJoin(joinExpr);
            return this;
        }

        public SelectWhere Where<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            var where = new SelectWhere(expr);
            return where.And(predicate);
        }

        public SelectWhere Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            var where = new SelectWhere(expr);
            return where.And(predicate);
        }
        public OrderBy OrderBy<TTableType>(Expression<Func<TTableType, object>> orderExpr, Direction direction = null, NullsOrdering nullsOrdering = null)
        {
            return new OrderBy(expr).ThenBy(orderExpr, direction, nullsOrdering);
        }

        public GroupBy GroupBy<TTableType>(Expression<Func<TTableType, object>> groupByExpr)
        {
            return new GroupBy(expr).ThenBy(groupByExpr);
        }

        public FSharpList<SqlNode> ToSqlStream() { return expr.ToSqlStream(); }
    }

}
