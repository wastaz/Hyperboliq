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
    | Select of SelectExpression
    | Join of JoinExpression
    | From of FromExpression
    | Where of WhereExpression
    | GroupBy of GroupByExpression
    | OrderBy of OrderByExpression

    val GenerateStream : StreamInput seq -> SqlStream