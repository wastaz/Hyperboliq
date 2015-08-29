namespace Hyperboliq.Domain

#nowarn "40"

[<AutoOpen>]
module internal SqlGenUtils =
    open Hyperboliq
    open AST
    
    let ToPredecenceNumber op =
        match op with
        | BinaryOperation.Coalesce -> 0

        | BinaryOperation.Modulo
        | BinaryOperation.Multiply
        | BinaryOperation.Divide -> 2

        | BinaryOperation.Add
        | BinaryOperation.Subtract -> 3

        | BinaryOperation.Equal
        | BinaryOperation.NotEqual
        | BinaryOperation.GreaterThan
        | BinaryOperation.GreaterThanOrEqual
        | BinaryOperation.LessThan
        | BinaryOperation.LessThanOrEqual -> 4

        | BinaryOperation.And -> 6

        | BinaryOperation.In
        | BinaryOperation.Or -> 7
        
    let HasLowerPredecence op1 op2 = (ToPredecenceNumber op1) > (ToPredecenceNumber op2)
    
    let TranslateBinaryOperation op =
        match op with
        | Equal -> "="
        | GreaterThan -> ">"
        | GreaterThanOrEqual -> ">="
        | LessThan -> "<"
        | LessThanOrEqual -> "<="
        | In -> "IN"
        | BinaryOperation.Or -> "OR"
        | BinaryOperation.And -> "AND"
        | Add -> "+"
        | Subtract -> "-"
        | _ -> failwith "Not implemented"

    let TranslateCombinator combinator =
        match combinator with
        | ExpressionCombinatorType.And -> "AND"
        | ExpressionCombinatorType.Or -> "OR"
    
    let TranslateSetOperation op =
        match op with
        | SetOperationType.Union -> "UNION"
        | SetOperationType.UnionAll -> "UNION ALL"
        | SetOperationType.Intersect -> "INTERSECT"
        | SetOperationType.Minus -> "MINUS"

    let Join sep (l : System.String seq) =
        System.String.Join(sep, l)

    let JoinWithComma = Join ", "
    let JoinWithSpace = Join " "
    let JoinOptionsWithSpace = List.choose id >> JoinWithSpace

    let HandleTableRef (tref : ITableReference) = tref.ReferenceName
    let HandleTableDefinition (tdef : ITableDefinition) = tdef.Name
    let HandleTableDefinitionWithRef (ti : ITableIdentifier) = ti.Definition.Name + " " + ti.Reference.ReferenceName

    let HandleConditionalClauses (valueHandler : ValueNode -> string) (clauses : WhereClauseNode list) =
        match clauses with
        | [] -> ""
        | [ start ] -> valueHandler start.Expression
        | start :: rest ->
            let startVal = valueHandler start.Expression
            rest
            |> List.map (fun v -> sprintf "%s %s" (TranslateCombinator v.Combinator) (valueHandler v.Expression))
            |> JoinWithSpace
            |> (fun s -> [ startVal; s ] |> JoinWithSpace)


    let HandleConstant (c : ConstantNode) = c

    let HandleNullValue () = "NULL"

    let HandleParameter (name : ParameterToken) = sprintf "@%s" name

    let HandleColumn (includeTableRef : bool) (dialect : ISqlDialect) ((col, colType, tbl) : ColumnToken) =
        let cname = dialect.QuoteColumnName col
        if includeTableRef then
            sprintf "%s.%s" (HandleTableRef tbl) cname
        else
            cname

    let HandleStarColumn (includeTableRef : bool) (col : StarColumnToken) = 
        if includeTableRef then col.ReferenceName + ".*" else "*"
        
    type SubExpressionHandler = ISqlDialect -> PlainSelectExpression -> string

    let rec HandleValueNode (subExprHandler : SubExpressionHandler) (includeTableRef : bool) (dialect : ISqlDialect) (vn : ValueNode) =
        match vn with
        | ValueNode.NullValue -> HandleNullValue ()
        | ValueNode.Column(c) -> HandleColumn includeTableRef dialect c
        | ValueNode.StarColumn(sc) -> HandleStarColumn includeTableRef sc
        | ValueNode.NamedColumn(nc) -> HandleNamedColumn subExprHandler includeTableRef dialect nc
        | ValueNode.WindowedColumn(wc) -> HandleWindowedColumn subExprHandler includeTableRef dialect wc
        | ValueNode.FunctionCall(ft) -> HandleFunctionValue subExprHandler includeTableRef dialect ft
        | ValueNode.Constant(c) -> HandleConstant c
        | ValueNode.Parameter(p) -> HandleParameter p
        | ValueNode.Aggregate(a) -> HandleAggregate subExprHandler includeTableRef dialect a
        | ValueNode.BinaryExpression(be) -> HandleBinaryExpression subExprHandler includeTableRef dialect be
        | ValueNode.SubExpression(se) -> subExprHandler dialect se |> sprintf "(%s)"
        | _ -> failwith "Not supported"
    
    and HandleAggregate (subExprHandler : SubExpressionHandler) (includeTableRef : bool) (dialect : ISqlDialect) ((aggregateType, payload) : AggregateToken) =
        let value = HandleValueNode subExprHandler includeTableRef dialect payload
        match aggregateType with
        | Count -> "COUNT(*)"
        | RowNumber -> "ROW_NUMBER()"
        | Avg -> sprintf "AVG(%s)" value
        | Min -> sprintf "MIN(%s)" value
        | Max -> sprintf "MAX(%s)" value
        | Sum -> sprintf "SUM(%s)" value

    and HandleNamedColumn (subExprHandler : SubExpressionHandler) (includeTableRef : bool) (dialect : ISqlDialect) (nc : AliasedColumnNode) =
        HandleValueNode subExprHandler includeTableRef dialect nc.Column
        |> (fun s -> sprintf "%s AS %s" s nc.Alias)

    and HandleFunctionValue (subExprHandler : SubExpressionHandler) (includeTableRef : bool) (dialect : ISqlDialect) ((fType, args) : FunctionToken) =
        let GetFunctionCall funType =
            match funType with
            | FunctionType.Upper -> "UPPER"
            | FunctionType.Lower -> "LOWER"
            | FunctionType.Concat -> "CONCAT"
        args
        |> List.map (HandleValueNode subExprHandler includeTableRef dialect)
        |> JoinWithComma
        |> sprintf "%s(%s)" (GetFunctionCall fType)

    and HandleWindowedColumn (subExprHandler : SubExpressionHandler) (includeTableRef : bool)  (dialect : ISqlDialect) ((aggregateToken, windowToken) : WindowedColumnNode) =
        let HandlePartitionBy dialect partitionNodes =
            let parts = 
                partitionNodes
                |> List.map (fun n -> match n with 
                                      | ValueNode.Column(c) -> HandleColumn includeTableRef dialect c |> Some
                                      | ValueNode.Aggregate(a) -> HandleAggregate subExprHandler includeTableRef dialect a |> Some
                                      | _ -> None)
                |> List.choose id
            match parts with 
            | [] -> None
            | _ -> parts |> JoinWithComma |> sprintf "PARTITION BY %s" |> Some

        let InternalHandleOrderBy dialect orderByNodes =
            match orderByNodes with
            | [] -> None
            | _ -> orderByNodes |> List.map (HandleOrderByClause subExprHandler includeTableRef dialect) |> JoinWithComma |> sprintf "ORDER BY %s" |> Some

        let HandleWindowToken dialect (wt : WindowNode) =
            let partition = HandlePartitionBy dialect wt.PartitionBy
            let order = InternalHandleOrderBy dialect wt.OrderBy
            [ partition; order ] |> JoinOptionsWithSpace |> sprintf "(%s)"

        [ 
            HandleAggregate subExprHandler includeTableRef dialect aggregateToken
            "OVER"
            HandleWindowToken dialect windowToken
        ] |> JoinWithSpace

    and HandleBinaryExpression (subExprHandler : SubExpressionHandler) (includeTableRef : bool) (dialect : ISqlDialect) (exp : BinaryExpressionNode) =
        let AddParensIfNecessary innerExp sql =
            match innerExp with
            | ValueNode.BinaryExpression(n) when HasLowerPredecence n.Operation exp.Operation -> sprintf "(%s)" sql 
            | _ -> sql

        let HandleNull dialect op exp =
            let compareVal = HandleValueNode subExprHandler includeTableRef dialect exp
            match op with
            | Equal -> sprintf "%s IS NULL" compareVal
            | NotEqual -> sprintf "%s IS NOT NULL" compareVal
            | _ -> failwith "Not supported"

        match exp.Lhs, exp.Rhs with
        | ValueNode.NullValue, _ -> HandleNull dialect exp.Operation exp.Rhs
        | _, ValueNode.NullValue -> HandleNull dialect exp.Operation exp.Lhs
        | _, _ ->
            let op = TranslateBinaryOperation exp.Operation
            let lhs = HandleValueNode subExprHandler includeTableRef dialect exp.Lhs |> AddParensIfNecessary exp.Lhs
            let rhs = HandleValueNode subExprHandler includeTableRef dialect exp.Rhs |> AddParensIfNecessary exp.Rhs
            sprintf "%s %s %s" lhs op rhs

    and HandleOrderByClause (subExprHandler : SubExpressionHandler) (includeTableRef : bool) (dialect : ISqlDialect) (clause : OrderByClauseNode) =
        let dir = match clause.Direction with
                    | Ascending -> "ASC"
                    | Descending -> "DESC"
        let nulls = match clause.NullsOrdering with
                    | NullsFirst -> Some("NULLS FIRST")
                    | NullsLast -> Some("NULLS LAST")
                    | NullsUndefined -> None
        let selector = HandleValueNode subExprHandler includeTableRef dialect clause.Selector
        [ Some(selector); Some(dir); nulls ]
        |> JoinOptionsWithSpace

    type BinaryExpressionHandler = ISqlDialect -> BinaryExpressionNode -> string
    
    let HandleWhere (binExpHandler : BinaryExpressionHandler) (dialect : ISqlDialect) (where : WhereExpressionNode option) =
        let HandleValue (dialect : ISqlDialect) (value : ValueNode) =
            match value with
            | ValueNode.BinaryExpression(n) -> binExpHandler dialect n
            | _ -> failwith "Not supported"

        let HandleWhereInternal (dialect : ISqlDialect) (where : WhereExpressionNode) =
            ({ Combinator = And; Expression = where.Start } :: where.AdditionalClauses)
            |> HandleConditionalClauses (HandleValue dialect) 
            |> sprintf "WHERE %s"

        match where with
        | Some(w) -> Some(HandleWhereInternal dialect w)
        | None -> None

