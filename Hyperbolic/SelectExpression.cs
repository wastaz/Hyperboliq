using System;
using System.Linq.Expressions;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.ExpressionParts;
using static Hyperboliq.Domain.StreamGenerator;
using static Hyperboliq.Domain.Stream;
using Microsoft.FSharp.Collections;
using Hyperboliq.Domain;

namespace Hyperboliq
{
    public class SelectExpression : ISqlQuery, ISqlStreamTransformable
    {
        internal SelectExpressionNode SelectExpr { get; private set; } = NewSelectExpression();

        internal FromExpressionNode FromExpression { get; private set; } = NewFromExpression();

        internal JoinExpression JoinExpression { get; private set; } = NewJoinExpression();

        internal WhereExpressionNode WhereExpression { get; private set; }

        internal GroupByExpressionNode GroupByExpression { get; private set; }

        internal OrderByExpressionNode OrderByExpression { get; private set; }

        internal enum CompositeExpressionMode
        {
            Where,
            Having,
        }

        internal CompositeExpressionMode CurrentCompositeExpressionMode { get; set; } = CompositeExpressionMode.Where;

        public SelectExpression SelectAll<TTableType>()
        {
            SelectExpr = SelectAllColumns(SelectExpr, TableReferenceFromType<TTableType>());
            return this;
        }

        public SelectExpression Select<TTableType>(Expression<Func<TTableType, object>> selectExpr)
        {
            return Select(TableReferenceFromType<TTableType>(), selectExpr);
        }

        public SelectExpression Select<TTableType>(ITableReference<TTableType> table, Expression<Func<TTableType, object>> selectExpr)
        {
            SelectExpr = SelectColumns(SelectExpr, selectExpr, table);
            return this;
        }

        public SelectExpression Distinct()
        {
            SelectExpr = MakeDistinct(SelectExpr);
            return this;
        }

        public SelectExpression From<TTableType>()
        {
            return From<TTableType>(TableReferenceFromType<TTableType>());
        }

        public SelectExpression From<TTableType>(ITableReference<TTableType> table)
        {
            FromExpression = AddFromTable(FromExpression, table);
            return this;
        }

        public SelectExpression Where<TTableType>(Expression<Func<TTableType, bool>> predicate) {
            CurrentCompositeExpressionMode = CompositeExpressionMode.Where;
            WhereExpression = NewWhereExpression(predicate, TableReferenceFromType<TTableType>());
            return this;
        }

        public SelectExpression Where<TFirstTable, TSecondTable>(Expression<Func<TFirstTable, TSecondTable, bool>> predicate) {
            CurrentCompositeExpressionMode = CompositeExpressionMode.Where;
            WhereExpression = NewWhereExpression(predicate, TableReferenceFromType<TFirstTable>(), TableReferenceFromType<TSecondTable>());
            return this;
        }

        public SelectExpression And<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            if(CurrentCompositeExpressionMode == CompositeExpressionMode.Where)
            {
                WhereExpression = AddWhereAndClause(WhereExpression, predicate, TableReferenceFromType<TTableType>());
            }
            else
            {
                GroupByExpression = AddHavingAndClause(GroupByExpression, predicate, TableReferenceFromType<TTableType>());
            }
            return this;
        }

        public SelectExpression And<TFirstTable, TSecondTable>(Expression<Func<TFirstTable, TSecondTable, bool>> predicate)
        {
            if (CurrentCompositeExpressionMode == CompositeExpressionMode.Where)
            {
                WhereExpression = AddWhereAndClause(WhereExpression, predicate, TableReferenceFromType<TFirstTable>(), TableReferenceFromType<TSecondTable>());
            }
            else
            {
                GroupByExpression = AddHavingAndClause(GroupByExpression, predicate, TableReferenceFromType<TFirstTable>(), TableReferenceFromType<TSecondTable>());
            }
            return this;
        }

