using System;
using System.Collections.Generic;
using System.Linq;
using Hyperboliq.Domain;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

#region Stupid usings since using static isn't available until C#6

using ValueNode = Hyperboliq.Domain.Stream.ValueNode;
using AggregateType = Hyperboliq.Domain.Stream.AggregateType;
using AliasedColumnNode = Hyperboliq.Domain.Stream.AliasedColumnNode;
using Direction = Hyperboliq.Domain.Stream.Direction;
using FunctionType = Hyperboliq.Domain.Stream.FunctionType;
using NullsOrdering = Hyperboliq.Domain.Stream.NullsOrdering;
using JoinType = Hyperboliq.Domain.Stream.JoinType;
using BinaryOperation = Hyperboliq.Domain.Stream.BinaryOperation;
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
using PlainSelectExpression = Hyperboliq.Domain.Stream.PlainSelectExpression;
using UpdateExpression = Hyperboliq.Domain.Stream.UpdateExpression;
using InsertExpression = Hyperboliq.Domain.Stream.InsertExpression;
using DeleteExpression = Hyperboliq.Domain.Stream.DeleteExpression;
using CommonTableExpression = Hyperboliq.Domain.Stream.CommonTableExpression;
using SelectValuesExpressionNode = Hyperboliq.Domain.Stream.SelectValuesExpressionNode;
using SelectExpressionToken = Hyperboliq.Domain.Stream.SelectExpressionToken;
using FromExpressionNode = Hyperboliq.Domain.Stream.FromExpressionNode;
using WhereExpressionNode = Hyperboliq.Domain.Stream.WhereExpressionNode;
using GroupByExpressionNode = Hyperboliq.Domain.Stream.GroupByExpressionNode;
using OrderByExpressionNode = Hyperboliq.Domain.Stream.OrderByExpressionNode;
using OrderByClauseNode = Hyperboliq.Domain.Stream.OrderByClauseNode;
using JoinClauseNode = Hyperboliq.Domain.Stream.JoinClauseNode;
using WhereClauseNode = Hyperboliq.Domain.Stream.WhereClauseNode;
using SetSelectExpression = Hyperboliq.Domain.Stream.SetSelectExpression;

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
        private static Type FigureOutTypeFor(string name, ITableReference tref)
        {
            return tref.Table.GetProperties().First(p => p.Name == name).PropertyType;
        }

        public static ValueNode Col<TTableType>(string columnDef)
        {
            var tref = Types.TableReferenceFromType<TTableType>();
            return ValueNode.NewColumn(new Tuple<string, Type, ITableReference>(columnDef, FigureOutTypeFor(columnDef, tref), tref));
        }

        public static ValueNode Col<TTable>(ITableIdentifier<TTable> r, string columnDef)
        {
            return ValueNode.NewColumn(new Tuple<string, Type, ITableReference>(columnDef, FigureOutTypeFor(columnDef, r.Reference), r.Reference));
        }

        public static ValueNode Col<TTable>(TableReferenceCreator<TTable> c, string colDef)
        {
            var tref = c.ToTableReference().Reference;
            return ValueNode.NewColumn(new Tuple<string, Type, ITableReference>(colDef, FigureOutTypeFor(colDef, tref), tref));
        }

        public static ValueNode Col(ITableReference r, string colDef)
        {
            return ValueNode.NewColumn(new Tuple<string, Type, ITableReference>(colDef, FigureOutTypeFor(colDef, r), r));
        }

        public static ValueNode Star<TTable>()
        {
            return ValueNode.NewStarColumn(Stream.StarColumnToken.NewStarColumnToken(Types.TableReferenceFromType<TTable>()));
        }

        public static ValueNode AliasedCol<TTable>(string colDef, string alias) {
            var tref = Types.TableReferenceFromType<TTable>();
            return ValueNode.NewNamedColumn(
                new AliasedColumnNode(
                    ValueNode.NewColumn(
                        new Tuple<string, Type, ITableReference>(colDef, FigureOutTypeFor(colDef, tref), tref)), 
                    alias));
        }

        public static ValueNode AliasedCol<TTable>(AggregateType type, ValueNode parameter, string alias)
        {
            return ValueNode.NewNamedColumn(
                new AliasedColumnNode(
                    ValueNode.NewAggregate(new Tuple<AggregateType, ValueNode>(type, parameter)),
                    alias));
        }

        public static ValueNode AliasedCol(
            string alias,
            AggregateType type,
            ValueNode aggregateParameter,
            IEnumerable<ValueNode> partitionBy = null,
            IEnumerable<OrderByClauseNode> orderBy = null)
        {
            return ValueNode.NewNamedColumn(
                new AliasedColumnNode(
                    WinCol(type, aggregateParameter, partitionBy, orderBy),
                    alias));
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

        public static ValueNode Func(FunctionType type, params ValueNode[] arguments)
        {
            return ValueNode.NewFunctionCall(
                new Tuple<FunctionType, FSharpList<ValueNode>>(type, ListModule.OfArray(arguments)));
        }

        public static InsertStatementHeadToken InsHead<TTableType>(params string[] colNames)
        {
            return 
                new InsertStatementHeadToken(
                    Types.TableReferenceFromType<TTableType>(),
                    ListModule.OfSeq(
                        colNames.Select(n => {
                            var tref = Types.TableReferenceFromType<TTableType>();
                            return new Tuple<string, Type, ITableReference>(n, FigureOutTypeFor(n, tref), tref); 
                        })));
        }

        private static UpdateSetToken CreateUpdateSetToken<TTableType>(string colDef, ValueNode stream)
        {
            var tref = Types.TableReferenceFromType<TTableType>();
            return new UpdateSetToken(new Tuple<string, Type, ITableReference>(colDef, FigureOutTypeFor(colDef, tref), tref), stream);
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
            var tref = Types.TableReferenceFromType<TTableType>();
            return InsertValueNode.NewColumn(new Tuple<string, Type, ITableReference>(columnDef, FigureOutTypeFor(columnDef, tref), tref));
        }

        public static ValueNode SubExp(
            SelectValuesExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return ValueNode.NewSubExpression(
                PlainSelectExpression.NewPlain(
                    new SelectExpressionToken(
                        select,
                        from,
                        where.ToOption(),
                        groupBy.ToOption(),
                        orderBy.ToOption())));
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
        
        public static SelectValuesExpressionNode Select(params ValueNode[] columns)
        {
            return new SelectValuesExpressionNode(false, ListModule.OfArray(columns));
        }

        public static SelectValuesExpressionNode SelectDistinct(params ValueNode[] columns)
        {
            return new SelectValuesExpressionNode(true, ListModule.OfArray(columns));
        }

        public static FromExpressionNode From<TType>(ITableIdentifier<TType> table)
        {
            return new FromExpressionNode(
                new FSharpList<ITableIdentifier>(table, FSharpList<ITableIdentifier>.Empty),
                FSharpList<JoinClauseNode>.Empty);
        }

        public static FromExpressionNode From<TType>(ITableIdentifier<TType> c, params JoinClauseNode[] joins)
        {
            return new FromExpressionNode(
                new FSharpList<ITableIdentifier>(c, FSharpList<ITableIdentifier>.Empty),
                ListModule.OfArray(joins));
        }
        
        public static FromExpressionNode From<TTable>(params JoinClauseNode[] joins)
        {
            return
                new FromExpressionNode(
                    new FSharpList<ITableIdentifier>(new TableIdentifier<TTable>(), FSharpList<ITableIdentifier>.Empty),
                    ListModule.OfArray(joins));
        }

        public static JoinClauseNode Join<TSource, TTarget>(
            ITableIdentifier<TSource> source, 
            ITableIdentifier<TTarget> target,
            JoinType type,
            ValueNode joinExpr)
        {
            return new JoinClauseNode(
                new FSharpList<ITableIdentifier>(source, FSharpList<ITableIdentifier>.Empty),
                target,
                type,
                joinExpr.ToOption());
        }

        public static JoinClauseNode Join<TSource, TTarget>(JoinType type, ValueNode joinExpr)
        {
            return new JoinClauseNode(
                new FSharpList<ITableIdentifier>(new TableIdentifier<TSource>(), FSharpList<ITableIdentifier>.Empty),
                new TableIdentifier<TTarget>(),
                type,
                joinExpr.ToOption());
        }


        public static JoinClauseNode Join<TSource1, TSource2, TTarget>(JoinType type, ValueNode joinExpr)
        {
            return new JoinClauseNode(
                ListModule.OfArray(new ITableIdentifier[] {
                    new TableIdentifier<TSource1>(),
                    new TableIdentifier<TSource2>()
                }),
                new TableIdentifier<TTarget>(),
                type,
                joinExpr.ToOption());
        }

        public static JoinClauseNode Join<TSource1, TSource2, TTarget>(
            ITableIdentifier<TSource1> source1, 
            ITableIdentifier<TSource2> source2, 
            ITableIdentifier<TTarget> target, 
            JoinType type, 
            ValueNode joinExpr)
        {
            return new JoinClauseNode(
                ListModule.OfArray(new ITableIdentifier[] { source1, source2 }),
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
            CommonTableExpression with,
            SetSelectExpression select)
        {
            return
                SqlExpression.NewSelect(
                    SelectExpression.NewComplex(
                        new Tuple<CommonTableExpression, PlainSelectExpression>(
                            with,
                            PlainSelectExpression.NewSet(select))));
        }

        public static SqlExpression SelectNode(
            SelectValuesExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return SqlExpression.NewSelect(
                SelectExpression.NewPlain(
                    PlainSelectExpression.NewPlain(
                        PlainSelect(select, from, where, groupBy, orderBy))));
        }

        public static SqlExpression SelectNode(
            CommonTableExpression with,
            SelectValuesExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return SqlExpression.NewSelect(
                SelectExpression.NewComplex(
                    new Tuple<CommonTableExpression, PlainSelectExpression>(
                        with, 
                        PlainSelectExpression.NewPlain(PlainSelect(select, from, where, groupBy, orderBy)))));
        }


        public static SelectExpressionToken PlainSelect(
            SelectValuesExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return
                new SelectExpressionToken(
                    select,
                    from,
                    where.ToOption(),
                    groupBy.ToOption(),
                    orderBy.ToOption());
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

        public static Stream.ICommonTableDefinition TableDef<TType>(
            TableReferenceCreator<TType> c,
            SetSelectExpression expr)
        {
            return
                new Stream.CommonTableDefinition<TType>(PlainSelectExpression.NewSet(expr), c);
        }

        public static Stream.ICommonTableDefinition TableDef<TType>(
            TableReferenceCreator<TType> c,
            SelectValuesExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return TableDef(c.ToTableReference(), select, from, where, groupBy, orderBy);
        }

        public static Stream.ICommonTableDefinition TableDef<TType>(
            TableIdentifier<TType> tdef,
            SelectValuesExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return
                new Stream.CommonTableDefinition<TType>(
                    PlainSelectExpression.NewPlain(
                        new SelectExpressionToken(select, from, where.ToOption(), groupBy.ToOption(), orderBy.ToOption())),
                    tdef);
        }

        public static Stream.ICommonTableDefinition TableDef<TType>(
            SelectValuesExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return TableDef(new TableIdentifier<TType>(), select, from, where, groupBy, orderBy);
        }

        public static CommonTableExpression With(params Stream.ICommonTableDefinition[] definitions)
        {
            return new CommonTableExpression(ListModule.OfArray(definitions));
        }

        public static SetSelectExpression Union(params SelectExpressionToken[] expr)
        {
            return new SetSelectExpression(
                Stream.SetOperationType.Union, 
                ListModule.OfSeq(expr.Select(e => PlainSelectExpression.NewPlain(e))));
        }
    }
}