module SelectSqlGen =
    open Hyperboliq
    open AST
     
    let rec HandleSelectValue = HandleValueNode HandlePlainSelectExpression true
    and HandleSelectColumn = HandleColumn true
    and HandleSelectAggregate = HandleAggregate HandlePlainSelectExpression true
    and HandleSelectBinaryExpression = HandleBinaryExpression HandlePlainSelectExpression true
    and HandleSelectOrderByClause = HandleOrderByClause HandlePlainSelectExpression true
    and HandleSelectWindowedColumn = HandleWindowedColumn HandlePlainSelectExpression true
    and HandleSelectNamedColumn = HandleNamedColumn HandlePlainSelectExpression true
    and HandleSelectFunctionValue = HandleFunctionValue HandlePlainSelectExpression true

    and HandleSelect (dialect : ISqlDialect) (select : SelectValuesExpressionNode) =
        let HandleValue (dialect : ISqlDialect) value =
            match value with
            | ValueNode.Column(c) -> HandleSelectColumn dialect c
            | ValueNode.StarColumn(sc) -> HandleStarColumn true sc
            | ValueNode.Aggregate(a) -> HandleSelectAggregate dialect a
            | ValueNode.WindowedColumn(wc) -> HandleSelectWindowedColumn dialect wc
            | ValueNode.NamedColumn(nc) -> HandleSelectNamedColumn dialect nc
            | ValueNode.FunctionCall(ft) -> HandleSelectFunctionValue dialect ft
            | _ -> failwith "Not supported"

        select.Values 
        |> List.map (HandleValue dialect)
        |> JoinWithComma
        |> sprintf "SELECT %s%s" (if select.IsDistinct then "DISTINCT " else "") 

    and HandleFrom (dialect : ISqlDialect) (from : FromExpressionNode) =
        let TranslateJoinType jt =
            match jt with
            | JoinType.InnerJoin -> "INNER JOIN"
            | JoinType.LeftJoin -> "LEFT JOIN"
            | JoinType.RightJoin -> "RIGHT JOIN"
            | JoinType.FullJoin -> "FULL JOIN"

        let HandleJoin (jc : JoinClauseNode) =
            let joinHead = sprintf "%s %s" (TranslateJoinType jc.Type) (HandleTableDefinitionWithRef jc.TargetTable)
            match jc.Condition with
            | None -> joinHead
            | Some(c) -> sprintf "%s ON %s" joinHead (HandleSelectValue dialect c)

        let fromPart = 
            from.Tables
            |> List.map HandleTableDefinitionWithRef
            |> JoinWithComma
            |> sprintf "FROM %s"
        
        let joinPart =
            from.Joins
            |> List.map HandleJoin
            |> JoinWithSpace

        if joinPart.Length = 0 then fromPart else JoinWithSpace [ fromPart; joinPart ]


    and HandleGroupBy (dialect : ISqlDialect) (groupBy : GroupByExpressionNode option) : string option =
        let HandleGroupByClause (dialect : ISqlDialect) (col : ValueNode) =
            match col with
            | ValueNode.Column(c) -> HandleSelectColumn dialect c
            | _ -> failwith "Not supported"

        let HandleValueNode (dialect : ISqlDialect) (value : ValueNode) =
            match value with
            | ValueNode.BinaryExpression(n) -> HandleSelectBinaryExpression dialect n
            | _ -> failwith "Not supported"

        let HandleGroupByInternal (dialect : ISqlDialect) (groupBy : GroupByExpressionNode) : string =
            let groupByPart =
                groupBy.Clauses
                |> List.map (HandleGroupByClause dialect)
                |> JoinWithComma
                |> sprintf "GROUP BY %s"
            match groupBy.Having with
            | [] -> groupByPart
            | _ -> 
                groupBy.Having
                |> HandleConditionalClauses (HandleValueNode dialect)
                |> sprintf "%s HAVING %s" groupByPart

        match groupBy with
        | Some(g) -> Some(HandleGroupByInternal dialect g)
        | None -> None

    and HandleOrderBy (dialect : ISqlDialect) (orderBy : OrderByExpressionNode option) : string option =
        match orderBy with
        | None -> None
        | Some(o) ->
            o.Clauses
            |> List.map (HandleSelectOrderByClause dialect)
            |> JoinWithComma
            |> sprintf "ORDER BY %s"
            |> Option.Some

    and HandleSelectExpressionToken dialect (select : SelectExpressionToken) =
        [
            Some(HandleSelect dialect select.Select)
            Some(HandleFrom dialect select.From)
            HandleWhere HandleSelectBinaryExpression dialect select.Where
            HandleGroupBy dialect select.GroupBy
            HandleOrderBy dialect select.OrderBy
        ]
        |> JoinOptionsWithSpace

    and HandleSetExpression dialect (exp : SetSelectExpression) =
        let joinToken = TranslateSetOperation exp.Operation |> sprintf " %s "
        exp.Operands
        |> List.map (HandlePlainSelectExpression dialect)
        |> Join joinToken 

    and HandleCommonTableExpression dialect (cte : CommonTableExpression) =
        let HandleDefinition dialect (definition : ICommonTableDefinition) = 
            let query = HandlePlainSelectExpression dialect definition.Query
            let tref = HandleTableDefinition definition.Table.Definition
            sprintf "%s AS (%s)" tref query

        cte.Definitions
        |> List.rev
        |> List.map (HandleDefinition dialect)
        |> JoinWithComma
        |> sprintf "WITH %s"

    and HandlePlainSelectExpression dialect (plainSelect : PlainSelectExpression) =
        match plainSelect with
        | PlainSelectExpression.Plain(exp) -> HandleSelectExpressionToken dialect exp
        | PlainSelectExpression.Set(exp) -> HandleSetExpression dialect exp

    and HandleSelectExpression dialect (select : SelectExpression) =
        match select with
        | Plain(exp) ->
            HandlePlainSelectExpression dialect exp
        | Complex(withPart, selectPart) ->
            let withPartStr = HandleCommonTableExpression dialect withPart 
            let selectPartStr = HandlePlainSelectExpression dialect selectPart
            [ withPartStr; selectPartStr ] |> JoinWithSpace

