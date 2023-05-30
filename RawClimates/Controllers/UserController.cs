using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using RawClimates.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RawClimates.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UserController : Controller
    {
        private readonly IMongoCollection<User> _productCollection;

        public UserController(IMongoCollection<User> productCollection)
        {
            _productCollection = productCollection;

           /* TTL*/
            var indexKeysDefinition = Builders<User>.IndexKeys.Ascending(u => u.DateTime);
            var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) };
            _productCollection.Indexes.CreateOne(new CreateIndexModel<User>(indexKeysDefinition, indexOptions));
        }

        /*b*/
        [HttpPost("api/AddNewUser")]
        public async Task<IActionResult> AddNewUser(User user)
        {
            var filter = Builders<User>.Filter.In(u => u.Role, new[] { RoleType.User, RoleType.Admin });
            var adminUsers = _productCollection.Find(filter).ToList();
            if(adminUsers.Count > 0)
            {
                var value = HttpContext.Session.GetString("username");
                if (value == "Admin")
                {
                    // Generate a salt
                    string salt = BCrypt.Net.BCrypt.GenerateSalt();
                    // Hash the password with the salt
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password, salt);
                    user.Password = hashedPassword;

                    await _productCollection.InsertOneAsync(user);
                    return Ok("New User added");
                }
                else
                {
                    return BadRequest("Seems like you have not logged in as a Admin");
                }
            }
            else
            {
                if(user.Role.ToString() == "Admin")
                {
                    //user.Role = RoleType.Admin;
                    // Generate a salt
                    string salt = BCrypt.Net.BCrypt.GenerateSalt();
                    // Hash the password with the salt
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password, salt);
                    user.Password = hashedPassword;

                    await _productCollection.InsertOneAsync(user);
                    return Ok("New Admin user added" );
                }
            }
            
        return BadRequest("Something went wrong!!!");        
        }

        /*[HttpGet("api/GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var value = HttpContext.Session.GetString("username");
            if (value == "Admin")
            {
                var products = await _productCollection.Find(_ => true).ToListAsync();
                return Ok(products);
            }
            else
            {
                return BadRequest("Seems like you have not logged in as a Admin");
            }


        }*/

         ///<summary>
         ///API endpoint for managing users.
         ///</summary>
       /* [HttpPut("api/UpdateAUser")]
        public async Task<IActionResult> UpdateAUser(string id, User updatedProduct, string id2, User updatedProduct2)
        {
            var value = HttpContext.Session.GetString("username");
            if (value == "Admin")
            {
                var filter = Builders<User>.Filter.Eq(p => p.Id, ObjectId.Parse(id));
                var result = await _productCollection.ReplaceOneAsync(filter, updatedProduct);

                if (result.ModifiedCount == 0)
                {
                    return NotFound();
                }

                return Ok(updatedProduct);
            }
            else
            {
                return BadRequest("Seems like you have not logged in as a Admin");
            }
        }*/

         /*k*/
        [HttpPut("api/UpdateAccessOfUsers")]
        public async Task<IActionResult> UpdateAccessOfUser( string id, RoleType newRole, string id2, RoleType newRole2)
        {
            var value = HttpContext.Session.GetString("username");
            if (value == "Admin")
            {
                var filter = Builders<User>.Filter.Eq(p => p.Id, ObjectId.Parse(id));
                var update = Builders<User>.Update.Set(u => u.Role, newRole);

                var filter2 = Builders<User>.Filter.Eq(p => p.Id, ObjectId.Parse(id2));
                var update2 = Builders<User>.Update.Set(u => u.Role, newRole2);

                var result = _productCollection.UpdateOne(filter, update);
                var result2 = _productCollection.UpdateOne(filter2, update2);

                if (result.ModifiedCount > 0 && result2.ModifiedCount > 0)
                {
                    return Ok("Role types modified successfully");
                }
                else if (result.ModifiedCount > 0 || result2.ModifiedCount > 0)
                {
                    return Ok("One User's Role types modified successfully");
                }
                else
                {
                    return BadRequest("No account found to be modified");
                }
            }
            else
            {
                return BadRequest("Seems like you have not logged in as a Admin");
            }
        }

        /*h*/
        [HttpDelete("api/DeleteAUser")]
        public async Task<IActionResult> DeleteAUser(string id)
        {
            var value = HttpContext.Session.GetString("username");
            if (value == "Admin")
            {
                var filter = Builders<User>.Filter.Eq(p => p.Id, ObjectId.Parse(id));
                var result = await _productCollection.DeleteOneAsync(filter);

                if (result.DeletedCount == 0)
                {
                    return NotFound("No user deleted");
                }
            }
            else
            {
                return BadRequest("Seems like you have not logged in as a Admin");
            }


            return Ok("Deleted Successfully");
        }

        /*i*/
        [HttpDelete("api/DeleteMultipleUsers")]
        public async Task<IActionResult> DeleteMultipleUsers()
        {
            var value = HttpContext.Session.GetString("username");
            if (value == "Admin")
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                var filter = Builders<User>.Filter.Lt("DateTime", thirtyDaysAgo);

                var result = await _productCollection.DeleteManyAsync(filter);

                if (result.DeletedCount > 0)
                {
                    return Ok("Count of deleted Accounts: " + result.DeletedCount);
                }

                else
                {
                    return BadRequest("No account found to be modified");
                }
            }
            else
            {
                return BadRequest("Seems like you have not logged in as a Admin");
            }

        }

    }
}