        public SelectExpression Or<TTableType>(Expression<Func<TTableType, bool>> predicate)
        {
            if (CurrentCompositeExpressionMode == CompositeExpressionMode.Where)
            {
                WhereExpression = AddWhereOrClause(WhereExpression, predicate, TableReferenceFromType<TTableType>());
            }
            else
            {
                GroupByExpression = AddHavingOrClause(GroupByExpression, predicate, TableReferenceFromType<TTableType>());
            }
            return this;
        }

        public SelectExpression Or<TFirstTable, TSecondTable>(Expression<Func<TFirstTable, TSecondTable, bool>> predicate)
        {
            if (CurrentCompositeExpressionMode == CompositeExpressionMode.Where)
            {
                WhereExpression = AddWhereOrClause(WhereExpression, predicate, TableReferenceFromType<TFirstTable>(), TableReferenceFromType<TSecondTable>());
            }
            else
            {
                GroupByExpression = AddHavingOrClause(GroupByExpression, predicate, TableReferenceFromType<TFirstTable>(), TableReferenceFromType<TSecondTable>());
            }
            return this;
        }

        public SelectExpression InnerJoin<TSourceTable, TTargetTable>(Expression<Func<TSourceTable, TTargetTable, bool>> joinExpr)
            => InnerJoin(TableReferenceFromType<TSourceTable>(), TableReferenceFromType<TTargetTable>(), joinExpr);

        public SelectExpression InnerJoin<TFirstSourceTable, TSecondSourceTable, TTargetTable>(
            Expression<Func<TFirstSourceTable, TSecondSourceTable, TTargetTable, bool>> joinExpr)
            => AddJoin(
                TableReferenceFromType<TFirstSourceTable>(), 
                TableReferenceFromType<TSecondSourceTable>(), 
                TableReferenceFromType<TTargetTable>(), 
                Domain.Stream.JoinType.InnerJoin, 
                joinExpr);

        public SelectExpression InnerJoin<TSourceTable, TTargetTable>(ITableReference<TSourceTable> sourceTable, 
                                                                   ITableReference<TTargetTable> targetTable,
                                                                   Expression<Func<TSourceTable, TTargetTable, bool>> joinExpr) 
            => AddJoin(sourceTable, targetTable, Domain.Stream.JoinType.InnerJoin, joinExpr);

        public SelectExpression InnerJoin<TFirstSourceTable, TSecondSourceTable, TTargetTable>(
            ITableReference<TFirstSourceTable> sourceTable1,
            ITableReference<TSecondSourceTable> sourceTable2,
            ITableReference<TTargetTable> targetTable,
            Expression<Func<TFirstSourceTable, TSecondSourceTable, TTargetTable, bool>> joinExpr)
            => AddJoin(sourceTable1, sourceTable2, targetTable, Domain.Stream.JoinType.InnerJoin, joinExpr);


        public SelectExpression LeftJoin<TSourceTable, TTargetTable>(Expression<Func<TSourceTable, TTargetTable, bool>> joinExpr)
            => AddJoin(TableReferenceFromType<TSourceTable>(), TableReferenceFromType<TTargetTable>(), Domain.Stream.JoinType.LeftJoin, joinExpr);

        public SelectExpression RightJoin<TSourceTable, TTargetTable>(Expression<Func<TSourceTable, TTargetTable, bool>> joinExpr)
            => AddJoin(TableReferenceFromType<TSourceTable>(), TableReferenceFromType<TTargetTable>(), Domain.Stream.JoinType.RightJoin, joinExpr);

        public SelectExpression FullJoin<TSourceTable, TTargetTable>(Expression<Func<TSourceTable, TTargetTable, bool>> joinExpr)
            => AddJoin(TableReferenceFromType<TSourceTable>(), TableReferenceFromType<TTargetTable>(), Domain.Stream.JoinType.FullJoin, joinExpr);

