
using System.IO.Compression;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Yavsc.Models;
using Yavsc.Models.Blog;

namespace Yavsc.Controllers
{
    [Authorize()]
    public class DatabaseController : Controller
    {
        private readonly ILogger<DatabaseController> logger;
        private readonly ApplicationDbContext applicationDbContext;

        public DatabaseController(ApplicationDbContext applicationDbContext,
        ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DatabaseController>();
            this.applicationDbContext = applicationDbContext;
        }

        public IActionResult GetBlog()
        {
            var data = applicationDbContext.BlogSpot.ToArray();
            return Ok(data);
        }

        public IActionResult GetUsers()
        {
            var data = applicationDbContext.Users.ToArray();
            return Ok(data);
        }

        public IActionResult ImportUsers(String usersJson)
        {
            int failures = 0;
            var input = JsonConvert.DeserializeObject<ApplicationUser[]>(usersJson);
            foreach (var user in input)
            {
                try
                {
                    applicationDbContext.Users.Add(user);
                }
                catch (Exception ex)
                {
                    failures++;
                }
            }
            return Ok(failures);
        }
        public IActionResult ImportBlog(String blogJson)
        {
            int failures = 0;
            var input = JsonConvert.DeserializeObject<BlogPost[]>(blogJson);
            foreach (var post in input)
            {
                try
                {
                    applicationDbContext.BlogSpot.Add(post);
                }
                catch (Exception ex)
                {
                    failures++;
                }
            }
            return Ok(failures);
        }
    }
}
