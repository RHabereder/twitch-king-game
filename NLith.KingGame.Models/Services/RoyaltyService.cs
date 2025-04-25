using NLith.TwitchLib.Models;
using NLith.TwitchLib.Services;
using Streamer.bot.Plugin.Interface;
using System;

namespace NLith.KingGame.Backend.Services
{
    public class RoyaltyService
    {
        IInlineInvokeProxy CPH;
        public RoyaltyService(IInlineInvokeProxy _CPH)
        {
            CPH = _CPH;
        }

        /// <summary>
        ///     Returns the Kings Successor
        /// </summary>
        /// <returns></returns>
        public string GetKingsSuccessor()
        {
            VarService varSvc = new VarService(CPH);
            return varSvc.GetGlobalVariable<string>(ConfigService.KINGS_SUCCESSOR_VAR_NAME, ConfigService.IS_GAME_PERSISTENT);
        }

        /// <summary>
        ///     Abstracted Method to get the Username of the current King. 
        ///     Reduces redundant code and increases resilience towards API-Changes
        /// </summary>
        /// <returns>String that represents Name of the current King</returns>
        public string GetKingUsername()
        {
            VarService varService = new VarService(CPH);
            return varService.GetGlobalVariable<string>(ConfigService.KINGS_NAME_VAR_NAME, ConfigService.IS_GAME_PERSISTENT);
        }

        /// <summary>
        ///     Streamerbot Action that returns the current King
        /// </summary>
        /// <returns></returns>
        public void GetSuccessor()
        {
            MessageService msgService = new MessageService(CPH);

            string king = GetKingUsername();
            string successor = GetKingsSuccessor();

            if (KingHasSuccessor())
            {
                msgService.SendTwitchReply(string.Format("King {0} set {1} as their successor", king, successor));
            }
            else
            {
                msgService.SendTwitchReply(string.Format("King {0} has currently no successor!", king));
            }
        }

        /// <summary>
        ///     Internal Method to jail (Timeout) a user for a predefined amount of time
        /// </summary>
        /// <param name="inmate">String that represents a potentially unsanitized Username</param>
        /// <param name="reason">Reason to give the user for being Jailed</param>
        /// <seealso cref="ConfigService.INITIAL_JAIL_TIME"/>
        public void JailUser(string inmate, string reason)
        {
            UserService userService = new UserService(CPH);
            string userToJail = userService.SanitizeUsername(inmate);

            CPH.TwitchTimeoutUser(userToJail, ConfigService.INITIAL_JAIL_TIME, reason);
            CPH.PlaySoundFromFolder("C:\\Users\\rex\\OneDrive\\Dokumente\\Audacity\\CCC\\KingGame\\Jail", 1, false, true);
            string announcement = string.Format("User {0} has been Jailed for {1} seconds for: {2}", userToJail, ConfigService.INITIAL_JAIL_TIME, reason);
            new AnnouncementService(CPH).AnnounceToAudience(announcement, null);
        }

        /// <summary>
        ///     Checks if a King has a successor set
        /// </summary>
        /// <returns></returns>
        public bool KingHasSuccessor()
        {
            return !(string.IsNullOrEmpty(GetKingsSuccessor()));
        }

        /// <summary>
        ///     Wrapper to set a kings successor Global-Var
        /// </summary>
        /// <param name="chosenSuccessor"></param>
        public void SetKingsSuccessor(string chosenSuccessor)
        {
            VarService varSvc = new VarService(CPH);
            varSvc.SetGlobalVariable<string>(ConfigService.KINGS_SUCCESSOR_VAR_NAME, chosenSuccessor, ConfigService.IS_GAME_PERSISTENT);

            AnnouncementService announcementSvc = new AnnouncementService(CPH);

            string announcement = string.Format("King {0} has chosen {1} as their new successor! Should our King pass, {1} will inherit the crown!", GetKingUsername(), chosenSuccessor);
            IssueRoyalDecree(announcement);

        }

        public void IssueRoyalDecree(string decree)
        {
            string queenVoiceID = ConfigService.VOICE_TYPE_VOICE_ID_MAPPING[VoiceTypes.QUEEN];
            string kingVoiceID = ConfigService.VOICE_TYPE_VOICE_ID_MAPPING[VoiceTypes.KING];
            TTSService tts = new TTSService(CPH);
            CPH.PlaySoundFromFolder("C:\\Users\\rex\\OneDrive\\Dokumente\\Audacity\\CCC\\Fanfare", 1, false, true);
            if (new RoyaltyService(CPH).GetKingUsername().Equals("hey_zelfa"))
            {
                tts.CallTTS(queenVoiceID, decree, false);
            }
            else
            {
                tts.CallTTS(kingVoiceID, decree, false);
            }

            CPH.TwitchAnnounce(decree, true, ConfigService.TWITCH_ANNOUNCE_COLOR_DEFAULT, true);
        }


        /// <summary>
        ///     Internal Function to set/extend a Kings Protection
        ///     Should make the king invulnerable to Attacks and cost a set fee
        /// </summary>
        /// <seealso cref="ConfigService.KINGS_PROTECTION_FEE"/>
        /// <seealso cref="ConfigService.KINGS_PROTECTION_LENGTH"/>
        public void GoIntoHiding()
        {

            WalletService walletService = new WalletService(CPH);
            walletService.FinePlayerAmount(GetKingUsername(), ConfigService.KINGS_PROTECTION_FEE);

            SetKingsProtection(5);
            // Implement a timer for the Kings Protection
            // Currently the "Protection" is just the Global Command Cooldown, which can't be controlled from the CPH Class
            // Making it a proper timer and activating it on demand should suffice
            
        }

        /// <summary>
        ///     Internal function to set a kings Protection 
        /// </summary>
        /// <param name="timeInMinutes">Amount of Time the kings protection should be set/expanded by</param>
        /// <exception cref="NotImplementedException"></exception>
        private void SetKingsProtection(int timeInMinutes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Internal function to check if a king has protection 
        /// </summary>
        /// <returns></returns>
        public bool IsKingProtected()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///     Internal Method to Check if the Redeeming user Is King
        /// </summary>
        /// <returns></returns>
        public bool UserIsKing(string username)
        {
            return username.ToLower().Equals(GetKingUsername().ToLower());
        }
    }
}
