using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Models.CYOAdventure
{
    [Serializable]
    [JsonObject]
    internal class ChooseYourOwnAdventureBranch
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public List<ChooseYourOwnAdventureBranch> Branches { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Subtitle { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public bool IsEnd { get; set; }
    }
}
