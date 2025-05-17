using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVSite.Core.DTOs
{
    public class CachedPortfolio
    {
        public List<PortfolioRepoDto> Repos { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}
