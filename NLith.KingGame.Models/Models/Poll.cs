using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Models
{

    [Serializable]
    internal class Poll
    {
        public Poll() { }

        public string Title { get; set; }
        public List<String> Choices {  get; set; }
        public Dictionary<string, int> Votes { get; set; }        
    }
}
