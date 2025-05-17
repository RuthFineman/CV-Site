using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVSite.Core.DTOs
{
    public class PortfolioRepoDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Stars { get; set; }
        public int PullRequests { get; set; }
        public DateTimeOffset? LastCommitDate { get; set; }
        public List<string> Languages { get; set; }
        public string Homepage { get; set; }
    }
}
