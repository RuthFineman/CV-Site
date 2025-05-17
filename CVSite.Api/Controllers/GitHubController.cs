using CVSite.Core.DTOs;
using CVSite.Core.Interfaces;
//using CVSite.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CVSite.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase
    {
        private readonly IGitHubService _gitHubService;

        public GitHubController(IGitHubService gitHubService)
        {
            _gitHubService = gitHubService;
        }

        [HttpGet("portfolio")]
        public async Task<IActionResult> GetPortfolio()
        {
            var portfolio = await _gitHubService.GetPortfolio();
            return Ok(portfolio);
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchRepositories([FromQuery] string? repoName, [FromQuery] string? language)
        {
            var results = await _gitHubService.SearchRepositories(repoName, language);
            return Ok(results); // החזרת רשימת PortfolioRepoDto בתוך IActionResult
        }
    }
}
