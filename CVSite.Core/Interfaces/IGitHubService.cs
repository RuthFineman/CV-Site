using CVSite.Core.DTOs;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVSite.Core.Interfaces
{
    public interface IGitHubService
    {
        Task<List<PortfolioRepoDto>> GetPortfolio();
        Task<List<PortfolioRepoDto>> SearchRepositories(string? repoName, string? language);
    }
}
