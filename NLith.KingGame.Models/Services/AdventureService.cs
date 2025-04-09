using Newtonsoft.Json;
using NLith.KingGame.Backend.Models;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    public class AdventureService
    {
        private TTSService ttsService;
        private string adventureMP3Root = "C:\\Users\\rex\\OneDrive\\Dokumente\\Audacity\\CCC\\KingGame\\Expeditions";


        IInlineInvokeProxy CPH;
        public AdventureService(IInlineInvokeProxy _CPH) 
        { 
            this.CPH = _CPH;
        }


        public void GenerateAdventure(string username)
        {
            MessageService msgService = new MessageService(CPH);

            CPH.LogInfo("Reading File ./kingly_expeditions.json into String");
            string adventuresFileText = File.ReadAllText("./kingly_expeditions.json");
            CPH.LogInfo("Deserializing into List<Adventure>");
            var deserializedAdventures = new List<Adventure>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(adventuresFileText));
            var ser = new DataContractJsonSerializer(deserializedAdventures.GetType());
            deserializedAdventures = ser.ReadObject(ms) as List<Adventure>;
            ms.Close();

            Random rng = new Random();
            

            CPH.LogInfo("Deserialized into " + deserializedAdventures.ToString());
            CPH.LogInfo("Length: " + deserializedAdventures.Count);
            Adventure adventure = deserializedAdventures[rng.Next(0, deserializedAdventures.Count - 1)];
            CPH.LogInfo("Picked adventure " + adventure.Title);            
            string pathToAdventureSoundfile = adventureMP3Root + Path.DirectorySeparatorChar+ adventure.Type + Path.DirectorySeparatorChar + adventure.Title.Replace(" ", "_") + ".mp3";
            CPH.LogInfo("Constructed Path to MP3: " + pathToAdventureSoundfile);

            CPH.LogInfo("Playing soundfile: " + pathToAdventureSoundfile);
            CPH.PlaySound(pathToAdventureSoundfile, 10f, true);

            WalletService walletService = new WalletService(CPH);
            UserService userService = new UserService(CPH);

            int reward = rng.Next(adventure.TreasureMinimum, adventure.TreasureMaximum);
            string message = string.Format("Congratulations {0}! Your expedition rewarded you with {1}{2}!", username, reward, ConfigService.CURRENCY_NAME);
            msgService.SendTwitchReply(message);
            walletService.AwardPlayerAmount(username, reward);
        }
    }
}
