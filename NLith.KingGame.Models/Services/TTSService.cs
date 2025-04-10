using NLith.KingGame.Backend.Models;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    public class TTSService
    {

        IInlineInvokeProxy CPH;
        public TTSService(IInlineInvokeProxy _CPH)
        {
            CPH = _CPH;
        }


        /// <summary>
        ///     Convenience Function to trigger the Speaker.bot TTS
        /// </summary>
        /// <param name="voice">Voice-ID to use in the TTS</param>
        /// <param name="announcement">Announcement Text</param>
        /// <param name="isPaidAnnouncement">Boolean flag to mark an announcement as paid</param>
        /// <param name="playername">Nullable Name of a play that needs to be referenced, in case of a refund</param>
        public void CallTTS(VoiceTypes voice, string announcement, bool isPaidAnnouncement, string playername)
        {
            if (ConfigService.ENABLE_TTS)
            {
                CPH.TtsSpeak(ConfigService.VOICE_TYPE_VOICE_ID_MAPPING[voice], announcement, true);
            }
            else
            {
                // Notify User of TTS not being enabled
                MessageService msgService = new MessageService(CPH);
                UserService userService = new UserService(CPH);
                msgService.SendTwitchReply("TTS is currently not enabled!");
                // If it was a paid announcement, refund the cost
                if (isPaidAnnouncement && playername != null)
                {
                    new WalletService(CPH).AwardPlayerAmount(playername, ConfigService.PAID_ANNOUNCEMENT_PRICE);
                }
            }
        }
    }
}
