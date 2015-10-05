namespace Hyperboliq.Domain

module ExpressionVisitor =
    open Hyperboliq

    type VisitableExpression = 
    | LinqExpression of System.Linq.Expressions.Expression
    | Quotation of Quotations.Expr

    type ExpressionContext = ITableReference seq

    val Visit : VisitableExpression -> ExpressionContext -> AST.ValueNode option
    val VisitWithCustomConfig : ExpressionVisitorConfig -> VisitableExpression -> ExpressionContext -> AST.ValueNode option