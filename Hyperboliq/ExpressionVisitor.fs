namespace Hyperboliq.Domain

module internal LinqExpressionVisitor =
    open Hyperboliq
    open AST
    open System.Linq
    open System.Linq.Expressions

    let ToBinaryOperation et =
        match et with
        | ExpressionType.Equal -> Equal
        | ExpressionType.NotEqual -> NotEqual
        | ExpressionType.GreaterThan -> GreaterThan
        | ExpressionType.GreaterThanOrEqual -> GreaterThanOrEqual
        | ExpressionType.LessThan -> LessThan
        | ExpressionType.LessThanOrEqual -> LessThanOrEqual
        | ExpressionType.AndAlso -> BinaryOperation.And
        | ExpressionType.OrElse -> BinaryOperation.Or
        | ExpressionType.Add -> Add
        | ExpressionType.Subtract -> Subtract
        | ExpressionType.Multiply -> Multiply
        | ExpressionType.Divide -> Divide
        | ExpressionType.Modulo -> Modulo
        | ExpressionType.Coalesce -> Coalesce
        | _ -> failwith "Not implemented"

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
        | :? ParameterExpression as pexp -> Some (m.Member.Name, m.Type, pexp)
        | _ -> None

    let (|StringConcatenation|_|) (op : BinaryOperation, lhs : ValueNode, rhs : ValueNode) =
        match op with
        | Add -> 
            match lhs, rhs with 
            | ValueNode.Column(_, t1, _), ValueNode.Column(_, t2, _) when t1 = typeof<System.String> && t2 = typeof<System.String> ->
                Some([ lhs; rhs ])
            | ValueNode.FunctionCall(FunctionType.Concat, args), ValueNode.Column(_, t, _) when t = typeof<System.String> ->
                Some(args @ [ rhs ])
            | ValueNode.FunctionCall(FunctionType.Concat, args1), ValueNode.FunctionCall(FunctionType.Concat, args2) ->
                Some(args1 @ args2)
            | ValueNode.Column(_, t, _), ValueNode.FunctionCall(FunctionType.Concat, args) when t = typeof<System.String> ->
                Some(lhs :: args)
            | _ -> None
        | _ -> None

    let VisitSqlParameter mbrName (exp : ConstantExpression) : ValueNode =
        let flags = System.Reflection.BindingFlags.Instance ||| System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic
        let fieldInfo = exp.Value.GetType().GetField(mbrName, flags)
        match fieldInfo.GetValue(exp.Value) with
        | :? ExpressionParameter as p -> ValueNode.Parameter(p.Name)
        | _ -> failwith "Not implemented"

    let VisitMemberAccess (cfg : ExpressionVisitorConfig) (exp : MemberExpression) (context : EvaluationContext) : ValueNode =
        match exp with
        | SqlParameterExpression (mbrName, expr) -> VisitSqlParameter mbrName expr 
        | ParameterExpression (mbrName, mbrType, expr) -> 
            let binding = FindBinding context expr.Name
            ValueNode.Column(mbrName, mbrType, TableRef binding)
        | :? MemberExpression as me ->
            let lambda = Expression.Lambda(Expression.Convert(me, typeof<obj>))
            let getter = lambda.Compile()
            match me.Type with
            | t when t = typeof<System.DateTime> ->
                getter.DynamicInvoke() :?> System.DateTime
                |> fun dt -> System.DateTime.SpecifyKind(dt, System.DateTimeKind.Unspecified).ToString("O")
                |> sprintf "'%s'" 
                |> ValueNode.Constant
            | _ -> 
                sprintf "'%s'" (getter.DynamicInvoke().ToString()) |> ValueNode.Constant  
        | _ -> failwith "Not implemented"

    let VisitConstant (exp : ConstantExpression) : ValueNode =
        match exp.Value with
        | null -> ValueNode.NullValue
        | :? SelectExpression as se -> 
            match se with 
            | Plain(q) -> ValueNode.SubExpression(q)
            | _ -> failwith "Not implemented"
        | :? string as s-> ValueNode.Constant(sprintf "'%s'" s)
        | x ->  ValueNode.Constant(x.ToString())

    let (|CompiledNullLambda|_|) (e : Expression) : ValueNode option =
        try
            let result = Expression.Lambda(e).Compile().DynamicInvoke()
            match result with
            | null -> 
                Some ValueNode.NullValue
            | :? ValueNode as vn -> 
                Some vn
            | :? ISelectExpressionTransformable as ss -> 
                match ss.ToSelectExpression() with
                | Plain(q) -> Some(ValueNode.SubExpression(q))
                | _ -> None
            | :? SelectExpression as ss ->
                match ss with
                | Plain(q) -> Some (ValueNode.SubExpression(q))
                | _ -> None
            | _ -> 
                Some (ValueNode.Constant(result.ToString()))
        with
            | _ -> None

    let rec VisitSqlMethodCall (cfg : ExpressionVisitorConfig) (exp : MethodCallExpression) (context : EvaluationContext) : ValueNode = 
        let args = VisitExpressionList cfg <| List.ofArray (exp.Arguments.ToArray()) <| context
        match exp.Method.Name, args with
        | "In", ValueList([ lhs; rhs ]) -> ValueNode.BinaryExpression({ Lhs = lhs; Rhs = rhs; Operation = BinaryOperation.In })
        | "SubExpr", ValueList([ expr ]) -> expr
        | "Max", ValueList([ expr ]) -> ValueNode.Aggregate(Max, expr)
        | "Min", ValueList([ expr ]) -> ValueNode.Aggregate(Min, expr)
        | "Avg", ValueList([ expr ]) -> ValueNode.Aggregate(Avg, expr)
        | "Sum", ValueList([ expr ]) -> ValueNode.Aggregate(Sum, expr)
        | "Count", ValueList(_) -> ValueNode.Aggregate(Count, ValueNode.NullValue)
        | "RowNumber", ValueList(_) -> ValueNode.Aggregate(RowNumber, ValueNode.NullValue)
        | _ -> failwith "Not implemented"

    and VisitMethodCall (cfg : ExpressionVisitorConfig) (exp : MethodCallExpression) context : ValueNode =
        let VisitStringMethodCall (smc : MethodCallExpression) context =
            match smc.Method.Name with
            | "ToUpper" | "ToUpperInvariant" -> 
                ValueNode.FunctionCall(FunctionType.Upper, [ VisitMemberAccess cfg (smc.Object :?> MemberExpression) context ])
            | "ToLower" | "ToLowerInvariant" ->
                ValueNode.FunctionCall(FunctionType.Lower, [ VisitMemberAccess cfg (smc.Object :?> MemberExpression) context ])
            | _ -> failwith "Not implemented"

        match exp with
        | x when x.Method.DeclaringType = typeof<Sql> -> VisitSqlMethodCall cfg exp context
        | x when x.Method.DeclaringType = typeof<System.String> -> VisitStringMethodCall x context
        | CompiledNullLambda stream -> stream
        | _ -> failwith "Not implemented"

        
    and VisitUnary (cfg : ExpressionVisitorConfig) (exp : UnaryExpression) context =
        match exp with
        | x when x.NodeType = ExpressionType.Convert && typeof<ExpressionParameter>.IsAssignableFrom(x.Operand.Type) -> 
            InternalVisit cfg exp.Operand context
        | x when x.NodeType = ExpressionType.Convert && x.Method <> null ->
            match Expression.Lambda(x).Compile().DynamicInvoke() with
            | null -> ValueNode.NullValue
            | x -> ValueNode.Constant(x.ToString())
        | _ -> InternalVisit cfg exp.Operand context

    and VisitBinary cfg (exp : BinaryExpression) (context : EvaluationContext) : ValueNode = 
        let lhs = InternalVisit cfg exp.Left context 
        let rhs = InternalVisit cfg exp.Right context 
        let op = ToBinaryOperation exp.NodeType
        match op, lhs, rhs with
        | StringConcatenation args -> ValueNode.FunctionCall(FunctionType.Concat, args)
        | _ -> ValueNode.BinaryExpression({ Lhs = lhs; Operation = op; Rhs = rhs })

    and VisitNew cfg (exp : NewExpression) (context : EvaluationContext) : ValueNode =
        let VisitColumnReference (mbr : System.Reflection.MemberInfo) (arg : Expression) (context : EvaluationContext) : ValueNode =
            let value = InternalVisit cfg arg context
            match value, cfg.IsUpdate with
            | ValueNode.Column(c, _, _), _ when c <> mbr.Name ->
                ValueNode.NamedColumn({ Alias = mbr.Name; Column = value })
            | ValueNode.Aggregate(_), _
            | ValueNode.WindowedColumn(_), _
            | ValueNode.Constant(_), _
            | ValueNode.BinaryExpression(_), false ->
                ValueNode.NamedColumn({ Alias = mbr.Name; Column = value })
            | _ -> value

        let mbr = Expression.Convert(exp, typeof<System.Object>) 
        let lmb = Expression.Lambda<System.Func<System.Object>>(mbr)
        try 
            let getter = lmb.Compile()
            let result = getter.DynamicInvoke()
            match result with
            | :? ExpressionParameter as param -> ValueNode.Parameter(param.Name)
            | _ -> 
                let binding = FindBinding context (getter.ToString())
                ValueNode.Column(ParamName binding, result.GetType(), TableRef binding)
        with
            | :? System.Collections.Generic.KeyNotFoundException 
            | :? System.InvalidOperationException -> 
                let members = List.ofSeq exp.Members
                let arguments = List.ofSeq exp.Arguments
                List.zip members arguments 
                |> List.map (fun (mbr, arg) -> VisitColumnReference mbr arg context)
                |> ValueNode.ValueList

    and VisitExpressionList (cfg : ExpressionVisitorConfig) (expList : Expression list) (context : EvaluationContext) : ValueNode =
        let HandleSingleExpression (context : EvaluationContext) (exp : Expression) : ValueNode =
            match exp with
            | :? NewArrayExpression as arrayExp -> 
                VisitExpressionList cfg <| List.ofArray (arrayExp.Expressions.ToArray()) <| context 
            | _ -> InternalVisit cfg exp context

        ValueNode.ValueList(List.map (HandleSingleExpression context) expList)
        

    and InternalVisit (cfg : ExpressionVisitorConfig) (exp : Expression) context : ValueNode =
        match exp with
        | null -> ValueNode.NullValue
        | :? LambdaExpression as x -> InternalVisit cfg x.Body context
        | :? NewExpression as x -> VisitNew cfg x context
        | :? MemberExpression as x -> VisitMemberAccess cfg x context
        | :? BinaryExpression as x -> VisitBinary cfg x context
        | :? ConstantExpression as x -> VisitConstant x
        | :? UnaryExpression as x -> VisitUnary cfg x context
        | :? MethodCallExpression as x -> VisitMethodCall cfg x context
        | _ -> failwith "Not implemented"

    let BindEvaluationContext (exp : LambdaExpression) (context : ITableReference seq) : EvaluationContext =
        exp.Parameters
        |> Seq.map (fun p -> p.Name)
        |> Seq.zip <| context
        |> List.ofSeq


    let VisitWithCustomConfig (cfg : ExpressionVisitorConfig) (exp : Expression) (context : ITableReference seq) : ValueNode option =
        match exp with
        | null -> None
        | :? LambdaExpression as x -> BindEvaluationContext x context |> InternalVisit cfg exp |> fun r -> Some(r)
        | _ -> failwith "Not implemented"

    let Visit exp ctx = VisitWithCustomConfig { IsUpdate = false } exp ctx

