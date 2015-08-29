using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

#region Stupid usings since using static isn't available until C#6

using ValueNode = Hyperboliq.Domain.AST.ValueNode;
using AggregateType = Hyperboliq.Domain.AST.AggregateType;
using AliasedColumnNode = Hyperboliq.Domain.AST.AliasedColumnNode;
using Direction = Hyperboliq.Domain.AST.Direction;
using FunctionType = Hyperboliq.Domain.AST.FunctionType;
using NullsOrdering = Hyperboliq.Domain.AST.NullsOrdering;
using JoinType = Hyperboliq.Domain.AST.JoinType;
using BinaryOperation = Hyperboliq.Domain.AST.BinaryOperation;
using ExpressionCombinatorType = Hyperboliq.Domain.AST.ExpressionCombinatorType;
using BinaryExpressionNode = Hyperboliq.Domain.AST.BinaryExpressionNode;
using InsertStatementHeadToken = Hyperboliq.Domain.AST.InsertStatementHeadToken;
using UpdateSetToken = Hyperboliq.Domain.AST.UpdateSetToken;
using UpdateStatementHeadToken = Hyperboliq.Domain.AST.UpdateStatementHeadToken;
using InsertValueToken = Hyperboliq.Domain.AST.InsertValueToken;
using InsertValueNode = Hyperboliq.Domain.AST.InsertValueNode;
using SqlExpression = Hyperboliq.Domain.AST.SqlExpression;
using SelectExpression = Hyperboliq.Domain.AST.SelectExpression;
using PlainSelectExpression = Hyperboliq.Domain.AST.PlainSelectExpression;
using UpdateExpression = Hyperboliq.Domain.AST.UpdateExpression;
using InsertExpression = Hyperboliq.Domain.AST.InsertExpression;
using DeleteExpression = Hyperboliq.Domain.AST.DeleteExpression;
using CommonTableExpression = Hyperboliq.Domain.AST.CommonTableExpression;
using SelectValuesExpressionNode = Hyperboliq.Domain.AST.SelectValuesExpressionNode;
using SelectExpressionToken = Hyperboliq.Domain.AST.SelectExpressionToken;
using FromExpressionNode = Hyperboliq.Domain.AST.FromExpressionNode;
using WhereExpressionNode = Hyperboliq.Domain.AST.WhereExpressionNode;
using GroupByExpressionNode = Hyperboliq.Domain.AST.GroupByExpressionNode;
using OrderByExpressionNode = Hyperboliq.Domain.AST.OrderByExpressionNode;
using OrderByClauseNode = Hyperboliq.Domain.AST.OrderByClauseNode;
using JoinClauseNode = Hyperboliq.Domain.AST.JoinClauseNode;
using WhereClauseNode = Hyperboliq.Domain.AST.WhereClauseNode;
using SetSelectExpression = Hyperboliq.Domain.AST.SetSelectExpression;
using Hyperboliq.Domain;

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
            return ValueNode.NewStarColumn(Types.TableReferenceFromType<TTable>());
        }

        public static ValueNode AliasedCol(ValueNode node, string alias)
        {
            return ValueNode.NewNamedColumn(new AliasedColumnNode(node, alias));
        }

        public static ValueNode AliasedCol<TTable>(string colDef, string alias) {
            var tref = Types.TableReferenceFromType<TTable>();
            return AliasedCol(ValueNode.NewColumn(new Tuple<string, Type, ITableReference>(colDef, FigureOutTypeFor(colDef, tref), tref)), alias);
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
                new Tuple<Tuple<AggregateType, ValueNode>, AST.WindowNode>(
                    new Tuple<AggregateType, ValueNode>(type, aggregateParameter),
                    new AST.WindowNode(
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
            return ValueNode.NewParameter(paramName);
        }

        public static ValueNode Const(object constant)
        {
            return ValueNode.NewConstant(constant.ToString());
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
            return InsertValueNode.NewConstant(constant.ToString());
        }

        public static InsertValueNode InsParam(string paramName)
        {
            return InsertValueNode.NewParameter(paramName);
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
            SelectExpressionToken select)
        {
            return SqlExpression.NewSelect(
                SelectExpression.NewComplex(
                    new Tuple<CommonTableExpression, PlainSelectExpression>(with, PlainSelectExpression.NewPlain(select))));
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

        public static AST.ICommonTableDefinition TableDef<TType>(SetSelectExpression expr)
        {
            return new AST.CommonTableDefinition<TType>(PlainSelectExpression.NewSet(expr), new TableIdentifier<TType>());
        }

        public static AST.ICommonTableDefinition TableDef<TType>(
            TableReferenceCreator<TType> c,
            SetSelectExpression expr)
        {
            return
                new AST.CommonTableDefinition<TType>(PlainSelectExpression.NewSet(expr), c);
        }

        public static AST.ICommonTableDefinition TableDef<TType>(
            TableReferenceCreator<TType> c,
            SelectValuesExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return TableDef(c.ToTableReference(), select, from, where, groupBy, orderBy);
        }

        public static AST.ICommonTableDefinition TableDef<TType>(
            TableIdentifier<TType> tdef,
            SelectValuesExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return
                new AST.CommonTableDefinition<TType>(
                    PlainSelectExpression.NewPlain(
                        new SelectExpressionToken(select, from, where.ToOption(), groupBy.ToOption(), orderBy.ToOption())),
                    tdef);
        }

        public static AST.ICommonTableDefinition TableDef<TType>(
            SelectValuesExpressionNode select,
            FromExpressionNode from,
            WhereExpressionNode where = null,
            GroupByExpressionNode groupBy = null,
            OrderByExpressionNode orderBy = null)
        {
            return TableDef(new TableIdentifier<TType>(), select, from, where, groupBy, orderBy);
        }

        public static CommonTableExpression With(params AST.ICommonTableDefinition[] definitions)
        {
            return new CommonTableExpression(ListModule.OfArray(definitions));
        }

        public static SetSelectExpression Union(params SelectExpressionToken[] expr)
        {
            return new SetSelectExpression(
                AST.SetOperationType.Union, 
                ListModule.OfSeq(expr.Select(e => PlainSelectExpression.NewPlain(e))));
        }

        public static SetSelectExpression UnionAll(params SelectExpressionToken[] expr)
        {
            return new SetSelectExpression(
                AST.SetOperationType.UnionAll,
                ListModule.OfSeq(expr.Select(e => PlainSelectExpression.NewPlain(e))));
        }

        public static SetSelectExpression Intersect(params SelectExpressionToken[] expr)
        {
            return new SetSelectExpression(
                AST.SetOperationType.Intersect,
                ListModule.OfSeq(expr.Select(e => PlainSelectExpression.NewPlain(e))));
        }

        public static SetSelectExpression Minus(params SelectExpressionToken[] expr)
        {
            return new SetSelectExpression(
                AST.SetOperationType.Minus,
                ListModule.OfSeq(expr.Select(e => PlainSelectExpression.NewPlain(e))));
        }

    }
}
