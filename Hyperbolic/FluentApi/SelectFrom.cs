using Microsoft.FSharp.Collections;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;

namespace Hyperboliq
{
    public class SelectFrom<TTableType> : ISqlQuery, ISqlStreamTransformable
    {
        private SelectExpression expr;

        internal SelectFrom(SelectExpression expr)
        {
            this.expr = expr.From<TTableType>();
        }

        public Join InnerJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr)
        {
            return new Join(expr).InnerJoin(joinExpr);
        }

        public Join InnerJoin<TSource1, TSource2, TTarget>(Expression<Func<TSource1, TSource2, TTarget, bool>> joinExpr)
        {
            return new Join(expr).InnerJoin(joinExpr);
        }


        public Join LeftJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr)
        {
            return new Join(expr).LeftJoin(joinExpr);
        }


        public Join RightJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr)
        {
            return new Join(expr).RightJoin(joinExpr);
        }


        public Join FullJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr)
        {
            return new Join(expr).FullJoin(joinExpr);
        }

        public SelectWhere Where<TTable>(Expression<Func<TTable, bool>> predicate)
        {
            expr.Where(predicate);
            return new SelectWhere(expr);
        }

        public SelectWhere Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            expr.Where(predicate);
            return new SelectWhere(expr);
        }

        public OrderBy OrderBy<TTable>(Expression<Func<TTable, object>> orderExpr, Direction direction = null, NullsOrdering nullsOrdering = null)
        {
            return new OrderBy(expr).ThenBy(orderExpr, direction, nullsOrdering);
        }

        public GroupBy GroupBy<TTable>(Expression<Func<TTable, object>> groupByExpr)
        {
            return new GroupBy(expr).ThenBy(groupByExpr);
        }

        public FSharpList<SqlNode> ToSqlStream() => expr.ToSqlStream();

        public string ToSql(ISqlDialect dialect) => expr.ToSql(dialect);

        public static implicit operator SelectExpression(SelectFrom<TTableType> select) => select.expr;
    }
}
