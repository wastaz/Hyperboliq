namespace Hyperboliq.Domain

module Stream =
    open Types

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

    type AggregateType = Max | Min | Avg | Count

    type ExpressionCombinatorType = And | Or

    type JoinClauseNode = { 
        SourceTables: ITableReference list
        TargetTable: ITableReference
        Type: JoinType
        Condition: ValueNode option
    }

    and FromExpressionNode = {
        Tables : ITableReference list
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
        | Parameter of ParameterToken
        | Aggregate of AggregateToken
        | SubExpression of SelectExpression
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
    
    and SelectExpression =
        {
            Select : SelectExpressionNode
            From : FromExpressionNode
            Where : WhereExpressionNode option
            GroupBy : GroupByExpressionNode option
            OrderBy : OrderByExpressionNode option
        }

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

