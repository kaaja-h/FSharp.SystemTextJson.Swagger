module internal FSharp.SystemTextJson.Swagger.Tests.TestCommon



open System.Text.Json
open System.Text.Json.Serialization
open FSharp.SystemTextJson.Swagger
open Microsoft.Extensions.DependencyInjection
open Swashbuckle.AspNetCore.SwaggerGen
open System
open Xunit




let prepareGenerator  (jsonOptions:JsonSerializerOptions) (fsOptions: JsonFSharpOptions) (setup: (SwaggerGenOptions -> unit) option ) =
     let options = new SwaggerGenOptions()
     FSharp.SystemTextJson.Swagger.DependencyExtension.SetupDefaultOptions fsOptions setup options
     let resolver = DependencyExtension.PrepareSerializerDataContractResolver jsonOptions fsOptions
     DependencyExtension.PrepareFilters () |> Seq.fold (fun _ c -> options.SchemaGeneratorOptions.SchemaFilters.Add c ) () 
     new SchemaGenerator(options.SchemaGeneratorOptions, resolver )
    

let prepareGeneratorSimple() =
    let fsOptions = new JsonFSharpOptions()
    let jsonOptions= new JsonSerializerOptions()
    prepareGenerator jsonOptions fsOptions None

let generateSimple (t:Type)=
    let generator = prepareGeneratorSimple()
    let schemaRepository = new SchemaRepository()
    let schema = generator.GenerateSchema(t, schemaRepository)
    schema, schemaRepository
    
let generateWithOptions (jsonOptions:JsonSerializerOptions) (fsOptions: JsonFSharpOptions) (t:Type)=
    let generator = prepareGenerator jsonOptions fsOptions None
    let schemaRepository = new SchemaRepository()
    let schema = generator.GenerateSchema(t,schemaRepository)
    schema, schemaRepository