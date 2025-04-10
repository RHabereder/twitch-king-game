using NLith.KingGame.Backend.Models;
using Streamer.bot.Plugin.Interface;
using System;

namespace NLith.KingGame.Backend.Services
{
    public class AnnouncementService
    {
        private IInlineInvokeProxy CPH;

        public AnnouncementService(IInlineInvokeProxy _CPH)
        {
            this.CPH = _CPH;
        }

        /// <summary>
        ///     Wrapper to send an announcement to Chat and the TTS without a referenced user
        /// </summary>
        /// <param name="announcement">String that represents the announcement text</param>
        public void AnnounceToAudience(string announcement)
        {
            AnnounceToAudience(announcement, null);
        }

        /// <summary>
        ///     Wrapper to send an announcement to Chat and the TTS
        /// </summary>
        /// <param name="announcement">String that represents the announcement text</param>
        /// <param name="username">Username to reference in case it is a paid announcement that must be refunded</param>
        public void AnnounceToAudience(string announcement, string username)
        {
            TTSService tts = new TTSService(CPH);
            //CPH.SendMessage(announcement, true, true);
            //CPH.TwitchAnnounce(announcement, true, ConfigService.TWITCH_ANNOUNCE_COLOR_DEFAULT, false);
            tts.CallTTS(VoiceTypes.KING, announcement, false, username);
        }
    }
}