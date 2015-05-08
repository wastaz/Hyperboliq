using Microsoft.FSharp.Collections;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using Microsoft.FSharp.Core;
using Hyperboliq.Domain;

namespace Hyperboliq
{
    public class SelectFrom<TTableType> : ISqlQuery, ISqlExpressionTransformable, ISelectExpressionTransformable
    {
        private Domain.Stream.SelectExpression expr;

        internal SelectFrom(ITableReference<TTableType> table, SelectExpressionNode selectNode)
        {
            var from = ExpressionParts.AddFromTable(ExpressionParts.NewFromExpression(), table);
            expr = new SelectExpression(
                selectNode,
                from,
                FSharpOption<WhereExpressionNode>.None,
                FSharpOption<GroupByExpressionNode>.None,
                FSharpOption<OrderByExpressionNode>.None);
        }

        public Join InnerJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr)
        {
            return new Join(expr).InnerJoin(joinExpr);
        }

        public Join InnerJoin<TSource, TTarget>(
            ITableReference<TSource> source, 
            ITableReference<TTarget> target, 
            Expression<Func<TSource, TTarget, bool>> joinExpr)
        {
            return new Join(expr).InnerJoin(source, target, joinExpr);
        }

        public Join InnerJoin<TSource1, TSource2, TTarget>(
            ITableReference<TSource1> source1,
            ITableReference<TSource2> source2,
            ITableReference<TTarget> target,
            Expression<Func<TSource1, TSource2, TTarget, bool>> joinExpr)
        {
            return new Join(expr).InnerJoin(source1, source2, target, joinExpr);
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
            return new SelectWhere(expr).And(predicate);
        }

        public SelectWhere Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate)
        {
            return new SelectWhere(expr).And(predicate);
        }

        public OrderBy OrderBy<TTable>(Expression<Func<TTable, object>> orderExpr, Direction direction = null, NullsOrdering nullsOrdering = null)
        {
            return new OrderBy(expr).ThenBy(orderExpr, direction, nullsOrdering);
        }

        public GroupBy GroupBy<TTable>(Expression<Func<TTable, object>> groupByExpr)
        {
            return new GroupBy(expr).ThenBy(groupByExpr);
        }

        public SelectExpression ToSelectExpression() => expr;

        public SqlExpression ToSqlExpression() => SqlExpression.NewSelect(expr);

        public string ToSql(ISqlDialect dialect) => SqlGenerator.SqlifyExpression(dialect, ToSqlExpression());

        public static implicit operator SelectExpression(SelectFrom<TTableType> select) => select.expr;
    }
}
