namespace Hyperboliq.Domain

module DeleteExpressionParts =
    open Stream

    let WithWhereClause deleteExpr where =
        { deleteExpr with DeleteExpression.Where = Some(where) }
