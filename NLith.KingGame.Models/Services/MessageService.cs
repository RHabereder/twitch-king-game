using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    public class MessageService
    {
        IInlineInvokeProxy CPH;
        public MessageService(IInlineInvokeProxy _CPH) 
        { 
            this.CPH = _CPH;
        }


        /// <summary>
        ///     Internal Method to send a Twitch Message to Chat. Reduces redundant code and increases resilience towards API-Changes
        /// </summary>
        /// <param name="message">String that represents the Message to be sent to chat</param>
        public void SendTwitchReply(string message)
        {
            if (CPH.TryGetArg("msgId", out string messageId))
            {
                CPH.TwitchReplyToMessage(message, messageId, true, true);
            }
        }
    }
}
