﻿using NLith.KingGame.Backend.Models;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    public class MiningService
    {
        IInlineInvokeProxy CPH;
        public MiningService(IInlineInvokeProxy _CPH) 
        { 
            this.CPH = _CPH;
        }

        public void Mine(String username)
        {
            UserService userSvc = new UserService(CPH);
            MessageService msgSvc = new MessageService(CPH);
            MiningService miningSvc = new MiningService(CPH);
            TaxService taxSvc = new TaxService(CPH);
            AnnouncementService announcementSvc = new AnnouncementService(CPH);
           
            
            Random randGen = new Random();
            InventoryService inventorySvc = new InventoryService(CPH);
            // Get Players inventory, it holds some important values like roll-boosts and injury-reduction
            Inventory inv = inventorySvc.GetPlayerInventory(username);

            
            // Apply the Rollboosts            
            if (randGen.Next(100) < (ConfigService.MINING_TREASURE_RATE + inv.CurrentRollBost))
            {
                TreasureService gen = new TreasureService();
                Item item = gen.GenerateTreasure();
                inv.AddItem(item);
                inventorySvc.SetPlayerInventory(username, inv);

                string announcement = string.Format("User {0} just found a {1} {2}! Congratulations on your find!!", username, item.Tier.ToString(), item.Name);
                msgSvc.SendTwitchReply(string.Format("You found a hidden treasure! You found a {0} {1} which is worth {2} {3} (tax-free)!", item.Tier.ToString(), item.Name, item.Value, ConfigService.CURRENCY_NAME));
                announcementSvc.AnnounceToAudience(announcement);
            }

            // Mining has a chance to result in Injury
            // You don't get to take home your haul, but you can keep your treasure!
            if (new Random().Next(100) < (ConfigService.MINING_INITIAL_ACCIDENT_RATE - inv.CurrentInjuryReduction))
            {
                miningSvc.MiningAccident(username);
                inv.ReduceEquipmentDurability();
            }
            // Otherwise you have to pay taxes and get your haul!
            else
            {
                // Roll for the Haul-Amount
                int haul = randGen.Next(ConfigService.MINING_MINIMUM_REWARD_AMOUNT, ConfigService.MINING_MAXIMUM_REWARD_AMOUNT);

                int paidTaxes = taxSvc.PayTaxes(haul);
                int paidSalary = PayMiner(username, haul);

                msgSvc.SendTwitchReply(string.Format("@{0} you mined {1} {2}! You paid {3} {2} in Taxes and were rewarded the remaining {4} {2}!",
                        username, haul, ConfigService.CURRENCY_NAME, paidTaxes, paidSalary));
            }
        }


        /// <summary>
        ///     Simulates a mining Accident and fines a Player. 
        ///     Should maybe get different Messages along with heights of fines (like stubbed your toe, vs broke your arm)
        /// </summary>
        /// <returns></returns>
        public bool MiningAccident(string username)
        {
            WalletService walletSvc = new WalletService(CPH);
            MessageService msgSvc = new MessageService(CPH);

            Random randomGen = new Random();

            int fine = randomGen.Next(ConfigService.MINING_MINIMUM_FINE_AMOUNT, ConfigService.MINING_MAXIMUM_FINE_AMOUNT);
            walletSvc.FinePlayerAmount(username, fine);

            new TTSService(CPH).CallTTS(VoiceTypes.REGULAR, string.Format("Weeeee you weeeee you, here comes the ambulance to rescue {0}!", username), false, null);
            msgSvc.SendTwitchReply(string.Format("Oh no! You had a terrible accident while mining! ({0}% chance). The treatment cost you {1} {2}", ConfigService.MINING_INITIAL_ACCIDENT_RATE, fine, ConfigService.CURRENCY_NAME));
            return true;
        }

        /// <summary>
        ///     Abstracted Method to calculate a miners haul after-tax. 
        ///     Pays it out with awardPlayerAmount(...). 
        /// </summary>
        /// <param name="username">String that represents the user to be paid</param>
        /// <param name="taxableHaul">Integer that represants the taxable Amount</param>
        /// <returns>Integer that represents the salary after Taxes</returns>
        public int PayMiner(string username, int taxableHaul)
        {
            WalletService walletSvc = new WalletService(CPH);
            TaxService taxSvc = new TaxService(CPH);
            float taxRate = taxSvc.GetTaxRate();
            int taxes = (int)Math.Floor(taxableHaul * (taxRate / 100));
            int paidSalary = taxableHaul - taxes;
            walletSvc.AwardPlayerAmount(username, paidSalary);

            return paidSalary;
        }
    }
}
