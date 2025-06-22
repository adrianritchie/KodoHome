using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using KodoHome.BinNight.Models;
using NetDaemon.Extensions.MqttEntityManager;

namespace KodoHome.BinNight;

[NetDaemonApp]
public class BinNightApp  : IAsyncInitializable
{
    private static readonly string _sensorId = "binary_sensor.bin_night_tonight";

    private readonly HttpClient _httpClient;
    private readonly IMqttEntityManager _entityManager;
    private readonly ILogger<BinNightApp> _logger;

    public BinNightApp(IScheduler scheduler, HttpClient httpClient, IMqttEntityManager entityManager, ILogger<BinNightApp> logger)
    {
        _httpClient = httpClient;
        _entityManager = entityManager;
        _logger = logger;
        scheduler.SchedulePeriodic(TimeSpan.FromHours(1), async () => await GetBinNightAsync().ConfigureAwait(false));
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BinNightApp initializing...");

        await _entityManager.CreateAsync(
            _sensorId,
            new() { Name = "Bin Night Tonight", PayloadOn = "On", PayloadOff = "Off" },
            new { icon = "mdi:trash-can" }
        ).ConfigureAwait(false);

        _logger.LogInformation("Sensor created: {SensorId}", _sensorId);
    }

    public async Task GetBinNightAsync()
    {
        var url = "https://guernsey.isl-fusion.com/api/address/lbQ8CD9Lrvh3DFW8";
        var response = await _httpClient.GetFromJsonAsync<BinNightCollections>(url);

        if (response == null || response.ServiceDates.Count == 0)
        {
            await _entityManager.SetAvailabilityAsync(_sensorId, "down").ConfigureAwait(false);
            _logger.LogWarning("Failed to retrieve bin night data from API.");
            return;
        }

        var isTonight = response.ServiceDates.TryGetValue(DateTime.UtcNow.ToString("yyyy-MM-dd"), out var collectionNight);

        await _entityManager.SetAvailabilityAsync(_sensorId, "up").ConfigureAwait(false);
        
        if (isTonight)
        {
            _logger.LogInformation("Bin night is tonight. Updating images...");

            await _entityManager.SetStateAsync(_sensorId, "On").ConfigureAwait(false);

            var imageUrls = collectionNight!.Services.Select(
                s => {
                    var imagePath = response.Services[s.Value].ImageUrl;
                    return $"https://guernsey.isl-fusion.com{imagePath}";
                }).ToList();

            var attributes = new SensorImageAttributes(
                imageUrls[0],
                imageUrls[1],
                imageUrls[2]
            );

            await _entityManager.SetAttributesAsync(_sensorId, attributes).ConfigureAwait(false);
        
            _logger.LogInformation("Sensor updated: {SensorId}", _sensorId);
        }
        else
        {
            await _entityManager.SetStateAsync(_sensorId, "Off").ConfigureAwait(false);
            _logger.LogInformation("No bin collection tonight.");
        }
    }
}