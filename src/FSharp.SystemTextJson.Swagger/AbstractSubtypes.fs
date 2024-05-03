namespace FSharp.SystemTextJson.Swagger

open System



type internal AbstractTupleDefinition<'Ttuple> =
    interface
    end


type internal AbstractUnion<'Tunion> =
    interface
    end

type UnionCase<'Tunion>() =
    class
    end


type EmptyArray() =
    class
    end

type RecordForUnionCase<'TUnionCase> =
    class
    end

type internal AbstractType =
    | TupleType of (Type)
    | UnionType of (Type)
    | AbstractUnionCase of (Type)
    | Other
