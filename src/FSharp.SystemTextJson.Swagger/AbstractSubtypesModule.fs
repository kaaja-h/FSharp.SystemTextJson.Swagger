module internal FSharp.SystemTextJson.Swagger.AbstractSubtypes

open System
open System.Collections.Concurrent
open System.Reflection
open System.Reflection.Emit
open System.Text.Json.Serialization
open Microsoft.FSharp.Reflection

let abstractTupleType = typedefof<AbstractTupleDefinition<_>>

let abstractUnionType = typedefof<AbstractUnion<_>>

let abstractUnionCase = typedefof<UnionCase<_>>

let recordForUnionCase = typedefof<RecordForUnionCase<_>>


let makeAbstractGenericTypeArray (tGeneric: Type) (tInner: Type[]) =
    let abstractType = tGeneric.MakeGenericType(tInner)
    abstractType.MakeArrayType()

let isArrayOfGenericType (t: Type) (tGeneric: Type) =
    t.IsArray
    && let elememtType = t.GetElementType() in
       elememtType.IsGenericType && elememtType.GetGenericTypeDefinition() = tGeneric

let (|GenericType|_|) (tToCompare) (t: Type) =
    if t.IsGenericType && t.GetGenericTypeDefinition() = tToCompare then
        Some(t.GenericTypeArguments)
    else
        None

let (|GenericInherit|_|) (tToCompare: Type) (t: Type) =
    if
        t.BaseType <> null
        && t.BaseType.IsGenericType
        && t.BaseType.GetGenericTypeDefinition() = tToCompare
    then
        Some(t.BaseType.GenericTypeArguments)
    else
        None

let getType (ty: Type) =
    match ty with
    | GenericType abstractTupleType [| tupleType |] -> TupleType(tupleType)
    | GenericType abstractUnionType [| unionType |] -> UnionType(unionType)
    | _ -> Other




let generateEnumForCache moduleName enumName case =
    let aName = new AssemblyName(moduleName)
    let ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run)
    let mb = ab.DefineDynamicModule(moduleName)

    let eb =
        mb.DefineType(
            enumName,
            TypeAttributes.AutoLayout ||| TypeAttributes.AnsiClass ||| TypeAttributes.Sealed,
            typeof<System.Enum>,
            PackingSize.Unspecified,
            TypeBuilder.UnspecifiedTypeSize
        )

    let converterConstructor =
        typedefof<JsonConverterAttribute>.GetConstructor ([| typeof<Type> |])

    let myCABuilder =
        new CustomAttributeBuilder(converterConstructor, [| typedefof<JsonStringEnumConverter> |])

    eb.SetCustomAttribute(myCABuilder)

    eb.DefineField(
        "value__",
        typedefof<int32>,
        FieldAttributes.Public
        ||| FieldAttributes.SpecialName
        ||| FieldAttributes.RTSpecialName
    )
    |> ignore

    let f =
        eb.DefineField(case, eb, FieldAttributes.Static ||| FieldAttributes.Public ||| FieldAttributes.Literal)

    f.SetConstant(0)
    eb.CreateType()



let generateEnum =
    let cache = ConcurrentDictionary<string * string * string, Type>()

    fun moduleName enumName case ->
        cache.GetOrAdd((moduleName, enumName, case), (fun (a, b, c) -> generateEnumForCache a b c))



let private generateCasesForCache (case: UnionCaseInfo) =

    let moduleName =
        sprintf "%s.%s" case.DeclaringType.Namespace case.DeclaringType.Name

    let aName = new AssemblyName(moduleName)
    let ab = AssemblyBuilder.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run)
    let mb = ab.DefineDynamicModule(moduleName)
    let parentType = abstractUnionCase.MakeGenericType(case.DeclaringType)
    let classBuilder = mb.DefineType(case.Name, TypeAttributes.Public, parentType)
    classBuilder.CreateType()

let generateCases =
    let cache = ConcurrentDictionary<UnionCaseInfo, Type>()
    fun u -> cache.GetOrAdd(u, generateCasesForCache)
