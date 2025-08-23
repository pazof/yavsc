
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
            return ReturnDbSet(applicationDbContext.BlogSpot);
        }

        public IActionResult GetUsers()
        {
            return ReturnDbSet(applicationDbContext.Users);
        }

        public IActionResult GeActivities()
        {
            return ReturnDbSet(applicationDbContext.Activities);
        }

        public IActionResult ImportUsers(String usersJson)
        {
            return DBSetImportFromJson(applicationDbContext.Users, usersJson);
        }

        public IActionResult ImportBlog(String blogJson)
        {
             return DBSetImportFromJson(applicationDbContext.BlogSpot, blogJson);
        }

        IActionResult ReturnDbSet<T>(DbSet<T> dbSet) where T : class
        {
            var data = dbSet.ToArray();
            return Ok(data);
        }

        private IActionResult DBSetImportFromJson<T>(DbSet<T> dbSet, string usersJson) where T : class
        {
            int failures = 0;
            var input = JsonConvert.DeserializeObject<T[]>(usersJson);
            foreach (var user in input)
            {
                try
                {
                    dbSet.Add(user);
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
