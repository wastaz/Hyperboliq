namespace Hyperboliq.Tests.TokenGeneration

[<CLIMutable>]
type PersonLite =
    { Name : string
      Age : int }

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
