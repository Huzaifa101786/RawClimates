using MongoDB.Driver;
using RawClimates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RawClimates.Controllers
{
    public class Trigger
    {
        private readonly IMongoCollection<WeatherData> weather_dt;

        public Trigger(IMongoCollection<WeatherData> weatherCollection)
        {
            weather_dt = weatherCollection;
        }

        public void StartTrigger()
        {
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<WeatherData>>();

            var options = new ChangeStreamOptions
            {
                FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
            };
            var cursor = weather_dt.Watch(pipeline, options);

            foreach (var change in cursor.ToEnumerable())
            {
                var document = change.FullDocument;

                // Extract relevant fields from the document
                var humidity = document.Humidity;
                var temperature = document.Temperature;

                // Check for invalid weather readings
                if (humidity > 100 || temperature > 60 || temperature < -50)
                {
                    // Remove the document
                    weather_dt.DeleteOne(Builders<WeatherData>.Filter.Eq("_id", document.Id));
                }
            }
        }
    }
}
