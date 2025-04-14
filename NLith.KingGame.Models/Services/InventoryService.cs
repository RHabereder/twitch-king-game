using NLith.KingGame.Backend.Models;
using NLith.KingGame.Backend.Services;
using Streamer.bot.Plugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    public class InventoryService
    {
        IInlineInvokeProxy CPH;

        public InventoryService(IInlineInvokeProxy _CPH)
        {
            CPH = _CPH;
        }

        /// <summary>
        ///     Convenience method to sell a single Item from the inventory
        /// </summary>
        /// <param name="itemName">Name of the item to sell</param>
        /// <returns></returns>
        public void SellSingleItem(string itemName, string username)
        {
            MessageService msgService = new MessageService(CPH);
            WalletService walletService = new WalletService(CPH);
            UserService userService = new UserService(CPH);

            string redeemer = username;

            Inventory inv = GetPlayerInventory(redeemer);

            Item item = null;


            item = inv.Items.Find(i => i.Name.ToLower().Equals(itemName.ToLower()));
            // If its not in the Items Stack, it is in one of the other collections

            if (item == null)
            {
                CPH.LogInfo($"Looking for Item {itemName} in Collection {"EquippedItems"}");
                if (inv.EquippedItems != null)
                {
                    item = inv.EquippedItems.Find(i => i.Name.ToLower().Contains(itemName.ToLower()));
                }
            }
            else if (item == null)
            {
                CPH.LogInfo($"Looking for Item {itemName} in Collection {"Treasures"}");
                if (inv.Treasures != null)
                {
                    item = inv.Treasures.Find(i => i.Name.ToLower().Contains(itemName.ToLower()));
                }
            }

            if (item != null)
            {
                CPH.LogInfo($"Found Item {item.Name}, Switching on {item.GetType()}");



                string inventoryString = "";
                inv.Items.ForEach(i => inventoryString += i.Name + ", ");
                CPH.LogInfo(inventoryString);

                CPH.LogInfo($"Removing {item.Name} from {redeemer}'s Inventory");
                inv = inv.RemoveItem(item, CPH);
                inventoryString = "";
                inv.Items.ForEach(i => inventoryString += i.Name + ", ");
                CPH.LogInfo(inventoryString);


                CPH.LogInfo($"Setting inventory of {redeemer}'s Inventory");
                SetPlayerInventory(redeemer, inv);
                CPH.LogInfo($"Awarding {redeemer} Item Value of {item.Value}");

                bool itemHasBeenRemoved = (inv.Items.Find(searchItem => searchItem.Name.Equals(item.Name) && searchItem.Value.Equals(item.Value)) == null);

                if(itemHasBeenRemoved)
                {
                    walletService.AwardPlayerAmount(redeemer, item.Value);
                    msgService.SendTwitchReply($"You sold your {item.Name} for {item.Value} {ConfigService.CURRENCY_NAME}!");

                } else
                {
                    CPH.LogDebug($"Something went wrong, {item.Name} should have been removed, but was not");
                    msgService.SendTwitchReply($"Something went wrong! Your item was not sold!");
                }
            }
            else
            {
                msgService.SendTwitchReply($"You don't have a {itemName} in your inventory!");
            }
        }
        
        /// <summary>
        ///     Internal Method to sell all Treasures with a specific name (Like those 1000 Bat Fangs that clutter your inventory)
        /// </summary>
        /// <param name="redeemer"></param>
        /// <param name="v"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void SellAllTreasuresWithName(string redeemer, string itemName)
        {
            WalletService walletSvc = new WalletService(CPH);
            MessageService msgSvc = new MessageService(CPH);

            Inventory inv = GetPlayerInventory(redeemer);
            int lumpSum = 0;
            inv.Items.ForEach(item =>
            {
                if(item.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                {
                    lumpSum += item.Value;
                }
            });
            CPH.LogInfo($"Calculated sell-sum of {lumpSum}");

            // Removing all Items 
            int removedItems = inv.Items.RemoveAll(item => item.Name.Equals(itemName));
            inv.CrunchNumbers();
            CPH.LogInfo($"Removed {removedItems} {itemName} items");

            // Make sure all Items have been removed, before awarding the Player the sell-sum
            // The int should be -1 if all Items have been successfully removed
            int remainingItemsAfterSale = inv.Items.FindIndex(searchItem => searchItem.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
            if (remainingItemsAfterSale == -1 && removedItems > 0)
            {
                walletSvc.AwardPlayerAmount(redeemer, lumpSum);
                msgSvc.SendTwitchReply($"You sold all your {itemName} for {lumpSum} {ConfigService.CURRENCY_NAME}!");

                // Finally set the Inventory
                SetPlayerInventory(redeemer, inv);
            } 
            else if(lumpSum > 0 && remainingItemsAfterSale > -1)
            {
                CPH.LogInfo($"Something went wrong here, Sell-Amount is {lumpSum} but inv.Items still contains {remainingItemsAfterSale} items");
            }             
        }

        /// <summary>
        ///     Internal method to sell all Treasures from the inventory
        ///     Currently unused, since it is quite Dangerous
        /// </summary>
        /// <returns></returns>
        public void SellAllTreasures(string username)
        {
            WalletService walletService = new WalletService(CPH);
            MessageService msgService = new MessageService(CPH);
            InventoryService invService = new InventoryService(CPH);
            string redeemer = username;


            Inventory inv = invService.GetPlayerInventory(redeemer);

            int totalTreasureValue = inv.TotalTreasureWorth;
            inv.Treasures.Clear();
            inv.CrunchNumbers();

            invService.SetPlayerInventory(redeemer, inv);
            walletService.AwardPlayerAmount(redeemer, totalTreasureValue);
            msgService.SendTwitchReply($"You sold all your treasures for {totalTreasureValue} {ConfigService.CURRENCY_NAME}!");
        }

        public void ListPlayersInventory(string username)
        {
            Inventory inv = GetPlayerInventory(username);

            if (inv.Items.Count > 0)
            {
                // Sort the Inventory by Value, if needed
                inv.Items.Sort((x,y) => x.Value.CompareTo(y.Value));
                
                string items = "";
                List<String> itemMessages = new List<string>();


                inv.Items.ForEach(item =>
                {
                    items += $"{item.Name}({item.Value}), ";
                });

                items = $"You have the following treasures in your inventory: {items} and it is worth {inv.TotalInventoryWorth} {ConfigService.CURRENCY_NAME}";
                while (items.Length > 450)
                {
                    itemMessages.Add(items.Substring(0, 450));
                    items = items.Substring(450);
                }
                itemMessages.Add(items);

                itemMessages.ForEach(message =>
                {
                    MessageService msgService = new MessageService(CPH);
                    msgService.SendTwitchReply(message);
                });
            }
            else
            {
                CPH.SendMessage("Your inventory is currently empty! Go and head into the Mine to spelunk for treasure!");
            }
        }

        /// <summary>
        ///     WIP, Unused
        ///     Internal Method to add an item to a players inventory
        /// </summary>
        /// <param name="username"></param>
        /// <param name="item"></param>
        public void AddItemToPlayerInventory(string username, Item item)
        {
            Inventory inventory = GetPlayerInventory(username);
            SetPlayerInventory(username, inventory.AddItem(item));
        }

        /// <summary>
        ///     Convenience Method to get a players Inventory
        /// </summary>
        /// <param name="user">User to pull the Inventory for</param>
        /// <returns>Inventory of the Player</returns>
        public Inventory GetPlayerInventory(string user)
        {
            VarService varService = new VarService(CPH);
            Inventory inv = varService.GetUserVariable<Inventory>(user, ConfigService.INVENTORY_VAR_NAME);
            if (inv == null)
            {
                inv = new Inventory();
                varService.SetUserVariable<Inventory>(user, ConfigService.INVENTORY_VAR_NAME, inv);
            }
            return inv;
        }

        /// <summary>
        ///     Internal Method to set a players Inventory
        /// </summary>
        /// <param name="inv"></param>
        public void SetPlayerInventory(string playerName, Inventory inv)
        {
            VarService varService = new VarService(CPH);
            varService.SetUserVariable(playerName, ConfigService.INVENTORY_VAR_NAME, inv);
        }


        /// <summary>
        ///     WIP, Unused
        ///     Lists Items and their remaining durability
        /// </summary>
        /// <returns></returns>
        public void ListDurability(string username)
        {
            string messageString = "";
            Inventory inv = GetPlayerInventory(username);
            foreach (Item item in inv.EquippedItems)
            {
                if (item.IsEquipment)
                {
                    Tool tool = (Tool)item;

                    messageString += $"{tool.Name} has {tool.Usages} Durability left, ";

                    MessageService msgService = new MessageService(CPH);
                    msgService.SendTwitchReply(messageString);
                }
            }
        }

    }
}
