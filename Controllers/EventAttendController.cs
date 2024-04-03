using EventLink.Data;
using EventLink.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventLink.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class EventAttendController : ControllerBase
    {
        private readonly AppDbContext ctx;

        public EventAttendController(AppDbContext context)
        {
            ctx = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> AllAttendees(int id)
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

            if (evt.HostId == userId)
            {
                var complete_detail_of_attendees =
                    attendees
                    .Select(ea => new { ea.UserId, UserName = ea.User.Username, ea.User.Email })
                    .ToList();

                return Ok(complete_detail_of_attendees);
            }


            return Ok(attendees.Select(ea => new { ea.UserId, UserName = ea.User.Username }).ToList());
        }


        [HttpPost("{id}")]
        public async Task<IActionResult> AttendEvent(int id)
        {
            var evt = await ctx.Events.FindAsync(id);

            if (evt == null)
            {
                return NotFound("No such event");
            }

            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));

            if (userId == evt.HostId)
            {
                return BadRequest("Host can't be an attendee of an event");
            }

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
                return NotFound("No attendee details on this event");
            }

            ctx.EventAttendees.Remove(oldAttendDetails);

            await ctx.SaveChangesAsync();
            return Accepted("Deleted The Attendee from this Event");
        }


        [HttpGet("CheckAttending/{id}")]
        public async Task<IActionResult> CheckAttending(int id)
        {
            var evt = await ctx.Events.FindAsync(id);

            if (evt == null)
            {
                return NotFound("No such event");
            }

            var userId = Convert.ToInt32(HttpContext.User.FindFirstValue("UserId"));


            var attendingEvent = await ctx.EventAttendees.FindAsync(userId, id);

            if (attendingEvent == null)
            {
                return Ok(new
                {
                    Result = false,
                    Message = "Not attending this event"
                });
            }

            return Ok(new
            {
                Result = true,
                Message = ""
            });
        }
    }
}
