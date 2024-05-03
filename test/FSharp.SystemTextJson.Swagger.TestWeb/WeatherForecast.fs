namespace FSharp.SystemTextJson.Swagger.TestWeb

open System
open System.ComponentModel.DataAnnotations

type ttt = {
    i:int option
}

type Union =
    |Aaa
    |Bbb
    |Ccc of int

type LLL(myReadWriteProperty:string) =
    
    let mutable myInternalValue = myReadWriteProperty
    
    [<Required>]
    member this.MyReadWriteProperty
        with get () = myInternalValue
        and set (value) = myInternalValue <- value

type TestEnum =
    |AAA=1
    |BBB=2


type WeatherForecast =
    { Date: DateTime
      TemperatureC: int option
      Summary: string option
      ttt:ttt option
      set:Set<int>
      sss: ttt list
      m: Map<int,ttt>
      tuple: (int*string)
      lll:LLL
      u:Union
      ppp:TestEnum
      }


