using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using Yavsc.Models;
using Yavsc.Models.Chat;

namespace Yavsc.Controllers
{
    [Produces("application/json")]
    [Route("api/ChatRoomApi")]
    public class ChatRoomApiController : Controller
    {
        private ApplicationDbContext _context;

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
                return HttpBadRequest(ModelState);
            }

            ChatRoom chatRoom = await _context.ChatRoom.SingleAsync(m => m.Name == id);

            if (chatRoom == null)
            {
                return HttpNotFound();
            }

            return Ok(chatRoom);
        }

        // PUT: api/ChatRoomApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChatRoom([FromRoute] string id, [FromBody] ChatRoom chatRoom)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (id != chatRoom.Name)
            {
                return HttpBadRequest();
            }

            if (User.GetUserId() != chatRoom.OwnerId )
            {
                return HttpBadRequest(new {error = "OwnerId"});
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
                    return HttpNotFound();
                }
                else
                {
                    throw;
                }
            }

            return new HttpStatusCodeResult(StatusCodes.Status204NoContent);
        }

        // POST: api/ChatRoomApi
        [HttpPost]
        public async Task<IActionResult> PostChatRoom([FromBody] ChatRoom chatRoom)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest(ModelState);
            }

            if (User.GetUserId() != chatRoom.OwnerId )
            {
                return HttpBadRequest(new {error = "OwnerId"});
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
                    return new HttpStatusCodeResult(StatusCodes.Status409Conflict);
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
                return HttpBadRequest(ModelState);
            }
            ChatRoom chatRoom = await _context.ChatRoom.SingleAsync(m => m.Name == id);

           

            if (chatRoom == null)
            {
                return HttpNotFound();
            }

            if (User.GetUserId() != chatRoom.OwnerId )
            {
                if (!User.IsInRole(Constants.AdminGroupName))
                    return HttpBadRequest(new {error = "OwnerId"});
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