using Newtonsoft.Json;
using NLith.KingGame.Backend.Models.CYOAdventure;
using NLith.KingGame.Backend.Models.TwitchPolls;
using NLith.KingGame.Backend.Services;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.IO;

namespace NLith.KingGame.Backend.Services
{
    public class ChooseYourOwnAdventureService
    {
        readonly IInlineInvokeProxy CPH;

        readonly int VOTING_TIME_IN_SECONDS = 60;
        readonly string PATH_TO_CYOASoundfiles = "C:\\Users\\rex\\OneDrive\\Dokumente\\Audacity\\CCC\\CYOA";
        readonly string POLL_TITLE = "What should we do?";
        
        public ChooseYourOwnAdventureService(IInlineInvokeProxy _CPH)
        {
            this.CPH = _CPH;
        }


        public void RunCYOAAdventure()
        {
            AnnouncementService announcementService = new AnnouncementService(this.CPH);
            string jsonFilePath = "./the-ember-crown.json";


            CPH.LogInfo($"Attempting to deserialize {jsonFilePath}");

            string fileContent = File.ReadAllText(jsonFilePath);
            ChooseYourOwnAdventureBranch adventure = JsonConvert.DeserializeObject<ChooseYourOwnAdventureBranch>(fileContent);

            if (adventure != null)
            {
                CPH.LogInfo("Sucessfully deserialized Adventure into Object, starting Branch-Runner");
                if (!string.IsNullOrEmpty(adventure.Title))
                {
                    CPH.LogInfo($"Deserialized the adventure {adventure.Title}");
                    RunBranch(adventure);
                }
            }
        }

        private Choice GetPollResult(string pollName)
        {
            RootObject foo = PollService.GetRunningPolls(CPH);            
            PollData data = foo.data.Find(Poll => Poll.title.Equals(pollName));

            if (data != null && data.choices.Count > 0)
            {
                return DeterminePollWinner(data.choices);
            }
            return null;
        }

        private Choice DeterminePollWinner(List<Choice> choices)
        {
            int highestCount = 0;
            Choice winningChoice = null;
            choices.ForEach(choice =>
            {
                if (choice.votes > highestCount)
                {
                    highestCount = choice.votes;
                    winningChoice = choice;
                };
            });

            return winningChoice;
        }

        private void WaitForPollToEnd(int votingTime)
        {
            CPH.Wait(votingTime);
        }

        private void RunBranch(ChooseYourOwnAdventureBranch chooseYourOwnAdventureBranch)
        {
            VoteService votingService = new VoteService(CPH);
            CPH.LogInfo($"Running Branch {chooseYourOwnAdventureBranch.ID}: {chooseYourOwnAdventureBranch.Title}");

            List<ChooseYourOwnAdventureBranch> branches = chooseYourOwnAdventureBranch.Branches;
            // If there is more than 1 branch, it's a choice that should be put up for poll
            PlaySoundFile(chooseYourOwnAdventureBranch);
            if(!chooseYourOwnAdventureBranch.IsEnd)
            {
                CPH.LogInfo($"Branch {chooseYourOwnAdventureBranch.ID} has {chooseYourOwnAdventureBranch.Branches.Count} sub-branches");
                if (branches.Count > 1)
                {

                    List<String> choices = new List<string>();
                    foreach (var item in branches)
                    {
                        choices.Add(item.Title);
                    }
                    CPH.LogInfo($"Building Poll with {choices.Count} choices");
                    // These Lines use Twitch-Builtin Polls, which I do not have access to, so I built them myself.
                    //PutUpChoicesForPoll(choices, VOTING_TIME_IN_SECONDS);
                    //CPH.LogInfo($"Poll was started");
                    //WaitForPollToEnd(VOTING_TIME_IN_SECONDS);
                    //CPH.LogInfo($"Poll Ended, getting results");
                    //Choice winner = GetPollResult(POLL_TITLE);
                    //CPH.LogInfo($"Got Winner, determining branch-id");

                    int winner = votingService.StartPoll("NLithBot", "What should we do?", choices, VOTING_TIME_IN_SECONDS);
                    int branchToRun = branches.FindIndex(branch => branch.Title == choices[winner]);
                    CPH.LogInfo($"Running Branch {branches[branchToRun].ID} next");
                    RunBranch(branches[branchToRun]);
                }            
                else
                {
                    CPH.LogInfo($"No choices to poll on, Running Branch {branches[0].ID} next");
                    RunBranch(branches[0]);
                }
            } else
            {
                new TTSService(CPH).CallTTS(Models.VoiceTypes.ADVENTURE, "This story has been told, thank you for playing!", false, "");
            }
        }

        private void PutUpChoicesForPoll(List<string> choices, int votingTime)
        {
            foreach (var item in choices)
            {
                CPH.LogDebug($"Provided choice: {item}");
            }
            //PollService.CreatePoll(choices, votingTime);
            //CPH.TwitchPollCreate("What should we do?", choices, votingTime);            
        }

        private void PlaySoundFile(ChooseYourOwnAdventureBranch branch)
        {
            string PathToSoundFileForBranch = $"{PATH_TO_CYOASoundfiles}{Path.DirectorySeparatorChar}{branch.ID}.mp3";
            CPH.LogInfo($"Playing Soundfile {PathToSoundFileForBranch} for Branch {branch.ID}");
            CPH.PlaySound(PathToSoundFileForBranch, 1, true, branch.ID.ToString(), false);
        }
    }
}
