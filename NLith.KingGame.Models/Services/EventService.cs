using Newtonsoft.Json;
using NLith.KingGame.Backend.Models;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.IO;

namespace NLith.KingGame.Backend.Services
{
    public class EventService
    {
        readonly IInlineInvokeProxy CPH;

        public List<Event> allEvents;


        public EventService(IInlineInvokeProxy _CPH)
        {
            this.CPH = _CPH;
            allEvents = ReadEventsFromDisk();
        }

        public List<Event> ReadEventsFromDisk()
        {
            string pathToEventsFile = ConfigService.PATH_EVENTS_FILE;

            string fileContent = File.ReadAllText(pathToEventsFile);
            List<Event> events = JsonConvert.DeserializeObject<List<Event>>(fileContent);

            events.ForEach(e => {
                CPH.LogInfo(e.EventTitle);
                e.PossibleRewards.ForEach(reward =>
                {
                    CPH.LogInfo(reward.RewardValue.ToString());
                });                

            });

            return events;
        }

        public Event GenerateEvent()
        {
            if (allEvents != null && allEvents.Count > 0)
            {
                Random rng = new Random();
                return allEvents[rng.Next(allEvents.Count-1)];
            }
            return null;
        }

    }
}
