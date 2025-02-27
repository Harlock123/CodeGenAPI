namespace CodeGenAPI;

/// <summary>
/// The WeatherForecast class is used to store the weather forecast information. Still useful for testing connecting to the API. So it remains for now.
/// </summary>
public class WeatherForecast
{
    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    /// <summary>
    /// Gets the temperature in Fahrenheit. Using a lambda expression to calculate the temperature in Fahrenheit.
    /// </summary>
    /// <remarks>
    /// The temperature in Fahrenheit is calculated by converting the Celsius temperature to Fahrenheit using the formula:
    /// Fahrenheit = 32 + (Celsius / 0.5556)
    /// </remarks>
    /// <value>
    /// The temperature in Fahrenheit.
    /// </value>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}


