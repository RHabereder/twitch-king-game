using Streamer.bot.Plugin.Interface;
using Streamer.bot.Plugin.Interface.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    public class UserService
    {
        IInlineInvokeProxy CPH;

        public UserService(IInlineInvokeProxy _CPH)
        {
            this.CPH = _CPH;
        }

        /// <summary>
        ///     Users may use @username for easier identification of users in chat. 
        ///     But most CPH Commands don't like that, so we need to sanitize the username and remove the @-Character
        /// </summary>
        /// <param name="username">Username to sanitize</param>
        /// <returns>A sanitized String of a username</returns>
        public string SanitizeUsername(string username)
        {
            if (username.StartsWith("@"))
                username = username.Substring(1);

            TwitchUserInfo userInfo = CPH.TwitchGetUserInfoByLogin(username);
            return userInfo.UserName;
        }

        /// <summary>
        ///     Gets the current users name from the Args
        /// </summary>
        /// <returns></returns>
        public string GetCurrentUserName(Dictionary<string, object> args)
        {
            if (args["user"] != null && args["user"].GetType() == typeof(string))
            {
                return args["user"].ToString();
            }
            return "";
        }
    }
}
