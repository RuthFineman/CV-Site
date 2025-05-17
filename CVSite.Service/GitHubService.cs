using Microsoft.Extensions.Options;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CVSite.Core.DTOs;
using CVSite.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Logging;

namespace CVSite.Service
{
    public class GitHubService : IGitHubService
    {
        private readonly GitHubClient _client;
        private readonly GitHubOptions _options;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GitHubService> _logger;

        public GitHubService(
            IOptions<GitHubOptions> options,
            IMemoryCache cache,
            ILogger<GitHubService> logger)
        {
            _options = options.Value;
            _cache = cache;
            _logger = logger;

            _client = new GitHubClient(new ProductHeaderValue("CVSiteApp"))
            {
                Credentials = new Credentials(_options.Token)
            };
        }

        public async Task<List<PortfolioRepoDto>> GetPortfolio()
        {
            string cacheKey = "portfolioCacheKey";

            if (_cache.TryGetValue(cacheKey, out CachedPortfolio cachedPortfolio))
            {
                try
                {
                    var latestUpdate = await GetLatestUpdateDate();

                    if (latestUpdate <= cachedPortfolio.LastUpdated)
                    {
                        // ה-cache טרי, מחזירים את המידע מה-cache
                        return cachedPortfolio.Repos;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get latest update date from GitHub. Returning cached data.");
                    // במקרה של שגיאה ב-GetLatestUpdateDate, נחזיר את המידע מה-cache
                    return cachedPortfolio.Repos;
                }
                // אחרת, נעדכן את ה-cache עם מידע חדש
            }

            // אם אין cache או שהמידע לא טרי - מושכים את המידע מחדש מה-GitHub
            var result = new List<PortfolioRepoDto>();

            try
            {
                var repos = await _client.Repository.GetAllForUser(_options.Username);

                foreach (var repo in repos)
                {
                    try
                    {
                        var languages = await _client.Repository.GetAllLanguages(_options.Username, repo.Name);
                        var pulls = await _client.PullRequest.GetAllForRepository(_options.Username, repo.Name);
                        IReadOnlyList<GitHubCommit> commits;

                        try
                        {
                            commits = await _client.Repository.Commit.GetAll(_options.Username, repo.Name);
                        }
                        catch (ApiException ex) when (ex.Message.Contains("Git Repository is empty"))
                        {
                            continue; // לדלג על ריפוזיטורי ריק
                        }

                        if (commits.Count == 0)
                            continue;

                        var languageNames = languages.Select(l => l.Name).ToList();

                        result.Add(new PortfolioRepoDto
                        {
                            Name = repo.Name,
                            Description = repo.Description,
                            Stars = repo.StargazersCount,
                            PullRequests = pulls.Count,
                            LastCommitDate = commits.FirstOrDefault()?.Commit.Author.Date,
                            Languages = languageNames,
                            Homepage = repo.Homepage
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Failed to process repo {repo.Name}, skipping.");
                    }
                }

                var latestUpdateDate = await GetLatestUpdateDate() ?? DateTimeOffset.UtcNow;

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                var newCacheValue = new CachedPortfolio
                {
                    Repos = result,
                    LastUpdated = latestUpdateDate
                };

                _cache.Set(cacheKey, newCacheValue, cacheEntryOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve portfolio data from GitHub.");
                if (_cache.TryGetValue(cacheKey, out CachedPortfolio fallbackCache))
                {
                    return fallbackCache.Repos;
                }
                throw; // אם אין גם cache, זורקים את השגיאה למעלה
            }

            return result;
        }

        private async Task<DateTimeOffset?> GetLatestUpdateDate()
        {
            try
            {
                var repos = await _client.Repository.GetAllForUser(_options.Username);

                DateTimeOffset? latestCommitDate = null;

                foreach (var repo in repos)
                {
                    try
                    {
                        var commits = await _client.Repository.Commit.GetAll(_options.Username, repo.Name);
                        if (commits.Count > 0)
                        {
                            var commitDate = commits.First().Commit.Author.Date;
                            if (latestCommitDate == null || commitDate > latestCommitDate)
                                latestCommitDate = commitDate;
                        }
                    }
                    catch (ApiException ex) when (ex.Message.Contains("Git Repository is empty"))
                    {
                        continue;
                    }
                }

                return latestCommitDate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get latest commit date from GitHub");
                return null;
            }
        }

        public async Task<List<PortfolioRepoDto>> SearchRepositories(string? repoName, string? language)
        {
            var searchRequest = new SearchRepositoriesRequest(repoName ?? "");

            if (!string.IsNullOrWhiteSpace(language))
            {
                if (Enum.TryParse<Language>(language, ignoreCase: true, out var langEnum))
                {
                    searchRequest.Language = langEnum;
                }
            }

            searchRequest.User = _options.Username;

            var searchResult = await _client.Search.SearchRepo(searchRequest);

            var result = searchResult.Items.Select(repo => new PortfolioRepoDto
            {
                Name = repo.Name,
                Description = repo.Description,
                Stars = repo.StargazersCount,
                PullRequests = 0, // אין מידע בפונקציית חיפוש, אפשר להשאיר 0
                LastCommitDate = repo.UpdatedAt.DateTime, // או createdAt לפי העדפה
                Languages = new List<string>(), // אין מידע בשלב זה
                Homepage = repo.Homepage
            }).ToList();

            return result;
        }
    }
}
