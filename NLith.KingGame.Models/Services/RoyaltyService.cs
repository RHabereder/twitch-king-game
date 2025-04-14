using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return varSvc.GetGlobalVariable<string>(ConfigService.KINGS_SUCCESSOR_VAR_NAME);
        }

        /// <summary>
        ///     Abstracted Method to get the Username of the current King. 
        ///     Reduces redundant code and increases resilience towards API-Changes
        /// </summary>
        /// <returns>String that represents Name of the current King</returns>
        public string GetKingUsername()
        {
            VarService varService = new VarService(CPH);
            return varService.GetGlobalVariable<string>(ConfigService.KINGS_NAME_VAR_NAME);
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

            //CPH.TwitchTimeoutUser(userToJail, ConfigService.INITIAL_JAIL_TIME, reason);
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
            varSvc.SetGlobalVariable<string>(ConfigService.KINGS_SUCCESSOR_VAR_NAME, chosenSuccessor);

            AnnouncementService announcementSvc = new AnnouncementService(CPH);

            string announcement = string.Format("King {0} has chosen {1} as their new successor! Should our King pass, {1} will inherit the crown!", GetKingUsername(), chosenSuccessor);
            announcementSvc.IssueRoyalDecree(announcement);

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
