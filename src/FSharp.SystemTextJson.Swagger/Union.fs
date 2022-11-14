module internal FSharp.SystemTextJson.Swagger.Union

open System
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Builder
open Microsoft.FSharp.Reflection
open Microsoft.OpenApi.Models
open Swashbuckle.AspNetCore.SwaggerGen

let internal optionTy = typedefof<option<_>>
let internal voptionTy = typedefof<voption<_>>
let internal skippableTy = typedefof<Skippable<_>>


let internal getWrappedType (typeToConvert:Type) (innerResolver:ISerializerDataContractResolver) =
    let wt = typeToConvert.GetGenericArguments()[0]
    innerResolver.GetDataContractForType(Helper.getGenericType typeToConvert)
    
let isSingleFieldUnionCaseType (typeToConvert:Type) =
    let cases = FSharpType.GetUnionCases typeToConvert
    cases.Length = 1 && cases[0].GetFields().Length = 1

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
    elif fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.UnwrapSingleCaseUnions && isSingleFieldUnionCaseType typeToConvert then
            (((FSharpType.GetUnionCases typeToConvert)[0]).GetFields()[0]).PropertyType |> innerResolver.GetDataContractForType
    else
            let abstratType = AbstractSubtypes.abstractUnionType.MakeGenericType(typeToConvert)
            DataContract.ForObject(abstratType, Seq.empty, jsonConverter = Helper.getJsonConverterFunc options )


let prepareEnumTypeForSingleCases (typeToConvert:Type) value =
    let unionCases = FSharpType.GetUnionCases typeToConvert
    let enumItems = [value]        
    AbstractSubtypes.generateEnum (sprintf "%s.%s"  typeToConvert.Namespace typeToConvert.Name) (value+"Enum") enumItems
    
   
let getVirtualSubtypes (typeToConvert:Type) (fsOptions: JsonFSharpOptions) =
    
    let unionCases = FSharpType.GetUnionCases typeToConvert
    let subtypes = unionCases |>Seq.map (
           fun case-> 
                       if (fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.UnwrapFieldlessTags && case.GetFields() |> Seq.isEmpty ) then
                           prepareEnumTypeForSingleCases typeToConvert case.Name                           
                       else 
                           AbstractSubtypes.generateCases case                           
            )
    subtypes
    
    



let getUnionCaseInnerFields (typeToConvert:Type) (typeCase:Type) =
    FSharpType.GetUnionCases typeToConvert |> Seq.filter ( fun c-> c.Name = typeCase.Name) |> Seq.head |> fun c-> c.GetFields() 
    

let createDataContractForAdjacentTag (typeToConvert:Type) (typeCase:Type) (fsOptions: JsonFSharpOptions) (options: JsonSerializerOptions) (innerResolver:ISerializerDataContractResolver) =
    let enumType = prepareEnumTypeForSingleCases typeToConvert typeCase.Name
    let caseNameDataProperty = DataProperty(fsOptions.UnionTagName, enumType, true, false, false, false, null )
    
    let fields = getUnionCaseInnerFields typeToConvert typeCase |> Seq.map (fun f -> f.PropertyType)|> Seq.toArray
    let dataContractFields = if fields.Length = 0 then
                                 seq{yield caseNameDataProperty}
                             elif fields.Length = 1 && (
                                    (fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.UnwrapSingleFieldCases)
                                    || (fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.UnwrapRecordCases &&  FSharpType.IsRecord fields[0])
                                    )then
                                 let fieldProperty = DataProperty(fsOptions.UnionFieldsName, fields[0], true, false,false,false,null)
                                 seq{
                                     yield caseNameDataProperty
                                     yield fieldProperty
                                 }
                             
                                 
                             else
                                 let dataType =   
                                     if fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.NamedFields then
                                         typedefof<RecordForUnionCase<_>>.MakeGenericType(typeCase)    
                                     else
                                         FSharpType.MakeTupleType(fields )
                                 let dataProperty = DataProperty(fsOptions.UnionFieldsName, dataType, true, false,false,false,null)
                                 seq{
                                     yield caseNameDataProperty
                                     yield dataProperty
                                     }
    DataContract.ForObject(typeCase, dataContractFields, jsonConverter = Helper.getJsonConverterFunc options)
    

