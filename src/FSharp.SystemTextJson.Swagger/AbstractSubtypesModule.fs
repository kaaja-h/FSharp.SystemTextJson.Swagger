module internal FSharp.SystemTextJson.Swagger.AbstractSubtypes

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open System.Text.Json.Serialization
open FSharp.SystemTextJson
open Microsoft.FSharp.Reflection

let abstractTupleType = typedefof<AbstractTupleDefinition<_>>

let abstractUnionType = typedefof<AbstractUnion<_>>

let abstractUnionCase = typedefof<UnionCase<_>>

let recordForUnionCase = typedefof<RecordForUnionCase<_>>


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
       
let (|GenericInherit|_|) (tToCompare: Type) (t: Type) =
   if t.BaseType <> null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition()= tToCompare then
       Some( t.BaseType.GenericTypeArguments)
   else
       None

let getType (ty:Type) =
    match ty with
    | GenericType abstractTupleType [|tupleType|] -> TupleType(tupleType)
    | GenericType abstractUnionType [|unionType|] -> UnionType(unionType)
    | _ -> Other
    


    
let generateEnumForCache moduleName enumName case =
    let aName = new AssemblyName(moduleName)
    let ab = AssemblyBuilder.DefineDynamicAssembly(aName,  AssemblyBuilderAccess.Run )
    let mb = ab.DefineDynamicModule(moduleName)
    let enumBuilder = mb.DefineEnum(enumName, TypeAttributes.Public, typedefof<int32>)
    let converterConstructor = typedefof<JsonConverterAttribute>.GetConstructor([|typeof<Type>|])
    let myCABuilder = new CustomAttributeBuilder(converterConstructor,[|typedefof<JsonStringEnumConverter>|])
    enumBuilder.SetCustomAttribute(myCABuilder)
    enumBuilder.DefineLiteral(case,0)
    enumBuilder.CreateType()



let generateEnum =
    let cache = ConcurrentDictionary<string*string*string, Type>()
    fun moduleName enumName case ->
        cache.GetOrAdd ( (moduleName,enumName, case ),  fun (a,b,c) -> generateEnumForCache a b c  )
        
    
    
let generateCases (case:UnionCaseInfo)  =
    let moduleName = sprintf "%s.%s" case.DeclaringType.Namespace case.DeclaringType.Name 
    let aName = new AssemblyName(moduleName)
    let ab = AssemblyBuilder.DefineDynamicAssembly(aName,  AssemblyBuilderAccess.Run )
    let mb = ab.DefineDynamicModule(moduleName)
    let parentType = abstractUnionCase.MakeGenericType(case.DeclaringType)
    let classBuilder = mb.DefineType(case.Name,TypeAttributes.Public,parentType )
    classBuilder.CreateType()
    