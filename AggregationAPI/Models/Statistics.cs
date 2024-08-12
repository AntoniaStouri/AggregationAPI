namespace AggregationAPI.Models
{
    public class Statistics
    {
        public int TotalRequests { get; set; } = 0;

        public double AverageResponseTime { get; set; } = 0;

        public List<long> ResponseTime { get; set; } = new();

        public PerformanceBuckets Performance {  get; set; } = new();

        public class PerformanceBuckets
        {
            public int Fast { get; set; }    
            public int Average { get; set; } 
            public int Slow { get; set; }    
        }

        public class AggregatedStatistics
        {
            public Statistics WeatherStatistics { get; set; } = new();
            public Statistics NewsStatistics { get; set; } = new();
            public Statistics CountriesStatistics { get; set; } = new();
        }
        public void RecordRequest(long elapsedMilliseconds)
        {
            TotalRequests++;
            ResponseTime.Add(elapsedMilliseconds);

            AverageResponseTime = ResponseTime.Any()
               ? ResponseTime.Average()
               : 0.0;

            if (elapsedMilliseconds < 100)
                Performance.Fast++;
            else if (elapsedMilliseconds < 200)
                Performance.Average++;
            else
                Performance.Slow++;
        }

    }
}


  

  