module UpdateSqlGen =
    open AST

    let HandleUpdateSetToken dialect (token : UpdateSetToken) =
        let col = HandleColumn false dialect token.Column
        let value = HandleValueNode SelectSqlGen.HandlePlainSelectExpression false dialect token.Value
        sprintf "%s = %s" col value

    let HandleUpdateSet dialect (updateSet : UpdateStatementHeadToken) =
        updateSet.SetExpressions
        |> List.map (HandleUpdateSetToken dialect)
        |> JoinWithComma
        |> sprintf "UPDATE %s SET %s" updateSet.Table.Table.Name    

    let HandleUpdateExpression dialect (update : UpdateExpression) =
        [
            Some(HandleUpdateSet dialect update.UpdateSet)
            HandleWhere (HandleBinaryExpression SelectSqlGen.HandlePlainSelectExpression false) dialect update.Where
        ]
        |> JoinOptionsWithSpace

module DeleteSqlGen =
    open AST

    let HandleFrom dialect (from : FromExpressionNode) =
        from.Tables
        |> List.map HandleTableDefinitionWithRef
        |> JoinWithComma
        |> sprintf "DELETE FROM %s"

    let HandleDeleteExpression dialect (delete : DeleteExpression) =
        [
            Some(HandleFrom dialect delete.From)
            HandleWhere (HandleBinaryExpression SelectSqlGen.HandlePlainSelectExpression true) dialect delete.Where
        ]
        |> JoinOptionsWithSpace

