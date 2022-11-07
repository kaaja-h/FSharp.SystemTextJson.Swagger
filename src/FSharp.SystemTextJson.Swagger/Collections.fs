module internal FSharp.SystemTextJson.Swagger.Collections

open System
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.FSharp.Reflection
open Swashbuckle.AspNetCore.SwaggerGen

let createDataContractList (typeToConvert:Type) (options: JsonSerializerOptions) =
    let innerType = Helper.getGenericType typeToConvert
    DataContract.ForArray(typeToConvert, innerType, Helper.getJsonConverterFunc options)
 
let isWrappedString (ty: Type) =
        TypeCache.isUnion ty
        && let cases = FSharpType.GetUnionCases(ty, true) in
           cases.Length = 1
           && let fields = cases[ 0 ].GetFields() in
              fields.Length = 1 && fields[0].PropertyType = typeof<string> 
 
let createDataContractMap (typeToConvert:Type)  (options: JsonSerializerOptions) =
    let genArgs = typeToConvert.GetGenericArguments()
    
    if (isWrappedString genArgs[0] || genArgs[0] = typeof<string> ) then
        DataContract.ForDictionary(typeToConvert, genArgs[1], jsonConverter= (Helper.getJsonConverterFunc options) )
    else
        let listType = AbstractSubtypes.makeAbstractGenericTypeArray AbstractSubtypes.abstractMapType genArgs
        DataContract.ForArray(typeToConvert,listType, Helper.getJsonConverterFunc options )
        
  
type MapSchemaFilter() =
    interface ISchemaFilter with
        member this.Apply(schema, context) =
            if AbstractSubtypes.isArrayOfGenericType context.Type AbstractSubtypes.abstractMapType  then
                    schema.MaxItems <- 2
                    schema.MinItems <- 2
            
 