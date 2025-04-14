using NLith.KingGame.Backend.Models;
using Streamer.bot.Plugin.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NLith.KingGame.Backend.Services
{
    public class VoteService
    {
        readonly IInlineInvokeProxy CPH;
        public VoteService(IInlineInvokeProxy _CPH) 
        { 
            this.CPH = _CPH;
        }

        public int StartPoll(string redeemer, string title, List<string> choices, int timeToVoteInSeconds)
        {
            CPH.LogDebug($"{redeemer} started Vote with Title {title} and {choices.Count} choices, users have {timeToVoteInSeconds} seconds to Vote");

            Poll runningPoll = GetRunningPoll();
            // Poll is empty, so probably reset or not running yet
            // So we build a new one and save it
            if (runningPoll == null || string.IsNullOrEmpty(runningPoll.Title))
            {
                runningPoll = new Poll() { Title = title, Choices = choices};
                SetRunningPoll(runningPoll);
            }
            CPH.LogDebug("Starting vote now");
            AnnouncePoll(redeemer, title, choices, timeToVoteInSeconds);
            CPH.LogDebug("Waiting until end of vote now");
            WaitUntilEndOfVote(timeToVoteInSeconds);
            CPH.LogDebug("Collecting Votes now");
            int winningChoice = CollectVotes();
            CPH.LogDebug("Deleting PollState now");
            DeletePollState();

            return winningChoice;
        }

        private void SetRunningPoll(Poll runningPoll)
        {
            VarService varSvc = new VarService(CPH);
            varSvc.SetGlobalVariable<Poll>(ConfigService.POLL_VAR_NAME, runningPoll);
        }

        private Poll GetRunningPoll()
        {
            VarService varSvc = new VarService(CPH);
            CPH.LogDebug("Getting POLL_VAR_NAME from varSvc");

            Poll runningPoll = varSvc.GetGlobalVariable<Poll>(ConfigService.POLL_VAR_NAME);
            if(runningPoll == null)
            {
                CPH.LogDebug("Poll is null, creating a new one");
                runningPoll = new Poll() { Votes = new Dictionary<string, int>(), Choices = new List<string>(), Title = "" };
            }

            return runningPoll;
        }

        public void VoteOnOption(string redeemer, int chosenOption)
        {
            Poll runningPoll = GetRunningPoll();
            CPH.LogDebug($"{redeemer} voted {chosenOption}");
            // If a vote exists, replace it, otherwise add it
            if (runningPoll != null)
            {
                CPH.LogDebug($"Found the following Poll: {runningPoll.Title}");
                if(runningPoll.Votes == null)
                {
                    CPH.LogDebug($"Poll hat no Votes List, initializing again");
                    runningPoll.Votes = new Dictionary<string, int>();
                } 

                if (runningPoll.Votes.ContainsKey(redeemer))
                {
                    CPH.LogDebug($"{redeemer} voted {runningPoll.Votes[redeemer]} already, resetting to {chosenOption}");
                    runningPoll.Votes.Remove(redeemer);
                    runningPoll.Votes.Add(redeemer, chosenOption);
                }
                else
                {
                    CPH.LogDebug($"{redeemer} casted new vote to {chosenOption}");
                    runningPoll.Votes.Add(redeemer, chosenOption);
                }
                SetRunningPoll(runningPoll);
            } else
            {
                CPH.LogDebug("Didn't find a poll in the state");
            }            
        }

        

        public void AnnouncePoll(string redeemer, string title, List<string> choices, int timeToVoteInSeconds)
        {
            TTSService ttsSvc = new TTSService(CPH);
            string announcement = $"A poll has started: {title}. You have the following options: ";
            ttsSvc.CallTTS(VoiceTypes.NEWS, announcement, false, redeemer);
            for (int i = 0; i < choices.Count; i++)
            {
                ttsSvc.CallTTS(VoiceTypes.NEWS, $"Option {i + 1}: {choices[i]}", false, redeemer);
            }
            ttsSvc.CallTTS(VoiceTypes.NEWS, $"Use the Vote-Command !vote with the number you want to vote on", false, redeemer);
            ttsSvc.CallTTS(VoiceTypes.NEWS, $"Your Time starts now, you have {timeToVoteInSeconds} seconds", false, redeemer);
            CPH.TwitchAnnounce("Vote with !vote <number>");
        }

        private void WaitUntilEndOfVote(int timeToVoteInSeconds)
        {
            TTSService ttsSvc = new TTSService(CPH);
            int waitSecondsInMillis = (timeToVoteInSeconds * 1000);
            CPH.LogDebug($"Waiting for {waitSecondsInMillis} milliseconds");
            CPH.Wait(waitSecondsInMillis);
            string announcement = $"Time is up chat! The vote has ended and will now be tallied";
            ttsSvc.CallTTS(VoiceTypes.NEWS, announcement, false, null);
            CPH.Wait(5000);
        }
        private int CollectVotes()
        {
            Dictionary<int, int> votes = new Dictionary<int, int>();
            Poll currentPoll = GetRunningPoll();
            
            foreach (var vote in currentPoll.Votes.Values)
            {
                if (votes.ContainsKey(vote))
                {
                    votes[vote]++;
                }
                else
                {
                    votes[vote] = 1;
                }
            }

            foreach (KeyValuePair<string, int> item in currentPoll.Votes)
            {
                CPH.LogDebug($"User {item.Key} voted option {item.Value} ");
            }
            foreach (KeyValuePair<int, int> item in votes)
            {
                CPH.LogDebug($"Option {item.Key} has {item.Value} votes");
            }
            int winningValue = votes.OrderByDescending(vc => vc.Value).First().Key;
            CPH.LogDebug($"Returning Value {winningValue}");
            // We have to subtract 1, because the choices are Zero-Based, while Vote Options are not
            return winningValue-1;
        }
        private void DeletePollState()
        {
            VarService varSvc = new VarService(CPH);
            varSvc.SetGlobalVariable(ConfigService.POLL_VAR_NAME, new Poll());
        }

    }
}
