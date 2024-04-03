using EventLink.Data;
using EventLink.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            var events = ctx.Events
                .Include(e => e.Host)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.Image,
                    e.Date,
                    e.CreatedAt,
                    e.HostId,
                    Hostname = e.Host.Username,
                });

            return Ok(events);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEvent(int id)
        {
            var evt = await ctx.Events
                .Include(e => e.Host)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.Image,
                    e.Date,
                    e.CreatedAt,
                    e.HostId,
                    Hostname = e.Host.Username,
                })
                .FirstAsync(e => e.Id == id);

            if (evt == null) return NotFound();

            return Ok(evt);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateEvent([FromForm] CreateEvent newEvent)
        {
            if (!ModelState.IsValid) return BadRequest("Check the model of data");

            var hostId = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));

            var image = newEvent.Image;
            var extension = Path.GetExtension(image.FileName).ToLower();

            List<string> validExtension = [".png", ".jpeg", ".jpg"];

            if (validExtension.IndexOf(extension) == -1)
            {
                return BadRequest("Image extension not allowed");
            }

            if (image.Length > 5 * 1024 * 1024)
            {
                return BadRequest("Image size should be less than 5MB");
            }

            var fileName = Guid.NewGuid().ToString() + extension;
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Events", fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var finalImagePath = Path.Combine("Events", fileName);



            var evt = new Event()
            {
                Name = newEvent.Name,
                Description = newEvent.Description,
                HostId = hostId,
                CreatedAt = DateTime.UtcNow,
                Image = finalImagePath,
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

            if (evt == null)
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
        public async Task<IActionResult> UpdateEvent([FromRoute] int id, [FromBody] CreateEvent newEvent)
        {
            if (!ModelState.IsValid) return BadRequest("Check the model of data");

            var evt = await ctx.Events.FindAsync(id);

            if (evt == null) return NotFound();

            var hostId = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));

            if (hostId != evt.HostId)
            {
                return Unauthorized("Not the host");
            }

            evt.Name = newEvent.Name;
            evt.Description = newEvent.Description;
            evt.CreatedAt = DateTime.UtcNow;
            evt.Image = ""; // TODO
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
