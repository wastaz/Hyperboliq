using System;
using FluentAssertions;
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
        public static IEnumerable<SqlNode> StreamFrom(params SqlNode[] tokens)
        {
            return tokens;
        }

        public static SqlNode Kw(KeywordNode kw)
        {
            return SqlNode.NewKeyword(kw);
        }

        public static SqlNode Col<TTableType>(string columnDef)
        {
            return SqlNode.NewColumn(new Tuple<string, ITableReference>(columnDef, TableReferenceFromType<TTableType>()));
        }

        public static SqlNode Col(ITableReference r, string columnDef)
        {
            return SqlNode.NewColumn(new Tuple<string, ITableReference>(columnDef, r));
        }

        public static SqlNode Aggregate(AggregateType type, params SqlNode[] parameters)
        {
            return SqlNode.NewAggregate(new Tuple<AggregateType, FSharpList<SqlNode>>(type, ListModule.OfArray(parameters)));
        }

        public static SqlNode Tbl<TTableType>()
        {
            return SqlNode.NewTable(TableToken.NewTableToken(TableReferenceFromType<TTableType>()));
        }

        public static SqlNode Tbl(ITableReference r) {
            return SqlNode.NewTable(TableToken.NewTableToken(r));
        }

        public static SqlNode BinExp(SqlNode lhs, BinaryOperation op, SqlNode rhs)
        {
            return SqlNode.NewBinaryExpression(
                new BinaryExpressionNode(
                    new FSharpList<SqlNode>(lhs, FSharpList<SqlNode>.Empty),
                    op,
                    new FSharpList<SqlNode>(rhs, FSharpList<SqlNode>.Empty)));
        }

        public static SqlNode BinExp(IEnumerable<SqlNode> lhs, BinaryOperation op, IEnumerable<SqlNode> rhs)
        {
            return SqlNode.NewBinaryExpression(
                new BinaryExpressionNode(
                    ListModule.OfArray(lhs.ToArray()),
                    op,
                    ListModule.OfArray(rhs.ToArray())));
        }

        public static SqlNode Param(string paramName)
        {
            return SqlNode.NewParameter(ParameterToken.NewParameterToken(paramName));
        }

        public static SqlNode Const(object constant)
        {
            return SqlNode.NewConstant(ConstantNode.NewConstantNode(constant.ToString()));
        }

        public static SqlNode Null()
        {
            return SqlNode.NullValue;
        }

        public static InsertStatementHeadToken InsHead<TTableType>(params string[] colNames)
        {
            return 
                new InsertStatementHeadToken(
                    TableReferenceFromType<TTableType>(),
                    ListModule.OfSeq(
                        colNames.Select(n => new Tuple<string, ITableReference>(n, TableReferenceFromType<TTableType>()))));
        }

        private static UpdateSetToken CreateUpdateSetToken<TTableType>(string colDef, FSharpList<SqlNode> stream)
        {
            return new UpdateSetToken(new Tuple<string, ITableReference>(colDef, TableReferenceFromType<TTableType>()), stream);
        }

        public static UpdateSetToken Ust<TTableType>(string colDef, object c)
        {
            return CreateUpdateSetToken<TTableType>(colDef, new FSharpList<SqlNode>(Const(c), FSharpList<SqlNode>.Empty));
        }

        public static UpdateSetToken Ust<TTableType>(string colDef, SqlNode node)
        {
            return CreateUpdateSetToken<TTableType>(colDef, new FSharpList<SqlNode>(node, FSharpList<SqlNode>.Empty));
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

        public static SqlNode SubExp(
            SelectExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return SqlNode.NewSubExpression(
                new SelectExpression(
                    select,
                    from,
                    OptionModule.OfObj(where),
                    OptionModule.OfObj(groupBy),
                    OptionModule.OfObj(orderBy)));
        }

        public static OrderByClauseNode OrderClause(SqlNode col, Direction direction, NullsOrdering nullsOrdering = null)
        {
            return new OrderByClauseNode(
                direction,
                nullsOrdering ?? NullsOrdering.NullsUndefined,
                new FSharpList<SqlNode>(col, FSharpList<SqlNode>.Empty));
        }

        public static OrderByExpressionNode OrderBy(params OrderByClauseNode[] clauses)
        {
            return new OrderByExpressionNode(ListModule.OfArray(clauses));
        }

        public static SqlNode Ord(SqlNode col, Direction direction, NullsOrdering nullsOrdering = null)
        {
            var no = nullsOrdering == null ? NullsOrdering.NullsUndefined : nullsOrdering;
            return SqlNode.NewOrderingToken(new Ordering(new FSharpList<SqlNode>(col, FSharpList<SqlNode>.Empty), direction, no));
        }

        public static SelectExpressionNode Select(params SqlNode[] columns)
        {
            return new SelectExpressionNode(false, ListModule.OfArray(columns));
        }

        public static SelectExpressionNode SelectDistinct(params SqlNode[] columns)
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

        public static JoinClauseNode Join<TSource, TTarget>(JoinType type, SqlNode joinExpr)
        {
            return new JoinClauseNode(
                new FSharpList<ITableReference>(TableReferenceFromType<TSource>(), FSharpList<ITableReference>.Empty),
                TableReferenceFromType<TTarget>(),
                type,
                new FSharpList<SqlNode>(joinExpr, FSharpList<SqlNode>.Empty));
        }
        public static JoinClauseNode Join<TSource1, TSource2, TTarget>(JoinType type, SqlNode joinExpr)
        {
            return new JoinClauseNode(
                ListModule.OfArray(new ITableReference[] { TableReferenceFromType<TSource1>(), TableReferenceFromType<TSource2>() }),
                TableReferenceFromType<TTarget>(),
                type,
                new FSharpList<SqlNode>(joinExpr, FSharpList<SqlNode>.Empty));
        }

        public static JoinClauseNode Join(ITableReference source1, ITableReference source2, ITableReference target, JoinType type, SqlNode joinExpr)
        {
            return new JoinClauseNode(
                ListModule.OfArray(new[] { source1, source2 }),
                target,
                type,
                new FSharpList<SqlNode>(joinExpr, FSharpList<SqlNode>.Empty));
        }

        public static JoinClauseNode Join(ITableReference source, ITableReference target, JoinType type, SqlNode joinExpr)
        {
            return new JoinClauseNode(
                new FSharpList<ITableReference>(source, FSharpList<ITableReference>.Empty),
                target,
                type,
                new FSharpList<SqlNode>(joinExpr, FSharpList<SqlNode>.Empty));
        }

        public static WhereExpressionNode Where(SqlNode start, params WhereClauseNode[] additionalClauses)
        {
            return
                new WhereExpressionNode(new FSharpList<SqlNode>(start, FSharpList<SqlNode>.Empty), ListModule.OfArray(additionalClauses));
        }

        public static WhereClauseNode And(SqlNode stream)
        {
            return new WhereClauseNode(ExpressionCombinatorType.And, new FSharpList<SqlNode>(stream, FSharpList<SqlNode>.Empty));
        }

        public static WhereClauseNode Or(SqlNode stream)
        {
            return new WhereClauseNode(ExpressionCombinatorType.Or, new FSharpList<SqlNode>(stream, FSharpList<SqlNode>.Empty));
        }

        public static GroupByExpressionNode GroupBy(params SqlNode[] clauses)
        {
            return new GroupByExpressionNode(ListModule.OfArray(clauses), FSharpList<WhereClauseNode>.Empty);
        }

        public static GroupByExpressionNode GroupBy(IEnumerable<SqlNode> clauses, params WhereClauseNode[] having)
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

        public static void ShouldEqual(this IEnumerable<SqlNode> self, IEnumerable<SqlNode> expected)
        {
            self.Should().HaveSameCount(expected);
            var selfList = self.ToList();
            for (int i = 0; i < self.Count(); ++i)
            {
                var source = self.ElementAt(i);
                var target = expected.ElementAt(i);
                source.Should().Be(target);
            }
        }
    }
}
