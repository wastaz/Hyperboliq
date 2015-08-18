namespace Hyperboliq.Tests.TokenGeneration

module TokenGeneration_ExpressionVisitorTests =
    open NUnit.Framework
    open FsUnit
    open TestModel
    open Hyperboliq
    open Hyperboliq.Domain
    open Hyperboliq.Domain.ExpressionVisitor
    open Hyperboliq.Domain.AST

    [<Test>]
    let ``It can visit a simple column selector`` () =
        let expr = <@@ fun (p : Person) -> p.Age @@>
        let result = ExpressionVisitor.Visit (Quotation(expr)) [ Types.TableReferenceFromType<Person> ]
        let expected = ValueNode.Column("Age", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference) |> Some
        result |> should equal expected

    [<Test>]
    let ``It can visit a multiple column selector`` () =
        let expr = <@@ fun (p : Person) -> p.Age, p.Name @@>
        let result = ExpressionVisitor.Visit (Quotation(expr)) [ Types.TableReferenceFromType<Person> ]
        let expected = 
            ValueNode.ValueList(
                [ ValueNode.Column("Age", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)
                  ValueNode.Column("Name", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)
                ]) |> Some
        result |> should equal expected

    [<Test>]
    let ``It should be able to generate aliased column token`` () =
        let expr = <@@ fun (p : Person) -> let old = p.Age in (p.Name, old) @@>
        let result = ExpressionVisitor.Visit (Quotation(expr)) [ Types.TableReferenceFromType<Person> ]
        let expected = 
            ValueNode.ValueList(
                [ ValueNode.Column("Name", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)
                  ValueNode.NamedColumn({ Alias = "old"; Column = ValueNode.Column("Age", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)})
                ]) |> Some
        result |> should equal expected

    [<Test>]
    let ``It should be able to generate several aliased column tokens`` () =
        let expr = <@@ fun (p : Person) -> 
                            let old = p.Age
                            let callsign = p.Name
                            (p.Id, callsign, old) @@>
        let result = ExpressionVisitor.Visit (Quotation(expr)) [ Types.TableReferenceFromType<Person> ]
        let expected =
            ValueNode.ValueList(
                [ ValueNode.Column("Id", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)
                  ValueNode.NamedColumn({ Alias = "callsign"; Column = ValueNode.Column("Name", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)})
                  ValueNode.NamedColumn({ Alias = "old"; Column = ValueNode.Column("Age", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)})
                ]) |> Some
        result |> should equal expected

    [<Test>]
    let ``It should be able to use tuple deconstruction to generate several aliased column tokens`` () =
        let expr = <@@ fun (p : Person) -> let (old, callsign) = (p.Age, p.Name) in (p.Id, callsign, old) @@>
        let result = ExpressionVisitor.Visit (Quotation(expr)) [ Types.TableReferenceFromType<Person> ]
        let expected =
            ValueNode.ValueList(
                [ ValueNode.Column("Id", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)
                  ValueNode.NamedColumn({ Alias = "callsign"; Column = ValueNode.Column("Name", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)})
                  ValueNode.NamedColumn({ Alias = "old"; Column = ValueNode.Column("Age", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)})
                ]) |> Some
        result |> should equal expected

    [<Test>]
    let ``It should be able to generate aliases for constants`` () =
        let expr = <@@ fun (p : Person) -> let numberOfMonkeys = 42 in (p.Name, numberOfMonkeys) @@>
        let result = ExpressionVisitor.Visit (Quotation(expr)) [ Types.TableReferenceFromType<Person> ]
        let expected =
            ValueNode.ValueList(
                [ ValueNode.Column("Name", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)
                  ValueNode.NamedColumn({ Alias = "numberOfMonkeys"; Column = ValueNode.Constant(ConstantNode("42")) })
                ]) |> Some
        result |> should equal expected

    [<Test>]
    let ``It should be able to select a value from another variable in scope`` () =
        let foo = 42
        let expr = <@@ fun (p : Person) -> let magicNumber = foo in (p.Name, magicNumber) @@>
        let result = ExpressionVisitor.Visit (Quotation(expr)) [ Types.TableReferenceFromType<Person> ]
        let expected = 
            ValueNode.ValueList(
                [ ValueNode.Column("Name", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)
                  ValueNode.NamedColumn({ Alias = "magicNumber"; Column = ValueNode.Constant(ConstantNode("42")) })
                ]) |> Some
        result |> should equal expected

    type SillyTestType = {
        Foo : int
        Bar : string
    }

    [<Test>]
    let ``It should be able to select a value from another object in scope`` () =
        let foo = { Foo = 42; Bar = "Banana" }
        let expr = <@@ fun (p : Person) -> let magicNumber = foo.Foo in (p.Name, magicNumber) @@>
        let result = ExpressionVisitor.Visit (Quotation(expr)) [ Types.TableReferenceFromType<Person> ]
        let expected = 
            ValueNode.ValueList(
                [ ValueNode.Column("Name", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)
                  ValueNode.NamedColumn({ Alias = "magicNumber"; Column = ValueNode.Constant(ConstantNode("42")) })
                ]) |> Some
        result |> should equal expected

    let ComparisonTestCases = [
        [| <@@ fun (p : Person) -> p.Age = 42 @@> :> obj; BinaryOperation.Equal :> obj |]
        [| <@@ fun (p : Person) -> p.Age <> 42 @@> :> obj; BinaryOperation.NotEqual:> obj |]
        [| <@@ fun (p : Person) -> p.Age > 42 @@> :> obj; BinaryOperation.GreaterThan :> obj |]
        [| <@@ fun (p : Person) -> p.Age >= 42 @@> :> obj; BinaryOperation.GreaterThanOrEqual :> obj |]
        [| <@@ fun (p : Person) -> p.Age < 42 @@> :> obj; BinaryOperation.LessThan :> obj |]
        [| <@@ fun (p : Person) -> p.Age <= 42 @@> :> obj; BinaryOperation.LessThanOrEqual :> obj |]
    ]

    [<Test>]
    [<TestCaseSource("ComparisonTestCases")>]
    let ``It should be able to visit a binary comparison expression`` expr op =
        let result = ExpressionVisitor.Visit (Quotation(expr)) [ Types.TableReferenceFromType<Person> ]
        let expected = 
            ValueNode.BinaryExpression(
                { Operation = op
                  Lhs = ValueNode.Column("Age", typeof<Person>, Types.TableReferenceFromType<Person> :> ITableReference)
                  Rhs = ValueNode.Constant(ConstantNode("42")) 
                }) |> Some
        result |> should equal expected
