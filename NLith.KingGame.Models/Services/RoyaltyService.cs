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
            CPH.PlaySoundFromFolder("C:\\Users\\rex\\OneDrive\\Dokumente\\Audacity\\CCC\\KingGame\\Jail", 75, false, true);
            string announcement = string.Format("User {0} has been Jailed for {1} seconds for: {2}", userToJail, ConfigService.INITIAL_JAIL_TIME, reason);
            new AnnouncementService(CPH).AnnounceToAudience(announcement, null);
        }       

        /// <summary>
        ///     Internal Method to Check if the Redeeming user Is King
        /// </summary>
        /// <returns></returns>
        public bool UserIsKing(string username)
        {
            UserService userService = new UserService(CPH);
            return username.ToLower().Equals(GetKingUsername().ToLower());
        }
    }
}
