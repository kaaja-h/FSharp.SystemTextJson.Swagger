module internal FSharp.SystemTextJson.Swagger.Tuple

open System
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.FSharp.Reflection
open Swashbuckle.AspNetCore.SwaggerGen

let createDataContractTuple (typeToConvert:Type) (options: JsonSerializerOptions) =
    let listType = AbstractSubtypes.abstractTupleType.MakeGenericType(typeToConvert)
    DataContract.ForArray(typeToConvert,listType, Helper.getJsonConverterFunc options )


type TupleSchemaFilter() =
    interface ISchemaFilter with
        member this.Apply(schema, context) =
            if TypeCache.getKind context.Type = TypeCache.TypeKind.Tuple  then
                    let cnt = context.Type |> FSharpType.GetTupleElements |> Array.length
                    schema.MaxItems <- cnt
                    schema.MinItems <- cnt
            if context.Type = typedefof<EmptyArray> then
                    schema.MaxItems <- 0
                    schema.MinItems <- 0