let createDataContractForExternalTag (typeToConvert:Type) (typeCase:Type) (fsOptions: JsonFSharpOptions) (options: JsonSerializerOptions) (innerResolver:ISerializerDataContractResolver) =
    let fields = getUnionCaseInnerFields typeToConvert typeCase |> Seq.map (fun f -> f.PropertyType)|> Seq.toArray
    let dataContractFieldType =
        if fields.Length = 1 && fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.UnwrapSingleFieldCases then
            fields[0]
        elif  fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.NamedFields then
            typedefof<RecordForUnionCase<_>>.MakeGenericType(typeCase)
        elif fields.Length = 0 then
            typedefof<EmptyArray>            
        else
            FSharpType.MakeTupleType(fields )
    let dataProperty = DataProperty(typeCase.Name, dataContractFieldType, true, false,false,false,null)                
    DataContract.ForObject(typeCase, [|dataProperty|], jsonConverter = Helper.getJsonConverterFunc options)            
    
    
let createDataContractForCase (typeToConvert:Type) (typeCase:Type) (fsOptions: JsonFSharpOptions) (options: JsonSerializerOptions) (innerResolver:ISerializerDataContractResolver) =
     
    if fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.AdjacentTag then
        createDataContractForAdjacentTag typeToConvert typeCase fsOptions options innerResolver
    elif fsOptions.UnionEncoding.HasFlag JsonUnionEncoding.ExternalTag then
        createDataContractForExternalTag typeToConvert typeCase fsOptions options innerResolver
    else
        failwith "unknown encoding"
    
    
let createDataContractForRecordCase (typeToConvert:Type) (typeCase:Type) (recordType:Type) (fsOptions: JsonFSharpOptions) (options: JsonSerializerOptions) (innerResolver:ISerializerDataContractResolver) =
    let uci = typeToConvert |> FSharpType.GetUnionCases |> Seq.find( fun c -> c.Name = typeCase.Name)
    let fieldNames = Helper.getJsonFieldNames uci.GetCustomAttributes
    let fields =
                let fields = uci.GetFields()
                let usedFieldNames = Dictionary()
                let fieldsAndNames =
                    if fsOptions.UnionEncoding.HasFlag(JsonUnionEncoding.UnionFieldNamesFromTypes) then
                        fields
                        |> Array.mapi (fun i p ->
                            let useTypeName =
                                if i = 0 && fields.Length = 1 then
                                    p.Name = "Item"
                                else
                                    p.Name = "Item" + string (i + 1)
                            let name = if useTypeName then p.PropertyType.Name else p.Name
                            let nameIndex =
                                match usedFieldNames.TryGetValue(name) with
                                | true, ix -> ix + 1
                                | false, _ -> 1
                            usedFieldNames[name] <- nameIndex
                            p, name, nameIndex
                        )
                    else
                        fields |> Array.map (fun p -> p, p.Name, 1)
                fieldsAndNames
                |> Array.map (fun (p, name, nameIndex) ->
                    let name =
                        let mutable nameCount = 1
                        if
                            nameIndex = 1
                            && not (usedFieldNames.TryGetValue(name, &nameCount) && nameCount > 1)
                        then
                            name
                        else
                            name + string nameIndex
                    let canBeSkipped = Helper.ignoreNullValues options || Helper.isSkippableType p.PropertyType
                    let names =
                        match fieldNames.TryGetValue(name) with
                        | true, names -> names |> Array.map (fun n -> n.AsString())
                        | false, _ ->
                            let policy =
                                match fsOptions.UnionFieldNamingPolicy with
                                | null -> options.PropertyNamingPolicy
                                | policy -> policy
                            [| Helper.convertName policy name |]
                    {| Type = p.PropertyType
                       Names = names
                       Nullable = Helper.isNullable p.PropertyType
                       Required = not canBeSkipped
                       IsSkip = Helper.isSkip p.PropertyType
                       PropetyInfo = p
                        |}
                )  
    let properties = fields |> Seq.map (fun f -> DataProperty(f.Names[0], f.Type, f.Required, f.Nullable, false, false, f.PropetyInfo ))
    DataContract.ForObject( recordType, properties)
      

    