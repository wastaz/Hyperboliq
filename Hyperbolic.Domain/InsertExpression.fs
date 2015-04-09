namespace Hyperboliq.Domain


module InsertExpressionPart =
    open Types
    open Stream

    let ToInsertValueNodes stream =
        let SqlNodeToInsertValue node =
            match node with
            | SqlNode.NullValue -> InsertValueNode.NullValue
            | SqlNode.Column(c) -> InsertValueNode.Column(c)
            | SqlNode.Constant(c) -> InsertValueNode.Constant(c)
            | SqlNode.Parameter(p) -> InsertValueNode.Parameter(p)
            | _ -> failwith "Not implemented"
        stream 
        |> List.map SqlNodeToInsertValue

    type InsertIntoExpression = 
        { 
            Table : ITableReference 
            Columns : ColumnToken list
        }

    let NewInsertIntoExpression tbl columnSelector = 
        { 
            Table = tbl
            Columns = (ExpressionVisitor.Visit columnSelector [ tbl ]) 
                      |> List.map (fun token -> match token with | SqlNode.Column(c) -> Some(c) | _ -> None)
                      |> List.choose id
        }

    let PropertySortByName (p : System.Reflection.PropertyInfo) = p.Name
    
    let ApplyOnProperties (tblType : System.Type)  sortFn f =
        tblType.GetProperties()
        |> Array.sortBy sortFn
        |> Array.toList
        |> List.map f
        
    let NewInsertIntoExpressionWithAllColumns<'a> tbl =
        {
            Table = tbl
            Columns = (ApplyOnProperties typeof<'a> PropertySortByName (fun p -> (p.Name, tbl)))
        }

    type InsertValuesExpression =
        {
            Values : InsertValueNode list list
        }

    let NewInsertValuesExpression () =
        { Values = [] }

    let AddInsertValue intoExpr valuesExpr value =
        let PropertyToConstant instance (p : System.Reflection.PropertyInfo) =
            match p.GetMethod.Invoke(instance, [||]) with
            | null -> NullValue
            | :? string as x  -> Constant(ConstantNode("'" + x + "'"))
            | v -> Constant(ConstantNode(v.ToString()))
        let idxMap =
            intoExpr.Columns
            |> List.mapi (fun i (col, _) -> (col, i))
            |> Map.ofList

        PropertyToConstant value 
        |> ApplyOnProperties (value.GetType()) (fun p -> idxMap.[p.Name])
        |> (fun constants -> { valuesExpr with Values = constants :: valuesExpr.Values })
