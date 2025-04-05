using HackNewsApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HackNewsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoriesController : ControllerBase
    {
        private readonly HackerNewsService _service;
        public StoriesController(HackerNewsService service) 
        {
            _service = service;
        }
        [HttpGet("{count}")]
        public async Task<IActionResult> GetTopStories (int count)
        {
            if (count < 1 || count > 500)
                return BadRequest("Cantidad debe estar entre 1 y 500");
            var stories = await _service.GetTopStoriesAsync(count);
            return Ok(stories);
        }
    }
}
