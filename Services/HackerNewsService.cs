namespace HackNewsApi.Services
{
    using HackNewsApi.Models;
    using Microsoft.Extensions.Caching.Memory;
    using System.Net.Http.Json;
    using System.Text.Json;

    public class HackerNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        public HackerNewsService(HttpClient httpClient, IMemoryCache memoryCache)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
        }

        public async Task<List<StoryDto>> GetTopStoriesAsync(int count)
        {
            var cacheKey = $"topstories-{count}";
            if (_memoryCache.TryGetValue(cacheKey, out List<StoryDto>? cachedStories))
            {
                return cachedStories;
            }
            var storyIds = await _httpClient.GetFromJsonAsync<List<int>?>("https://hacker-news.firebaseio.com/v0/beststories.json");

            var topIds = storyIds.Take(500);  // limitar a 500
            var tasks = topIds.Select(id => GetStoryById(id));
            var stories = await Task.WhenAll(tasks);
            var topN = stories
            .Where(s => s != null)
            .OrderByDescending(s => s.Score)
            .Take(count)
            .ToList();
            _memoryCache.Set(cacheKey, topN, TimeSpan.FromMinutes(5));
            return topN;

        }

        private async Task<StoryDto> GetStoryById(int id)
        {
            try
            {
                var story = await _httpClient.GetFromJsonAsync<JsonElement>($"https://hacker-news.firebaseio.com/v0/item/{id}.json");
                if (story.ValueKind != JsonValueKind.Object) return null;
                return new StoryDto
                {
                    Title = story.GetProperty("title").GetString(),
                    Uri = story.TryGetProperty("url", out var uriProp)?uriProp.GetString():"",
                    PostedBy = story.GetProperty("by").GetString(),
                    Time = DateTimeOffset.FromUnixTimeSeconds(story.GetProperty("time").GetInt64()).UtcDateTime,
                    Score = story.GetProperty("score").GetInt32(),
                    CommentCount = story.TryGetProperty("descendants", out var comments) ? comments.GetInt32() : 0,
                };
            }
            catch
            {
                return null;
            }

        }


    }
}