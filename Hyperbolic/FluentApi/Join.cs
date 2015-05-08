using Microsoft.FSharp.Collections;
using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.SelectExpressionParts;
using Hyperboliq.Domain;

namespace Hyperboliq
{
    public class Join : ISqlExpressionTransformable, ISqlTransformable
    {
        private SelectExpression expr;
        internal Join(SelectExpression expr)
        {
            this.expr = expr;
        }

        public Join InnerJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr) =>
            PerformTwoTableJoin(JoinType.InnerJoin, TableReferenceFromType<TSource>(), TableReferenceFromType<TTarget>(), joinExpr);

        public Join InnerJoin<TSource, TTarget>(
            ITableReference<TSource> source, 
            ITableReference<TTarget> target, 
            Expression<Func<TSource, TTarget, bool>> joinExpr) =>
            PerformTwoTableJoin(JoinType.InnerJoin, source, target, joinExpr);

        public Join InnerJoin<TSource1, TSource2, TTarget>(Expression<Func<TSource1, TSource2, TTarget, bool>> joinExpr) =>
            PerformThreeTableJoin(JoinType.InnerJoin, TableReferenceFromType<TSource1>(), TableReferenceFromType<TSource2>(), TableReferenceFromType<TTarget>(), joinExpr);

        public Join InnerJoin<TSource1, TSource2, TTarget>(
            ITableReference<TSource1> source1, 
            ITableReference<TSource2> source2, 
            ITableReference<TTarget> target, 
            Expression<Func<TSource1, TSource2, TTarget, bool>> joinExpr) =>
            PerformThreeTableJoin(JoinType.InnerJoin, source1, source2, target, joinExpr);


        public Join LeftJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr) =>
            PerformTwoTableJoin(JoinType.LeftJoin, TableReferenceFromType<TSource>(), TableReferenceFromType<TTarget>(), joinExpr);

        public Join RightJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr) =>
            PerformTwoTableJoin(JoinType.RightJoin, TableReferenceFromType<TSource>(), TableReferenceFromType<TTarget>(), joinExpr);

        public Join FullJoin<TSource, TTarget>(Expression<Func<TSource, TTarget, bool>> joinExpr) =>
            PerformTwoTableJoin(JoinType.FullJoin, TableReferenceFromType<TSource>(), TableReferenceFromType<TTarget>(), joinExpr);

        public SelectWhere Where<TTableType>(Expression<Func<TTableType, bool>> predicate) =>
            new SelectWhere(expr).And<TTableType>(predicate);

        public SelectWhere Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> predicate) =>
            new SelectWhere(expr).And<TTable1, TTable2>(predicate);

        public OrderBy OrderBy<TTableType>(Expression<Func<TTableType, object>> orderExpr, Direction direction = null, NullsOrdering nullsOrdering = null) =>
            new OrderBy(expr).ThenBy(orderExpr, direction, nullsOrdering);

        public GroupBy GroupBy<TTableType>(Expression<Func<TTableType, object>> groupByExpr) =>
            new GroupBy(expr).ThenBy(groupByExpr);

        public SqlExpression ToSqlExpression() => SqlExpression.NewSelect(expr);

        public string ToSql(ISqlDialect dialect) => SqlGenerator.SqlifyExpression(dialect, ToSqlExpression());

        private Join PerformTwoTableJoin<TSource, TTarget>(
            JoinType type, 
            ITableReference<TSource> source, 
            ITableReference<TTarget> target, 
            Expression<Func<TSource, TTarget, bool>> joinExpr)
        {
            expr =
                WithFromClause(
                    expr,
                    AddJoinClause(
                        expr.From,
                        CreateJoinClause(type, joinExpr, target, source)));
            return this;
        }

        private Join PerformThreeTableJoin<TSource1, TSource2, TTarget>(
            JoinType type, 
            ITableReference<TSource1> source1,
            ITableReference<TSource2> source2,
            ITableReference<TTarget> target,
            Expression<Func<TSource1, TSource2, TTarget, bool>> joinExpr)
        {
            expr =
                WithFromClause(
                    expr,
                    AddJoinClause(
                        expr.From,
                        CreateJoinClause(type, joinExpr, target, source1, source2)));
            return this;
        }
    }

}
