namespace Hyperboliq.Tests.TokenGeneration

[<CLIMutable>]
type PersonLite =
  { Name : string
    Age : int 
  }

[<CLIMutable>]
type Person =
  { Id : int
    Name : string
    Age : int
    LivesAtHouseId : int
    ParentId : int 
  }

[<CLIMutable>]
type House =
  { Id : int
    Address : string 
  }
    
[<CLIMutable>] 
type Car =
  { Id : int
    Brand : string
    DriverId : int
    Age : int 
  }

[<CLIMutable>]
type RecursivePerson = 
  { Level : int
    Name : string
    ParentId : int
  }

[<CLIMutable>]
type PersonLitePagingResult = 
  { RowNumber : int
    Name : string
    Age : int
  }
