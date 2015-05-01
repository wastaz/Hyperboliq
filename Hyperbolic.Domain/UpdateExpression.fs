﻿namespace Hyperboliq.Domain

module UpdateExpressionPart =
    open Types
    open Stream

    type UpdateSetExpression = 
        {
            Column : ColumnToken
            Value : SqlStream
        }

    type UpdateExpression = 
        {
            Table : ITableReference
            SetExpressions : UpdateSetExpression list
        }

    let NewUpdateExpression tbl =
        { 
            Table = tbl
            SetExpressions = []
        }

    let private ToColumnTokens tbl colSelector = 
        let tokens = ExpressionVisitor.Visit colSelector [ tbl ]
        tokens
        |> Seq.map (fun t -> match t with | SqlNode.Column(c) -> Some(c) | _ -> None)
        |> Seq.choose id
        |> Seq.toList

    let private ToValue (v : obj) =
        match v with
            | null -> [ SqlNode.NullValue ]
            | :? ISqlStreamTransformable as ss -> [ SqlNode.SubExpression(ss.ToSqlStream()) ]
            | :? string as s-> [ SqlNode.Constant(ConstantNode(sprintf "'%s'" s))]
            | x ->  [ SqlNode.Constant(ConstantNode(x.ToString())) ]

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
        |> (fun v -> { expr with UpdateExpression.SetExpressions = List.concat [ v; expr.SetExpressions ] })

    let AddSingleValueSetExpression expr colSelector value =
        { expr with UpdateExpression.SetExpressions = (ToSetExpression expr.Table colSelector value) :: expr.SetExpressions }

    let AddValueExpression expr colSelector valueSelector =
        let cols = ColumnsByName expr.Table colSelector
        let values = ExpressionVisitor.Visit valueSelector [ expr.Table ]
        List.zip cols values
        |> List.map (fun (c, v) -> {Column = c; Value = [ v ]})
        |> (fun v -> { expr with UpdateExpression.SetExpressions = List.concat [ v; expr.SetExpressions ] })