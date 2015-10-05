namespace Hyperboliq.Tests.TokenGeneration

[<AutoOpen>]
module TestModel =

    [<CLIMutable>]
    type Person =
        { Id : int
          Name : string
          Age : int
          LivesAtHouseId : int
          ParentId : int }

    [<CLIMutable>]
    type House =
        { Id : int
          Address : string }
    
    [<CLIMutable>]    
    type Car =
        { Id : int
          Brand : string
          DriverId : int
          Age : int }

module TestHelpers =
    open Hyperboliq.Domain.AST

    let EmptySelect =
        { Select = { IsDistinct = false; Values = [] }
          From = { Tables = []; Joins = [] }
          Where = None
          GroupBy = None
          OrderBy = None }
    let ToPlainSelect = PlainSelectExpression.Plain >> SelectExpression.Plain >> SqlExpression.Select
