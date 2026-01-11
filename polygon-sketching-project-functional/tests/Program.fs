open FsCheck
open Expecto


module Sorting =
    let sorted xs = 
        xs 
        |> List.pairwise 
        |> List.forall (fun (p0, p1) -> p0 <= p1)

    let canSort (xs : list<int>) = 
        xs |> List.sort |> sorted

    
let tests =
    testList "Main" [
        PolygonDrawing.Tests.polygonEditingTests
        test "Hello World" {
            let subject = "Hello World"
            Expect.equal subject "Hello World" "The strings should equal"
        }
        test "Sorting" {
            FsCheck.Check.QuickThrowOnFailure Sorting.canSort
        }
    ]


[<EntryPoint>]
let main args =
    let a = [CLIArguments.Debug; CLIArguments.Sequenced]
    runTestsWithCLIArgs a args tests