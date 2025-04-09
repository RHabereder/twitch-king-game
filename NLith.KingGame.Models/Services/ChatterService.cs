using Streamer.bot.Plugin.Interface;
using Streamer.bot.Plugin.Interface.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    /// <summary>
    ///     Extraction of Logic concerning Chatters into it's own service
    /// </summary>
    public class ChatterService
    {
        private IInlineInvokeProxy CPH;
        public string PATH_ACTIVE_CHATTERS_FILE;          // Path to File where active Chatters should be written to (should be truncated on every start)
        public List<string> MONITOR_KNOWN_BOTS;
        public bool MONITOR_BROADCASTER_AS_CHATTER = true;
        public bool MONITOR_BOTS_AS_CHATTER = false;
        public string[] MONITOR_DENY_LIST = new string[] { };

        /// <summary>
        /// Minimal Constructor to inject the CPH, offers sensible Defaults
        /// </summary>
        /// <param name="_CPH">CPH Proxy that is created in the Main-Class</param>        
        public ChatterService(IInlineInvokeProxy _CPH)
        {
            this.CPH = _CPH;
            this.PATH_ACTIVE_CHATTERS_FILE = "./activeChatters.txt";
            this.MONITOR_KNOWN_BOTS = new List<string>() { };
            this.MONITOR_BOTS_AS_CHATTER = false;
            this.MONITOR_BROADCASTER_AS_CHATTER = false;
            this.MONITOR_DENY_LIST = new string[] { };
        }

        /// <summary>
        /// All-Args Constructor to inject the CPH  
        /// </summary>
        /// <param name="_CPH"></param>
        /// <param name="_PathActiveChattersFile"></param>
        /// <param name="_ListOfChannelBots"></param>
        /// <param name="_MonitorBroadcasterAsChatter"></param>
        /// <param name="_MonitorBotsAsChatter"></param>
        /// <param name="_MonitorDenyList"></param>
        public ChatterService(IInlineInvokeProxy _CPH, string _PathActiveChattersFile, List<string> _ListOfChannelBots, bool _MonitorBroadcasterAsChatter, bool _MonitorBotsAsChatter, string[] _MonitorDenyList)
        {
            this.CPH = _CPH;
            this.PATH_ACTIVE_CHATTERS_FILE = _PathActiveChattersFile;
            this.MONITOR_KNOWN_BOTS = _ListOfChannelBots;
            this.MONITOR_DENY_LIST = _MonitorDenyList;
            this.MONITOR_BOTS_AS_CHATTER = _MonitorBotsAsChatter;
            this.MONITOR_BROADCASTER_AS_CHATTER = _MonitorBroadcasterAsChatter;
        }

        public List<TwitchUserInfo> GetAllChattersUserInfo()
        {
            // Check if Monitoring File exists
            List<TwitchUserInfo> allUserInfos = new List<TwitchUserInfo>();
            if (File.Exists(PATH_ACTIVE_CHATTERS_FILE))
            {
                String[] chatters = File.ReadAllLines(PATH_ACTIVE_CHATTERS_FILE);
                foreach (var chatter in chatters)
                {
                    allUserInfos.Add(CPH.TwitchGetUserInfoByLogin(chatter));
                }
            }
            return allUserInfos;
        }

        /// <summary>
        ///     Convenience Function to read the Active-Chatters File and return a list of Names
        /// </summary>
        /// <returns>
        ///     List of all active Chatters from this stream
        /// </returns>
        /// <seealso cref="PATH_ACTIVE_CHATTERS_FILE"/>
        public List<string> GetAllChatters()
        {
            // Check if Monitoring File exists
            List<String> chatters = new List<string>();
            if (File.Exists(PATH_ACTIVE_CHATTERS_FILE))
            {
                chatters = new List<string>(File.ReadAllLines(PATH_ACTIVE_CHATTERS_FILE));
            }
            return chatters;
        }

        /// <summary>
        ///     Convenience-Method to get a random chatters TwitchUserInfo Object
        /// </summary>
        /// <returns>
        ///     A String representing a random chatters username
        /// </returns>
        /// <seealso cref="PATH_ACTIVE_CHATTERS_FILE"/>
        public string GetRandomChatter()
        {
            if (File.Exists(PATH_ACTIVE_CHATTERS_FILE))
            {
                List<String> chatters = new List<string>(File.ReadAllLines(PATH_ACTIVE_CHATTERS_FILE));
                Random rand = new Random();
                return chatters[rand.Next(0, chatters.Count)];
            }
            return null;
        }

        /// <summary>
        ///     Convenience-Method to get a random chatters TwitchUserInfo Object
        /// </summary>
        /// <returns>
        ///     A TwitchUserInfo object for a random chatter
        /// </returns>
        /// <seealso cref="PATH_ACTIVE_CHATTERS_FILE"/>
        public TwitchUserInfo GetRandomChatterUserInfo()
        {
            if (File.Exists(PATH_ACTIVE_CHATTERS_FILE))
            {
                List<String> chatters = new List<string>(File.ReadAllLines(PATH_ACTIVE_CHATTERS_FILE));
                Random rand = new Random();
                return CPH.TwitchGetUserInfoByLogin(chatters[rand.Next(0, chatters.Count)]);
            }
            return null;
        }

        /// <summary>
        ///     Convenience-Method to get a TwitchUserInfo Object for a specified activeChatter
        /// </summary>
        /// <param name="name">
        ///     The name of the User to pull Info for
        /// </param>
        /// <returns>
        ///     A TwitchUserInfo object for a specified chatter
        /// </returns>
        /// <seealso cref="PATH_ACTIVE_CHATTERS_FILE"/>
        public TwitchUserInfo GetChatterByName(string name)
        {
            if (File.Exists(PATH_ACTIVE_CHATTERS_FILE))
            {
                List<String> chatters = new List<string>(File.ReadAllLines(PATH_ACTIVE_CHATTERS_FILE));
                string user = chatters.Find(chatter => chatter.Equals(name));

                if (user != null)
                {
                    return CPH.TwitchGetUserInfoByLogin(user);
                }
            }
            return null;
        }

        /// <summary>
        ///     Convenience Method to check if a User is a known bot.
        ///     Checks against the List _ListOfChannelBots that was provided on construction
        /// </summary>
        /// <param name="username">
        ///     The name of the User to check
        /// </param>
        /// <returns>
        ///     true if the user is a known bot, false otherwise
        /// </returns>
        /// <seealso cref="MONITOR_KNOWN_BOTS"/>
        /// <seealso cref="PATH_ACTIVE_CHATTERS_FILE"/>
        /// <seealso cref="ConfigService"/>
        public bool UserIsKnownBot(string username)
        {
            List<string> knownBots = new List<String>(this.MONITOR_KNOWN_BOTS);
            return knownBots.Contains(username);
        }

        /// <summary>
        ///     Checks if a User is on the Deny List
        /// </summary>
        /// <param name="username">
        ///     Name of the user to check
        /// </param>
        /// <returns>
        ///     true if the user is on the denylist
        /// </returns>
        public bool UserIsExcluded(string username)
        {
            List<string> deniedUsers = new List<String>(this.MONITOR_DENY_LIST);
            return deniedUsers.Contains(username);
        }

        /// <summary>
        ///     Adds a chatter to the active chatters file, if they aren't on it yet
        /// </summary>
        /// <param name="username"></param>
        public void AddChatterToMonitoringFile(string username)
        {
            // Check if the user is alreadey on the monitoring list
            // Can happen if the user lurks for too long and is recognized as a new chatter again
            CPH.LogInfo(string.Format("Attempting monitoring for User {0}", username));
            if (GetChatterByName(username) == null)
            {
                CPH.LogInfo(string.Format("User {0} is not monitored as active Chatter yet", username));
                if (!UserIsKnownBot(username) && !UserIsExcluded(username))
                {
                    CPH.LogInfo("User is not a known bot, or on the deny-List, pulling Data");
                    TwitchUserInfo user = CPH.TwitchGetUserInfoByLogin(username);
                    if (user != null)
                    {
                        File.AppendAllText(PATH_ACTIVE_CHATTERS_FILE, username + Environment.NewLine, Encoding.UTF8);
                        CPH.LogInfo(string.Format("Added user {0} to active chatters List", username));
                    }
                    else
                    {
                        CPH.LogWarn("Userdata could not be pulled, something went wrong here! This should not happen!");
                    }
                } else
                {
                    CPH.LogInfo("User is either a known bot, or on the deny-List, skipping!");
                }
            } else
            {
                CPH.LogInfo(string.Format("User {0} is already monitored as active Chatter, skipping", username));
            }
        }

        /// <summary>
        ///     Convenience-Method to clear a Monitoring-File by Deleting and Recreating it
        /// </summary>
        public void ClearMonitoringFile()
        {
            if(File.Exists(PATH_ACTIVE_CHATTERS_FILE))
            {
                CPH.LogDebug("Monitoring-File exists, Deleting now");
                File.Delete(PATH_ACTIVE_CHATTERS_FILE);
                CPH.LogDebug("Monitoring-File deleted, Recreating now");
                File.Create(PATH_ACTIVE_CHATTERS_FILE).Close();
                CPH.LogDebug("Clearing finished, exiting");
            } else
            {
                CPH.LogWarn("Monitoring-File doesn't exist, skipping clearing!");
            }
        }
    }
}
