using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Mews.Integrations.Cnb.Contracts.Models;
using Mews.Integrations.Cnb.Models;

namespace Mews.Integrations.Cnb.Clients;

public class CnbClient(HttpClient httpClient)
{
    public async Task<CnbClientExchangeRateResponse> GetDailyExchangeRates(DateTimeOffset date, CnbSupportedLanguage language, CancellationToken cancellationToken)
    {
        var queryString = BuildQueryString(date, language);
        var response = await SendGetRequestAsync($"cnbapi/exrates/daily?{queryString}", cancellationToken);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<CnbClientExchangeRateResponse>(responseBody)!;
    }

    private async Task<HttpResponseMessage> SendGetRequestAsync(string relativeUrl, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(httpClient.BaseAddress!, relativeUrl));

        return await httpClient.SendAsync(request, cancellationToken);
    }
    
    private static string BuildQueryString(DateTimeOffset? date, CnbSupportedLanguage language)
    {
        var formattedDate = (date ?? DateTimeOffset.UtcNow).ToString("yyyy-MM-dd");
        return $"date={Uri.EscapeDataString(formattedDate)}&lang={Uri.EscapeDataString(MapCnbLanguage(language))}";
    }

    private static string MapCnbLanguage(CnbSupportedLanguage language)
    {
        return language switch
        {
            CnbSupportedLanguage.En => "EN",
            _ => "CZ"
        };
    }
}
