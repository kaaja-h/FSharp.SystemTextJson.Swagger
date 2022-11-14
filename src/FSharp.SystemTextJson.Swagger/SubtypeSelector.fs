module internal FSharp.SystemTextJson.Swagger.SubtypeSelector

open System
open System.Collections.Generic
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.FSharp.Reflection


let selector (fsOptions: JsonFSharpOptions) (defaultSelector:Func<Type, IEnumerable<Type>>) (typeToConvert:Type) :IEnumerable<Type> =
    match AbstractSubtypes.getType  typeToConvert with
    | TupleType(tupleType) ->
        FSharpType.GetTupleElements(tupleType) |> Seq.distinct
    | UnionType(unionType) -> Union.getVirtualSubtypes(unionType)  (fsOptions |> Helper.getEffectiveFsOptions unionType)      
    | _ ->
        defaultSelector.Invoke(typeToConvert)
