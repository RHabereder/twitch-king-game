using NLith.KingGame.Backend.Models;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    /// <summary>
    ///     Preparation for Extraction of functionality pertaining Variable-Handling
    /// </summary>
    public class VarService
    {

        IInlineInvokeProxy CPH;
        
        public VarService(IInlineInvokeProxy _CPH)
        {
            this.CPH = _CPH;
        }

        
    }
}
