using NLith.KingGame.Backend.Models;
using NLith.TwitchLib.Models;
using NLith.TwitchLib.Services;
using Streamer.bot.Plugin.Interface;
using System;

namespace NLith.KingGame.Backend.Services
{
    public class ShopService
    {
        private IInlineInvokeProxy CPH;

        public ShopService(IInlineInvokeProxy _CPH)
        {
            this.CPH = _CPH;
        }

        /// <summary>
        ///     (WIP)
        ///     Prep-Method to prepare and initialize Shops 
        /// </summary>
        /// <returns></returns>
        public void SetUpShop()
        {
            VarService varService = new VarService(CPH);
            Random rand = new Random();
            int roll = rand.Next(1, 2);

            Shop<Equipment> equipmentShop = new Shop<Equipment>();
            equipmentShop.RestockShop();
            varService.SetGlobalVariable<Shop<Equipment>>(ConfigService.EQUIPMENT_SHOP_VAR_NAME, equipmentShop, ConfigService.IS_GAME_PERSISTENT);

            Shop<Tool> toolShop = new Shop<Tool>();
            toolShop.RestockShop();
            varService.SetGlobalVariable<Shop<Tool>>(ConfigService.TOOL_SHOP_VAR_NAME, toolShop, ConfigService.IS_GAME_PERSISTENT);
        }

        /// <summary>
        ///     (WIP) Command to Buy Insurance
        ///     Insurance will protect you from Mining Accidents and other random events
        /// </summary>
        /// <returns>Boolean required by Streamerbot</returns>
        public void BuyInsurance(string username)
        {
            WalletService walletService = new WalletService(CPH);
            VarService varService = new VarService(CPH);

            walletService.FinePlayerAmount(username, ConfigService.INSURANCE_FEE_AMOUNT);
            varService.SetUserVariable(username, ConfigService.INSURANCE_VAR_NAME, true, ConfigService.IS_GAME_PERSISTENT);
        }

        /// <summary>
        ///     Timer-Method to restock shops
        /// </summary>
        /// <returns></returns>
        public void RestockShop()
        {
            SetUpShop();
            string announcement = "The Shop has been restocked! Check it with !shop";
            new AnnouncementService(CPH).AnnounceToAudience(announcement, ConfigService.VOICE_TYPE_VOICE_ID_MAPPING[VoiceTypes.REGULAR]);
        }

        public void DisplayShopStock()
        {
            VarService varService = new VarService(CPH);
            Shop<Tool> toolShop = varService.GetGlobalVariable<Shop<Tool>>(ConfigService.TOOL_SHOP_VAR_NAME);
            CPH.LogInfo("Shop: " + toolShop);

            String items = "";
            toolShop.Items.ForEach(item => {
                items += string.Format("{0}({1} {2}), ", item.Name, item.Value, ConfigService.CURRENCY_SYMBOL);
            });
            CPH.LogInfo("Items in Shop: " + items);
            CPH.LogInfo("Items in Class: " + toolShop.Items.Count);
            CPH.TwitchAnnounce(string.Format("The Shop has the following things in Stock: {0}", items), true, ConfigService.TWITCH_ANNOUNCE_COLOR_DEFAULT, true);
        }
    }
}
