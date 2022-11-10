module FSharp.SystemTextJson.Swagger.Tests.Union

open Xunit


type ComplexUnionType =
    |EmptyCase
    |EmptyCase2
    |StringCase of (string)
    |StringCase2 of (string)
    |TupleCase of (int*string)
    
[<Fact>]    
let complexUnionTypeTest()=
    let complexUnionType = typedefof<ComplexUnionType>
    let schema,repository = TestCommon.generateSimple complexUnionType
    ()