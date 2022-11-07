namespace FSharp.SystemTextJson.Swagger.TestWeb

#nowarn "20"

open System.Text.Json
open System.Text.Json.Serialization
open FSharp.SystemTextJson.Swagger
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =

        let builder =  WebApplication.CreateBuilder(args)
        let fsOptions = JsonFSharpOptions() // setup options here 
        //setup usage of JsonFSharpConverter                
        builder.Services.AddControllers()
                    .AddJsonOptions(fun opts ->
                        opts.JsonSerializerOptions.Converters.Add(JsonFSharpConverter(fsOptions))) 
        // setup usage of SwaggerForSystemTextJson - use instead AddSwaggerGen 
        builder.Services.AddSwaggerForSystemTextJson(fsOptions)
        
        let app = builder.Build()
        
        if (app.Environment.IsDevelopment()) then
            app.UseSwagger() |> ignore
            app.UseSwaggerUI() |> ignore


        app.UseAuthorization()
        app.MapControllers()

        app.Run()

        exitCode
