namespace FSharp.SystemTextJson.Swagger

open System
open System.Runtime.CompilerServices
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Http.Json
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Options
open Swashbuckle.AspNetCore.SwaggerGen






type internal FsharpDataContractResolver(options:JsonSerializerOptions, fsOptions:JsonFSharpOptions) =
    
    
    

    let json = JsonSerializerDataContractResolver(options)
    
    let customOptions (t: Type) =
        match t.GetCustomAttributes(typedefof<JsonFSharpConverterAttribute>, false) with
        | [| atr |] ->
            atr :?> IJsonFSharpConverterAttribute
            |> (fun d -> d.Options)
            |> Some
        | _ -> None
    
    interface ISerializerDataContractResolver with
        member this.GetDataContractForType(``type``) =
            let fsOptions =
                customOptions ``type``
                |> Option.defaultValue fsOptions
            let r= json.GetDataContractForType(``type``)
            match TypeCache.getKind ``type`` with
            | TypeCache.TypeKind.Record -> Record.createDataContract ``type`` fsOptions options
            | TypeCache.TypeKind.Union -> Union.createDataContract  ``type`` fsOptions options this
            | TypeCache.TypeKind.List -> Collections.createDataContractList ``type`` options
            | TypeCache.TypeKind.Set -> Collections.createDataContractList ``type`` options
            | TypeCache.TypeKind.Map -> Collections.createDataContractMap ``type`` options
            | TypeCache.TypeKind.Tuple -> Tuple.createDataContractTuple ``type`` options
            | _ ->
                match ``type`` with
                | AbstractSubtypes.GenericType AbstractSubtypes.abstractUnionCase [|unionType;caseType |]   ->
                        Union.createDataContractForCase unionType caseType ``type`` fsOptions options json
                       // json.GetDataContractForType(``type``)
                | _ -> json.GetDataContractForType(``type``)
            

[<Extension>]
type DependencyExtension() =
   
    
    static member SetupDefaultOptions (fsOptions:JsonFSharpOptions) (setup: (SwaggerGenOptions -> unit) option ) (options:SwaggerGenOptions)=
            if (Option.isSome setup) then
                (Option.get setup) options
            options.UseAllOfToExtendReferenceSchemas()
            options.UseOneOfForPolymorphism()
            options.SchemaFilter<Tuple.TupleSchemaFilter>()
            let currentSelector = options.SchemaGeneratorOptions.SubTypesSelector
            options.SchemaGeneratorOptions.SubTypesSelector <- SubtypeSelector.selector fsOptions currentSelector
    
    static member PrepareSerializerDataContractResolver (options:JsonSerializerOptions) (fsOptions:JsonFSharpOptions) : ISerializerDataContractResolver=
        FsharpDataContractResolver(options, fsOptions)
    
    static member PrepareFilters(): seq<ISchemaFilter> =
        seq{
            yield Tuple.TupleSchemaFilter()
        }            
    
    [<Extension>]
    static member AddSwaggerForSystemTextJson (services: IServiceCollection, fsOptions:JsonFSharpOptions ,?setup: SwaggerGenOptions -> unit ) =
        
        services.AddTransient<ISerializerDataContractResolver>(
                fun (s:IServiceProvider)->
                    let options = s.GetRequiredService<IOptions<JsonOptions>>()
                    DependencyExtension.PrepareSerializerDataContractResolver options.Value.SerializerOptions fsOptions
            ) |> ignore
            
        services.AddSwaggerGen(DependencyExtension.SetupDefaultOptions fsOptions setup)      
            
