﻿module internal FSharp.SystemTextJson.Swagger.Helper

open System
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.FSharp.Reflection
open Microsoft.OpenApi.Models


let failf format =
    Printf.kprintf (JsonException >> raise) format

let getJsonNames kind (getAttributes: Type -> obj []) =
    match getAttributes typeof<JsonNameAttribute>
          |> Array.choose (function
              | :? JsonNameAttribute as attr when isNull attr.Field -> Some attr
              | _ -> None)
        with
    | [||] ->
        match getAttributes typeof<JsonPropertyNameAttribute> with
        | [| :? JsonPropertyNameAttribute as attr |] -> ValueSome [| JsonName.String attr.Name |]
        | _ -> ValueNone
    | [| attr |] -> ValueSome attr.AllNames
    | _ ->
        failf "To provide multiple names for the same %s, use a single JsonNameAttribute with multiple arguments" kind


let convertName (policy: JsonNamingPolicy) (name: string) =
    match policy with
    | null -> name
    | policy -> policy.ConvertName(name)


let isSkippableType (ty: Type) =
    ty.IsGenericType
    && ty.GetGenericTypeDefinition() = typedefof<Skippable<_>>


let isSkip (ty: Type) =
    if isSkippableType ty then
        let getTag =
            FSharpValue.PreComputeUnionTagReader(ty)

        fun x -> getTag x = 0
    else
        fun _ -> false

let ignoreNullValues (options: JsonSerializerOptions) =
    options.IgnoreNullValues
    || options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull

let isNullable(ty: Type) =
    ty.IsGenericType && (ty.GetGenericTypeDefinition() = typedefof<Option<_>> || ty.GetGenericTypeDefinition() = typedefof<ValueOption<_>>)
    
    
let getJsonConverterFunc (serializerOptions:JsonSerializerOptions) =
    fun value -> JsonSerializer.Serialize(value, serializerOptions)
    
let internal getGenericType (typeToConvert:Type)  =
    typeToConvert.GetGenericArguments()[0]
    