namespace Hyperboliq.Domain

module ExpressionVisitor =
    open Types
    open Stream
    open System.Linq
    open System.Linq.Expressions

    type EvaluationBinding = string * ITableReference
    type EvaluationContext = EvaluationBinding list

    let ParamName (binding : EvaluationBinding) = fst binding
    let TableRef (binding : EvaluationBinding) = snd binding

    let FindBinding ctx paramName =
        Seq.find (fun b -> (ParamName b) = paramName) ctx

    let (|SqlParameterExpression|_|) (m : MemberExpression) =
        match m with
        | x when typeof<ExpressionParameter>.IsAssignableFrom x.Type ->
            match x.Expression with
            | :? ConstantExpression as ce -> Some (m.Member.Name, ce)
            | _ -> None
        | _ -> None

    let (|ParameterExpression|_|) (m : MemberExpression) = 
        match m.Expression with
        | :? ParameterExpression as pexp -> Some (m.Member.Name, pexp)
        | _ -> None

    let VisitSqlParameter mbrName (exp : ConstantExpression) =
        let flags = System.Reflection.BindingFlags.Instance ||| System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic
        let fieldInfo = exp.Value.GetType().GetField(mbrName, flags)
        match fieldInfo.GetValue(exp.Value) with
        | :? ExpressionParameter as p -> [ SqlNode.Parameter(ParameterToken(p.Name)) ]
        | _ -> failwith "Not implemented"

    let VisitMemberAccess (exp : MemberExpression) (context : EvaluationContext) =
        match exp with
        | SqlParameterExpression (mbrName, expr) ->  VisitSqlParameter mbrName expr 
        | ParameterExpression (mbrName, expr) -> 
            let binding = FindBinding context expr.Name
            [ SqlNode.Column(mbrName, TableRef binding) ]
        | _ -> failwith "Not implemented"

    let VisitConstant (exp : ConstantExpression) =
        match exp.Value with
        | null -> [ SqlNode.NullValue ]
        | :? ISqlStreamTransformable as ss -> ss.ToSqlStream()
        | :? string as s-> [ SqlNode.Constant(ConstantNode(sprintf "'%s'" s))]
        | x ->  [ SqlNode.Constant(ConstantNode(x.ToString())) ]

    let (|CompiledNullLambda|_|) (e : Expression) =
        try
            let result = Expression.Lambda(e).Compile().DynamicInvoke()
            match result with
            | null -> Some [ SqlNode.NullValue ]
            | :? SqlStream as ss -> Some ss
            | :? ISqlStreamTransformable as ss -> Some (ss.ToSqlStream())
            | _ -> Some [ SqlNode.Constant(ConstantNode(result.ToString())) ]
        with
            | _ -> Option.None

    let rec VisitSqlMethodCall (exp : MethodCallExpression) context = 
        let args = VisitExpressionList <| List.ofArray (exp.Arguments.ToArray()) <| context
        match exp.Method.Name, args with
        | "In", [ lhs; rhs ] -> [ SqlNode.BinaryExpression({ Lhs = lhs; Rhs = [ SubExpression(rhs) ]; Operation = BinaryOperation.In }) ]
        | "SubExpr", [ expr ] -> [ SubExpression(expr) ]
        | "Max", [ expr ] -> [ Aggregate(Max, expr) ]
        | "Min", [ expr ] -> [ Aggregate(Min, expr) ]
        | "Avg", [ expr ] -> [ Aggregate(Avg, expr) ]
        | "Count", [] -> [Aggregate(Count, [])]
        | "Count", [ expr ]-> [ Aggregate(Count, expr) ]
        | _ -> failwith "Not implemented"

    and VisitMethodCall (exp : MethodCallExpression) context =
        match exp with
        | x when x.Method.DeclaringType = typeof<Sql> -> VisitSqlMethodCall exp context
        | CompiledNullLambda stream -> stream
        | _ -> failwith "Not implemented"

    and VisitUnary (exp : UnaryExpression) context =
        match exp with
        | x when x.NodeType = ExpressionType.Convert && typeof<ExpressionParameter>.IsAssignableFrom(x.Operand.Type) -> 
            InternalVisit exp.Operand context
        | x when x.NodeType = ExpressionType.Convert && x.Method <> null ->
            match Expression.Lambda(x).Compile().DynamicInvoke() with
            | null -> [ SqlNode.NullValue ]
            | x -> [ SqlNode.Constant(ConstantNode(x.ToString())) ]
        | _ -> InternalVisit exp.Operand context

    and VisitBinary (exp : BinaryExpression) (context : EvaluationContext) = 
        [ 
            SqlNode.BinaryExpression(
                { 
                    Lhs = InternalVisit exp.Left context 
                    Operation = ToBinaryOperation exp.NodeType
                    Rhs = InternalVisit exp.Right context 
                }
            ) 
        ]

    and VisitNew (exp : NewExpression) (context : EvaluationContext) =
        let mbr = Expression.Convert(exp, typeof<System.Object>) 
        let lmb = Expression.Lambda<System.Func<System.Object>>(mbr)
        try 
            let getter = lmb.Compile()
            let result = getter.DynamicInvoke()
            match result with
            | :? ExpressionParameter as param -> [ SqlNode.Parameter(ParameterToken(param.Name)) ]
            | _ -> 
                let binding = FindBinding context (getter.ToString())
                [ SqlNode.Column(ParamName binding, TableRef binding) ]
        with
            | :? System.Collections.Generic.KeyNotFoundException 
            | :? System.InvalidOperationException -> 
                VisitExpressionList <| List.ofArray (exp.Arguments.ToArray()) <| context |> List.concat

    and VisitExpressionList (expList : Expression list) (context : EvaluationContext) : SqlStream list =
        let HandleSingleExpression (exp : Expression) (context : EvaluationContext) : SqlStream =
            match exp with
            | :? NewArrayExpression as arrayExp -> VisitExpressionList <| List.ofArray (arrayExp.Expressions.ToArray()) <| context |> List.concat
            | _ -> InternalVisit exp context
        List.map (fun x -> HandleSingleExpression x context) expList

    and InternalVisit (exp : Expression) context : SqlStream =
        match exp with
        | null -> []
        | :? LambdaExpression as x -> InternalVisit x.Body context
        | :? NewExpression as x -> VisitNew x context
        | :? MemberExpression as x -> VisitMemberAccess x context
        | :? BinaryExpression as x -> VisitBinary x context
        | :? ConstantExpression as x -> VisitConstant x
        | :? UnaryExpression as x -> VisitUnary x context
        | :? MethodCallExpression as x -> VisitMethodCall x context
        | _ -> failwith "Not implemented"

    let BindEvaluationContext (exp : LambdaExpression) (context : ITableReference seq) : EvaluationContext =
        exp.Parameters
        |> Seq.map (fun p -> p.Name)
        |> Seq.zip <| context
        |> List.ofSeq

    let Visit (exp : Expression) (context : ITableReference seq) : SqlStream =
        match exp with
        | null -> []
        | :? LambdaExpression as x -> BindEvaluationContext x context |> InternalVisit exp 
        | _ -> failwith "Not implemented"