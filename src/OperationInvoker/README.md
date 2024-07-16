# ESCd.Extensions.OperationInvoker

[![Version](https://img.shields.io/nuget/vpre/ESCd.Extensions.OperationInvoker)](https://www.nuget.org/packages/ESCd.Extensions.OperationInvoker)

A lightweight CQRS-like service pattern.

## Basic Usage

1. Define an Operation:
```csharp
public static class WeatherOperation
{
    public record GetForecast : IOperation<WeatherForecast[]>;
}
```

2. Handle the Operation:
```csharp
internal sealed class GetForecastHandler : IOperationHandler<WeatherOperation.GetForecast, WeatherForecast[]>
{
    public async Task<WeatherForecast[]> Invoke( WeatherOperation.GetForecast operation, CancellationToken cancellation )
    {
        ArgumentNullException.ThrowIfNull( operation );

        return ...;
    }
}
```

3. Add the Handler:
```csharp
public static IServiceCollection AddOperations(this IServiceCollection services)
    => services.AddOperationHandler<GetForecastHandler>();
```

4. Invoke the Operation:
```csharp
public sealed class WeatherController
{
    [HttpGet("/api/weather")]
    public async Task<ActionResult> Weather([FromServices] IOperationInvoker operations )
    {
        var forecast = await operations.Invoke(
            new WeatherOperation.GetForecast(),
            HttpContext.RequestAborted);

        return Ok(forecast);
    }
}
```

> *Something missing, still have questions? Please open an Issue or submit a PR!*