module internal FSharp.SystemTextJson.Swagger.AbstractSubtypes

open System
open FSharp.SystemTextJson

let abstractMapType = typedefof<AbstractMapDefinition<_, _>>


let abstractTupleType = typedefof<AbstractTupleDefinition<_>>

let makeAbstractGenericTypeArray (tGeneric:Type) (tInner:Type[]) =
    let abstractType = tGeneric.MakeGenericType( tInner )
    abstractType.MakeArrayType()
    
let isArrayOfGenericType (t:Type) (tGeneric:Type) =
    t.IsArray && 
           let elememtType = t.GetElementType()
           elememtType.IsGenericType  && elememtType.GetGenericTypeDefinition() = tGeneric

let getType (ty:Type) =
    if ty.IsGenericType && ty.GetGenericTypeDefinition() =  abstractMapType then
        let gen = ty.GetGenericArguments()
        MapType(gen[0],gen[1])
    elif (ty.IsGenericType && ty.GetGenericTypeDefinition() = abstractTupleType ) then
        let gen = ty.GetGenericArguments()
        TupleType(gen[0])
    else
        Other