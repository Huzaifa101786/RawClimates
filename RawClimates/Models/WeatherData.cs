using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RawClimates.Models
{
    public class WeatherData
    {
        public ObjectId Id { get; set; }
        public string DeviceName {get; set;}
        public double Precipitation { get; set; }
        //[JsonPropertyName("Time.$date.$numberLong")]
        public DateTime Time { get; set; } 
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Temperature { get; set; }
        public double AtmosphericPressure { get; set; }
        public double MaxWindSpeed { get; set; }
        public double SolarRadiation { get; set; }
        public double VaporPressure { get; set; }
        public double Humidity { get; set; }
        public double WindDirection { get; set; }
    }
}
