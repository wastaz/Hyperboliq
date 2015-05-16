namespace Hyperboliq.Domain

module UpdateExpressionPart =
    open Types
    open Stream

    let NewUpdateExpression tbl =
        { 
            Table = tbl
            SetExpressions = []
        }

    let private ToColumnTokens tbl colSelector = 
        let tokens = ExpressionVisitor.Visit colSelector [ tbl ]
        match tokens with
        | None -> []
        | Some(ValueNode.Column(c)) -> [ c ]
        | Some(ValueList(valueList)) ->  
            valueList
            |> List.map (fun t -> match t with | ValueNode.Column(c) -> Some(c) | _ -> None)
            |> List.choose id
        | Some(_) -> []

    let private ToValue (v : obj) : ValueNode =
        match v with
            | null -> ValueNode.NullValue
            | :? SelectExpression as ss -> ValueNode.SubExpression(ss)
            | :? string as s-> ValueNode.Constant(ConstantNode(sprintf "'%s'" s))
            | x ->  ValueNode.Constant(ConstantNode(x.ToString()))

    let private ToSetExpression tbl colSelector (value : obj) =
        { 
            Column = 
                match ToColumnTokens tbl colSelector with
                | [ c ] -> c
                | _ -> failwith "Expected single column"
            Value = ToValue value
        }

    let private ColumnsByName tbl colSelector =
        ToColumnTokens tbl colSelector
        |> List.sortByDescending (fun (name, _) -> name)

    let private PropertiesByName v =
        v.GetType().GetProperties()
        |> Array.sortByDescending (fun p -> p.Name)
        |> Array.toList
        
    let AddMultipleValueSetExpression expr colSelector values =
        let cols = ColumnsByName expr.Table colSelector
        let properties = PropertiesByName values
        List.zip cols properties
        |> List.map (fun (c, p) -> { Column = c; Value = ToValue (p.GetValue(values)) })
        |> (fun v -> { expr with SetExpressions = List.concat [ v; expr.SetExpressions ] })
        

    let AddSingleValueSetExpression expr colSelector value =
        { expr with SetExpressions = (ToSetExpression expr.Table colSelector value) :: expr.SetExpressions }

    let AddObjectSetExpression<'a, 'b> expr (colSelector : System.Linq.Expressions.Expression<System.Func<'a, 'b>>) (objVal : 'b) =
        if(typeof<'b>.IsValueType || typeof<'b> = typeof<System.String>) then
            AddSingleValueSetExpression expr colSelector objVal
        else
            AddMultipleValueSetExpression expr colSelector objVal

    let AddValueExpression head colSelector valueSelector =
        let cols = ColumnsByName head.Table colSelector
        let values = ExpressionVisitor.Visit valueSelector [ head.Table ]
        match values with
        | None -> head
        | Some(ValueList(valueList)) ->
            List.zip cols valueList
            |> List.map (fun (c, v) -> { Column = c; Value = v })
            |> (fun v -> { head with SetExpressions = v @ head.SetExpressions })
        | Some(value) ->
            { head with SetExpressions = { Column = (List.head cols); Value = value } :: head.SetExpressions }
        

    let NewUpdateHead tbl : UpdateStatementHeadToken = 
        {
            Table = tbl
            SetExpressions = []
        }


    let WithWhereClause updateExpr where =
        { updateExpr with UpdateExpression.Where = Some(where) }

    let WithHead updateExpr head =
        { updateExpr with UpdateExpression.UpdateSet = head }