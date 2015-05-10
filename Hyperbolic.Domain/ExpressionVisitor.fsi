namespace Hyperboliq.Domain

module ExpressionVisitor =
    val Visit : System.Linq.Expressions.Expression -> Types.ITableReference seq -> Stream.ValueNode option
