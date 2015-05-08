namespace Hyperboliq.Domain

module SelectExpressionParts =
    open Stream

    let WithFromClause selectExpr from =
        { selectExpr with SelectExpression.From = from }

    let WithWhereClause selectExpr where =
        { selectExpr with SelectExpression.Where = Some(where) }

    let WithGroupClause selectExpr group =
        { selectExpr with SelectExpression.GroupBy = Some(group) }

    let WithOrderClause selectExpr order =
        { selectExpr with SelectExpression.OrderBy = Some(order) }
