namespace FSharp.SystemTextJson.Swagger.TestWeb.Controllers

open System
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open FSharp.SystemTextJson.Swagger.TestWeb

[<ApiController>]
[<Route("[controller]/[action]")>]
type WeatherForecastController(logger: ILogger<WeatherForecastController>) =
    inherit ControllerBase()

    let summaries =
        [| "Freezing"
           "Bracing"
           "Chilly"
           "Cool"
           "Mild"
           "Warm"
           "Balmy"
           "Hot"
           "Sweltering"
           "Scorching" |]

    [<HttpGet>]
    member _.Get():WeatherForecast[] =
        [| |]


    [<HttpPost>]
    member _.Put(d:WeatherForecast)=
        d