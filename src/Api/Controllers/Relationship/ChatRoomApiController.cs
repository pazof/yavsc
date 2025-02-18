using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Helpers;
using Yavsc.Models;
using Yavsc.Models.Chat;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/ChatRoomApi")]
    public class ChatRoomApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatRoomApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ChatRoomApi
        [HttpGet]
        public IEnumerable<ChatRoom> GetChatRoom()
        {
            return _context.ChatRoom;
        }

        // GET: api/ChatRoomApi/5
        [HttpGet("{id}", Name = "GetChatRoom")]
        public async Task<IActionResult> GetChatRoom([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ChatRoom chatRoom = await _context.ChatRoom.SingleAsync(m => m.Name == id);

            if (chatRoom == null)
            {
                return NotFound();
            }

            return Ok(chatRoom);
        }

        // PUT: api/ChatRoomApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChatRoom([FromRoute] string id, [FromBody] ChatRoom chatRoom)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != chatRoom.Name)
            {
                return BadRequest();
            }

            if (User.GetUserId() != chatRoom.OwnerId )
            {
                return BadRequest(new {error = "OwnerId"});
            }

            _context.Entry(chatRoom).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChatRoomExists(id))
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

        // POST: api/ChatRoomApi
        [HttpPost]
        public async Task<IActionResult> PostChatRoom([FromBody] ChatRoom chatRoom)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (User.GetUserId() != chatRoom.OwnerId )
            {
                return BadRequest(new {error = "OwnerId"});
            }

            _context.ChatRoom.Add(chatRoom);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ChatRoomExists(chatRoom.Name))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("GetChatRoom", new { id = chatRoom.Name }, chatRoom);
        }

        // DELETE: api/ChatRoomApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChatRoom([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ChatRoom chatRoom = await _context.ChatRoom.SingleAsync(m => m.Name == id);

           

            if (chatRoom == null)
            {
                return NotFound();
            }

            if (User.GetUserId() != chatRoom.OwnerId )
            {
                if (!User.IsInRole(Constants.AdminGroupName))
                    return BadRequest(new {error = "OwnerId"});
            }

            _context.ChatRoom.Remove(chatRoom);
            await _context.SaveChangesAsync();

            return Ok(chatRoom);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ChatRoomExists(string id)
        {
            return _context.ChatRoom.Count(e => e.Name == id) > 0;
        }
    }
}