module internal QuotationVisitor =
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Microsoft.FSharp.Quotations
    open Microsoft.FSharp.Quotations.Patterns
    open Swensen.Unquote.Operators

    type PropertyGetExpression = Expr option * System.Reflection.PropertyInfo * Expr list
    type LetExpression = Var * Expr * Expr
    type CallInfo = Expr option * System.Reflection.MethodInfo * Expr list
    type ValueInfo = obj * System.Type
    type IfThenElseExpression = Expr * Expr * Expr

    type BindingValue =
    | Constant of value : string
    | Parameter of table : ITableReference
    | Expression of expression : ValueNode
    | PlainValue of value : ValueNode
    | TableValue of propertyName : string * table : ITableReference

    type Binding = string * BindingValue

    type EvaluationContext = Binding list
    
    let MatchesBindingName (str : string) ((name, _) : Binding) = str = name
    let FindBinding ctx paramName = Seq.find (MatchesBindingName paramName) ctx
    let FindPropertyBinding ctx objName propName = 
        FindBinding ctx objName
        |> function 
            | (_, Parameter(table)) -> propName, TableValue(propName, table)
            | _ -> failwith ("No binding found for: " + objName)

    let BindEvaluationContext (parameters : string list) (context : ITableReference seq) : EvaluationContext =
        parameters
        |> Seq.zip <| context
        |> Seq.map (fun (p, tr) -> p, Parameter(tr))
        |> List.ofSeq

    let RecursiveCollect (selector : Expr -> ('a * Expr) option) (exp : Expr) : 'a list * Expr =
        let rec internalRecursiveCollect exp res =
            let selectorResult = selector exp
            match selectorResult with
            | Some(current, body) ->
                internalRecursiveCollect body (current :: (fst res), body)
            | None ->
                fst res
                |> List.rev
                |> fun l -> l, (snd res)
        internalRecursiveCollect exp ([], exp)

    let CollectLet (exp : Expr) : (Var * Expr) list * Expr =
        let selector e =
            match e with
            | Let(var, body, expr) -> Some((var, body), expr)
            | _ -> None
        RecursiveCollect selector exp

    let CollectParameters (exp : Expr) : string list * Expr =
        let paramSelector e =
            match e with
            | Lambda(p, b) -> Some(p.Name, b)
            | _ -> None
        RecursiveCollect paramSelector exp

    let TypeForColumn (tref : ITableReference) col =
        tref.Table.GetProperties()
        |> Array.find (fun p -> p.Name.Equals(col, System.StringComparison.InvariantCultureIgnoreCase))
        |> fun pi -> pi.PropertyType

    let TryCreateSubExpressionNode o =
        let toSqlExpressionTransformable (v : obj) =
            match v with
            | :? ISqlExpressionTransformable as x -> x.ToSqlExpression() |> Some
            | _ -> None
        let toSubExpression v =
            match v with
            | SqlExpression.Select(Plain(ps)) -> ValueNode.SubExpression(ps) |> Some
            | _ -> None
        o |> toSqlExpressionTransformable |> Option.bind toSubExpression

    let rec BindingToValueNode (context : EvaluationContext) (binding : Binding) =
        match binding with
        | name, Constant(value) -> ValueNode.NamedColumn({ Alias = name; Column = ValueNode.Constant(value) })
        | name, TableValue(pn, tbl) when name = pn -> ValueNode.Column(pn, TypeForColumn tbl pn, tbl)
        | name, TableValue(pn, tbl) -> ValueNode.NamedColumn({ Alias = name; Column = ValueNode.Column(pn, TypeForColumn tbl pn, tbl) })
        | name, Expression(v) -> ValueNode.NamedColumn({ Alias = name; Column = v })
        | name, PlainValue(v) -> v
        | _ -> failwith "Cannot transform binding to valuenode"
        
    let VisitVar (context : EvaluationContext) (v : Var) =
        FindBinding context v.Name |> BindingToValueNode context

    let VisitValue (context : EvaluationContext) ((v, t) : ValueInfo) =
        match v with
        | :? string as s -> 
            s |> sprintf "'%s'" |> ValueNode.Constant
        | :? ExpressionParameter as p -> 
            p.Name |> ValueNode.Parameter
        | _ -> 
            v.ToString() |> sprintf "%s" |> ValueNode.Constant
        
    let VisitProperty (context : EvaluationContext) ((obj, property, l) : PropertyGetExpression) =
        match obj with
        | Some(Var(x)) -> 
            FindPropertyBinding context x.Name property.Name
            |> BindingToValueNode context
        | _ -> failwith "Huh?"

    let (|BinaryExpressionCall|_|) (methodInfo : System.Reflection.MethodInfo) =
        match methodInfo.Name with
        | "op_Equality" -> Some(BinaryOperation.Equal)
        | "op_Inequality" -> Some(BinaryOperation.NotEqual)
        | "op_GreaterThan" -> Some(BinaryOperation.GreaterThan)
        | "op_GreaterThanOrEqual" -> Some(BinaryOperation.GreaterThanOrEqual)
        | "op_LessThan" -> Some(BinaryOperation.LessThan)
        | "op_LessThanOrEqual" -> Some(BinaryOperation.LessThanOrEqual)
        | "op_Addition" -> Some(BinaryOperation.Add)
        | "op_Subtraction" -> Some(BinaryOperation.Subtract)
        | "op_Multiply" -> Some(BinaryOperation.Multiply)
        | "op_Division" -> Some(BinaryOperation.Divide)
        | "op_Modulus" -> Some(BinaryOperation.Modulo)
        | "In" when methodInfo.DeclaringType = typeof<Sql> -> Some(BinaryOperation.In)
        | _ -> None

    let (|NoArgsAggregateCall|_|) (methodInfo : System.Reflection.MethodInfo)  =
        if methodInfo.DeclaringType = typeof<Hyperboliq.Domain.Sql> then
            match methodInfo.Name with
            | "Count" -> Some(AggregateType.Count)
            | "RowNumber" -> Some(AggregateType.RowNumber)
            | _ -> None
        else
            None

    let (|UnaryAggregateCall|_|) (methodInfo : System.Reflection.MethodInfo) =
        if methodInfo.DeclaringType = typeof<Hyperboliq.Domain.Sql> then
            match methodInfo.Name with
            | "Sum" -> Some(AggregateType.Sum)
            | "Max" -> Some(AggregateType.Max)
            | "Min" -> Some(AggregateType.Min)
            | "Avg" -> Some(AggregateType.Avg)
            | _ -> None
        else
            None

    let rec VisitNewTuple (context : EvaluationContext) (expressions : Expr list) =
        expressions
        |> List.map (VisitQuotation context)
        |> ValueNode.ValueList

    and VisitLet (context : EvaluationContext) ((var, body, expr) : LetExpression) =
        match body with
        | PropertyGet(Some(Var(x)), prop, _) ->
            let _, binding = FindPropertyBinding context x.Name prop.Name
            (var.Name, binding) :: context |> fun ctx -> VisitQuotation ctx expr
        | PropertyGet(Some(Value(x, _)), prop, _) ->
            let value = prop.GetGetMethod().Invoke(x, [||]).ToString()
            (var.Name, BindingValue.Constant(value)) :: context |> fun ctx -> VisitQuotation ctx expr
        | NewTuple(exprs) -> 
            let values = 
                exprs
                |> List.map (VisitQuotation context)
                |> List.choose (function 
                    | ValueNode.Column(name, _, tref) -> Some(TableValue(name, tref))
                    | ValueNode.Constant(c) -> Some(Constant(c))
                    | _ -> None)
            let (tupleBindings, next) = CollectLet expr
              
            let newContext =
                tupleBindings
                |> List.choose (function 
                    | v, TupleGet(e, i) -> Some(v, e, i) 
                    | _ -> None)
                |> List.sortBy (fun (_, _, i) -> i)
                |> List.zip values 
                |> List.map (fun (b, (var, _, _)) -> var.Name, b)
                |> fun l -> List.concat [ l; context ]
            VisitQuotation newContext next
        | Call(c) ->
            (var.Name, VisitCall context c |> BindingValue.Expression) :: context
            |> fun ctx -> VisitQuotation ctx expr
        | Value(value, _) -> 
            (var.Name, Constant(value.ToString())) :: context 
            |> fun ctx -> VisitQuotation ctx expr
        | _ -> failwith "Not implemented"
    
    and VisitCall (context : EvaluationContext) ((exprOpt, methodInfo, exprList) : CallInfo) =
        match exprOpt, methodInfo, exprList with
        | None, NoArgsAggregateCall aggregate, [] ->
            ValueNode.Aggregate(aggregate, ValueNode.NullValue)
        | None, UnaryAggregateCall aggregate, operand :: [] ->
            ValueNode.Aggregate(aggregate, VisitQuotation context operand)
        | None, BinaryExpressionCall op, lhs :: rhs :: [] -> 
            ValueNode.BinaryExpression(
                { Operation = op
                  Lhs = VisitQuotation context lhs
                  Rhs = VisitQuotation context rhs
                })
        | None, _, [ op ] when methodInfo.DeclaringType = typeof<Hyperboliq.Domain.Sql> && methodInfo.Name = "Parameter" -> 
            VisitQuotation context op
        | None, mi, [ arg ] when mi.DeclaringType = typeof<Hyperboliq.Domain.Sql> && methodInfo.Name = "SubExpr" ->
            evalRaw arg 
            |> TryCreateSubExpressionNode
            |> Option.UnwrapOrFail
        | Some(instance), mi, el  ->
            let evaluatedInstance = evalRaw instance
            mi.Invoke(evaluatedInstance, el |> List.map (fun e -> evalRaw e :> obj) |> Array.ofList)
            |> TryCreateSubExpressionNode 
            |> Option.UnwrapOrFail
        | _ -> failwith "Not implemented"

    and VisitIfThenElse (context : EvaluationContext) ((predicate, trueBranch, falseBranch) : IfThenElseExpression) =
        let pred = VisitQuotation context predicate
        match trueBranch, falseBranch with
        | Value(v, _), _ when (unbox v) = true ->
            { Operation = BinaryOperation.Or 
              Lhs = pred
              Rhs = VisitQuotation context falseBranch } |> ValueNode.BinaryExpression
        | _, Value(v, _) when (unbox v) = false -> 
            { Operation = BinaryOperation.And
              Lhs = pred
              Rhs = VisitQuotation context trueBranch } |> ValueNode.BinaryExpression
        | _ -> failwith "Not implemented"

    and VisitApplication (context : EvaluationContext) ((e1, e2) : Expr*Expr) =
        match e1 with
        | Lambda(_) -> 
            let (parameters, body) = CollectParameters e1
            let newCtx = 
                match VisitQuotation context e2 with
                | ValueList(lst) ->
                    List.zip parameters lst
                    |> List.map (fun (a, b) -> a, PlainValue(b))
                    |> (fun l -> l @ context)
                | _ -> failwith "Not implemented"
            VisitQuotation newCtx body
        | _ -> failwith "Not implemented"

    and VisitNew (context : EvaluationContext) ((ctor, expressions) : (System.Reflection.ConstructorInfo * Expr list)) =
        if typeof<ExpressionParameter>.IsAssignableFrom(ctor.DeclaringType) then
            let args = expressions |> List.head |> VisitQuotation context
            match args with
            | ValueNode.Constant(s) -> s.Trim([| '\'' |]) |> ValueNode.Parameter
            | _ -> failwith "Not implemented"
        else
            failwith "Not implemented"

    and VisitQuotation (context : EvaluationContext) (exp : Expr) =
        match exp with
        | IfThenElse(ifThenElse) -> VisitIfThenElse context ifThenElse
        | Lambda(_, body) -> VisitQuotation context body 
        | Let(ln) -> VisitLet context ln
        | PropertyGet(pge) -> VisitProperty context pge
        | NewTuple(exprs) -> VisitNewTuple context exprs
        | Call(callInfo) -> VisitCall context callInfo
        | Application(applicationInfo) -> VisitApplication context applicationInfo
        | Var(v) -> VisitVar context v
        | Value(v) -> VisitValue context v
        | QuoteTyped(e) -> VisitQuotation context e
        | NewObject(e) -> VisitNew context e
        | _ -> failwith "Not implemented"

    let VisitWithCustomConfig cfg exp context =
        match exp with
        | Lambda(_) -> 
            let (parameters, body) = CollectParameters exp
            let evaluationContext = BindEvaluationContext parameters context
            VisitQuotation evaluationContext body |> Some
        | _ -> failwith "Not implemented"

    let Visit = VisitWithCustomConfig { IsUpdate = false }

module ExpressionVisitor =
    open Hyperboliq
    

    type VisitableExpression = 
    | LinqExpression of System.Linq.Expressions.Expression
    | Quotation of Quotations.Expr

    type ExpressionContext = ITableReference seq

    let Visit exp ctx =
        match exp with
        | LinqExpression(linqExpression) -> LinqExpressionVisitor.Visit linqExpression ctx
        | Quotation(quotation) -> QuotationVisitor.Visit quotation ctx
       
    let VisitWithCustomConfig cfg exp ctx =
        match exp with
        | LinqExpression(linqExpression) -> LinqExpressionVisitor.VisitWithCustomConfig cfg linqExpression ctx
        | Quotation(quotation) -> QuotationVisitor.VisitWithCustomConfig cfg quotation ctx
