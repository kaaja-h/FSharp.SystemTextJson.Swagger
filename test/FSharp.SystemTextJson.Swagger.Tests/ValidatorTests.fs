module FSharp.SystemTextJson.Swagger.Tests.ValidatorTests

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open FSharp.SystemTextJson.Swagger.Tests.CommonTestTypes
open FSharp.SystemTextJson.Swagger.Tests.Union
open Microsoft.OpenApi.Models
open Microsoft.OpenApi.Writers
open NJsonSchema
open Newtonsoft.Json.Serialization
open Xunit


let private prepareJsonSchema  (schema:OpenApiSchema) =
    use sw = new StringWriter()
    let jw = OpenApiJsonWriter(sw) 
    schema.SerializeAsV3WithoutReference(jw)
    jw.Flush()
    sw.ToString()

let optionsWithCamelCase() =
    let opt = JsonSerializerOptions()
    opt.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
    opt

let testData : obj[] list =
    [
        [| "simple tuple" ;typeof<int*string>; (10,"sss");JsonSerializerOptions();JsonFSharpOptions()|]
        [| "simple list";typeof<int list>; [10;20;30];JsonSerializerOptions();JsonFSharpOptions()|]
        [|"complex map";typeof<Map<int,int>>; Map[(10,20);(30,40)];JsonSerializerOptions();JsonFSharpOptions()|]
        [|"inlined map";typeof<Map<string,int>>; Map[("xxx",20);("ddd",40)];JsonSerializerOptions();JsonFSharpOptions()|]
        [|"Record with options";typeof<RecordTest>; {intData=0;intOptionData=Some 1};JsonSerializerOptions();JsonFSharpOptions()|]
        //Option not works because of validator
        //[|typeof<RecordTest>; {intData=0;intOptionData=None};JsonSerializerOptions();JsonFSharpOptions()|]
        
        //
        [|"simple AdjecentTag NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag) |]
        [|"simple AdjecentTag WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag) |]
        [|"simple AdjecentTag WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag) |]
        // AdjecentTag with custom unionTagName
        [| "AdjecentTag with custom unionTagName NoArgs";typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag, unionTagName = "test") |]
        [| "AdjecentTag with custom unionTagName WithOneArg";typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag, unionTagName = "test") |]
        [| "AdjecentTag with custom unionTagName WithArgs";typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag, unionTagName = "test") |]
        // AdjecentTag with custom unionTagNamingPolicy
        [| "AdjecentTag with custom unionTagNamingPolicy NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag, unionTagNamingPolicy = JsonNamingPolicy.CamelCase) |]
        [| "AdjecentTag with custom unionTagNamingPolicy WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag, unionTagNamingPolicy = JsonNamingPolicy.CamelCase) |]
        [| "AdjecentTag with custom unionTagNamingPolicy WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag, unionTagNamingPolicy = JsonNamingPolicy.CamelCase) |]
        // AdjecentTag with UnwrapFieldlessTags
        [| "AdjecentTag with UnwrapFieldlessTags NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| "AdjecentTag with UnwrapFieldlessTags WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| "AdjecentTag with UnwrapFieldlessTags WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        // AdjecentTag with UnwrapSingleCaseUnions
        [| "AdjecentTag with UnwrapSingleCaseUnions NoArgs";typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleCaseUnions)) |]
        [| "AdjecentTag with UnwrapSingleCaseUnions WithOneArg";typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleCaseUnions)) |]
        [| "AdjecentTag with UnwrapSingleCaseUnions WithArgs";typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleCaseUnions)) |]
        [| "AdjecentTag with UnwrapSingleCaseUnions boxed ";typedefof<BoxedString>;Box("ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleCaseUnions)) |]
        // AdjecentTag with UnwrapSingleFieldCases
        [| "AdjecentTag with UnwrapSingleFieldCases NoArgs";typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| "AdjecentTag with UnwrapSingleFieldCases WithOneArg";typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| "AdjecentTag with UnwrapSingleFieldCases WithArgs";typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        // AdjecentTag with NamedFields
        [| "AdjecentTag with NamedFields NoArgs";typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.NamedFields )) |]
        [| "AdjecentTag with NamedFields WithOneArg";typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.NamedFields )) |]
        [| "AdjecentTag with NamedFields WithArgs";typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.NamedFields )) |]
        // AdjecentTag with NamedFields and unionFieldNamingPolicy
        [| "AdjecentTag with NamedFields and unionFieldNamingPolicy NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.NamedFields ), unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
        [| "AdjecentTag with NamedFields and unionFieldNamingPolicy WithOneArg";typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.NamedFields ), unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
        [| "AdjecentTag with NamedFields and unionFieldNamingPolicy WithArgs";typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.NamedFields ), unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
        
        //simple ExternalTag
        [| "simple ExternalTag NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.ExternalTag) |]
        [| "simple ExternalTag WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.ExternalTag) |]
        [| "simple ExternalTag WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.ExternalTag) |]      
        // ExternalTag with UnwrapFieldlessTags
        [| "ExternalTag with UnwrapFieldlessTags NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| "ExternalTag with UnwrapFieldlessTags WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| "ExternalTag with UnwrapFieldlessTags WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        // ExternalTag with UnwrapSingleFieldCases
        [| "ExternalTag with UnwrapSingleFieldCases NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| "ExternalTag with UnwrapSingleFieldCases WithOneArg";typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| "ExternalTag with UnwrapSingleFieldCases WithArgs";typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        // ExternalTag with NamedFields
        [| "ExternalTag with NamedFields NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.NamedFields )) |]
        [| "ExternalTag with NamedFields WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.NamedFields )) |]
        [| "ExternalTag with NamedFields WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.NamedFields )) |]
        // ExternalTag with NamedFields and custom unionFieldNamingPolicy
        [| "ExternalTag with NamedFields and custom unionFieldNamingPolicy NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.NamedFields ),unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
        [| "ExternalTag with NamedFields and custom unionFieldNamingPolicy WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.NamedFields ),unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
        [| "ExternalTag with NamedFields and custom unionFieldNamingPolicy WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.NamedFields ),unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
        
        // simple InternalTag
        [| "simple InternalTag NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.InternalTag) |]
        [| "simple InternalTag WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.InternalTag) |]
        [| "simple InternalTag WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.InternalTag) |]
        // InternalTag with UnwrapFieldlessTags
        [| "InternalTag with UnwrapFieldlessTags NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| "InternalTag with UnwrapFieldlessTags WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| "InternalTag with UnwrapFieldlessTags WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        // InternalTag with UnwrapSingleFieldCases
        [| "InternalTag with UnwrapSingleFieldCases NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| "InternalTag with UnwrapSingleFieldCases WithOneArg";typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| "InternalTag with UnwrapSingleFieldCases WithArgs";typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        // InternalTag with NamedFields
        [| "InternalTag with NamedFields NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.NamedFields )) |]
        [| "InternalTag with NamedFields WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.NamedFields )) |]
        [| "InternalTag with NamedFields WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.NamedFields )) |]
        // InternalTag with NamedFields and custom unionFieldNamingPolicy
        [| "InternalTag with NamedFields and custom unionFieldNamingPolicy NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.NamedFields ),unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
        [| "InternalTag with NamedFields and custom unionFieldNamingPolicy WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.NamedFields ),unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
        [| "InternalTag with NamedFields and custom unionFieldNamingPolicy WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.InternalTag |||JsonUnionEncoding.NamedFields ),unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
        // simple Untagged
        [| "simple Untagged NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.Untagged) |]
        [| "simple Untagged WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.Untagged) |]
        [| "simple Untagged WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.Untagged) |]
        // Untagged with UnwrapFieldlessTags
        [| "Untagged with UnwrapFieldlessTags NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| "Untagged with UnwrapFieldlessTags WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| "Untagged with UnwrapFieldlessTags WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        // Untagged with UnwrapSingleFieldCases
        [| "Untagged with UnwrapSingleFieldCases NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| "Untagged with UnwrapSingleFieldCases WithOneArg";typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| "Untagged with UnwrapSingleFieldCases WithArgs";typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        // Untagged with NamedFields
        [| "Untagged with NamedFields NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.NamedFields )) |]
        [| "Untagged with NamedFields WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.NamedFields )) |]
        [| "Untagged with NamedFields WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.NamedFields )) |]
        // Untagged with NamedFields and custom unionFieldNamingPolicy
        [| "Untagged with NamedFields and custom unionFieldNamingPolicy NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.NamedFields ),unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
        [| "Untagged with NamedFields and custom unionFieldNamingPolicy WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.NamedFields ),unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
        [| "Untagged with NamedFields and custom unionFieldNamingPolicy WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.Untagged |||JsonUnionEncoding.NamedFields ),unionFieldNamingPolicy = JsonNamingPolicy.CamelCase) |]
    ]


[<Theory>]
[<MemberData(nameof(testData))>]
let validate (testName )(typeToSerialize:Type) (data) (jsonOptions:JsonSerializerOptions) (fsOptions: JsonFSharpOptions) =
    jsonOptions.Converters.Add(JsonFSharpConverter(fsOptions))    
    let schema, repository = TestCommon.generateWithOptions jsonOptions fsOptions typeToSerialize
    let inlineSchema = TestCommon.inlineAllReferencedSchemas repository schema
    let schemaJson = prepareJsonSchema inlineSchema
    let t= JsonSchema.FromJsonAsync(schemaJson)
    t.Wait()
    let schema = t.Result
    let json = JsonSerializer.Serialize(data, jsonOptions)
    let errors = schema.Validate(json)
    Assert.Empty(errors)
    ()


    
let oneRowTest : obj[] list =
    [
// simple Untagged
        [| "simple Untagged NoArgs"; typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.Untagged) |]
        [| "simple Untagged WithOneArg"; typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.Untagged) |]
        [| "simple Untagged WithArgs"; typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.Untagged) |]
]
   
[<Theory>]
[<MemberData(nameof(oneRowTest))>]
let TestSimpleVal name (typeToSerialize:Type) (data) (jsonOptions:JsonSerializerOptions) (fsOptions: JsonFSharpOptions)=
   validate name  typeToSerialize data jsonOptions fsOptions
  