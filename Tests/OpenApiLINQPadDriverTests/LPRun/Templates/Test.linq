var number = 2;
var summary = "test";

var weathers = await this.WeatherClient.GetWeatherForecastAsync(number, summary);
weathers.Should().HaveCount(number)
.And.ContainEquivalentOf(new WeatherForecast
{
	Date = DateTime.Now.Date.AddDays(2),
	TemperatureC = number,
	Summary = summary
}, Reason());