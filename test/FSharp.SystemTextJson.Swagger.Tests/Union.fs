module FSharp.SystemTextJson.Swagger.Tests.Union

open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.OpenApi.Any
open Microsoft.OpenApi.Models
open Microsoft.OpenApi.Writers
open Xunit


type Example =
    | NoArgs
    | WithOneArg of aFloat: float
    | WithArgs of anInt: int * aString: string
    

let isSingleCaseEnumWithValue (value) (schema:OpenApiSchema)  =
    schema.Enum.Count = 1 &&
    (schema.Enum[0] :?> OpenApiString).Value = value
    
let findAdjacentTagCase (oneOf:seq<OpenApiSchema>) repository value=
    oneOf |> Seq.filter (
            fun s ->
                s.Properties["Case"] |> (TestCommon.getReferencedSchema repository)
                |> isSingleCaseEnumWithValue value       
        ) |> Seq.toArray

[<Fact>]
let AdjacentTag_NoOptions_test()=
    let fsOptions = JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag)
    let jsonOptions = JsonSerializerOptions()

    let schema, repository = TestCommon.generateWithOptions jsonOptions fsOptions typedefof<Example> |>
                                fun (a,b)-> (TestCommon.inlineAllReferencedSchemas  b a),b 
    Assert.NotNull(schema.OneOf)
    Assert.NotEmpty(schema.OneOf)
    let oneOf = schema.OneOf |> Seq.map  (TestCommon.getReferencedSchema repository)
    //all cases should have property Case
    Assert.All( oneOf, fun d-> Assert.True(d.Properties.ContainsKey "Case") )
    // there should be one option without fields property
    let noArgs = findAdjacentTagCase oneOf repository "NoArgs" 
    Assert.NotEmpty noArgs
    Assert.Equal (1, noArgs.Length)
    Assert.Equal (1, noArgs[0].Properties.Count )
    
    let withOneArg = findAdjacentTagCase oneOf repository "WithOneArg"
    Assert.NotEmpty withOneArg
    Assert.Equal (1, withOneArg.Length)
    Assert.Equal (2, withOneArg[0].Properties.Count )
    Assert.True ((withOneArg[0].Properties.ContainsKey) "Fields" )
    Assert.Equal("array",withOneArg[0].Properties["Fields"].Type)
    
    let withArgs = findAdjacentTagCase oneOf repository "WithArgs"
    Assert.NotEmpty withArgs
    Assert.Equal (1, withArgs.Length)
    Assert.Equal (2, withArgs[0].Properties.Count )
    Assert.True ((withArgs[0].Properties.ContainsKey) "Fields" )
    let withArgsFields = withArgs[0].Properties["Fields"]
    Assert.Equal("array",withArgsFields.Type)
  
    ()
    


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