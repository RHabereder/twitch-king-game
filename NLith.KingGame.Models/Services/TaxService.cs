using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    public class TaxService
    {
        IInlineInvokeProxy CPH;

        public TaxService(IInlineInvokeProxy _CPH)
        {
            CPH = _CPH;
        }

        /// <summary>
        ///     Sets the Tax-Rate and announces the Change to other players
        /// </summary>
        /// <param name="newRate">New Tax-Rate in Percent</param>
        /// <returns></returns>
        public bool SetTax(float newRate)
        {
            VarService varSvc = new VarService(CPH);
            AnnouncementService announcementSvc = new AnnouncementService(CPH);

            float oldRate = varSvc.GetGlobalVariable<float>(ConfigService.CUSTOM_TAX_RATE_VAR_NAME);
            // In case the rate has never been changed yet, this might throw an error, so we set the default just to be sure     
            if (oldRate == 0)
            {
                oldRate = ConfigService.INITIAL_TAX_RATE;
            }
            string announcement = string.Format("Hear ye, hear ye! King {0} changed the taxes from {1}% to {2}%, effective immediately!", new RoyaltyService(CPH).GetKingUsername(), oldRate, newRate);
            announcementSvc.AnnounceToAudience(announcement, null);

            varSvc.SetGlobalVariable(ConfigService.CUSTOM_TAX_RATE_VAR_NAME, newRate);
            return true;
        }

        /// <summary>
        ///     Abstracted Method to get the current Tax-Rate. 
        ///     If the king hasn't set a custom rate, it defaults to the INITIAL_TAX_RATE.
        /// </summary>
        /// <returns>Current Tax-Rate as Float</returns>
        public float GetTaxRate()
        {
            VarService varService = new VarService(CPH);

            float taxRate;
            if (varService.GetGlobalVariable<float>(ConfigService.CUSTOM_TAX_RATE_VAR_NAME) == 0)
            {
                taxRate = ConfigService.INITIAL_TAX_RATE;
            }
            else
            {
                taxRate = varService.GetGlobalVariable<float>(ConfigService.CUSTOM_TAX_RATE_VAR_NAME);
            }

            return taxRate;
        }

        /// <summary>
        ///     Abstracted Method to pay a tax on a taxableSum. 
        ///     Pays it out to the king via awardPlayerAmount(...). 
        /// </summary>
        /// <param name="taxableSum">Integer that represents the sum to be taxed</param>
        /// <returns>Integer that represents the paid Tax-Amount</returns>
        public int PayTaxes(int taxableSum)
        {
            WalletService walletService = new WalletService(CPH);
            RoyaltyService royaltyService = new RoyaltyService(CPH);

            float taxRate = GetTaxRate();
            int taxes = (int)Math.Floor(taxableSum * (taxRate / 100));
            walletService.AwardPlayerAmount(royaltyService.GetKingUsername(), taxes);

            return taxes;
        }
    }
}
