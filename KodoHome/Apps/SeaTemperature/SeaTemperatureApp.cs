using System.Net.Http;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using HomeAssistantGenerated;
using HtmlAgilityPack;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Integration;

namespace KodoHome.SeaTemperature;

[NetDaemonApp]
public class SeaTemperatureApp  : IAsyncInitializable
{
    private static readonly string _sensorId = "sensor.sea_temperature";
    private readonly HttpClient _httpClient;
    private readonly IMqttEntityManager _entityManager;
    private readonly ILogger<SeaTemperatureApp> _logger;

    public SeaTemperatureApp(IScheduler scheduler, HttpClient httpClient, IMqttEntityManager entityManager, ILogger<SeaTemperatureApp> logger)
    {
        _httpClient = httpClient;
        _entityManager = entityManager;
        _logger = logger;
        scheduler.ScheduleCron("5 * * * *", async () => await GetSeaTemperatureAsync().ConfigureAwait(false));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _entityManager.CreateAsync(
            _sensorId,
            new EntityCreationOptions {
                DeviceClass = "TEMPERATURE",
                Name = "Sea Temperature",
                PayloadAvailable = "online",
                PayloadNotAvailable = "offline",
            },
            new {
                unit_of_measurement = "°C"
            }).ConfigureAwait(false);

        _logger.LogInformation("SeaTemperatureApp initialized and sensor created: {SensorId}", _sensorId);
    }
    
    public async Task GetSeaTemperatureAsync()
    {
        var url = "https://harbours.gg/article/151764/Shipping-Forecast";
        var response = await _httpClient.GetStringAsync(url);

        if (!SeaTemperatureParser.TryParse(response, out var seaTemperature))
        {
            await _entityManager.SetAvailabilityAsync(_sensorId, "offline").ConfigureAwait(false);
            _logger.LogWarning("Failed to parse sea temperature from response.");
            return;
        }

        await _entityManager.SetAvailabilityAsync(_sensorId, "online").ConfigureAwait(false);
        await _entityManager.SetStateAsync(_sensorId, seaTemperature!.Celcius.ToString()).ConfigureAwait(false);

        _logger.LogInformation("Sea temperature updated: {Temperature}°C", seaTemperature.Celcius);        
    }
}