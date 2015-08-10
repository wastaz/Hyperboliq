namespace Hyperboliq.Domain

module ExpressionVisitor =
    open Hyperboliq

    type ExpressionVisitorConfig = {
        IsUpdate : bool
    }

    val Visit : System.Linq.Expressions.Expression -> ITableReference seq -> Stream.ValueNode option
    val VisitWithCustomConfig : ExpressionVisitorConfig -> System.Linq.Expressions.Expression -> ITableReference seq -> Stream.ValueNode option
