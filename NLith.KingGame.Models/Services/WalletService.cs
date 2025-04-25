using NLith.TwitchLib.Services;
using Streamer.bot.Plugin.Interface;
using System;

namespace NLith.KingGame.Backend.Services
{
    public class WalletService
    {
        IInlineInvokeProxy CPH;
        UserService userService;
        MessageService messageService;
        public WalletService(IInlineInvokeProxy _CPH) 
        {
            CPH = _CPH;
            userService = new UserService(_CPH);
            messageService = new MessageService(_CPH);
        }

        /// <summary>
        ///     Abstracted Method to reduce a players walletbalance. 
        ///     Reduces redundant code and increases resilience towards API-Changes
        /// </summary>
        /// <param name="username">Name of the user to fine</param>
        /// <param name="amount">Amount to be fined</param>
        public void FinePlayerAmount(string username, int amount)
        {
            int currentBalance = CPH.GetTwitchUserVar<int>(username, ConfigService.PLAYER_MONEY_VAR_NAME, true);
            currentBalance -= amount;
            CPH.SetTwitchUserVar(username, ConfigService.PLAYER_MONEY_VAR_NAME, currentBalance, true);
        }

        /// <summary>
        ///     Abstracted Method to increase a players walletbalance. 
        ///     Reduces redundant code and increases resilience towards API-Changes
        /// </summary>
        /// <param name="username">String that represents the User to award</param>
        /// <param name="amount">Integer that represents the award amount</param>
        public void AwardPlayerAmount(string username, int amount)
        {
            CPH.LogInfo("Username :" + username);
            username = userService.SanitizeUsername(username);
            CPH.LogInfo("Sanitized Username :" + username);
            int currentBalance = CPH.GetTwitchUserVar<int>(username, ConfigService.PLAYER_MONEY_VAR_NAME, true);
            CPH.LogInfo("Current Balance :" + currentBalance);
            currentBalance += amount;
            CPH.LogInfo("Setting Walletbalance to :" + currentBalance);
            CPH.SetTwitchUserVar(username, ConfigService.PLAYER_MONEY_VAR_NAME, currentBalance, true);
        }

        /// <summary>
        ///     Logic-Function to give a player Money Uses the Fine and Award functions to handle the Transaction
        /// </summary>
        /// <param name="grantor">Username from the user that gives the money away</param>
        /// <param name="grantee">Username of the user that receives the money</param>
        /// <param name="sum">Sum that is gifted</param>
        private void GiveMoneyFromTo(string grantor, string grantee, int sum)
        {
            WalletService walletService = new WalletService(CPH);
            walletService.FinePlayerAmount(grantor, sum);
            walletService.AwardPlayerAmount(grantee, sum);
        }

        /// <summary>
        ///     Function that Gifts a player Money
        /// </summary>
        /// <param name="grantor"></param>
        /// <param name="grantee"></param>
        /// <param name="sumArg"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void GiftMoney(string grantor, string grantee, string sumArg)
        {
            int sum = 0;
            if (int.TryParse(sumArg, out sum))
            {
                if (sum > 0)
                {                    
                    if (UserHasEnoughMoneyToGift(grantor, sum))
                    {
                        CPH.SendMessage(string.Format("{0} decided to gift {1} the sum of {2} {3}! {1} don't forget to say thank you!", grantor, grantee, sum, ConfigService.CURRENCY_NAME));
                        GiveMoneyFromTo(grantor, grantee, sum);
                    }
                    else
                    {
                        MessageService msgService = new MessageService(CPH);
                        msgService.SendTwitchReply("You don't have enough money for this transaction! You'd go into debt and debt is bad!");
                    }
                }
            }
        }

        /// <summary>
        ///     Internal Method to determine if a User has enough money in their account to gift a specific sum to someone else
        /// </summary>
        /// <param name="username">User to check</param>
        /// <param name="sum">Sum that should be checked for</param>
        /// <returns></returns>
        public bool UserHasEnoughMoneyToGift(string username, int sum)
        {
            VarService varService = new VarService(CPH);            
            return (sum <= varService.GetUserVariable<int>(username, ConfigService.PLAYER_MONEY_VAR_NAME, ConfigService.IS_GAME_PERSISTENT));
        }
    }
}
