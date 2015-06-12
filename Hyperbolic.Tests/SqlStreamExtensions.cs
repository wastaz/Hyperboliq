using System;
using System.Collections.Generic;
using System.Linq;
using Hyperboliq.Domain;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

#region Stupid usings since using static isn't available until C#6

using ValueNode = Hyperboliq.Domain.Stream.ValueNode;
using ITableReference = Hyperboliq.Domain.Types.ITableReference;
using AggregateType = Hyperboliq.Domain.Stream.AggregateType;
using Direction = Hyperboliq.Domain.Stream.Direction;
using NullsOrdering = Hyperboliq.Domain.Stream.NullsOrdering;
using JoinType = Hyperboliq.Domain.Stream.JoinType;
using BinaryOperation = Hyperboliq.Domain.Types.BinaryOperation;
using ExpressionCombinatorType = Hyperboliq.Domain.Stream.ExpressionCombinatorType;
using BinaryExpressionNode = Hyperboliq.Domain.Stream.BinaryExpressionNode;
using ParameterToken = Hyperboliq.Domain.Stream.ParameterToken;
using ConstantNode = Hyperboliq.Domain.Stream.ConstantNode;
using InsertStatementHeadToken = Hyperboliq.Domain.Stream.InsertStatementHeadToken;
using UpdateSetToken = Hyperboliq.Domain.Stream.UpdateSetToken;
using UpdateStatementHeadToken = Hyperboliq.Domain.Stream.UpdateStatementHeadToken;
using InsertValueToken = Hyperboliq.Domain.Stream.InsertValueToken;
using InsertValueNode = Hyperboliq.Domain.Stream.InsertValueNode;
using SqlExpression = Hyperboliq.Domain.Stream.SqlExpression;
using SelectExpression = Hyperboliq.Domain.Stream.SelectExpression;
using UpdateExpression = Hyperboliq.Domain.Stream.UpdateExpression;
using InsertExpression = Hyperboliq.Domain.Stream.InsertExpression;
using DeleteExpression = Hyperboliq.Domain.Stream.DeleteExpression;
using CommonTableExpression = Hyperboliq.Domain.Stream.CommonTableExpression;
using SelectExpressionNode = Hyperboliq.Domain.Stream.SelectExpressionNode;
using FromExpressionNode = Hyperboliq.Domain.Stream.FromExpressionNode;
using WhereExpressionNode = Hyperboliq.Domain.Stream.WhereExpressionNode;
using GroupByExpressionNode = Hyperboliq.Domain.Stream.GroupByExpressionNode;
using OrderByExpressionNode = Hyperboliq.Domain.Stream.OrderByExpressionNode;
using OrderByClauseNode = Hyperboliq.Domain.Stream.OrderByClauseNode;
using JoinClauseNode = Hyperboliq.Domain.Stream.JoinClauseNode;
using WhereClauseNode = Hyperboliq.Domain.Stream.WhereClauseNode;

#endregion

namespace Hyperboliq.Tests
{
    public static class ObjectExtension
    {
        public static FSharpOption<T> ToOption<T>(this T obj)
        {
            if (obj == null)
            {
                return FSharpOption<T>.None;
            }

            return new FSharpOption<T>(obj);
        }
    }

    public static class SqlStreamExtensions
    {
        public static ValueNode Col<TTableType>(string columnDef)
        {
            return ValueNode.NewColumn(new Tuple<string, ITableReference>(columnDef, Types.TableReferenceFromType<TTableType>()));
        }

        public static ValueNode Col(ITableReference r, string columnDef)
        {
            return ValueNode.NewColumn(new Tuple<string, ITableReference>(columnDef, r));
        }

        public static ValueNode WinCol(
            AggregateType type, 
            ValueNode aggregateParameter, 
            IEnumerable<ValueNode> partitionBy = null, 
            IEnumerable<OrderByClauseNode> orderBy = null)
        {
            return ValueNode.NewWindowedColumn(
                new Tuple<Tuple<AggregateType, ValueNode>, Stream.WindowNode>(
                    new Tuple<AggregateType, ValueNode>(type, aggregateParameter),
                    new Stream.WindowNode(
                        partitionBy == null ? ListModule.Empty<ValueNode>() : ListModule.OfSeq(partitionBy),
                        orderBy == null ? ListModule.Empty<OrderByClauseNode>() : ListModule.OfSeq(orderBy))));
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
                    Types.TableReferenceFromType<TTableType>(),
                    ListModule.OfSeq(
                        colNames.Select(n => new Tuple<string, ITableReference>(n, Types.TableReferenceFromType<TTableType>()))));
        }

