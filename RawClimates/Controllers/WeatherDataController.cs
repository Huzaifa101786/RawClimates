using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using RawClimates.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RawClimates.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class WeatherDataController : Controller
    {
        private readonly IMongoCollection<WeatherData> _weatherData;
        //private readonly Trigger _invalidWeatherTrigger;

        public WeatherDataController(IMongoCollection<WeatherData> weatherdata)
        {
            _weatherData = weatherdata;
            //_invalidWeatherTrigger = invalidWeatherTrigger;
        }
       
        /*a*/
        [HttpPost("api/AddNewWeatherData")]
        public async Task<IActionResult> AddNewWeatherData(WeatherData data)
        {
            var sessionAccess = HttpContext.Session.GetString("username");
            if (sessionAccess == "Admin" || sessionAccess == "User")
            {
                
                await _weatherData.InsertOneAsync(data);

                var filter = Builders<WeatherData>.Filter.Or(
                 Builders<WeatherData>.Filter.Gt(x => x.Humidity, 100),
                 Builders<WeatherData>.Filter.Gt(x => x.Temperature, 60),
                 Builders<WeatherData>.Filter.Lt(x => x.Temperature, -50)
             );

                _weatherData.DeleteMany(filter);
                /* var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<WeatherData>>();

                 var options = new ChangeStreamOptions
                 {
                     FullDocument = ChangeStreamFullDocumentOption.UpdateLookup
                 };
                 var cursor = _weatherData.Watch(pipeline, options);

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
                         _weatherData.DeleteOne(Builders<WeatherData>.Filter.Eq("_id", document.Id));
                     }
                 }*/

                return Ok(data);
            }
            else
            {
                return BadRequest("Seems like you have not logged in ");
            }

        }

        /*c*/
        [HttpPost("api/BulkUploadByJsonFile")]
        public IActionResult BulkUploadByJsonFile(IFormFile file)
        {
            var sessionAccess = HttpContext.Session.GetString("username");
            if (sessionAccess == "Admin" || sessionAccess == "User")
            {
                if (file == null || file.Length <= 0)
                return BadRequest("No file was uploaded.");

                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    var jsonString = reader.ReadToEnd();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        //Converters = { new DateTimeConverter() }
                    };

                    var data = JsonSerializer.Deserialize<WeatherData[]>(jsonString, options);


                    _weatherData.InsertMany(data);
                    return Ok("Data uploaded successfully.");

                }
            }
            else
            {
                return BadRequest("Seems like you have not logged in ");
            }

           
        }

        /*d*/
        [HttpGet("api/MaximumPrecipitationInLast5Months")]
        public async Task<IActionResult> MaximumPrecipitationInLast5Months()
        {
                var sessionAccess = HttpContext.Session.GetString("username");
                if (sessionAccess == "Admin" || sessionAccess == "User")
                {
                    var fiveMonthsAgo = DateTime.Now.AddMonths(-150);
                    var filter = Builders<WeatherData>.Filter.Gte(x => x.Time, fiveMonthsAgo);


                    var projection = Builders<WeatherData>.Projection.Include(x => x.DeviceName)
                     .Include(x => x.Precipitation)
                     .Include(x => x.Time);
                    var result = await _weatherData.Find(filter).Project(projection).ToListAsync();

                    double precipitation = 0.00;
                    string sensor = "";
                    DateTime? dt = null;

                    foreach (BsonDocument document in result)
                    {
                        if (document.Contains("Precipitation"))
                        {
                            var value = document["Precipitation"];
                            if (value.IsDouble)
                            {
                                double prec = value.AsDouble;

                                if (prec > precipitation)
                                {
                                    precipitation = prec;
                                    sensor = ((string)document["DeviceName"]);
                                    dt = document["Time"].ToUniversalTime();                              
                                }
                            }
                        }
                    }
                    return Ok("Precipitation: " + precipitation + "\n" + "Device Name: " + sensor + "\n" + "Reading Date/Time: "+dt );
            }
            else
            {
                return BadRequest("Seems like you have not logged in ");
            }
        }

        /*[HttpGet("api/GetAllWeatherData")]
        public async Task<IActionResult> GetAllWeatherData()
        {
             var sessionAccess = HttpContext.Session.GetString("username");
             if (sessionAccess == "Admin" || sessionAccess == "User")
             {
                var filter = Builders<WeatherData>.Filter.Eq(x => x.DeviceName, "Woodford_Sensor");
                var result = await _weatherData.Find(filter).Limit(100).ToListAsync();

                return Ok(result);
             }
             else
             {
                 return BadRequest("Seems like you have not logged in ");
             }
        }*/

        /*e*/
        [HttpGet("api/GetOtherDetails")]
        public async Task<IActionResult> GetOtherDetails(string sensor,DateTime time)
            {
                var sessionAccess = HttpContext.Session.GetString("username");
                if (sessionAccess == "Admin" || sessionAccess == "User")
                {
                    string stationName = sensor;  //Woodford_Sensor
                    var fiveMonthsAgo = time;
                    var filter = Builders<WeatherData>.Filter.Gte(x => x.Time, fiveMonthsAgo);
                    //DateTime targetDateTime = new DateTime(2021, 05, 07), 03, 44, 04); // Example: May 22, 2023, 12:00:00 PM

                    // Build the filter to match the specific station and date-time
                    var filter2 = Builders<WeatherData>.Filter.Eq(x => x.DeviceName, stationName)
                                 & Builders<WeatherData>.Filter.Gte(x => x.Time, fiveMonthsAgo);

                    // Define the projection to include the desired fields
                    var projection = Builders<WeatherData>.Projection
                                    .Include(x => x.Temperature)
                                    .Include(x => x.AtmosphericPressure)
                                    .Include(x => x.SolarRadiation)
                                    .Include(x => x.Precipitation);

                    // Retrieve the document with the specified station, date, and time
                    var result = await _weatherData.Find(filter2).Project(projection).FirstOrDefaultAsync();
                    WeatherData weatherData = BsonSerializer.Deserialize<WeatherData>(result);

                    // Access the desired fields from the result
                    double temperature = weatherData?.Temperature ?? 0.0;
                    double atmosphericPressure = weatherData?.AtmosphericPressure ?? 0.0;
                    double radiation = weatherData?.SolarRadiation ?? 0.0;
                    double precipitation = weatherData?.Precipitation ?? 0.0;

                    var output = new Output { Temperature = temperature, AtmosphericPressure = atmosphericPressure, SolarRadiation = radiation, Precipitation = precipitation };
                    return Ok(output);
                }

                else
                {
                    return BadRequest("Seems like you have not logged in ");
                }
            }

        /*f*/
        [HttpGet("api/GetOtherDetailsOnSpecificTime")]
        public async Task<IActionResult> GetOtherDetailsOnSpecificTime(DateTime startdate, DateTime finishdate)
        {
            var sessionAccess = HttpContext.Session.GetString("username");
            if (sessionAccess == "Admin" || sessionAccess == "User")
            {
                 DateTime startDate = startdate;/*new DateTime(2020, 1, 1, 0, 0, 0);*/ // Example: January 1, 2023, 12:00:00 AM
                 DateTime finishDate = finishdate; /*new DateTime(2023, 5, 23, 23, 59, 59);*/ // Example: January 31, 2023, 11:59:59 PM
                 
                 // Build the filter to match the date/time range
                 var filter = Builders<WeatherData>.Filter.And(
                     Builders<WeatherData>.Filter.Gte(x => x.Time, startDate),
                     Builders<WeatherData>.Filter.Lte(x => x.Time, finishDate)
                 );
                 
                 // Define the projection to include the desired fields
                 var projection = Builders<WeatherData>.Projection
                     .Include(x => x.DeviceName)
                     .Include(x => x.Time)
                     .Include(x => x.Temperature)
                     .Exclude("_id"); // Exclude the default "_id" field from the result
                 
                 // Sort the documents by temperature in descending order
                 var sort = Builders<WeatherData>.Sort.Descending(x => x.Temperature);
                 // Retrieve the document with the maximum temperature for each station within the date/time range
                 
                 var result = await _weatherData.Aggregate()
                 .Match(filter)
                 .Sort(sort)
                 .Group(x => x.DeviceName, g => new
                 {
                     DeviceName = g.Key,
                     ReadingDateTime = g.First().Time,
                     Temperature = g.First().Temperature
                 })
                 .Project(x => new WeatherData
                 {
                     DeviceName = x.DeviceName,
                     Time = x.ReadingDateTime,
                     Temperature = x.Temperature
                 })
                 .ToListAsync();
                 
                 
                 string s_name = string.Empty;
                 DateTime d_time = DateTime.MinValue;
                 double temp = 0.00;
                 List<GetOtherDetailsOnSpecificTime> myList = new List<GetOtherDetailsOnSpecificTime>();
                 
                 foreach (var data in result)
                 {
                     s_name = data.DeviceName;
                     d_time = data.Time;
                     temp = data.Temperature;
                     // Perform desired operations with the retrieved data
                     var getOtherDetailsOnSpecificTime = new GetOtherDetailsOnSpecificTime { DeviceName = s_name, ReadingTime = d_time, Temperature = temp };
                     myList.Add(getOtherDetailsOnSpecificTime);
                 }
                  return Ok(myList);
            }
            else
            {
                return BadRequest("Seems like you have not logged in ");
            }
        }

        /*g*/
        [HttpGet("api/EffecientQueryingByIndexKey")]
        public async Task<IActionResult> EffecientQueryingByIndexKey()
        {
            var sessionAccess = HttpContext.Session.GetString("username");
            if (sessionAccess == "Admin" || sessionAccess == "User")
            {

                // Create an index key on the Temperature field for efficient querying
                var indexKeysDefinition = Builders<WeatherData>.IndexKeys.Ascending(data => data.Temperature);
                var indexOptions = new CreateIndexOptions { Name = "Temperature" };

                var data = _weatherData.Indexes.CreateOne(new CreateIndexModel<WeatherData>(indexKeysDefinition, indexOptions));

                return Ok("Index Key created successfully");
            }
            else
            {
                return BadRequest("Seems like you have not logged in ");
            }
        }


        /*j*/
        [HttpPut("api/UpdatePrecipitationValue")]
        public async Task<IActionResult> UpdatePrecipitationValue(string id, double Precipitation)
        {
            var sessionAccess = HttpContext.Session.GetString("username");
            if (sessionAccess == "Admin")
            {
                 var filter = Builders<WeatherData>.Filter.Eq(p => p.Id, ObjectId.Parse(id));
                var update = Builders<WeatherData>.Update.Set("Precipitation", Precipitation);

                var result = await _weatherData.UpdateOneAsync(filter, update);
                   var filterx = Builders<WeatherData>.Filter.Or(
                    Builders<WeatherData>.Filter.Gt(x => x.Humidity, 100),
                    Builders<WeatherData>.Filter.Gt(x => x.Temperature, 60),
                    Builders<WeatherData>.Filter.Lt(x => x.Temperature, -50)
                );
                _weatherData.DeleteMany(filterx);


                if (result.ModifiedCount == 1)
                {
                    return Ok("Precipitation value updated successfully");
                }
                else
                {
                    return NotFound("Something went wrong!!!");
                }
            }
            else
            {
                return BadRequest("Seems like you have not logged in ");
            }
        }
        

    }

    
}
