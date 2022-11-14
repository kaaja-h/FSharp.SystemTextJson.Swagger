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
    use sw = StringWriter()
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
        //simple tuple
        [|typeof<int*string>; (10,"sss");JsonSerializerOptions();JsonFSharpOptions()|]
        // simple list
        [|typeof<int list>; [10;20;30];JsonSerializerOptions();JsonFSharpOptions()|]
        // complex map
        [|typeof<Map<int,int>>; Map[(10,20);(30,40)];JsonSerializerOptions();JsonFSharpOptions()|]
        // inpined map
        [|typeof<Map<string,int>>; Map[("xxx",20);("ddd",40)];JsonSerializerOptions();JsonFSharpOptions()|]
        // Record with options
        [|typeof<RecordTest>; {intData=0;intOptionData=Some 1};JsonSerializerOptions();JsonFSharpOptions()|]
        //Option not works because of validator
        //[|typeof<RecordTest>; {intData=0;intOptionData=None};JsonSerializerOptions();JsonFSharpOptions()|]
        
        //simple AdjecentTag
        [| typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag) |]
        [| typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag) |]
        [| typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.AdjacentTag) |]
        // AdjecentTag with UnwrapFieldlessTags
        [| typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        // AdjecentTag with UnwrapSingleCaseUnions
        [| typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleCaseUnions)) |]
        [| typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleCaseUnions)) |]
        [| typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleCaseUnions)) |]
        [| typedefof<BoxedString>;Box("ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleCaseUnions)) |]
        // AdjecentTag with UnwrapSingleFieldCases
        [| typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        // AdjecentTag with NamedFields
        [| typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.NamedFields )) |]
        [| typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.NamedFields )) |]
        [| typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.AdjacentTag |||JsonUnionEncoding.NamedFields )) |]
        
        //simple ExternalTag
        [| typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.ExternalTag) |]
        [| typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.ExternalTag) |]
        [| typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = JsonUnionEncoding.ExternalTag) |]      
        // ExternalTag with UnwrapFieldlessTags
        [| typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        [| typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapFieldlessTags)) |]
        // ExternalTag with UnwrapSingleFieldCases
        [| typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
        [| typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.UnwrapSingleFieldCases)) |]
       // ExternalTag with NamedFields
        [| typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.NamedFields )) |]
        [| typedefof<Example>; WithOneArg(3.14); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.NamedFields )) |]
        [| typedefof<Example>; WithArgs(1,"ssss"); JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.NamedFields )) |]

        

    
    ]


[<Theory>]
[<MemberData(nameof(testData))>]
let validate (typeToSerialize:Type) (data) (jsonOptions:JsonSerializerOptions) (fsOptions: JsonFSharpOptions) =
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
       [| typedefof<Example>; NoArgs; JsonSerializerOptions(); JsonFSharpOptions(unionEncoding = (JsonUnionEncoding.ExternalTag |||JsonUnionEncoding.NamedFields )) |]        
    ]
   
[<Theory>]
[<MemberData(nameof(oneRowTest))>]
let TestSimpleVal (typeToSerialize:Type) (data) (jsonOptions:JsonSerializerOptions) (fsOptions: JsonFSharpOptions)=
   validate  typeToSerialize data jsonOptions fsOptions
  