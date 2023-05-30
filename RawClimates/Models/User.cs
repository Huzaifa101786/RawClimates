using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RawClimates.Models
{
    
        public enum RoleType
        {
            [EnumMember(Value = "Admin")]
            Admin,

            [EnumMember(Value = "User")]
            User
        }


        public class User
        {
            public ObjectId Id { get; set; }

            [EmailAddress]
            public string UserName { get; set; }

            [MinLength(6)]
            public string Password { get; set; }
            public DateTime DateTime { get; set; } = System.DateTime.UtcNow;

            

            [BsonRepresentation(BsonType.String)]
            [JsonConverter(typeof(JsonStringEnumConverter))]
            public RoleType Role { get; set; }
        }
    

}
