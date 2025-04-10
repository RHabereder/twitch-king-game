using NLith.KingGame.Backend.Models;
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
                if (inv.EquippedItems != null)
                {
                    item = inv.EquippedItems.Find(i => i.Name.ToLower().Contains(itemName.ToLower()));
                }
            }
            else if (item == null)
            {
                if (inv.Treasures != null)
                {
                    item = inv.Treasures.Find(i => i.Name.ToLower().Contains(itemName.ToLower()));
                }
            }

            if (item != null)
            {
                switch (item.GetType().Name)
                {
                    case "Equipment":
                        inv.RemoveEquipment(item);
                        break;
                    case "Treasure":
                        inv.RemoveTreasure(item);
                        break;
                }

                SetPlayerInventory(redeemer, inv);
                walletService.AwardPlayerAmount(redeemer, item.Value);
                msgService.SendTwitchReply(string.Format("You sold your {0} for {1} {2}!", itemName, item.Value, ConfigService.CURRENCY_NAME));
            }
            else
            {
                msgService.SendTwitchReply(string.Format("You don't have a {0} in your inventory!", itemName));
            }
        }

        /// <summary>
        ///     Internal method to sell all Treasures from the inventory
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
            msgService.SendTwitchReply(string.Format("You sold all your treasures for {0} {1}!", totalTreasureValue, ConfigService.CURRENCY_NAME));
        }

        public void ListPlayersEquipment(string username)
        {
            UserService userService = new UserService(CPH);
            Inventory inv = GetPlayerInventory(username);

            if (inv.EquippedItems.Count > 0)
            {
                string items = "";
                inv.EquippedItems.ForEach(item =>
                {
                    switch (item.GetType().Name)
                    {
                        case "Equipment":
                            items += string.Format("{0}({1} {2})", item.Name, item.Value, ConfigService.CURRENCY_SYMBOL) + ", ";
                            break;
                        case "Tool":
                            Tool tool = (Tool)item;
                            items += string.Format("{0}({1} uses left)", tool.Name, tool.Usages) + ", ";
                            break;
                    }
                });
                MessageService msgService = new MessageService(CPH);
                msgService.SendTwitchReply(string.Format("You have the following treasures in your inventory: {0} and it is worth {1} {2}", items, inv.TotalTreasureWorth, ConfigService.CURRENCY_NAME));
            }
            else
            {
                CPH.SendMessage("Your inventory is currently empty! Go and head into the Mine to spelunk for treasure!");
            }
        }

        public void ListPlayersInventory(string username)
        {
            Inventory inv = GetPlayerInventory(username);

            if (inv.Items.Count > 0)
            {
                string items = "";
                List<String> itemMessages = new List<string>();

                inv.Items.ForEach(item =>
                {
                    items += string.Format("{0}({1})", item.Name, item.Value) + ", ";
                });

                items = string.Format("You have the following treasures in your inventory: {0} and it is worth {1} {2}", items, inv.TotalInventoryWorth, ConfigService.CURRENCY_NAME);
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

        public void ListPlayersTreasure(string username)
        {
            UserService userService = new UserService(CPH);
            Inventory inv = GetPlayerInventory(username);

            if (inv.Items.Count > 0)
            {

                string items = "";
                inv.Items.ForEach(item =>
                {
                    items += string.Format("{0}({1})", item.Name, item.Value) + ", ";
                });
                MessageService msgService = new MessageService(CPH);
                msgService.SendTwitchReply(string.Format("You have the following treasures in your inventory: {0} and it is worth {1} {2}", items, inv.TotalInventoryWorth, ConfigService.CURRENCY_NAME));
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
            inventory.AddItem(item);
            SetPlayerInventory(username, inventory);
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

                    messageString += string.Format("{0} has {1} Durability left, ", tool.Name, tool.Usages);

                    MessageService msgService = new MessageService(CPH);
                    msgService.SendTwitchReply(messageString);
                }
            }
        }
    }
}
