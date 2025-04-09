using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Models
{
    public class Adventure
    {
        public string Title { get; set; }
        public string Story { get; set; }
        public int LootRarity { get; set; }
        public int TreasureMinimum { get; set; }
        public int TreasureMaximum { get; set; }
        public string Type { get; set; }
    }
}
