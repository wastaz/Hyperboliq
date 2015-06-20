namespace Hyperboliq.Domain

module ExpressionVisitor =
    open Hyperboliq
    val Visit : System.Linq.Expressions.Expression -> ITableReference seq -> Stream.ValueNode option
