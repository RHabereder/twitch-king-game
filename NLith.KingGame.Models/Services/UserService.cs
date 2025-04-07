using Streamer.bot.Plugin.Interface;
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
            CPH = _CPH;
        }
    }
}
