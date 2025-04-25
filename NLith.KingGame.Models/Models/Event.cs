using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Models
{
    public class Event
    {
        public string EventText { get; set; }
        public string EventTitle { get; set; }
        
        public int InjuryChance { get; set; }
        

        public bool YieldsReward { get; set; }
        public List<Reward> PossibleRewards { get; set; }

        
        public bool IsVoicedByTTS { get; set; }
        public string PathToSoundfile { get; set; }
        public bool HasMultipleEndings { get; set; }
        public List<string> Endings { get; set; }

    }
}
