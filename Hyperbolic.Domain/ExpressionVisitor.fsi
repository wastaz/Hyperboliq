namespace Hyperboliq.Domain

module ExpressionVisitor =
    open Hyperboliq

    type ExpressionVisitorConfig = {
        IsUpdate : bool
    }

    val Visit : System.Linq.Expressions.Expression -> ITableReference seq -> AST.ValueNode option
    val VisitWithCustomConfig : ExpressionVisitorConfig -> System.Linq.Expressions.Expression -> ITableReference seq -> AST.ValueNode option
