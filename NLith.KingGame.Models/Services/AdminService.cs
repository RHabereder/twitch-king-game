using NLith.KingGame.Backend.Models;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    public class AdminService
    {
        private IInlineInvokeProxy CPH;

        public AdminService(IInlineInvokeProxy _CPH)
        {
            this.CPH = _CPH;
        }

        public void MoneyHax(string username, int amount)
        {
            if (amount > 0)
            {
                WalletService walletService = new WalletService(CPH);
                walletService.AwardPlayerAmount(username, amount);
            }
        }

        public void NukeAllState()
        {
            CPH.ClearNonPersistedGlobals();
            CPH.ClearNonPersistedUserGlobals();

            CPH.UnsetAllUsersVar(ConfigService.PLAYER_MONEY_VAR_NAME);
            CPH.UnsetAllUsersVar(ConfigService.INVENTORY_VAR_NAME);
            CPH.UnsetGlobalVar(ConfigService.KINGS_NAME_VAR_NAME);
            CPH.UnsetGlobalVar(ConfigService.KINGS_PROTECTION_VAR_NAME);
        }

        public void ResetAccountOfChatter(string username)
        {
            MessageService msgService = new MessageService(CPH);
            VarService varService = new VarService(CPH);

            CPH.LogInfo("Resetting Account of " + username);
            Inventory inventory = new Inventory();
            CPH.LogInfo("Resetting inventory of " + username);
            varService.SetUserVariable(username, ConfigService.INVENTORY_VAR_NAME, inventory);

            CPH.LogInfo("Resetting wallet of " + username);
            varService.SetUserVariable(username, ConfigService.PLAYER_MONEY_VAR_NAME, 0);
            msgService.SendTwitchReply(string.Format("@{0} your points and inventory were reset!", username));
        }
    }
}
