using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Chat;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/ChatRoomAccessApi")]
    public class ChatRoomAccessApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatRoomAccessApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ChatRoomAccessApi
        [HttpGet, Authorize("AdministratorOnly")]
        public IEnumerable<ChatRoomAccess> GetChatRoomAccess()
        {
            return _context.ChatRoomAccess;
        }

        // GET: api/ChatRoomAccessApi/5
        [HttpGet("{id}", Name = "GetChatRoomAccess"), Authorize("AdministratorOnly")]
        public async Task<IActionResult> GetChatRoomAccess([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ChatRoomAccess chatRoomAccess = await _context.ChatRoomAccess.SingleAsync(m => m.ChannelName == id);

            

            if (chatRoomAccess == null)
            {
                return NotFound();
            }

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid != chatRoomAccess.UserId && uid != chatRoomAccess.Room.OwnerId
             && ! User.IsInRole(Constants.AdminGroupName))
           
            {
                ModelState.AddModelError("UserId","get refused");
                return BadRequest(ModelState);
            }
            
            return Ok(chatRoomAccess);
        }

        // PUT: api/ChatRoomAccessApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChatRoomAccess([FromRoute] string id, [FromBody] ChatRoomAccess chatRoomAccess)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id != chatRoomAccess.ChannelName)
            {
                return BadRequest();
            }
            var room = _context.ChatRoom.First(channel => channel.Name == chatRoomAccess.ChannelName );

            if (uid != room.OwnerId && ! User.IsInRole(Constants.AdminGroupName))
            {
                ModelState.AddModelError("ChannelName", "access put refused");
                return BadRequest(ModelState);
            }

            _context.Entry(chatRoomAccess).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChatRoomAccessExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/ChatRoomAccessApi
        [HttpPost]
        public async Task<IActionResult> PostChatRoomAccess([FromBody] ChatRoomAccess chatRoomAccess)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var room = _context.ChatRoom.First(channel => channel.Name == chatRoomAccess.ChannelName );
            if (room == null || (uid != room.OwnerId && ! User.IsInRole(Constants.AdminGroupName)))
            {
                ModelState.AddModelError("ChannelName", "access post refused");
                return BadRequest(ModelState);
            }

            _context.ChatRoomAccess.Add(chatRoomAccess);
            try
            {
                await _context.SaveChangesAsync();
            }

            catch (DbUpdateException)
            {
                if (ChatRoomAccessExists(chatRoomAccess.ChannelName))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetChatRoomAccess", new { id = chatRoomAccess.ChannelName }, chatRoomAccess);
        }

        // DELETE: api/ChatRoomAccessApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatRoomAccess([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ChatRoomAccess chatRoomAccess = await _context.ChatRoomAccess.Include(acc => acc.Room).SingleAsync(m => m.ChannelName == id);
            if (chatRoomAccess == null)
            {
                return NotFound();
            }

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var room = _context.ChatRoom.First(channel => channel.Name == chatRoomAccess.ChannelName );
            if (room == null || (uid != room.OwnerId  && chatRoomAccess.UserId != uid && ! User.IsInRole(Constants.AdminGroupName)))
            {
                ModelState.AddModelError("UserId", "access drop refused");
                return BadRequest(ModelState);
            }

            _context.ChatRoomAccess.Remove(chatRoomAccess);
            await _context.SaveChangesAsync();

            return Ok(chatRoomAccess);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ChatRoomAccessExists(string id)
        {
            return _context.ChatRoomAccess.Count(e => e.ChannelName == id) > 0;
        }
    }
}
