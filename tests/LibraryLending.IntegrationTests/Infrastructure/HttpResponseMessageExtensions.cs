using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibraryLending.IntegrationTests.Infrastructure;

public static class HttpResponseMessageExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static async Task<T?> ReadJsonAsync<T>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }
}
