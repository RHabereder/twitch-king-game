using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    public class VarService
    {
        IInlineInvokeProxy CPH;
        UserService userService;
        public VarService(IInlineInvokeProxy _CPH) 
        { 
            this.CPH = _CPH;
            userService = new UserService(_CPH);
        }

        /// <summary>
        ///     Abstraction for SetUserVar for better persistence-control. 
        ///     Presets persistence from Settings
        /// </summary>
        /// <typeparam name="T">Can be anything that is serializable</typeparam>
        /// <param name="name">Name of the var to set the value for</param>
        /// <param name="value">New value to be set</param>
        public void SetUserVariable<T>(string username, string varname, T value)
        {
            CPH.SetTwitchUserVar(username, varname, value, ConfigService.IS_GAME_PERSISTENT);
        }
        /// <summary>
        ///     Abstraction for GetUserVar for better persistence-control. Presets persistence from Settings
        /// </summary>
        /// <typeparam name="T">Can be anything that is serializable</typeparam>
        /// <param name="username">Name of the user to load the var from</param>
        /// <param name="varname">Name of the var to load</param>
        /// <returns>Deserialized User-Scope Variable</returns>
        public T GetUserVariable<T>(string username, string varname)
        {
            return CPH.GetTwitchUserVar<T>(username, varname, ConfigService.IS_GAME_PERSISTENT);
        }

        /// <summary>
        ///     Abstraction for GetGlobalVar for better persistence-control. 
        ///     Presets persistence from Settings
        /// </summary>
        /// <typeparam name="T">Can be anything that is serializable</typeparam>
        /// <param name="name">Name of the value to pull from the Globals</param>
        /// <returns>Deserialized Global-Scope Variable</returns>
        public T GetGlobalVariable<T>(string name)
        {
            return CPH.GetGlobalVar<T>(name, ConfigService.IS_GAME_PERSISTENT);
        }

        /// <summary>
        ///     Abstraction for SetGlobalVar for better persistence-control. 
        ///     Presets persistence from Settings
        /// </summary>
        /// <typeparam name="T">Can be anything that is serializable</typeparam>
        /// <param name="name">Name of the Global-Var to set</param>
        /// <param name="value">New Value of the Global-Var</param>
        public void SetGlobalVariable<T>(string name, T value)
        {
            CPH.SetGlobalVar(name, value, ConfigService.IS_GAME_PERSISTENT);
        }

    }
}
