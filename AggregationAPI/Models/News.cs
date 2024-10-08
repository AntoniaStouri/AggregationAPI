﻿using System.Text.Json;

namespace AggregationAPI.Models
{
    public class News
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}