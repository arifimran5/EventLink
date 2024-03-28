using EventLink.Data;
using EventLink.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventLink.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventAttendController : ControllerBase
    {
        private readonly AppDbContext ctx;

        public EventAttendController(AppDbContext context)
        {
            ctx = context;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> AllAttendees(int id, [FromQuery] bool complete)
        {
            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));
            var evt = await ctx.Events.FindAsync(id);

            if (evt == null)
            {
                return NotFound("No such event");
            }

            var attendees = ctx.EventAttendees
                .Include(ea => ea.User)
                .Include(ea => ea.Event)
                .Where(ea => ea.EventId == id);

            if (complete)
            {
                if(evt.HostId != userId)
                {
                    return NotFound("Not the host of the event");
                }

                var complete_detail_of_attendees = 
                    attendees
                    .Select(ea => new { ea.UserId, UserName = ea.User.Username, ea.User.Email })
                    .ToList();

                return Ok(complete_detail_of_attendees);
            }


            return Ok(attendees.Select(ea => new { ea.UserId, UserName = ea.User.Username }).ToList());
        }


        [HttpPost("{id}")]
        [Authorize]
        public async Task<IActionResult> AttendEvent(int id)
        {
            var evt = await ctx.Events.FindAsync(id);

            if (evt == null)
            {
                return NotFound("No such event");
            }

            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));

            var oldAttendDetails = await ctx.EventAttendees.FindAsync(userId, id);

            if (oldAttendDetails != null)
            {
                return BadRequest("Already attending the event");
            }

            var newAttendDetail = new EventAttendee()
            {
                EventId = id,
                UserId = userId
            };

            ctx.EventAttendees.Add(newAttendDetail);
            try
            {
                await ctx.SaveChangesAsync();
                return Created();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> RemoveAttendee(int id)
        {
            var evt = await ctx.Events.FindAsync(id);

            if (evt == null)
            {
                return NotFound("No such event");
            }

            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));


            var oldAttendDetails = await ctx.EventAttendees.FindAsync(userId, id);

            if (oldAttendDetails == null)
            {
                return BadRequest("No attendee details on this event");
            }

            ctx.EventAttendees.Remove(oldAttendDetails);

            await ctx.SaveChangesAsync();
            return Accepted("Deleted The Attendee from this Event");
        }

    }
}
