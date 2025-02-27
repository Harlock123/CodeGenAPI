using Microsoft.AspNetCore.Mvc;

namespace CodeGenAPI.Controllers;

/// <summary>
/// Controller for retrieving weather forecasts. Leftover from the BoilerPlate template.
/// Still useful for testing connecting to the API. So it remains for now.
/// </summary>
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    /// <summary>
    /// Array of weather summaries.
    /// </summary>
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary>
    /// Represents a logger instance for the WeatherForecastController class.
    /// </summary>
    /// <typeparam name="WeatherForecastController">The type of the class that this logger is associated with.</typeparam>
    private readonly ILogger<WeatherForecastController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherForecastController"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">The logger used for logging.</param>
    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a collection of weather forecasts.
    /// </summary>
    /// <returns>
    /// An enumerable collection of <see cref="WeatherForecast"/> objects representing the weather forecasts.
    /// </returns>
    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}

