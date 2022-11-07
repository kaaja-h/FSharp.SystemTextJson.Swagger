namespace FSharp.SystemTextJson.Swagger

open System
open System.Collections.Generic
open System.Text.Json.Serialization


type internal AbstractMapDefinition<'TKey,'Tvalue> = interface end
   

type internal AbstractTupleDefinition<'Ttuple> = interface end




type internal AbstractType =
    |MapType of (Type*Type)
    |TupleType of (Type)
    |Other


