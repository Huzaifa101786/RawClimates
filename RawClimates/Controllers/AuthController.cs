using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RawClimates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RawClimates.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IMongoCollection<User> _users;

        public AuthController(IMongoCollection<User> productCollection)
        {
            _users = productCollection;
        }

        



        [HttpPost("api/Login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            //var filter = Builders<User>.Filter.Eq("Role", "Admin");
            var filter = Builders<User>.Filter.In(u => u.Role, new[] { RoleType.User, RoleType.Admin });
            var adminUsers = _users.Find(filter).ToList();
            // Validate the username and password
            HttpContext.Session.Clear();

            foreach (var item in adminUsers)
            {
                if (item.Role.ToString() == "Admin")
                {
                    if (item.UserName == username && BCrypt.Net.BCrypt.Verify(password, item.Password))
                    {
                        var updateFilter = Builders<User>.Filter.Eq(u => u.Role, RoleType.Admin) &
                                     Builders<User>.Filter.Eq(u => u.UserName, "username") &
                                     Builders<User>.Filter.Eq(u => u.Password, "password");
                        // IMongoCollection<User> productCollection = databaseName.GetCollection<User>("your_collection_name");
                        //var authController = new AuthController(productCollection);
                        var update = Builders<User>.Update.Set("DateTime", DateTime.UtcNow);
                        var updateResult = _users.UpdateOne(updateFilter, update);
                        if (updateResult.ModifiedCount > 0)
                        {
                            var x = updateResult;
                            return Ok();
                        }

                        HttpContext.Session.SetString("username", "Admin");
                        return Ok("Logged in Successfully");
                    }
                }

                else if (item.Role.ToString() == "User")
                {
                    if (item.UserName == username && BCrypt.Net.BCrypt.Verify(password, item.Password))
                    {
                        var updateFilter = Builders<User>.Filter.Eq(u => u.Role, RoleType.User) &
                                     Builders<User>.Filter.Eq(u => u.UserName, "username") &
                                     Builders<User>.Filter.Eq(u => u.Password, "password");
                        var update = Builders<User>.Update.Set("DateTime", DateTime.UtcNow);
                        var updateResult = _users.UpdateOne(updateFilter, update);

                        HttpContext.Session.SetString("username", "User");
                        return Ok("Logged in Successfully");
                    }
                }
                else
                {
                    return Unauthorized();
                }
            }
            return Unauthorized("No user found");
        }

         [HttpDelete("api/Logout")]
         public IActionResult Logout()
         {
             HttpContext.Session.Clear();
             return Ok();
         }
    }
}
