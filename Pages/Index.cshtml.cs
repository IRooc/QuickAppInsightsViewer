using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using System.Text;

namespace QuickAppInsightsViewer.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IConfiguration _configuration;

    List<ResultCell> results = new List<ResultCell>();

    [BindProperty(SupportsGet =true)]
    public int Hours { get; set; } = 12;

    public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }


    public async Task OnGet()
    {
        results = new List<ResultCell>();
        
        var entries = _configuration.GetSection("ConfigEntries").Get<ConfigEntries>();
        foreach(var entry in entries?.AppInsights ?? Enumerable.Empty<AppInsightEntry>())
        {
            await QueryAppInsights(entry.SubscriptionId, entry.ResourceGroupName, entry.AppInsightsName);
        }

        ViewData["result"] = results.OrderByDescending(r => r.timestamp).ToList();
        ViewData["summary"] = results.GroupBy(r => r.appName).Select(r => new AppSummary
        {
            Name = r.Key,
            Exceptions = r.Count(c => c.type == "exception"),
            Traces = r.Count(c => c.type == "trace"),
        }).ToList();

    }

    public async Task QueryAppInsights(string subscription, string resourceGroup, string appInsightsName)
    {
        var kcsb = new KustoConnectionStringBuilder($"https://ade.applicationinsights.io/subscriptions/{subscription}/resourcegroups/{resourceGroup}/providers/microsoft.insights/components/{appInsightsName}");

        using (var queryProvider = KustoClientFactory.CreateCslQueryProvider(kcsb))
        {
            // Your Kusto query
            string query = @$"
            traces 
            | where severityLevel > 1
            | where timestamp  > datetime_add('hour', -{Hours}, now())
            | where not(cloud_RoleName endswith ""-ci"")";

            // Execute the query
            var reader = await queryProvider.ExecuteQueryAsync(appInsightsName, query, new ClientRequestProperties { });
            // Read and output the results
            while (reader.Read())
            {
                results.Add(new ResultCell
                {
                    appName = appInsightsName,
                    timestamp = reader.GetDateTime(0),
                    type = "trace",
                    resource = reader["cloud_RoleName"]?.ToString(),
                    message = reader["message"]?.ToString(),
                    customDimensions = reader["customDimensions"]?.ToString()
                });
            }

            query = @$"
            exceptions 
            | where not(cloud_RoleName endswith ""-ci"")
            | where timestamp  > datetime_add('hour', -{Hours}, now())";
            // Execute the query
            reader = await queryProvider.ExecuteQueryAsync(appInsightsName, query, new ClientRequestProperties { });
            // Read and output the results
            while (reader.Read())
            {
                results.Add(new ResultCell
                {
                    appName = appInsightsName,
                    timestamp = reader.GetDateTime(0),
                    type = "exception",
                    resource = reader["cloud_RoleName"]?.ToString(),
                    message = reader["problemId"]?.ToString() + " " + reader["outerMessage"]?.ToString(),
                    customDimensions = reader["details"]?.ToString()
                });
            }


        }
    }

}

public record AppSummary
{
    public string Name;
    public int Exceptions;
    public int Traces;
}

public record ResultCell
{
    public string appName;
    public string resource;
    public string type;
    public DateTime timestamp;
    public string message;
    public string customDimensions;
}

public class AppInsightEntry
{
    public string SubscriptionId { get; set; }
    public string ResourceGroupName { get; set; } = null!;
    public string AppInsightsName { get; set; } = null!;
}

public class ConfigEntries
{
    public List<AppInsightEntry> AppInsights { get; set; }
}