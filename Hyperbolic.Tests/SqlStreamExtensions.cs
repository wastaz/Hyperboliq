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

        public static SqlNode InsHead<TTableType>(params string[] colNames)
        {
            return SqlNode.NewInsertHead(
                    new InsertStatementHeadToken(
                        TableToken.NewTableToken(TableReferenceFromType<TTableType>()),
                        ListModule.OfSeq(
                            colNames.Select(n => new Tuple<string, ITableReference>(n, TableReferenceFromType<TTableType>())))));
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

        public static SqlNode UpdHead<TTableType>(params UpdateSetToken[] setExprs)
        {
            return SqlNode.NewUpdateStatementHead(
                new UpdateStatementHeadToken(
                    TableReferenceFromType<TTableType>(),
                    ListModule.OfArray(setExprs)));
        }

        public static SqlNode InsVal(params InsertValueNode[] nodes)
        {
            return SqlNode.NewInsertValue(ListModule.OfArray(nodes));
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

        public static SqlNode SubExp(IEnumerable<SqlNode> stream)
        {
            return SqlNode.NewSubExpression(ListModule.OfSeq(stream));
        }

        public static SqlNode Ord(SqlNode col, Direction direction, NullsOrdering nullsOrdering = null)
        {

            var no = nullsOrdering == null ? NullsOrdering.NullsUndefined : nullsOrdering;
            return SqlNode.NewOrderingToken(new Ordering(new FSharpList<SqlNode>(col, FSharpList<SqlNode>.Empty), direction, no));
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
