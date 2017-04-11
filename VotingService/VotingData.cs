using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VotingService
{
    public class VotingData
    {
        public List<KeyValuePair<string, int>> VoteCounts { get; set; }
        public long TotalVotes { get; set; }
    }
}
