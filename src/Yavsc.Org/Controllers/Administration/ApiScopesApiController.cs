using IdentityServer8.EntityFramework.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Server.Helpers;

namespace Yavsc.Org.Controllers.Administration
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiScopesApiController : ControllerBase
    {
    private ApplicationDbContext dbContext;

    public ApiScopesApiController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        // GET: api/<Administration>
        [HttpGet]
        public async Task<IEnumerable<ApiScope>> Get(int skip = 0, int take = 25)
        {
            return await dbContext.ApiScopes.Skip(skip).Take(take).ToArrayAsync();
        }

        // GET api/<Administration>/5
        [HttpGet("{id}")]
        public async Task<ApiScope> Get(int id)
        {
            return await dbContext.ApiScopes.FirstOrDefaultAsync(s => s.Id == id);
        }

        // POST api/<Administration>
        [HttpPost]
        public async Task Post([FromBody] ApiScope value)
        {
            dbContext.ApiScopes.Add(value);
            await dbContext.SaveChangesAsync(User.GetUserId());
        }

        // PUT api/<Administration>/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] ApiScope value)
        {
            dbContext.Update(value);
            await dbContext.SaveChangesAsync(User.GetUserId());
        }

        // DELETE api/<Administration>/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var scope = await dbContext.ApiScopes.FirstAsync(s => s.Id == id);
            dbContext.Remove(scope);
        }
    }
}
