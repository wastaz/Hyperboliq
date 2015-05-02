namespace Hyperboliq.Domain

module StreamGenerator =
    open ExpressionParts
    open InsertExpressionPart
    open UpdateExpressionPart
    open Stream

    type StreamInput =
    | Delete 
    | InsertInto of InsertIntoExpression
    | InsertValues of InsertValuesExpression
    | UpdateSet of UpdateExpressionPart.UpdateExpression
    | Join of JoinExpression
    | From of FromExpression

    val GenerateStream : StreamInput seq -> SqlStream