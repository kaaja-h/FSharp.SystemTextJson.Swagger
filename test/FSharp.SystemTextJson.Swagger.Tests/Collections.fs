module FSharp.SystemTextJson.Swagger.Tests.Collections

open System.Text.Json
open System.Text.Json.Serialization
open FSharp.SystemTextJson.Swagger.Tests.CommonTestTypes
open Xunit




[<Fact>]    
let testIntList()=
    
    let listType = typeof<int list>
    let schema,_ = TestCommon.generateSimple listType 
    Assert.Equal("array",schema.Type)
    Assert.NotNull(schema.Items)
    Assert.Equal("integer", schema.Items.Type)
    Assert.Equal("int32",schema.Items.Format)
    ()
    
[<Fact>]    
let testIntSet()=
    let setType = typeof<Set<int>>
    let schema,_ = TestCommon.generateSimple setType 
    Assert.Equal("array",schema.Type)
    Assert.NotNull(schema.Items)
    Assert.Equal("integer", schema.Items.Type)
    Assert.Equal("int32",schema.Items.Format)
    ()
    
    
[<Fact>]    
let stringmapTest() =
    let stringmapType = typeof<Map<string,int>>
    let schema,_ = TestCommon.generateSimple stringmapType
    Assert.Equal("object",schema.Type)
    Assert.Equal(true,schema.AdditionalPropertiesAllowed)
    Assert.Equal("integer", schema.AdditionalProperties.Type)
    Assert.Equal("int32",schema.AdditionalProperties.Format)
    ()

[<Fact>]   
let boxedstringmapTest() =
    let boxedstringMapType = typeof<Map<BoxedString,int>>
    let schema,_ = TestCommon.generateSimple boxedstringMapType
    Assert.Equal("object",schema.Type)
    Assert.Equal(true,schema.AdditionalPropertiesAllowed)
    Assert.Equal("integer", schema.AdditionalProperties.Type)
    Assert.Equal("int32",schema.AdditionalProperties.Format)
    ()
   
[<Fact>]    
let nonstringMapTest()=
    let nonStringMap = typeof<Map<int,string>>
    let schema,_ = TestCommon.generateSimple nonStringMap
    Assert.Equal("array",schema.Type)
    Assert.NotNull(schema.Items)
    let itemType = schema.Items
    Assert.Equal("array", itemType.Type)
    Assert.NotNull(itemType.Items)
    Assert.Equal(2,itemType.MaxItems.Value)
    Assert.Equal(2,itemType.MinItems.Value)
    Assert.NotNull(itemType.Items.OneOf)
    let s = itemType.Items.OneOf |> Seq.filter ( fun c-> c.Type = "string") |> Seq.tryHead
    Assert.Equal(true,s.IsSome)
    let i = itemType.Items.OneOf |> Seq.filter ( fun c-> c.Type = "integer") |> Seq.tryHead
    Assert.Equal(true,i.IsSome)
    ()    