namespace Hyperboliq.Migration.Tests

module TestModel =

    type Person = {
        Id : int
        Name : string
        Age : int
        LivesAtHouseId : int
        ParentId : int
    }

    type Car = {
        Id : int
        Brand : string
        DriverId : int
        Age : int
    }

    type House = {
        Id : int
        Address : string
    }

