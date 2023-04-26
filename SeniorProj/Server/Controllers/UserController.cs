using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using SeniorProj.Shared;
using MailKit.Net.Smtp;
using MailKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MimeKit;

namespace SeniorProj.Server.Controllers;


[ApiController]
[Microsoft.AspNetCore.Mvc.Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IConfiguration _configuration;
    
    public UserController(IConfiguration configuration, ILogger<UserController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
/*
    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }
*/
    [HttpGet, Authorize]
    public ActionResult<string> GetMe()
    {
        var userName = User?.Identity?.Name;
        return Ok(userName);
    }





    [HttpPut]
    public async Task<ActionResult<string>> Put(UserDto request)
    {
        if (request != null)
        {
            using (var db = new UserDbContext())
            {
                try
                {
                    //Check if email or username already exists
                    if (UserExists(request))
                    {
                        return BadRequest("User registration failed");
                    }

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

    [HttpPost]
    public async Task<ActionResult<string>> LoginUser(UserLogin request)
    {
        User user = Find(request.Username);

        if (user != null)
        {
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                Console.Write("Bad password");
                return BadRequest("Wrong password.");
            }

            // create login token now
            string token = CreateToken(user);
            Console.WriteLine($"User successfully verified\n");
            Console.WriteLine(token);
            return Ok(token);
        }
        else
        {
            Console.WriteLine("Invalid credentials");
            return BadRequest("Invalid credentials");
        }
    }

    private string CreateToken(User user)
    {
        var role = "user";
        if (user.Username == "admin")
        {
            role = "admin";
        }
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.PostalCode, user.PostalCode),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }


    public bool UserExists(UserDto request)
    {
        List<User> list = new List<User>();
        using (var db = new UserDbContext())
        {
            var userList =
                from x in db.Users
                where (x.Username == request.Username.ToLower() || x.Email == request.Email)
                select x;
            foreach (var c in userList)
                list.Add(c);
        }
        
        if (list.Count == 0)
        {
            return false;
        }

        return true;
    }
    

    public User? Find(string username)
    {
        List<User> list = new List<User>();
        using (var db = new UserDbContext())
        {
            var userList =
                from x in db.Users
                orderby x.Username
                where x.Username == username.ToLower()
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
    public List<string> FindEmails(string[] zips)
    {
        List<string> list = new List<string>();
        using (var db = new UserDbContext())
        {
            var userList =
                from x in db.Users
                orderby x.Username
                where (zips.Contains(x.PostalCode) && x.Notifications.Value)
                select x.Email;
            foreach (var c in userList)
                list.Add(c);
        }

        if (list.Count == 0)
        {
            return null;
        }
        return list;
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

    [Route("admin/outages")]
    [HttpPost, Authorize]
    public async Task<ActionResult<string>> SubmitOutage(Outage outage)
    {
        if (outage != null) {
            using (var db = new UserDbContext())
            {
                try
                {
                    Console.WriteLine("trying to add outage to database");
                    db.Outages.Add(outage); // auto-increment Id
                    db.SaveChanges();
                    Console.WriteLine("added outage to db");
                    SendEmails(outage);
                    return Ok("sucessfully");

                }
                catch
                {
                    Console.WriteLine($"Adding outage \"{outage.Description}\" failed\n");
                    return BadRequest("failed");
                }
            }
        }
        Console.WriteLine("was null");

        return BadRequest("failed");
    }
    
    public void SendEmails(Outage outage)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress("Droplet", "droplet490@gmail.com"));
        if (outage.OutageZIP != null)
        {
            string[] zips = outage.OutageZIP.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            List<string> emails = FindEmails(zips);
            if (emails == null)
            {
                Console.WriteLine("No emails for affected areas");
                return;
            }
            foreach (var recipient in emails)
            {
                //bcc hides other emails from recipients
                email.Bcc.Add(new MailboxAddress("",recipient));
            }
        }
        else
        {
            Console.WriteLine("No zipcodes found");
            return;
        }
        //email.To.Add(new MailboxAddress("Brett", "C00414899@louisiana.edu"));

        email.Subject = $"Outage Notification";
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = $"The outage details from {outage.CompanyName} are as follows:" + "<br>" + "<br>" + 
                   $"{outage.Description}" + "<br>" +
                   $"Expected repair time: {outage.RepairDate} {outage.RepairTime}" + "<br>" + "<br>" + "<br>" + 
                   $"You have received this message from Droplet because you are subscribed for outage notifications for the following Zip Code(s): {outage.OutageZIP}"    
        };

        using (var smtp = new SmtpClient())
        {
            smtp.Connect("smtp.gmail.com", 587, false);

            // Note: only needed if the SMTP server requires authentication
            smtp.Authenticate("droplet490@gmail.com", "ucgiexbszllladme");

            smtp.Send(email);
            smtp.Disconnect(true);
        }
        Console.WriteLine("emails sent");
    }
    
    
    // Functions for the forum/reports page
    [Route("report")]
    [HttpPost, Authorize]
    public async Task<ActionResult<string>> SubmitOutage(ForumPost outage)
    {
        if (outage != null) {
            using (var db = new UserDbContext())
            {
                try
                {
                    outage.UserId = 1;
                    Console.WriteLine("trying to add outage to database");
                    db.ForumPosts.Add(outage); // auto-increment Id
                    db.SaveChanges();
                    Console.WriteLine("added outage to db");
                    return Ok("sucessfully");
                }
                catch
                {
                    Console.WriteLine($"Adding outage \"{outage.Message}\" failed\n");
                    return BadRequest("failed");
                }
            }
        }
        Console.WriteLine("was null");

        return BadRequest("failed");
    }
    
    [Route("posts")]
    [HttpGet]
    public ForumPost[] Get()
    {
        List<ForumPost> list = new List<ForumPost>();
        using (var db = new UserDbContext())
        {
            var reportList =
                from x in db.ForumPosts
                orderby x.Name
                select x;
            foreach (var c in reportList)
                list.Add(c);
        }
        return list.ToArray();
    }
  
    [Route("Outages")]
    [HttpGet]
    public Outage[] GetOutages()
    {
        List<Outage> list = new List<Outage>();
       
        using (var db = new UserDbContext())
        {
            var reportList =
                from x in db.Outages
                orderby x.Id
                select x;
            foreach (var c in reportList)
                list.Add(c);
        }
        return list.ToArray();
    }
    
}