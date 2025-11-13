using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _events;
        private readonly IRegistrationService _regs;
        public EventsController(IEventService events, IRegistrationService regs)
        {
            _events = events;
            _regs = regs;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _events.GetAllAsync());

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateEventRequest req)
            => Ok(await _events.CreateAsync(req));

        [AllowAnonymous]
        [HttpPost("{eventId}/register")]
        public async Task<IActionResult> Register(int eventId, [FromBody] RegisterRequest req)
        {
            try { return Ok(await _regs.RegisterAsync(eventId, req)); }
            catch (KeyNotFoundException) { return NotFound(new { message = "Event not found" }); }
        }

        [Authorize]
        [HttpGet("{eventId}/registrations")]
        public async Task<IActionResult> Registrations(int eventId)
            => Ok(await _events.GetRegistrationsAsync(eventId));

        [Authorize]
        [HttpDelete("{eventId}")]
        public async Task<IActionResult> Delete(int eventId)
        {
            var deleted = await _events.DeleteAsync(eventId);
            return deleted ? NoContent() : NotFound(new { message = "Event not found" });
        }
    }
}
