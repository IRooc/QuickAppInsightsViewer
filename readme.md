## Quickly view exceptions and traces from app insights

List the traces and exceptions from app insights you own

### Settings
in the appsettings file you need a list of `subscriptionId`, `resourceGroupName` and `appInsightsName` like in the example

```
{
  "ConfigEntries": {
    "AppInsights": {
      "0": {
        "SubscriptionId": "",
        "ResourceGroupName": "",
        "AppInsightsName": ""
      },
      "1": {
        "SubscriptionId": "",
        "ResourceGroupName": "",
        "AppInsightsName": ""
      },
    }
  }
}
```

### Running
Use `dotnet run` in the project folder, you can use a querystring parameter `?Hours=1` to only list the last hour