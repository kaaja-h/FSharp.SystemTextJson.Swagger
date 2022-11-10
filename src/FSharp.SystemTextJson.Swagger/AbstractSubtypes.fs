namespace FSharp.SystemTextJson.Swagger

open System
open System.Collections.Generic
open System.Text.Json.Serialization


type internal AbstractMapDefinition<'TKey,'Tvalue> = interface end
   

type internal AbstractTupleDefinition<'Ttuple> = interface end


type internal AbstractUnion<'Tunion> = interface end 

type AbstractUnionCase<'Tunion,'Tcase>() =
    let mutable case = ""
    member this.Case with get() = case
    member this.Case with set(c) = case <- c 
    

type internal AbstractEmptyCase<'Tunion> = interface end

type internal AbstractType =
    |MapType of (Type*Type)
    |TupleType of (Type)
    |UnionType of (Type)
    |AbstractUnionCase of (Type*Type)
    |Other


