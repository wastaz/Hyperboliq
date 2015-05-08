namespace Hyperboliq.Domain

module SqlGenerator =
    open Types
    open Stream

    type GeneratorSettings = {
        Dialect : ISqlDialect
        SkipTableReferences : bool
    }

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

    let HandleKeyword kw =
        match kw with
        | InsertInto -> "INSERT INTO"
        | KeywordNode.Select -> "SELECT"
        | KeywordNode.Delete -> "DELETE"
        | Values -> "VALUES"
        | KeywordNode.From -> "FROM"
        | KeywordNode.Where -> "WHERE"
        | KeywordNode.And -> "AND"
        | KeywordNode.Or -> "OR"
        | On -> "ON"
        | Join(InnerJoin) -> "INNER JOIN"
        | Join(LeftJoin) -> "LEFT JOIN"
        | Join(RightJoin) -> "RIGHT JOIN"
        | Join(FullJoin) -> "FULL JOIN"
        | KeywordNode.OrderBy -> "ORDER BY"
        | KeywordNode.GroupBy -> "GROUP BY"
        | Having -> "HAVING"
        | KeywordNode.Update -> "UPDATE"
        | Set -> "SET"
        | _ -> failwith "Not implemented"

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

    let AddColumnListingSeparator peekToken str =
        match peekToken with
        | Some(Aggregate(_, _)) | Some(SqlNode.Column(_, _)) -> str + ","
        | _ -> str

    let AddValueListingSeparator peekToken str =
        match peekToken with
        | Some(InsertValue(_)) -> str + ","
        | _ -> str

    let HandleColumn (dialect : GeneratorSettings) col (tbl : ITableReference) peek =
        match dialect.SkipTableReferences, col with
        | false, "*" -> tbl.ReferenceName + "." + col
        | true, "*" -> col
        | false, _ -> tbl.ReferenceName + "." + (dialect.Dialect.QuoteColumnName col)
        | true, _ -> (dialect.Dialect.QuoteColumnName col)
        |> AddColumnListingSeparator peek

    let matchSingleNode nodePredicate stream =
        match stream with
        | [] -> None
        | [ exp ] -> nodePredicate exp
        | _ -> None

    let (|SingleBinaryExpression|_|) (stream : SqlStream) = 
        let matcher exp = 
            match exp with
            | BinaryExpression(e) -> Some e
            | _ -> None
        matchSingleNode matcher stream 
        
    let (|SingleConstantExpression|_|) (stream : SqlStream) =
        let matcher exp = 
            match exp with
            | SqlNode.Constant(e) -> Some e
            | _ -> None
        matchSingleNode matcher stream

    let (|SingleSubExpression|_|) (stream : SqlStream) =
        let matcher exp =
            match exp with
            | SqlNode.SubExpression(e) -> Some e
            | _ -> None
        matchSingleNode matcher stream

    let HasLowerPredecence op1 op2 = (ToPredecenceNumber op1) > (ToPredecenceNumber op2)

    let rec FeedStream streamHandler (dialect : GeneratorSettings) stream (str : string) : string =
        let separator = if (str.Length = 0) then "" else " "
        match stream with 
        | [] -> str
        | hd :: nxt :: tl -> FeedStream streamHandler dialect (List.tail stream) (str + separator + (streamHandler dialect hd (Some(nxt))))
        | hd :: tl -> str + separator + (streamHandler dialect hd None)

    let HandleInsertToken dialect token peekToken =
        match token, peekToken with
        | InsertValueNode.NullValue, _ -> "NULL"
        | InsertValueNode.Constant(ConstantNode(c)), Some(InsertValueNode.Constant(_)) -> c + ","
        | InsertValueNode.Constant(ConstantNode(c)), _ -> c
        | InsertValueNode.Column(c), _ -> "Column"
        | InsertValueNode.Parameter(p), _ -> "Parameter"

    let HandleInsertHead dialect ({ Table = tbl; Columns = cols } : InsertStatementHeadToken) peek =
        let GetColumnName ((c, _) : ColumnToken) = c
        cols
        |> List.map GetColumnName
        |> List.toArray
        |> (fun colnames -> System.String.Join(", ", colnames))
        |> sprintf "%s (%s)" tbl.Table.Name

    let HandleInsertValue dialect value peek =
        sprintf "(%s)" (FeedStream HandleInsertToken dialect value "")
        |> AddValueListingSeparator peek

    let rec HandleBinaryExpression dialect ({ Lhs = left; Operation = op; Rhs = right } : BinaryExpressionNode) = 
        let AddParensIfNecessary stream sql =
            match stream with
            | SingleBinaryExpression exp when HasLowerPredecence exp.Operation op -> sprintf "(%s)" sql
            | _ -> sql
        let HandleNullComparison stream =
            InternalSqlify dialect stream
            |> AddParensIfNecessary stream
            |> fun s ->
                match op with 
                | BinaryOperation.Equal -> sprintf "%s IS NULL" s
                | BinaryOperation.NotEqual -> sprintf "%s IS NOT NULL" s
                | _ -> failwith "Unsupported comparison with null"
        match left, right with
        | [ SqlNode.NullValue ], _ -> 
            HandleNullComparison right
        | _, [ SqlNode.NullValue ] -> 
            HandleNullComparison left
        | _, _ -> 
            let lhs = InternalSqlify dialect left |> AddParensIfNecessary left
            let rhs = InternalSqlify dialect right |> AddParensIfNecessary right
            let operation = TranslateBinaryOperation op
            sprintf "%s %s %s" lhs operation rhs

    and HandleUpdateStatementHead dialect (stmtHead : UpdateStatementHeadToken) peek =
        let addParensIfNecessary stream sql =
            match stream with
            | SingleBinaryExpression _
            | SingleSubExpression _
            | SingleConstantExpression _ -> sql
            | _ -> sprintf "(%s)" sql
        let handleUpdateStatement ({ Column = (colName, _); Value = value } : UpdateSetToken) =
            InternalSqlify { dialect with SkipTableReferences = true } value
            |> addParensIfNecessary value
            |> sprintf "%s = %s" colName 
        stmtHead.SetExpressions
        |> List.map handleUpdateStatement
        |> (fun l -> System.String.Join(", ", (Array.ofList l)))
        |> sprintf "UPDATE %s SET %s" stmtHead.Table.Table.Name

    and HandleSubExpression dialect expr =
        SqlifyExpression dialect.Dialect (Select(expr)) |> sprintf "(%s)"

    and HandleOrderingToken dialect ({ Selector = selector; Direction = direction; NullsOrdering = nullsOrder } : Ordering) peekToken =
        Sqlify dialect.Dialect selector
        |> fun s -> match direction with
                    | Ascending -> s + " ASC"
                    | Descending -> s + " DESC"
        |> fun s -> match nullsOrder with
                    | NullsUndefined -> s
                    | NullsFirst -> s + " NULLS FIRST"
                    | NullsLast -> s + " NULLS LAST"
        |> fun s -> match peekToken with
                    | Some(OrderingToken(_)) -> s + ","
                    | _ -> s

    and HandleAggregate (dialect : GeneratorSettings) agg stream peekToken =
        Sqlify dialect.Dialect stream
        |> fun s -> 
            match agg with
            | Count -> "COUNT(*)"
            | Avg -> sprintf "AVG(%s)" s
            | Min -> sprintf "MIN(%s)" s
            | Max -> sprintf "MAX(%s)" s
        |> AddColumnListingSeparator peekToken

    and HandleToken dialect token peekToken =
        match token with
        | SqlNode.Keyword(kw) -> HandleKeyword kw
        | SqlNode.Column(col, tbl) -> HandleColumn dialect col tbl peekToken
        | SqlNode.Constant(ConstantNode(s)) -> s
        | SqlNode.Aggregate(agg, stream) -> HandleAggregate dialect agg stream peekToken
        | SqlNode.Parameter(ParameterToken(parameterName)) -> "@" + parameterName
        | SqlNode.Table(TableToken(tbl)) -> tbl.Table.Name + " " + tbl.ReferenceName
        | SqlNode.BinaryExpression(binExp) -> HandleBinaryExpression dialect binExp
        | SqlNode.SubExpression(subExpr) -> HandleSubExpression dialect subExpr
        | SqlNode.OrderingToken(orderingToken) -> HandleOrderingToken dialect orderingToken peekToken
        | SqlNode.InsertHead(headToken) -> HandleInsertHead dialect headToken peekToken
        | SqlNode.InsertValue(value) -> HandleInsertValue dialect value peekToken
        | SqlNode.UpdateStatementHead(value) -> HandleUpdateStatementHead dialect value peekToken
        | _ -> failwith "Not Implemented"

    and InternalSqlify settings stream =
        match stream with
        | [] -> ""
        | _ -> FeedStream HandleToken settings stream ""

    and Sqlify (dialect : ISqlDialect) (stream : SqlStream) =
        let initialSkipTableReferences =
            match List.tryHead stream with
            | Some(SqlNode.UpdateStatementHead(_)) -> true
            | _ -> false
        InternalSqlify { Dialect = dialect; SkipTableReferences = initialSkipTableReferences } stream
    
    and SqlifyExpression (dialect : ISqlDialect) (expr : SqlExpression) =
        ""

    let SqlifySeq (dialect : ISqlDialect) (stream : SqlNode seq) =
        List.ofSeq stream
        |> Sqlify dialect
