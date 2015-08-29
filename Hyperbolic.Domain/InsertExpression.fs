namespace Hyperboliq.Domain


module InsertExpressionPart =
    open AST

    let PropertySortByName (p : System.Reflection.PropertyInfo) = p.Name
    
    let ApplyOnProperties (properties : System.Reflection.PropertyInfo list) sortFn f =
        properties
        |> List.sortBy sortFn
        |> List.map f

    let FilterPropertiesByColumns intoExpr value =
        let colNames = intoExpr.Columns |> List.map (fun (a, _, _) -> a ) |> Set.ofList
        value.GetType().GetProperties()
        |> Array.filter (fun p -> Set.contains p.Name colNames)
        |> List.ofArray

    let AddInsertValue (intoExpr : InsertStatementHeadToken) (valuesExpr : InsertValueToken list) value =
        let PropertyToConstant instance (p : System.Reflection.PropertyInfo) =
            match p.GetMethod.Invoke(instance, [||]) with
            | null -> NullValue
            | :? string as x  -> Constant("'" + x + "'")
            | v -> Constant(v.ToString())

        let FindIndex (idxMap : Map<string, int>) (propertyInfo : System.Reflection.PropertyInfo) =
            idxMap.[propertyInfo.Name]

        let idxMap =
            intoExpr.Columns
            |> List.mapi (fun i (col, _, _) -> (col, i))
            |> Map.ofList
        
        ApplyOnProperties (FilterPropertiesByColumns intoExpr value) (FindIndex idxMap) (PropertyToConstant value)
        |> (fun constants -> { InsertValueToken.Values = constants } ::  valuesExpr)
        
    
    let AddColumns (insertExpr : InsertStatementHeadToken) columnSelector =
        let values = ExpressionVisitor.Visit columnSelector [ insertExpr.Table ]
        match values with
        | None -> insertExpr
        | Some(ValueList(valueList)) ->
            valueList
            |> List.map (fun token -> match token with | ValueNode.Column(c) -> Some(c) | _ -> None)
            |> List.choose id
            |> fun cols -> { insertExpr with Columns = cols }
        | Some(ValueNode.Column(col)) ->
            { insertExpr with Columns = [ col ]}
        | _ -> insertExpr

    let AddAllColumns (insertExpr : InsertStatementHeadToken) =
        ApplyOnProperties (List.ofArray (insertExpr.Table.Table.GetProperties())) PropertySortByName (fun p -> (p.Name, p.PropertyType, insertExpr.Table))
        |> fun cols -> { insertExpr with InsertStatementHeadToken.Columns = cols }