        private static UpdateSetToken CreateUpdateSetToken<TTableType>(string colDef, ValueNode stream)
        {
            return new UpdateSetToken(new Tuple<string, ITableReference>(colDef, Types.TableReferenceFromType<TTableType>()), stream);
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
                    Types.TableReferenceFromType<TTableType>(),
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
            return InsertValueNode.NewColumn(new Tuple<string, ITableReference>(columnDef, Types.TableReferenceFromType<TTableType>()));
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
                    FSharpOption<CommonTableExpression>.None,
                    select,
                    from,
                    where.ToOption(),
                    groupBy.ToOption(),
                    orderBy.ToOption()));
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
                    new FSharpList<ITableReference>(Types.TableReferenceFromType<TTable>(), FSharpList<ITableReference>.Empty),
                    ListModule.OfArray(joins));
        }

        public static JoinClauseNode Join<TSource, TTarget>(JoinType type, ValueNode joinExpr)
        {
            return new JoinClauseNode(
                new FSharpList<ITableReference>(Types.TableReferenceFromType<TSource>(), FSharpList<ITableReference>.Empty),
                Types.TableReferenceFromType<TTarget>(),
                type,
                joinExpr.ToOption());
        }
        public static JoinClauseNode Join<TSource1, TSource2, TTarget>(JoinType type, ValueNode joinExpr)
        {
            return new JoinClauseNode(
                ListModule.OfArray(new ITableReference[] { Types.TableReferenceFromType<TSource1>(), Types.TableReferenceFromType<TSource2>() }),
                Types.TableReferenceFromType<TTarget>(),
                type,
                joinExpr.ToOption());
        }

        public static JoinClauseNode Join(ITableReference source1, ITableReference source2, ITableReference target, JoinType type, ValueNode joinExpr)
        {
            return new JoinClauseNode(
                ListModule.OfArray(new[] { source1, source2 }),
                target,
                type,
                joinExpr.ToOption());
        }

        public static JoinClauseNode Join(ITableReference source, ITableReference target, JoinType type, ValueNode joinExpr)
        {
            return new JoinClauseNode(
                new FSharpList<ITableReference>(source, FSharpList<ITableReference>.Empty),
                target,
                type,
                joinExpr.ToOption());
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
                    FSharpOption<CommonTableExpression>.None,
                    select,
                    from,
                    where.ToOption(),
                    groupBy.ToOption(),
                    orderBy.ToOption()));
        }

        public static SqlExpression SelectNode(
            CommonTableExpression with,
            SelectExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return SqlExpression.NewSelect(
                new SelectExpression(
                    with.ToOption(),
                    select,
                    from,
                    where.ToOption(),
                    groupBy.ToOption(),
                    orderBy.ToOption()));
        }

        public static SqlExpression DeleteNode(FromExpressionNode from, WhereExpressionNode where = null)
        {
            return SqlExpression.NewDelete(
                new DeleteExpression(from, where.ToOption()));
        }

        public static SqlExpression UpdateNode(UpdateStatementHeadToken head, WhereExpressionNode where = null)
        {
            return SqlExpression.NewUpdate(
                new UpdateExpression(
                    head,
                    where.ToOption()));
        }

        public static SqlExpression InsertNode(InsertStatementHeadToken head, params InsertValueToken[] values)
        {
            return SqlExpression.NewInsert(
                new InsertExpression(
                    head,
                    ListModule.OfArray(values)));
        }

        public static Stream.ICommonTableDefinition TableDef<TType>(Types.ISqlQuery)
        {
            return 
                new Stream.CommonTableDefinition<TType>(
                    SqlExpression.NewSelect(null)
                    );
                new SelectExpression(
                    FSharpOption<CommonTableExpression>.None,
                    select,
                    from,
                    where.ToOption(),
                    groupBy.ToOption(),
                    orderBy.ToOption()));
        }

        public static CommonTableExpression With(params Stream.ICommonTableDefinition[] definitions)
        {
            return new CommonTableExpression(ListModule.OfArray(definitions));
        }
    }
}
