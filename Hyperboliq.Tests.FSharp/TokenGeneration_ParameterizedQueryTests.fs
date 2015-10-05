namespace Hyperboliq.Tests.TokenGeneration

module ParameterizedQueryTests = 
    open NUnit.Framework
    open FsUnit
    open Hyperboliq
    open Hyperboliq.Domain.AST
    open Hyperboliq.Domain

    type testtype () =
        static member parameter<'a> (str : string) : 'a =
            Unchecked.defaultof<'a>
        static member parameter<'a> (obj : ExpressionParameter) : 'a =
            Unchecked.defaultof<'a>


    [<Test>]
    let ``It should be able to use a one off parameter in a query`` () =
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age < Sql.Parameter (ExpressionParameter<int>("age")) @>)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected = 
            { TestHelpers.EmptySelect with 
                Select = { IsDistinct = false
                           Values = [ ValueNode.StarColumn(tref.Reference) ] }
                From = { Tables = [ tref ]; Joins = [] }
                Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)
                                    Operation = BinaryOperation.LessThan
                                    Rhs = ValueNode.Parameter("age") } |> ValueNode.BinaryExpression
                          AdditionalClauses = []
                        } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected

    [<Test>]
    let ``It should be able to parameterize a query`` () =
        let ageParam = ExpressionParameter<int>("age")
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > Sql.Parameter ageParam @>)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { TestHelpers.EmptySelect with 
                Select = { IsDistinct = false
                           Values = [ ValueNode.StarColumn(tref.Reference) ] }
                From = { Tables = [ tref ]; Joins = [] }
                Where = { Start = { Lhs = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)
                                    Operation = BinaryOperation.GreaterThan
                                    Rhs = ValueNode.Parameter("age") } |> ValueNode.BinaryExpression
                          AdditionalClauses = []
                        } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected

    [<Test>]
    let ``It should be possible to use the parameter in more complex expression`` () =
        let ageParam = ExpressionParameter<int>("age")
        let expr = Select.Star<Person>()
                         .From<Person>()
                         .Where(<@ fun (p : Person) -> p.Age > Sql.Parameter ageParam && Sql.Parameter ageParam < 90 @>)
                         .Or(<@ fun (p : Person) -> p.Age < Sql.Parameter ageParam @>)
        let result = expr.ToSqlExpression()

        let tref = TableIdentifier<Person>()
        let expected =
            { TestHelpers.EmptySelect with 
                Select = { IsDistinct = false
                           Values = [ ValueNode.StarColumn(tref.Reference) ] }
                From = { Tables = [ tref ]; Joins = [] }
                Where = { Start = { Lhs = { Lhs = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)
                                            Operation = BinaryOperation.GreaterThan
                                            Rhs = ValueNode.Parameter("age")
                                          } |> ValueNode.BinaryExpression 
                                    Operation = BinaryOperation.And
                                    Rhs = { Lhs = ValueNode.Parameter("age")
                                            Operation = BinaryOperation.LessThan
                                            Rhs = ValueNode.Constant("90")
                                          } |> ValueNode.BinaryExpression } |> ValueNode.BinaryExpression
                          AdditionalClauses = [ { Combinator = ExpressionCombinatorType.Or 
                                                  Expression = { Lhs = ValueNode.Column("Age", typeof<int>, tref.Reference :> ITableReference)
                                                                 Operation = BinaryOperation.LessThan
                                                                 Rhs = ValueNode.Parameter("age") } |> ValueNode.BinaryExpression } ]
                        } |> Some
            } |> TestHelpers.ToPlainSelect
        result |> should equal expected