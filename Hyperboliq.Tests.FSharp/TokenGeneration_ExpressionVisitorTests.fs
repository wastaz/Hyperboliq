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
