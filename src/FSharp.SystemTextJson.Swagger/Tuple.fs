module internal FSharp.SystemTextJson.Swagger.Tuple

open System
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.FSharp.Reflection
open Microsoft.OpenApi.Models
open Swashbuckle.AspNetCore.SwaggerGen

let createDataContractTuple (typeToConvert:Type) (options: JsonSerializerOptions) =
    let listType = AbstractSubtypes.abstractTupleType.MakeGenericType(typeToConvert)
    DataContract.ForArray(typeToConvert,listType, Helper.getJsonConverterFunc options )


type TupleSchemaFilter() =
    
    let getReferencedSchema (repo:SchemaRepository) (a:OpenApiSchema) =
        if (a.Reference = null) then
            a
        else
            repo.Schemas[a.Reference.Id]
    
    interface ISchemaFilter with
        member this.Apply(schema, context) =
            if TypeCache.getKind context.Type = TypeCache.TypeKind.Tuple  then
                    let cnt = context.Type |> FSharpType.GetTupleElements |> Array.length
                    schema.MaxItems <- cnt
                    schema.MinItems <- cnt
            if context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() = AbstractSubtypes.abstractTupleType
                && not (schema.OneOf |> Seq.isEmpty )
                && schema.OneOf |> Seq.map (getReferencedSchema context.SchemaRepository) |> Seq.exists (fun c-> c.Type = "string" && c.Enum |> Seq.isEmpty)
                && schema.OneOf |> Seq.map (getReferencedSchema context.SchemaRepository) |> Seq.exists (fun c-> c.Type = "string" && not (c.Enum |> Seq.isEmpty))
                then              
                schema.AnyOf <- schema.OneOf
                schema.OneOf <- new System.Collections.Generic.List<_>()
            if context.Type = typedefof<EmptyArray> then
                    schema.MaxItems <- 0
                    schema.MinItems <- 0