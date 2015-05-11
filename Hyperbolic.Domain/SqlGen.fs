namespace Hyperboliq.Domain

module SqlGen =
    open Types
    open Stream

    let Join sep (l : System.String seq) =
        System.String.Join(sep, l)

    let JoinWithComma = Join ", "
    let JoinWithSpace = Join " "

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
        | LessThan -> "<"
        | In -> "IN"
        | BinaryOperation.Or -> "OR"
        | BinaryOperation.And -> "AND"
        | Add -> "+"
        | Subtract -> "-"
        | _ -> failwith "Not implemented"

    (*
    // Probably redesign aggregates. Maybe even get rid of sqlstream altogether, wouldn't that be nice?
    let HandleAggregate (dialect : ISqlDialect) (agg : AggregateToken) =
        match agg with
        | Count, s -> "COUNT(*)"
        | Avg, s -> sprintf "AVG(%s)" s
        | Min, s -> sprintf "MIN(%s)" s
        | Max, s -> sprintf "MAX(%s)" s
    *)
    
    let HandleConstant (ConstantNode(c) : ConstantNode) = c

    let HandleNullValue () = "NULL"

    let HandleColumn (dialect : ISqlDialect) (column : ColumnToken) =
        match column with
        | "*", t -> sprintf "%s.*" t.ReferenceName
        | c, t -> sprintf "%s.%s" t.ReferenceName (dialect.QuoteColumnName c)

    let HandleParameter (ParameterToken(name) : ParameterToken) = sprintf "@%s" name

    let rec HandleValueNode (dialect : ISqlDialect) (vn : ValueNode) =
        match vn with
        | ValueNode.NullValue -> HandleNullValue ()
        | ValueNode.Column(c) -> HandleColumn dialect c
        | ValueNode.Constant(c) -> HandleConstant c
        | ValueNode.Parameter(p) -> HandleParameter p
        | ValueNode.BinaryExpression(be) -> HandleBinaryExpression dialect be
        | ValueNode.SubExpression(se) -> HandleSelectExpression dialect se |> sprintf "(%s)"
        | _ -> failwith "Not supported"

    and HandleBinaryExpression (dialect : ISqlDialect) (exp : BinaryExpressionNode) =
        let AddParensIfNecessary innerExp sql =
            match innerExp with
            | ValueNode.BinaryExpression(n) when HasLowerPredecence n.Operation exp.Operation -> sprintf "(%s)" sql 
            | _ -> sql

        let HandleNull dialect op exp =
            let compareVal = HandleValueNode dialect exp
            match op with
            | Equal -> sprintf "%s IS NULL" compareVal
            | NotEqual -> sprintf "%s IS NOT NULL" compareVal
            | _ -> failwith "Not supported"

        match exp.Lhs, exp.Rhs with
        | ValueNode.NullValue, _ -> HandleNull dialect exp.Operation exp.Rhs
        | _, ValueNode.NullValue -> HandleNull dialect exp.Operation exp.Lhs
        | _, _ ->
            let op = TranslateBinaryOperation exp.Operation
            let lhs = HandleValueNode dialect exp.Lhs |> AddParensIfNecessary exp.Lhs
            let rhs = HandleValueNode dialect exp.Rhs |> AddParensIfNecessary exp.Rhs
            sprintf "%s %s %s" lhs op rhs

    and HandleSelect (dialect : ISqlDialect) (select : SelectExpressionNode) =
        let HandleValue (dialect : ISqlDialect) value =
            match value with
            | ValueNode.Column(c) -> HandleColumn dialect c
            //| SqlNode.Aggregate(a) -> HandleAggregate dialect a
            | _ -> failwith "Not supported"

        select.Values 
        |> List.map (HandleValue dialect)
        |> JoinWithComma
        |> sprintf "SELECT %s%s" (if select.IsDistinct then "DISTINCT " else "") 

    and HandleFrom (dialect : ISqlDialect) includeTableRef (from : FromExpressionNode) =
        let HandleTable includeTableRef (t : ITableReference) =
            t.Table.Name + (if includeTableRef then " " + t.ReferenceName else "")
        
        let TranslateJoinType jt =
            match jt with
            | JoinType.InnerJoin -> "INNER JOIN"
            | JoinType.LeftJoin -> "LEFT JOIN"
            | JoinType.RightJoin -> "RIGHT JOIN"
            | JoinType.FullJoin -> "FULL JOIN"

        let HandleJoin (jc : JoinClauseNode) =
            let joinHead = sprintf "%s %s" (TranslateJoinType jc.Type) (HandleTable true jc.TargetTable)
            match jc.Condition with
            | None -> joinHead
            | Some(c) -> sprintf "%s ON %s" joinHead (HandleValueNode dialect c)

        let fromPart = 
            from.Tables
            |> List.map (HandleTable includeTableRef)
            |> JoinWithComma
            |> sprintf "FROM %s"
        
        let joinPart =
            from.Joins
            |> List.map HandleJoin
            |> JoinWithSpace

        if joinPart.Length = 0 then fromPart else JoinWithSpace [ fromPart; joinPart ]


    and HandleWhere (dialect : ISqlDialect) includeTableRef (where : WhereExpressionNode option) =
        let HandleValue (dialect : ISqlDialect) (value : ValueNode) =
            match value with
            | ValueNode.BinaryExpression(n) -> HandleBinaryExpression dialect n
            | _ -> failwith "Not supported"

        let HandleCombinator combinator =
            match combinator with
            | And -> "AND"
            | Or -> "OR"

        let HandleAdditionalClauses (dialect : ISqlDialect) (clause : WhereClauseNode) =
            let combinator = HandleCombinator clause.Combinator
            let expression = HandleValue dialect clause.Expression
            JoinWithSpace [ combinator; expression ]

        let HandleWhereInternal (dialect : ISqlDialect) includeTableRef (where : WhereExpressionNode) =
            let start = HandleValue dialect where.Start
            let additionals = 
                List.map (HandleAdditionalClauses dialect) where.AdditionalClauses
                |> JoinWithSpace
                |> fun s -> if s.Length > 0 then " " + s else s
            sprintf "WHERE %s%s" start additionals

        match where with
        | Some(w) -> Some(HandleWhereInternal dialect includeTableRef w)
        | None -> None
    
    and HandleSelectExpression dialect (select : SelectExpression) =
        [
            Some(HandleSelect dialect select.Select)
            Some(HandleFrom dialect true select.From)
            HandleWhere dialect true select.Where
        ]
        |> List.choose id
        |> JoinWithSpace

    let SqlifyExpression dialect expression =
        match expression with
        | Select(select) -> HandleSelectExpression dialect select
        | Insert(insert) -> ""
        | Delete(delete) -> ""
        | Update(update) -> ""

