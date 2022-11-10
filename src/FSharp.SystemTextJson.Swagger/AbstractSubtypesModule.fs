module internal FSharp.SystemTextJson.Swagger.AbstractSubtypes

open System
open System.Reflection
open System.Reflection.Emit
open System.Text.Json.Serialization
open FSharp.SystemTextJson

let abstractMapType = typedefof<AbstractMapDefinition<_, _>>

let abstractTupleType = typedefof<AbstractTupleDefinition<_>>

let abstractUnionType = typedefof<AbstractUnion<_>>

let abstractUnionCase = typedefof<AbstractUnionCase<_,_>>
let abstractEmptyCase = typedefof<AbstractEmptyCase<_>>

let makeAbstractGenericTypeArray (tGeneric:Type) (tInner:Type[]) =
    let abstractType = tGeneric.MakeGenericType( tInner )
    abstractType.MakeArrayType()
    
let isArrayOfGenericType (t:Type) (tGeneric:Type) =
    t.IsArray && 
           let elememtType = t.GetElementType()
           elememtType.IsGenericType  && elememtType.GetGenericTypeDefinition() = tGeneric

let (|GenericType|_|) (tToCompare) (t: Type) =
   if t.IsGenericType && t.GetGenericTypeDefinition() = tToCompare then
       Some( t.GenericTypeArguments)
   else
       None

let getType (ty:Type) =
    match ty with
    | GenericType abstractMapType [|keyType;valType|] -> MapType(keyType,valType)
    | GenericType abstractTupleType [|tupleType|] -> TupleType(tupleType)
    | GenericType abstractUnionType [|unionType|] -> UnionType(unionType)
    | _ -> Other
    
    
let generateEnum moduleName enumName cases =
    let aName = new AssemblyName(moduleName)
    let ab = AssemblyBuilder.DefineDynamicAssembly(aName,  AssemblyBuilderAccess.Run )
    let mb = ab.DefineDynamicModule(moduleName)
    let enumBuilder = mb.DefineEnum(enumName, TypeAttributes.Public, typedefof<int32>)
    let converterConstructor = typedefof<JsonConverterAttribute>.GetConstructor([|typeof<Type>|])
    let myCABuilder = new CustomAttributeBuilder(converterConstructor,[|typedefof<JsonStringEnumConverter>|])
    enumBuilder.SetCustomAttribute(myCABuilder)
    
    cases |> Seq.fold (fun i c ->
                        enumBuilder.DefineLiteral(c,i) |> ignore
                        i + 1 ) 0 |> ignore
    enumBuilder.CreateType()    