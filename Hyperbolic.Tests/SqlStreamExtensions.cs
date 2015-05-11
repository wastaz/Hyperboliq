using System;
using System.Collections.Generic;
using System.Linq;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace Hyperboliq.Tests
{
    public static class SqlStreamExtensions
    {
        public static ValueNode Col<TTableType>(string columnDef)
        {
            return ValueNode.NewColumn(new Tuple<string, ITableReference>(columnDef, TableReferenceFromType<TTableType>()));
        }

        public static ValueNode Col(ITableReference r, string columnDef)
        {
            return ValueNode.NewColumn(new Tuple<string, ITableReference>(columnDef, r));
        }

        public static ValueNode Aggregate(AggregateType type, ValueNode parameter)
        {
            return ValueNode.NewAggregate(new Tuple<AggregateType, ValueNode>(type, parameter));
        }
        public static ValueNode BinExp(ValueNode lhs, BinaryOperation op, ValueNode rhs)
        {
            return ValueNode.NewBinaryExpression(new BinaryExpressionNode(lhs, op, rhs));
        }

        public static ValueNode Param(string paramName)
        {
            return ValueNode.NewParameter(ParameterToken.NewParameterToken(paramName));
        }

        public static ValueNode Const(object constant)
        {
            return ValueNode.NewConstant(ConstantNode.NewConstantNode(constant.ToString()));
        }

        public static ValueNode Null()
        {
            return ValueNode.NullValue;
        }

        public static InsertStatementHeadToken InsHead<TTableType>(params string[] colNames)
        {
            return 
                new InsertStatementHeadToken(
                    TableReferenceFromType<TTableType>(),
                    ListModule.OfSeq(
                        colNames.Select(n => new Tuple<string, ITableReference>(n, TableReferenceFromType<TTableType>()))));
        }

        private static UpdateSetToken CreateUpdateSetToken<TTableType>(string colDef, ValueNode stream)
        {
            return new UpdateSetToken(new Tuple<string, ITableReference>(colDef, TableReferenceFromType<TTableType>()), stream);
        }

        public static UpdateSetToken Ust<TTableType>(string colDef, object c)
        {
            return CreateUpdateSetToken<TTableType>(colDef, Const(c));
        }

        public static UpdateSetToken Ust<TTableType>(string colDef, ValueNode node)
        {
            return CreateUpdateSetToken<TTableType>(colDef, node);
        }

        public static UpdateStatementHeadToken UpdHead<TTableType>(params UpdateSetToken[] setExprs)
        {
            return 
                new UpdateStatementHeadToken(
                    TableReferenceFromType<TTableType>(),
                    ListModule.OfArray(setExprs));
        }

        public static InsertValueToken InsVal(params InsertValueNode[] nodes)
        {
            return new InsertValueToken(ListModule.OfArray(nodes));
        }

        public static InsertValueNode InsConst(object constant)
        {
            return InsertValueNode.NewConstant(ConstantNode.NewConstantNode(constant.ToString()));
        }

        public static InsertValueNode InsParam(string paramName)
        {
            return InsertValueNode.NewParameter(ParameterToken.NewParameterToken(paramName));
        }

        public static InsertValueNode InsCol<TTableType>(string columnDef)
        {
            return InsertValueNode.NewColumn(new Tuple<string, ITableReference>(columnDef, TableReferenceFromType<TTableType>()));
        }

        public static ValueNode SubExp(
            SelectExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return ValueNode.NewSubExpression(
                new SelectExpression(
                    select,
                    from,
                    OptionModule.OfObj(where),
                    OptionModule.OfObj(groupBy),
                    OptionModule.OfObj(orderBy)));
        }

        public static OrderByClauseNode OrderClause(ValueNode col, Direction direction, NullsOrdering nullsOrdering = null)
        {
            return new OrderByClauseNode(
                direction,
                nullsOrdering ?? NullsOrdering.NullsUndefined,
                col);
        }

        public static OrderByExpressionNode OrderBy(params OrderByClauseNode[] clauses)
        {
            return new OrderByExpressionNode(ListModule.OfArray(clauses));
        }
        
        public static SelectExpressionNode Select(params ValueNode[] columns)
        {
            return new SelectExpressionNode(false, ListModule.OfArray(columns));
        }

        public static SelectExpressionNode SelectDistinct(params ValueNode[] columns)
        {
            return new SelectExpressionNode(true, ListModule.OfArray(columns));
        }

        public static FromExpressionNode From(params ITableReference[] tables)
        {
            return new FromExpressionNode(ListModule.OfArray(tables), FSharpList<JoinClauseNode>.Empty);
        }

        public static FromExpressionNode From(ITableReference table, params JoinClauseNode[] joins)
        {
            return
                new FromExpressionNode(
                    new FSharpList<ITableReference>(table, FSharpList<ITableReference>.Empty),
                    ListModule.OfArray(joins));
        }

        public static FromExpressionNode From<TTable>(params JoinClauseNode[] joins)
        {
            return
                new FromExpressionNode(
                    new FSharpList<ITableReference>(TableReferenceFromType<TTable>(), FSharpList<ITableReference>.Empty),
                    ListModule.OfArray(joins));
        }

        public static JoinClauseNode Join<TSource, TTarget>(JoinType type, ValueNode joinExpr)
        {
            return new JoinClauseNode(
                new FSharpList<ITableReference>(TableReferenceFromType<TSource>(), FSharpList<ITableReference>.Empty),
                TableReferenceFromType<TTarget>(),
                type,
                OptionModule.OfObj(joinExpr));
        }
        public static JoinClauseNode Join<TSource1, TSource2, TTarget>(JoinType type, ValueNode joinExpr)
        {
            return new JoinClauseNode(
                ListModule.OfArray(new ITableReference[] { TableReferenceFromType<TSource1>(), TableReferenceFromType<TSource2>() }),
                TableReferenceFromType<TTarget>(),
                type,
                OptionModule.OfObj(joinExpr));
        }

        public static JoinClauseNode Join(ITableReference source1, ITableReference source2, ITableReference target, JoinType type, ValueNode joinExpr)
        {
            return new JoinClauseNode(
                ListModule.OfArray(new[] { source1, source2 }),
                target,
                type,
                OptionModule.OfObj(joinExpr));
        }

        public static JoinClauseNode Join(ITableReference source, ITableReference target, JoinType type, ValueNode joinExpr)
        {
            return new JoinClauseNode(
                new FSharpList<ITableReference>(source, FSharpList<ITableReference>.Empty),
                target,
                type,
                OptionModule.OfObj(joinExpr));
        }

        public static WhereExpressionNode Where(ValueNode start, params WhereClauseNode[] additionalClauses)
        {
            return
                new WhereExpressionNode(start, ListModule.OfArray(additionalClauses));
        }

        public static WhereClauseNode And(ValueNode stream)
        {
            return new WhereClauseNode(ExpressionCombinatorType.And, stream);
        }

        public static WhereClauseNode Or(ValueNode stream)
        {
            return new WhereClauseNode(ExpressionCombinatorType.Or, stream);
        }

        public static GroupByExpressionNode GroupBy(params ValueNode[] clauses)
        {
            return new GroupByExpressionNode(ListModule.OfArray(clauses), FSharpList<WhereClauseNode>.Empty);
        }

        public static GroupByExpressionNode GroupBy(IEnumerable<ValueNode> clauses, params WhereClauseNode[] having)
        {
            return new GroupByExpressionNode(ListModule.OfSeq(clauses), ListModule.OfArray(having));
        }

        public static SqlExpression SelectNode(
            SelectExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return SqlExpression.NewSelect(
                new SelectExpression(
                    select,
                    from,
                    OptionModule.OfObj(where),
                    OptionModule.OfObj(groupBy),
                    OptionModule.OfObj(orderBy)));
        }

        public static SqlExpression DeleteNode(FromExpressionNode from, WhereExpressionNode where = null)
        {
            return SqlExpression.NewDelete(
                new DeleteExpression(from, OptionModule.OfObj(where)));
        }

        public static SqlExpression UpdateNode(UpdateStatementHeadToken head, WhereExpressionNode where = null)
        {
            return SqlExpression.NewUpdate(
                new UpdateExpression(
                    head,
                    OptionModule.OfObj(where)));
        }

        public static SqlExpression InsertNode(InsertStatementHeadToken head, params InsertValueToken[] values)
        {
            return SqlExpression.NewInsert(
                new InsertExpression(
                    head,
                    ListModule.OfArray(values)));
        }
    }
}
