module internal FSharp.SystemTextJson.Swagger.Union

open System
open System.Collections.Generic
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.OpenApi.Models
open Swashbuckle.AspNetCore.SwaggerGen

let internal optionTy = typedefof<option<_>>
let internal voptionTy = typedefof<voption<_>>
let internal skippableTy = typedefof<Skippable<_>>


let internal getWrappedType (typeToConvert:Type) (innerResolver:ISerializerDataContractResolver) =
    let wt = typeToConvert.GetGenericArguments()[0]
    innerResolver.GetDataContractForType(Helper.getGenericType typeToConvert) 

let createDataContract (typeToConvert:Type) (fsOptions: JsonFSharpOptions) (options: JsonSerializerOptions) (innerResolver:ISerializerDataContractResolver) =
    if fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.UnwrapOption
           && typeToConvert.IsGenericType
           && typeToConvert.GetGenericTypeDefinition() = optionTy then
            getWrappedType typeToConvert innerResolver
    elif fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.UnwrapOption
             && typeToConvert.IsGenericType
             && typeToConvert.GetGenericTypeDefinition() = voptionTy then
            getWrappedType typeToConvert innerResolver
    elif typeToConvert.IsGenericType
             && typeToConvert.GetGenericTypeDefinition() = skippableTy then
            getWrappedType typeToConvert innerResolver
    else
            failwith "neumim"
    
    
