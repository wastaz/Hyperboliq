namespace Hyperboliq.Tests.TokenGeneration

module TestHelpers =
    open Hyperboliq.Domain.AST

    let EmptySelect =
        { Select = { IsDistinct = false; Values = [] }
          From = { Tables = []; Joins = [] }
          Where = None
          GroupBy = None
          OrderBy = None
          LimitOffset = { Limit = None; Offset = None } 
        }
    let ToPlainSelect = PlainSelectExpression.Plain >> SelectExpression.Plain >> SqlExpression.Select
