module internal FSharp.SystemTextJson.Swagger.Tests.TestCommon



open System.Collections.Generic
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open FSharp.SystemTextJson.Swagger
open Microsoft.Extensions.DependencyInjection
open Microsoft.OpenApi.Models
open Microsoft.OpenApi.Writers
open NJsonSchema
open Swashbuckle.AspNetCore.SwaggerGen
open System
open Xunit




let prepareGenerator  (jsonOptions:JsonSerializerOptions) (fsOptions: JsonFSharpOptions) (setup: (SwaggerGenOptions -> unit) option ) =
     let options = new SwaggerGenOptions()
     FSharp.SystemTextJson.Swagger.DependencyExtension.SetupDefaultOptions fsOptions setup options
     let resolver = DependencyExtension.PrepareSerializerDataContractResolver jsonOptions fsOptions
     DependencyExtension.PrepareFilters (fsOptions) |> Seq.fold (fun _ c -> options.SchemaGeneratorOptions.SchemaFilters.Add c ) () 
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
    
let getReferencedSchema (repository:SchemaRepository) (schema:OpenApiSchema) =
    if (schema.Reference = null) then
        schema
    else
        repository.Schemas[schema.Reference.Id]
        
let rec inlineAllReferencedSchemas (repository:SchemaRepository) (schema:OpenApiSchema) =
    let schemaInner = getReferencedSchema repository schema
    let map = (getReferencedSchema repository) >>  (inlineAllReferencedSchemas repository )
    let mapList l =
        if l=null then
            null
        else
            l |> Seq.map map |> fun c -> List<OpenApiSchema>(c)
    schemaInner.AllOf <- schemaInner.AllOf |> mapList  
    schemaInner.OneOf <- schemaInner.OneOf |> mapList        
    schemaInner.AnyOf <- schemaInner.AnyOf |> mapList
    if schemaInner.Not <> null then
        schemaInner.Not <- map schemaInner.Not
    if schemaInner.Items <> null then
        schemaInner.Items <- map schemaInner.Items
    if schemaInner.AdditionalProperties <> null then
        schemaInner.AdditionalProperties <- map schemaInner.AdditionalProperties        
    if schemaInner.Properties <> null then
        let res = new Dictionary<string,OpenApiSchema>()
        schemaInner.Properties <- schemaInner.Properties |> Seq.fold (fun d i -> d.Add(i.Key, map i.Value); d ) res
    schemaInner    
    
 



    