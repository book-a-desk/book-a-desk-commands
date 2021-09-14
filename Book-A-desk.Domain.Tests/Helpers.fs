module Book_A_desk.Domain.Helpers.Tests

open Xunit

module Helpers = 
    let shouldEqual<'T> (expected: 'T) (actual: 'T) =
        Assert.Equal(expected, actual)

    let shouldBeOkAndEqualTo (expected: Result<_, _>) (actual: Result<_, _>) =
        match expected, actual with
        | Ok expectedOk , Ok actualOk -> actualOk |> shouldEqual expectedOk
        | _ , _  ->
            let errorMessage = sprintf "Expected inputs to be Ok but:\n Expected: %A\n Actual: %A" expected actual
            Assert.True(false, errorMessage)    
    
    let shouldBeErrorAndEqualTo (expected: Result<_, _>) (actual: Result<_, _>) =
        match expected, actual with
        | Error expectedError , Error actualError -> actualError |> shouldEqual expectedError
        | _ , _  ->
            let errorMessage = sprintf "Expected inputs to be Error but:\n Expected: %A\n Actual: %A" expected actual
            Assert.True(false, errorMessage)