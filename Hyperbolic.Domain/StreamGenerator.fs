namespace Hyperboliq.Domain

module StreamGenerator =
    open Types
    open InsertExpressionPart
    open UpdateExpressionPart
    open ExpressionParts
    open Stream

    type StreamInput =
        | Delete 
        | InsertInto of InsertIntoExpression
        | InsertValues of InsertValuesExpression
        | UpdateSet of UpdateExpressionPart.UpdateExpression
        | Join of JoinExpression
        | From of FromExpression

    let HandleFrom ({ Tables = tbls } : FromExpression) : SqlStream =
        tbls
        |> List.rev
        |> List.map (fun tref -> Table(TableToken(tref)))
        |> (fun y -> Keyword(KeywordNode.From) :: y)

    let HandleJoin ({ Clauses = joinClauses } : JoinExpression) : SqlStream =
        match joinClauses with
        | [] -> []
        | _ -> 
            joinClauses
            |> List.rev
            |> List.map (fun c -> c.Flatten() )
            |> List.concat

    let HandleInsertIntoExpression ({ Table = tbl; Columns = cols } : InsertIntoExpression) : SqlStream =
        Keyword(KeywordNode.InsertInto) :: [ InsertHead({ Table = TableToken(tbl); Columns = cols }) ]

    let HandleInsertValuesExpression (expr : InsertValuesExpression) : SqlStream =
        Keyword(KeywordNode.Values) :: (List.map (fun v -> InsertValue(v)) expr.Values |> List.rev)

    let HandleUpdateSet ({ Table = tbl; SetExpressions = exprs } : UpdateExpressionPart.UpdateExpression) : SqlStream =
        let HandleSetExpression (se : UpdateExpressionPart.UpdateSetExpression) : UpdateSetToken =
            { Column = se.Column; Value = se.Value; }
        [ UpdateStatementHead({ Table = tbl; SetExpressions = (List.map HandleSetExpression exprs |> List.rev) }) ]

    let Handle (part : StreamInput) : SqlStream =
        match part with
        | Delete -> [ Keyword(KeywordNode.Delete) ]
        | InsertInto expr -> HandleInsertIntoExpression expr
        | InsertValues expr -> HandleInsertValuesExpression expr
        | UpdateSet expr -> HandleUpdateSet expr
        | Join expr -> HandleJoin expr
        | From expr -> HandleFrom expr

    let GenerateStream parts =
        parts
        |> List.ofSeq
        |> List.map (fun part -> Handle part)
        |> List.concat

