namespace Hyperboliq.Domain

module Stream =
    open Hyperboliq

    type BinaryOperation =
        Equal | NotEqual | GreaterThan | GreaterThanOrEqual | LessThan | LessThanOrEqual
        | In | And | Or | Add | Subtract | Multiply | Divide | Modulo | Coalesce

    type ConstantNode = ConstantNode of string

    type ColumnToken = string * ITableReference

    type TableToken = TableToken of ITableReference

    type InsertStatementHeadToken = {
        Table : ITableReference
        Columns : ColumnToken list
    }

    type EscapeColumnTokenFn = ColumnToken -> string
    type EscapeTableTokenFn = TableToken -> string

    type DialectProvider = {
        EscapeColumn : EscapeColumnTokenFn;
        EscaleTable : EscapeTableTokenFn;
    }

    type ParameterToken = ParameterToken of string

    type Direction = Ascending | Descending
    type NullsOrdering = NullsUndefined | NullsFirst | NullsLast

    type JoinType = InnerJoin | LeftJoin | RightJoin | FullJoin

    type AggregateType = Max | Min | Avg | Count | Sum | RowNumber

    type ExpressionCombinatorType = And | Or

    type WindowedColumnNode = AggregateToken * WindowNode

    and WindowNode = {
        PartitionBy: ValueNode list
        OrderBy: OrderByClauseNode list
    }

    and AliasedColumnNode = {
        Column : ValueNode
        Alias : string
    }

    and JoinClauseNode = { 
        SourceTables: ITableIdentifier list
        TargetTable: ITableIdentifier
        Type: JoinType
        Condition: ValueNode option
    }

    and FromExpressionNode = {
        Tables : ITableIdentifier list
        Joins : JoinClauseNode list
    }

    and OrderByClauseNode = {
        Direction : Direction
        NullsOrdering : NullsOrdering
        Selector : ValueNode
    }

    and OrderByExpressionNode = {
        Clauses : OrderByClauseNode list
    }

    and SelectExpressionNode = {
        IsDistinct : bool
        Values : ValueNode list
    }

    and WhereClauseNode = {
        Combinator: ExpressionCombinatorType
        Expression: ValueNode
    }

    and GroupByExpressionNode = {
        Clauses : ValueNode list
        Having : WhereClauseNode list
    }

    and AggregateToken = AggregateType * ValueNode

    and BinaryExpressionNode = {
        Lhs : ValueNode
        Operation : BinaryOperation
        Rhs : ValueNode
    }

    and ValueNode =
        | NullValue
        | Constant of ConstantNode
        | Column of ColumnToken
        | WindowedColumn of WindowedColumnNode
        | NamedColumn of AliasedColumnNode
        | Parameter of ParameterToken
        | Aggregate of AggregateToken
        | SubExpression of PlainSelectExpression
        | BinaryExpression of BinaryExpressionNode
        | ValueList of ValueNode list

    and WhereExpressionNode = {
        Start: ValueNode
        AdditionalClauses: WhereClauseNode list
    }

    and UpdateStatementHeadToken = {
        Table : ITableReference
        SetExpressions : UpdateSetToken list
    }

    and UpdateSetToken = {
        Column : ColumnToken
        Value : ValueNode
    }
    
    and InsertValueNode =
        | NullValue
        | Constant of ConstantNode
        | Column of ColumnToken
        | Parameter of ParameterToken

    and InsertValueToken = { Values : InsertValueNode list }
    
    and PlainSelectExpression = 
        {
            Select : SelectExpressionNode
            From : FromExpressionNode
            Where : WhereExpressionNode option
            GroupBy : GroupByExpressionNode option
            OrderBy : OrderByExpressionNode option
        }
    and CommonTableValuedSelectExpression = CommonTableExpression * PlainSelectExpression

    and SelectExpression =
        | Plain of PlainSelectExpression
        | Complex of CommonTableValuedSelectExpression

    and ICommonTableDefinition =
        abstract Query : PlainSelectExpression with get
        abstract Table : ITableIdentifier with get

    and ICommonTableDefinition<'a> =
        abstract Query : PlainSelectExpression with get
        abstract Table : ITableIdentifier<'a> with get

    and CommonTableDefinition<'a> =
        { Query : PlainSelectExpression; TableDef : ITableIdentifier<'a> }
        interface ICommonTableDefinition<'a> with
            member x.Query with get() = x.Query
            member x.Table with get() = x.TableDef
        interface ICommonTableDefinition with
            member x.Query with get () = x.Query
            member x.Table with get() = x.TableDef :> ITableIdentifier

    and CommonTableExpression = { Definitions : ICommonTableDefinition list }

    type InsertExpression =
        {
            InsertInto : InsertStatementHeadToken
            InsertValues : InsertValueToken list
        }

    type DeleteExpression =
        {
            From : FromExpressionNode
            Where : WhereExpressionNode option
        }

    type UpdateExpression =
        {
            UpdateSet : UpdateStatementHeadToken
            Where : WhereExpressionNode option
        }

    type SqlExpression =
        | Select of SelectExpression
        | Insert of InsertExpression
        | Delete of DeleteExpression
        | Update of UpdateExpression

    type ISelectExpressionTransformable =
        abstract member ToSelectExpression : unit -> SelectExpression

    type ISqlExpressionTransformable =
        abstract member ToSqlExpression : unit -> SqlExpression

