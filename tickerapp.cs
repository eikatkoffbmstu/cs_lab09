using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private const string ApiToken = "b0dVUXZyaHBiUFBYaDd2bXFXd3I3a3pjVDJzUFp4OEZJaWJfekNWWnA5QT0";

    static async Task Main(string[] args)
    {
        var tickers = await File.ReadAllLinesAsync("ticker.txt");
        var startDate = "2020-01-01";
        var endDate = "2022-01-01";

        var averagePrices = new Dictionary<string, double>();

        var tasks = new List<Task>();

        foreach (var ticker in tickers)
        {
            tasks.Add(Task.Run(async () =>
            {
                var averagePrice = await GetAveragePrice(ticker, startDate, endDate);
                if (averagePrice.HasValue)
                {
                    averagePrices[ticker] = averagePrice.Value;
                }
            }));
        }

        await Task.WhenAll(tasks);

        foreach (var kvp in averagePrices)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
    }

    private static async Task<double?> GetAveragePrice(string ticker, string startDate, string endDate)
    {
        var url = $"https://api.marketdata.app/v1/stocks/candles/D/{ticker}/?from={startDate}&to={endDate}&token={ApiToken}";
        var response = await client.GetStringAsync(url);
        var data = JObject.Parse(response);

        if (data["s"].ToString() != "ok")
        {
            Console.WriteLine($"Ошибка при получении данных для {ticker}");
            return null;
        }

        double totalPrice = 0;
        int count = 0;

        foreach (var entry in data["data"])
        {
            double high = (double)entry["h"];
            double low = (double)entry["l"];
            totalPrice += (high + low) / 2;
            count++;
        }

        return count > 0 ? totalPrice / count : (double?)null;
    }
}
