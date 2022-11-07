module FSharp.SystemTextJson.Swagger.Tests.Tuple

open Xunit

[<Fact>]
let simpleTupleTypeTest()=
    let t=typeof<int*string>
    let schema, _ = TestCommon.generateSimple t
    Assert.Equal("array",schema.Type)
    Assert.Equal(2, schema.MaxItems.Value)
    Assert.Equal(2, schema.MinItems.Value)
    Assert.NotNull(schema.Items)
    Assert.NotNull(schema.Items.OneOf)
    let s = schema.Items.OneOf |> Seq.filter ( fun c-> c.Type = "string") |> Seq.tryHead
    Assert.Equal(true,s.IsSome)
    let i = schema.Items.OneOf |> Seq.filter ( fun c-> c.Type = "integer") |> Seq.tryHead
    Assert.Equal(true,i.IsSome)
    ()

[<Fact>]
let sameTypeTupleTest()=
    let t=typeof<int*string*int>
    let schema, _ = TestCommon.generateSimple t
    Assert.Equal("array",schema.Type)
    Assert.Equal(3, schema.MaxItems.Value)
    Assert.Equal(3, schema.MinItems.Value)
    Assert.NotNull(schema.Items)
    Assert.NotNull(schema.Items.OneOf)
    let s = schema.Items.OneOf |> Seq.filter ( fun c-> c.Type = "string") |> Seq.tryHead
    Assert.Equal(true,s.IsSome)
    let i = schema.Items.OneOf |> Seq.filter ( fun c-> c.Type = "integer") |> Seq.tryHead
    Assert.Equal(true,i.IsSome)
    Assert.Equal(2, schema.Items.OneOf |> Seq.length)
    ()