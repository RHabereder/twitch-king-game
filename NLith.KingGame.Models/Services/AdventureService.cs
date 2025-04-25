using Newtonsoft.Json;
using NLith.KingGame.Backend.Models;
using NLith.TwitchLib.Services;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.IO;

namespace NLith.KingGame.Backend.Services
{
    public class AdventureService
    {
        private string soundfilesRoot = "C:\\Users\\rex\\OneDrive\\Dokumente\\Audacity\\CCC\\KingGame\\";        

        private string adventuresJSON = $"{ConfigService.PATH_ADVENTURES_DIR}/adventures.json";
        private string expeditionJSON = $"{ConfigService.PATH_EXPEDITIONS_DIR}/expeditions.json";

        IInlineInvokeProxy CPH;
        public AdventureService(IInlineInvokeProxy _CPH) 
        { 
            this.CPH = _CPH;
        }


        public void GenerateAdventure(string username)
        {
            RunEvent(AdventureType.ADVENTURE, soundfilesRoot + "Adventures", username);        
        }

        public void GenerateExpedition(string username)
        {
            RunEvent(AdventureType.EXPEDITION, soundfilesRoot + "Expeditions", username);
        }


        private void RunEvent(AdventureType type, string pathToSoundfiles, string adventurer)
        {
            MessageService msgService = new MessageService(CPH);
            WalletService walletService = new WalletService(CPH);
            UserService userService = new UserService(CPH);

            string pathToEventFile = "";

            bool hasEndings = false;
            switch (type)
            {
                case AdventureType.ADVENTURE:
                    pathToEventFile = adventuresJSON;
                    hasEndings = true;
                    break;
                case AdventureType.EXPEDITION:
                    pathToEventFile = expeditionJSON;
                    break;
            }
            string fileContent = File.ReadAllText(pathToEventFile);
            List<Adventure> deserializedAdventures = JsonConvert.DeserializeObject<List<Adventure>>(fileContent);

            Random rng = new Random();


            CPH.LogInfo("Deserialized into " + deserializedAdventures.ToString());
            CPH.LogInfo("Length: " + deserializedAdventures.Count);
            Adventure adventure = deserializedAdventures[rng.Next(0, deserializedAdventures.Count)];

            CPH.LogInfo("Picked adventure " + adventure.Title);
            string pathToAdventureSoundfile = pathToSoundfiles + Path.DirectorySeparatorChar + adventure.Type + Path.DirectorySeparatorChar + adventure.Title.Replace(" ", "_") + ".mp3";
            CPH.LogInfo("Constructed Path to MP3: " + pathToAdventureSoundfile);

            CPH.LogInfo("Playing soundfile: " + pathToAdventureSoundfile);
            CPH.PlaySound(pathToAdventureSoundfile, 10f, true);

            string message;
            int reward = rng.Next(adventure.TreasureMinimum, adventure.TreasureMaximum);

            if (hasEndings)
            {
                bool hasSucceeded = (rng.Next(1, 3) == 1);
                string pathToEnding = pathToSoundfiles + Path.DirectorySeparatorChar + adventure.Type + Path.DirectorySeparatorChar + adventure.Title.Replace(" ", "_");

                if (hasSucceeded)
                {
                    pathToEnding += "_success.mp3";
                    message = string.Format("Congratulations {0}! Your {3} rewarded you with {1} {2}!", adventurer, reward, ConfigService.CURRENCY_SYMBOL, type.ToString().ToLower());
                }
                else
                {
                    pathToEnding += "_failure.mp3";
                    message = string.Format("Unfortunately your {3} ended in failure! Your Hospital Bill will cost you {1} {2}!", adventurer, reward, ConfigService.CURRENCY_SYMBOL, type.ToString().ToLower());
                }
                CPH.LogInfo("Playing soundfile: " + pathToEnding);
                CPH.PlaySound(pathToEnding, 1, true);
            }
            else
            {
                message = string.Format("Congratulations {0}! Your {3} rewarded you with {1} {2}!", adventurer, reward, ConfigService.CURRENCY_SYMBOL, type.ToString().ToLower());
            }

            msgService.SendTwitchReply(message);
            walletService.AwardPlayerAmount(adventurer, reward);
        }

        
    }
}
