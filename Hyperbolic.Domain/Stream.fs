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
    type KeywordNode = 
        Select | From | Where | And | Or | On | Join of JoinType | OrderBy | GroupBy | Having | Distinct | Delete | InsertInto | Values | Update | Set

    type AggregateType = Max | Min | Avg | Count

    type ExpressionCombinatorType = And | Or

    type JoinClauseNode = { 
        SourceTables: ITableReference list
        TargetTable: ITableReference
        Type: JoinType
        Condition: SqlStream
    }

    and FromExpressionNode = {
        Tables : ITableReference list
        Joins : JoinClauseNode list
    }

    and OrderByClauseNode = {
        Direction : Direction
        NullsOrdering : NullsOrdering
        Selector : SqlStream
    }

    and OrderByExpressionNode = {
        Clauses : OrderByClauseNode list
    }

    and SelectExpressionNode = {
        IsDistinct : bool
        Values : SqlNode list
    }

    and WhereClauseNode = {
        Combinator: ExpressionCombinatorType
        Expression: SqlStream
    }

    and GroupByExpressionNode = {
        Clauses : SqlNode list
        Having : WhereClauseNode list
    }

    and Ordering = {
        Selector : SqlStream
        Direction : Direction
        NullsOrdering : NullsOrdering
    }

    and AggregateToken = AggregateType * SqlStream

    and BinaryExpressionNode = {
        Lhs: SqlStream
        Operation: BinaryOperation
        Rhs: SqlStream
    }

    and WhereExpressionNode = {
        Start: SqlStream
        AdditionalClauses: WhereClauseNode list
    }

    and UpdateStatementHeadToken = {
        Table : ITableReference
        SetExpressions : UpdateSetToken list
    }

    and UpdateSetToken = {
        Column : ColumnToken
        Value : SqlStream
    }

    and SqlNode =
        | NullValue
        | Keyword of KeywordNode
        | Constant of ConstantNode
        | Table of TableToken
        | Column of ColumnToken
        | Parameter of ParameterToken
        | BinaryExpression of BinaryExpressionNode
        | SubExpression of SelectExpression
        | Aggregate of AggregateToken
        | OrderingToken of Ordering
        | Select of SelectExpressionNode
        | From of FromExpressionNode
        | Where of WhereExpressionNode
        | GroupBy of GroupByExpressionNode
        | OrderBy of OrderByExpressionNode
        | InsertHead of InsertStatementHeadToken
        | InsertValue of InsertValueNode list
        | UpdateStatementHead of UpdateStatementHeadToken

    and InsertValueNode =
        | NullValue
        | Constant of ConstantNode
        | Column of ColumnToken
        | Parameter of ParameterToken

    and InsertValueToken = { Values : InsertValueNode list }

    and SqlStream = SqlNode list
    
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

    type Sql private () =
        static member SubExpr<'a> (e : ISqlQuery) = Unchecked.defaultof<'a>
        static member In<'a> (value : 'a) (e : ISqlQuery) = false
        static member Max<'a> (value : 'a) = Unchecked.defaultof<'a>
        static member Min<'a> (value : 'a) = Unchecked.defaultof<'a>
        static member Avg<'a> (value : 'a) = Unchecked.defaultof<'a>
        static member Count () = Unchecked.defaultof<int>
