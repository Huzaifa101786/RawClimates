using System;

namespace RawClimates.Controllers
{
    internal class GetOtherDetailsOnSpecificTime
    {
        public string DeviceName { get; set; }
        public DateTime ReadingTime { get; set; }
        public double Temperature { get; set; }
    }
}