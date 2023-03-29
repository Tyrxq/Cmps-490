using System.Security.Cryptography;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using SeniorProj.Shared;

namespace SeniorProj.Server.Controllers;


[ApiController]
[Microsoft.AspNetCore.Mvc.Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }
    
    [HttpPut]
    public async Task<ActionResult<string>> Put(UserDto request)
    {
        if (request != null) {
            using (var db = new UserDbContext())
            {
                try
                {
                    
                        User newUser = new User();
                        
                        CreatePasswordHash(request.Password1, out byte[] passwordHash, out byte[] passwordSalt);

                        //all usernames will be stored as lowercase
                        newUser.Username = request.Username.ToLower();
                        newUser.Email = request.Email;
                        newUser.Notifications = request.Notifications;
                        newUser.PostalCode = request.PostalCode;
                        newUser.Name = request.Name;

                        newUser.PasswordHash = passwordHash;
                        newUser.PasswordSalt = passwordSalt;
                        
                        db.Users.Add(newUser); // auto-increment Id
                        db.SaveChanges();
                        return Ok("User registered sucessfully");

                }
                catch
                {
                    Console.WriteLine($"Adding {request.Username} failed\n");
                    return BadRequest("User registration failed");
                }
            }
        }
        else
            Console.WriteLine("User was null");

        return BadRequest("User registration failed");
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<string>> LoginUser(UserLogin request)
    {
        User user = Find(request);
        if (user != null)
        {
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }
            
            // create login token now
            
            return Ok("User login valid");
        }
        else
        {
            return BadRequest("Invalid credentials");
        }
    }
    
    
    public User? Find(UserLogin request)
    {
        List<User> list = new List<User>();
        using (var db = new UserDbContext())
        {
            var userList =
                from x in db.Users
                orderby x.Username
                where x.Username == request.Username.ToLower()
                select x;
            foreach (var c in userList)
                list.Add(c);
        }

        if (list.Count == 0)
        {
            return null;
        }
        else
        {
            return list[0];
        }
    }
    
    
    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
    
    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }
    
}