# AggregationAPI Documentation

## Configuration

### API Keys

To use the external APIs, you need to configure the API keys in your application in appsettings.json file.

1. *Weather API Key:*
   
```json
     {
         "WeatherAPIKey": "your-weather-api-key"
     }
```
2. *News API Key:*

```json
     {
         "NewsAPIKey": "your-news-api-key"
     }
```

## Endpoints

`GET api/Aggregated/aggregatedData`: 

#### Query Parameters:

- city (string): The name of the city to filter weather data.
- title (string): The title to filter news data.
- region (string): The region to filter countries.
- sortBy (string, optional): The sorting order of the aggregated data. Possible values are:
  - name – Sort by name.
  - population – Sort by population.
- filterBy (string, optional): The filter criteria for the aggregated data. Filtering can be by name or population values.
  
#### Success Response:

- *Status Code:* 200 OK
- *Content:*
```json
{
  "weatherData": {
    "city": "Athens",
    "description": "few clouds",
    "temperature": 306.77
  },
  "newsData": [
    {
      "title": "Rust Leaps Forward on Language Popularity Index",
      "description": "An anonymous reader shared this report from InfoWorld:\n\nRust has leaped to its highest position ever in the monthly Tiobe index of language popularity, scaling to the 13th spot this month, with placement in the top 10 anticipated in an upcoming edition. Previ…",
      "author": "EditorDavid",
      "publishedAt": "2024-07-14T11:34:00Z"
    }
 ],
  "countryData": [
    {
      "name": "Greece",
      "region": "Europe",
      "population": 10715549
    }
  ]
}
```
- *500 Internal Server Error*

    *Content:*
    ```
    {
        "message": "An internal error occurred while processing the request."
    }
    ```

`GET api/Aggregated/statistics`: 

#### Success Response:

- *Status Code:* 200 OK
- *Content:*
```json
{
  "result": {
    "weatherStatistics": {
      "totalRequests": 2,
      "averageResponseTime": 233,
      "responseTime": [
        341,
        125
      ],
      "performance": {
        "fast": 0,
        "average": 1,
        "slow": 1
      }
    },
    "newsStatistics": {
      "totalRequests": 2,
      "averageResponseTime": 892,
      "responseTime": [
        1332,
        452
      ],
      "performance": {
        "fast": 0,
        "average": 0,
        "slow": 2
      }
    },
    "countriesStatistics": {
      "totalRequests": 2,
      "averageResponseTime": 937.5,
      "responseTime": [
        1153,
        722
      ],
      "performance": {
        "fast": 0,
        "average": 0,
        "slow": 2
      }
    }
  },
  "id": 656,
  "exception": null,
  "status": 5,
  "isCanceled": false,
  "isCompleted": true,
  "isCompletedSuccessfully": true,
  "creationOptions": 0,
  "asyncState": null,
  "isFaulted": false
}
```
- *500 Internal Server Error*

    *Content:*
    ```
    {
        "message": "An internal error occurred while processing the request."
    }
    ```
