using EventLink.Data;
using EventLink.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventLink.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly AppDbContext ctx;

        public EventsController(AppDbContext ctx)
        {
            this.ctx = ctx;
        }


        [HttpGet]
        [Authorize]
        public IActionResult GetAllEvents([FromQuery] string? me)
        {
            if (me != null)
            {
                var userId = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));
                var userEvents = ctx.Events.Where(e => e.HostId == userId);
                return Ok(userEvents);
            }

            return Ok(ctx.Events);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEvent(int id)
        {
            var evt = await ctx.Events.FindAsync(id);

            if (evt == null) return NotFound();

            return Ok(evt);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEvent newEvent)
        {
            if (!ModelState.IsValid) return BadRequest("Check the model of data");

            var hostId = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));

            var evt = new Event()
            {
                Name = newEvent.Name,
                Description = newEvent.Description,
                Link = newEvent.Link,
                HostId = hostId,
                CreatedAt = DateTime.UtcNow,
                ImgUrl = newEvent.ImgUrl,
                Date = newEvent.Date
            };

            try
            {
                ctx.Events.Add(evt);
                await ctx.SaveChangesAsync();
                return Created();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var evt = await ctx.Events.FindAsync(id);

            if(evt == null)
            {
                return NotFound();
            }

            var hostId = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));

            if (hostId != evt.HostId)
            {
                return Unauthorized("Not the host");
            }

            ctx.Events.Remove(evt);
            await ctx.SaveChangesAsync();
            return Accepted();
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEvent([FromRoute]int id, [FromBody] CreateEvent newEvent)
        {
            if (!ModelState.IsValid) return BadRequest("Check the model of data");

            var evt = await ctx.Events.FindAsync(id);

            if (evt == null) return NotFound();

            var hostId = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));

            if(hostId != evt.HostId)
            {
                return Unauthorized("Not the host");
            }

            evt.Name = newEvent.Name;
            evt.Description = newEvent.Description;
            evt.Link = newEvent.Link;
            evt.CreatedAt = DateTime.UtcNow;
            evt.ImgUrl = newEvent.ImgUrl;
            evt.Date = newEvent.Date;
            
            try
            {
                await ctx.SaveChangesAsync();
                return Accepted();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
