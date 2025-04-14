using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Streamer.bot.Plugin.Interface;

namespace NLith.KingGame.Backend.Models
{
    [Serializable]
    public class Inventory
    {
        public List<Item> Items { get; set; }
        public List<Item> Treasures { get; set; }
        public List<Item> EquippedItems { get; set; }
        public int TotalEquipmentWorth { get; set; }
        public int TotalTreasureWorth { get; set; }
        public int TotalInventoryWorth { get; set; }

        /**
         * These two should at some point be moved to a different class, like a Player class/sheet or something
         **/
        public int CurrentRollBost { get; set; }
        public int CurrentInjuryReduction { get; set; }

        public Inventory()
        {
            Treasures = new List<Item>();
            Items = new List<Item>();
            EquippedItems = new List<Item>();
            TotalEquipmentWorth = 0;
        }

        /**
         * Function that moves an Item from one list to another
         * Recalculates all the Lists values again
         **/
        public void TransferItem(List<Item> from, List<Item> to, Item item)
        {
            from.Remove(item);
            to.Add(item);
            RecalculateWorth();
        }

        /**
         * Crunches all the Numbers and reclaculates all Modifiers and List-Values
         **/
        public void CrunchNumbers()
        {
            RecalculateInuryReduction();
            RecalculateRollBoost();
            RecalculateWorth();
        }

        public void RecalculateInuryReduction()
        {
            CurrentInjuryReduction = 0;
            foreach (Item item in EquippedItems)
            {
                if (item.AugmentType == AugmentType.InjuryReduction)
                {
                    CurrentInjuryReduction += item.AugmentValue;
                }
            }
        }

        public void RecalculateRollBoost()
        {
            CurrentRollBost = 0;
            foreach (Tool item in EquippedItems.Cast<Tool>())
            {
                if (item.AugmentType == AugmentType.RollBoost)
                {
                    CurrentRollBost += item.AugmentValue;
                }
            }
        }

        public void RecalculateWorth()
        {
            TotalEquipmentWorth = 0;
            TotalTreasureWorth = 0;
            TotalInventoryWorth = 0;

            foreach (Item item in Items)
            {
                TotalInventoryWorth += item.Value;
            }

            foreach (Item item in Treasures)
            {
                TotalTreasureWorth += item.Value;
            }

            foreach (Item item in EquippedItems)
            {
                TotalEquipmentWorth += item.Value;
            }
        }

        public void ReduceEquipmentDurability() {
            foreach (Tool item in EquippedItems.Cast<Tool>())
            {
                UseEquippedItem(item);
            }
        }

        public void EquipItem(Equipment equipment)
        {
            if (!IsItemEquipped(equipment))
            {
                TransferItem(Items, EquippedItems, equipment);
            }
            else
            {
                throw new Exception(string.Format("Item {0} is already equipped", equipment.Name));
            }
        }

        public void UnequipItem(Equipment equipment)
        {
            if (IsItemEquipped(equipment))
            {
                TransferItem(EquippedItems, Items, equipment);
            }
            else
            {
                throw new Exception(string.Format("Item {0} is not equipped", equipment.Name));
            }
        }

        public bool IsItemEquipped(Item item)
        {
            return EquippedItems.Contains(item);
        }

        public void UseEquippedItem(Tool tool)
        {
            if (IsItemEquipped(tool))
            {
                if (tool.Category == ToolCategory.SingleUse)
                {
                    EquippedItems.Remove(tool);
                    TotalEquipmentWorth -= tool.Value;
                }
                else
                {
                    // Remove the item from the equipped list, decrement the usages, and add it back if it still has usages left
                    EquippedItems.Remove(tool);
                    tool.Usages -= 1;
                    if (tool.Usages > 0)
                    {
                        EquippedItems.Add(tool);
                    }
                    else
                    {
                        throw new Exception(string.Format("Item {0} is not equipped", tool.Name));
                    }
                }
            }
        }

        internal Inventory RemoveItem(Item item, IInlineInvokeProxy CPH)
        {
            // The whole treasures vs Item thing still hasn't been finished
            // So we still add treasures to the items collection
            CPH.LogDebug(item.GetType().Name);
            switch (item.GetType().Name)
            {
                case "Equipment":
                case "Treasure":
                case "Tool":
                case "Item":
                    int index = this.Items.FindIndex(searchItem => searchItem.Value == item.Value && searchItem.Name.Equals(item.Name));
                    CPH.LogDebug($"Found {item.Name} with Value {item.Value} at Index {index}");
                    this.Items.RemoveAt(index);
                    break;
            }
            RecalculateWorth();
            return this;
        }

        internal Inventory AddItem(Item item)
        {

            switch (item.GetType().Name)
            {
                case "Equipment":
                    Items.Add(item);
                    break;
                case "Treasure":
                    // The whole treasures vs Item thing still hasn't been finished
                    // So we still add treasures to the items collection
                    Items.Add(item);
                    break;
            }
            RecalculateWorth();
            return this;
        }
    }
}