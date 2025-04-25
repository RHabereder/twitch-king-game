using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Models
{
    public class Reward
    {
        public bool IsCashReward { get; set; }
        public int RewardValue { get; set; }

        public bool IsTreasureReward { get; set; }
        public Treasure RewardItem { get; set; }

    }
}