module InsertSqlGen =
    open AST

    let HandleInsertInto dialect (into : InsertStatementHeadToken) =
        into.Columns
        |> List.map (HandleColumn false dialect)
        |> JoinWithComma
        |> sprintf "INSERT INTO %s (%s)" into.Table.Table.Name

    let HandleValues dialect (values : InsertValueToken list) =
        let HandleSingleValue (n : InsertValueNode) =
            match n with
            | InsertValueNode.Column(c) -> HandleColumn false dialect c
            | InsertValueNode.Constant(c) -> HandleConstant c
            | InsertValueNode.NullValue -> HandleNullValue ()
            | InsertValueNode.Parameter(p) -> HandleParameter p

        let HandleValueNodes (nodes : InsertValueNode list) =
            nodes
            |> List.map HandleSingleValue
            |> JoinWithComma
            |> sprintf "(%s)"

        values
        |> List.map (fun v -> HandleValueNodes v.Values)
        |> JoinWithComma
        |> sprintf "VALUES %s"

    let HandleInsertExpression dialect (insert : InsertExpression) =
        [
            HandleInsertInto dialect insert.InsertInto
            HandleValues dialect insert.InsertValues
        ]
        |> JoinWithSpace

module SqlGen =
    open AST

    let SqlifyExpression dialect expression =
        match expression with
        | Select(select) -> SelectSqlGen.HandleSelectExpression dialect select
        | Insert(insert) -> InsertSqlGen.HandleInsertExpression dialect insert
        | Delete(delete) -> DeleteSqlGen.HandleDeleteExpression dialect delete
        | Update(update) -> UpdateSqlGen.HandleUpdateExpression dialect update

