module internal FSharp.SystemTextJson.Swagger.Union

open System
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.FSharp.Reflection
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
            let abstratType = AbstractSubtypes.abstractUnionType.MakeGenericType(typeToConvert)
            DataContract.ForObject(abstratType, Seq.empty, jsonConverter = Helper.getJsonConverterFunc options )
    
   
let getVirtualSubtypes (typeToConvert:Type) =
    let nestedTypes = typeToConvert.GetNestedTypes()
    let unionCases = FSharpType.GetUnionCases typeToConvert
    let caseNames =  unionCases |> Seq.map ( fun c -> c.Name ) |> Set
    let emptyCases = unionCases |> Seq.filter (fun case -> case.GetFields() |> Seq.isEmpty)    
    
    let append = if (emptyCases |> Seq.isEmpty) then Seq.empty else seq{ yield AbstractSubtypes.abstractEmptyCase.MakeGenericType(typeToConvert) }
    
    let types = nestedTypes |> Seq.filter (fun t->  caseNames.Contains t.Name) |> Seq.append append
    types |> Seq.map ( fun c -> AbstractSubtypes.abstractUnionCase.MakeGenericType([|typeToConvert;c|]))
    
    

let assemblyBuilder = lazy(
        
    )    
    
let prepareEnumTypeForCases (typeToConvert:Type) (typeCase:Type) =
    let unionCases = FSharpType.GetUnionCases typeToConvert
    let enumItems =
        match typeCase with
        | AbstractSubtypes.GenericType AbstractSubtypes.abstractEmptyCase _ ->
            unionCases |> Seq.filter (fun c -> c.GetFields() |> Seq.isEmpty) |> Seq.map (fun c-> c.Name) 
        | _ ->
            seq{yield typeCase.Name}
                
    AbstractSubtypes.generateEnum (sprintf "%s.%s"  typeToConvert.Namespace typeToConvert.Name) (typeCase.Name+"Enum") enumItems

let getUnionCaseInnerFields (typeToConvert:Type) (typeCase:Type) =
    let unionCases = FSharpType.GetUnionCases typeToConvert
    match typeCase with
        | AbstractSubtypes.GenericType AbstractSubtypes.abstractEmptyCase _ ->
            Seq.empty 
        | _ ->
            unionCases |> Seq.filter (fun c -> c.Name = typeCase.Name) |> Seq.head |> fun c -> c.GetFields() 

let createDataContractForAdjacentTag (typeToConvert:Type) (typeCase:Type) (masterType:Type) (fsOptions: JsonFSharpOptions) (options: JsonSerializerOptions) (innerResolver:ISerializerDataContractResolver) =
    let enumType = prepareEnumTypeForCases typeToConvert typeCase
    let caseNameDataProperty = DataProperty(fsOptions.UnionTagName, enumType, true, false, false, false, null )
    
    let fields = getUnionCaseInnerFields typeToConvert typeCase |> Seq.map (fun f -> f.PropertyType)
    let dataContractFields = if fields |> Seq.isEmpty then
                                 seq{yield caseNameDataProperty}
                             else
                                 let toupleType = FSharpType.MakeTupleType(fields |> Seq.toArray)
                                 let toupleDataProperty = DataProperty(fsOptions.UnionFieldsName, toupleType, true, false,false,false,null)
                                 seq{
                                     yield caseNameDataProperty
                                     yield toupleDataProperty
                                     }
                                 
    
    DataContract.ForObject(masterType, dataContractFields, jsonConverter = Helper.getJsonConverterFunc options)
    
    
    
    
let createDataContractForCase (typeToConvert:Type) (typeCase:Type) (masterType:Type) (fsOptions: JsonFSharpOptions) (options: JsonSerializerOptions) (innerResolver:ISerializerDataContractResolver) =
     
    if fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.AdjacentTag then
        createDataContractForAdjacentTag typeToConvert typeCase masterType fsOptions options innerResolver
    else
        failwith "unknown encoding"
    
    
    


    