        private SelectExpression AddJoin<TFirstSourceTable, TSecondSourceTable, TTargetTable>(
            ITableReference<TFirstSourceTable> source1,
            ITableReference<TSecondSourceTable> source2,
            ITableReference<TTargetTable> target,
            Domain.Stream.JoinType type,
            Expression<Func<TFirstSourceTable, TSecondSourceTable, TTargetTable, bool>> joinExpr
            )
        {
            JoinExpression =
                AddJoinClause(
                    JoinExpression,
                    new JoinClause(ListModule.OfArray(new ITableReference[] { source1, source2 }), target, type, joinExpr));
            return this;
        }

        private SelectExpression AddJoin<TSourceTable, TTargetTable>(
            ITableReference<TSourceTable> source, 
            ITableReference<TTargetTable> target, 
            Domain.Stream.JoinType type, 
            Expression<Func<TSourceTable, TTargetTable, bool>> condition)
        {
            JoinExpression =
                AddJoinClause(
                    JoinExpression,
                    new JoinClause(ListModule.OfArray(new ITableReference[] { source }), target, type, condition));
            return this;
        }

        public SelectExpression GroupBy<TTableType>(Expression<Func<TTableType, object>> groupByExpr)
        {
            if(GroupByExpression == null)
            {
                GroupByExpression = NewGroupByExpression();
            }
            GroupByExpression = AddGroupByClause(GroupByExpression, groupByExpr, TableReferenceFromType<TTableType>());
            return this;
        }

        public SelectExpression Having<TTableType>(Expression<Func<TTableType, bool>> havingExpr)
        {
            CurrentCompositeExpressionMode = CompositeExpressionMode.Having;
            return And<TTableType>(havingExpr);
        }

        public SelectExpression OrderBy<TTableType>(Expression<Func<TTableType, object>> orderExpr, Direction direction, NullsOrdering nullsOrdering = null)
        {
            if(OrderByExpression == null)
            {
                OrderByExpression = NewOrderByExpression();
            }
            OrderByExpression =
                AddOrderingClause(
                    OrderByExpression,
                    TableReferenceFromType<TTableType>(), 
                    direction, 
                    nullsOrdering ?? NullsOrdering.NullsUndefined,
                    orderExpr);
            return this;
        }

        public SelectExpression ThenBy<TTableType>(Expression<Func<TTableType, object>> orderExpr, Direction direction, NullsOrdering nullsOrdering = null)
            => OrderBy(orderExpr, direction, nullsOrdering);

        public FSharpList<SqlNode> ToSqlStream()
        {
            var selectPart = new FSharpList<SqlNode>(SqlNode.NewSelect(SelectExpr), FSharpList<SqlNode>.Empty);
            var fromPart = new FSharpList<SqlNode>(SqlNode.NewFrom(FromExpression), FSharpList<SqlNode>.Empty);
            FSharpList<SqlNode> wherePart = null;
            if(WhereExpression != null)
            {
                wherePart = new FSharpList<SqlNode>(SqlNode.NewWhere(WhereExpression), FSharpList<SqlNode>.Empty);
            }
            FSharpList<SqlNode> groupPart = null;
            if(GroupByExpression != null)
            {
                groupPart = new FSharpList<SqlNode>(SqlNode.NewGroupBy(GroupByExpression), FSharpList<SqlNode>.Empty);
            }
            FSharpList<SqlNode> orderPart = null;
            if(OrderByExpression != null)
            {
                orderPart = new FSharpList<SqlNode>(SqlNode.NewOrderBy(OrderByExpression), FSharpList<SqlNode>.Empty);
            }

            var fromJoinStream = GenerateStream(
                new[] {
                    StreamInput.NewJoin(JoinExpression),
                });

            return ListModule.Concat(
                new[] {
                    selectPart,
                    fromPart,
                    fromJoinStream,
                    wherePart ?? FSharpList<SqlNode>.Empty,
                    groupPart ?? FSharpList<SqlNode>.Empty,
                    orderPart ?? FSharpList<SqlNode>.Empty,
                });
        }

        public string ToSql(ISqlDialect dialect)
        {
            return Domain.SqlGenerator.Sqlify(dialect, ToSqlStream());
        }
    }
}
