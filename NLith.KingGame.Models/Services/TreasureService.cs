using NLith.KingGame.Backend.Models;
using System;
using System.Collections.Generic;

namespace NLith.KingGame.Backend.Services
{
    public class TreasureService
    {
        // Loot Table
        // Can be changed, at any time
        public static readonly int TIER_1_ITEM_MIN_VALUE = 5;
        public static readonly int TIER_1_ITEM_MAX_VALUE = 400;
        public static readonly int TIER_1_ITEM_ROLL_CHANCE = 60;

        public static readonly int TIER_2_ITEM_MIN_VALUE = 400;
        public static readonly int TIER_2_ITEM_MAX_VALUE = 800;
        public static readonly int TIER_2_ITEM_ROLL_CHANCE = 50;

        public static readonly int TIER_3_ITEM_MIN_VALUE = 800;
        public static readonly int TIER_3_ITEM_MAX_VALUE = 1200;
        public static readonly int TIER_3_ITEM_ROLL_CHANCE = 20;

        public static readonly int TIER_4_ITEM_MIN_VALUE = 1200;
        public static readonly int TIER_4_ITEM_MAX_VALUE = 1600;
        public static readonly int TIER_4_ITEM_ROLL_CHANCE = 10;

        public static readonly int TIER_5_ITEM_MIN_VALUE = 1600;
        public static readonly int TIER_5_ITEM_MAX_VALUE = 5000;
        public static readonly int TIER_5_ITEM_ROLL_CHANCE = 2;

        public static readonly int TIER_6_ITEM_MIN_VALUE = 5000;
        public static readonly int TIER_6_ITEM_MAX_VALUE = 10000;
        public static readonly int TIER_6_ITEM_ROLL_CHANCE = 1;



        private readonly Dictionary<ItemTier, Tuple<int, int>> tierMap;
        private readonly Dictionary<ItemTier, List<String>> itemNames;

        /// <summary>
        ///     Zero-Args Constructor that generates the Loot-Table
        /// </summary>
        public TreasureService()
        {
            tierMap = new Dictionary<ItemTier, Tuple<int, int>>
            {
                { ItemTier.Common, new Tuple<int, int>(TIER_1_ITEM_MIN_VALUE, TIER_1_ITEM_MAX_VALUE) },
                { ItemTier.Uncommon, new Tuple<int, int>(TIER_2_ITEM_MIN_VALUE, TIER_2_ITEM_MAX_VALUE) },
                { ItemTier.Rare, new Tuple<int, int>(TIER_3_ITEM_MIN_VALUE, TIER_3_ITEM_MAX_VALUE) },
                { ItemTier.Epic, new Tuple<int, int>(TIER_4_ITEM_MIN_VALUE, TIER_4_ITEM_MAX_VALUE) },
                { ItemTier.Legendary, new Tuple<int, int>(TIER_5_ITEM_MIN_VALUE, TIER_5_ITEM_MAX_VALUE) },
                { ItemTier.Godlike, new Tuple<int, int>(TIER_6_ITEM_MIN_VALUE, TIER_6_ITEM_MAX_VALUE) }
            };

            itemNames = new Dictionary<ItemTier, List<String>>();

            List<String> commonItems = new List<String>
            {
                "Tiny Stone Statue",
                "Wooden Cross",
                "Rusty Dagger",
                "Old Leather Boots",
                "Old Leather Gloves",
                "Copper Ore",
                "Iron Ingots",
                "Healing Herb",
                "Basic Lantern",
                "Small Bag of Coins",
                "Rusty Sword",
                "Wooden Shield",
                "Simple Rope",
                "Torch",
                "Bag of Salt"
            };
            List<String> uncommonItems = new List<String>
            {
                "Battered Roundshield",
                "Bat Fang",
                "Silver Ore",
                "Potion of Minor Healing",
                "Enchanted Torch",
                "Mystic Dust",
                "Iron Shield",
                "Steel Dagger",
                "Leather Boots",
                "Scroll of Fireball",
                "Cloak of Warmth",
                "Gemstone Necklace"
            };
            List<String> rareItems = new List<String>
            {
                "Used Templar Sword",
                "Mithril Ore",
                "Gold Ore",
                "Potion of Strength",
                "Silver Ring of Protection",
                "Dragonbone Sword",
                "Glimmering Amulet",
                "Elven Bow",
                "Runic Pendant",
                "Diamond-encrusted Dagger",
                "Mithril Helm",
                "Boots of Speed"
            };
            List<String> epicItems = new List<String>
            {
                "Large Diamond",
                "Large Gold Nugget",
                "Necronomicon",
                "Diamond Ore",
                "Potion of Fire Resistance",
                "Phoenix Feather Cloak",
                "Runestone of Wisdom",
                "Sword of the Undying",
                "Moonstone Necklace",
                "Staff of the Archmage",
                "Armor of the Immortal",
                "Eye of the Storm Gem",
                "Wings of the Seraph",
                "Charlatan's Bells",
            };
            List<String> legendaryItems = new List<String>
            {
                "Golden Statue of Mistral",
                "Sword of Algernon" ,
                "The King's long lost Scepter",
                "Elemental Gem",
                "Cloak of Invisibility",
                "Hammer of the Ancients",
                "Dragon Scale Armor",
                "Crown of Eternal Glory",
                "Tome of Forbidden Magic",
                "Blade of the Eternal Knight",
                "Crystal Heart of the Mountain",
                "Chalice of Immortality",
                "Aegis of the Everwatch",
                "Queen's Scepter",
            };
            List<String> godlikeItems = new List<String>
            {
                "Jakey's Rubber Ducky",
                "Lumi's Lemonade",
                "Luminade",
                "MonkeySit's Paw",
                "Patto's Camera",
                "Jackie's Jester Hat",
                "Jackie's Jester Bells",
                "Zelfa's Fishing Rod",
                "Zelfa's Cat Mug",
                "Lithy's Glória",
                "Lithy's Secret Tepache Recipe",
                "Rae's Cinnamoroll Plushie",
                "Rae's Guitar",
                "Jakey's Lost Sandwich",
                "Jakey's Master List",
                "Dirky's Snotty Nose Nail",
                "Dirky's Cards",

            };

            itemNames.Add(ItemTier.Common, commonItems);
            itemNames.Add(ItemTier.Uncommon, uncommonItems);
            itemNames.Add(ItemTier.Rare, rareItems);
            itemNames.Add(ItemTier.Epic, epicItems);
            itemNames.Add(ItemTier.Legendary, legendaryItems);
            itemNames.Add(ItemTier.Godlike, godlikeItems);
        }


        /// <summary>
        ///     Generates a random Treasure
        /// </summary>
        /// <returns>
        ///     A Random Treasure from the Loot-Table
        /// </returns>
        public Treasure GenerateTreasure()
        {
            ItemTier tier;
            // First we roll for the Item Tier
            Random random = new Random();
            int roll = random.Next(0, 100);
            if (roll <= TIER_6_ITEM_ROLL_CHANCE)
            {
                tier = ItemTier.Godlike;
            }
            else if (roll <= TIER_5_ITEM_ROLL_CHANCE)
            {
                tier = ItemTier.Legendary;
            }
            else if (roll <= TIER_4_ITEM_ROLL_CHANCE)
            {
                tier = ItemTier.Epic;
            }
            else if (roll <= TIER_3_ITEM_ROLL_CHANCE)
            {
                tier = ItemTier.Rare;
            }
            else if (roll <= TIER_2_ITEM_ROLL_CHANCE)
            {
                tier = ItemTier.Uncommon;
            }
            else
            {
                tier = ItemTier.Common;
            }

            List<String> names = itemNames[tier];

            return new Treasure()
            {
                Name = names[random.Next(names.Count)],
                Value = random.Next(tierMap[tier].Item1, tierMap[tier].Item2),
                Tier = tier
            };
        }

        /// <summary>
        ///     Generates a random Treasure of the specified ItemTier
        /// </summary>
        /// <param name="tier">ItemTier that specifies the rarity of an Item</param>
        /// <returns>
        ///     A treasure of the specified ItemTier
        /// </returns>        
        /// <seealso cref="ItemTier"/>
        public Item GenerateTreasure(ItemTier tier)
        {
            // First we roll for the Item Tier
            Random random = new Random();
            List<String> names = itemNames[tier];
            return new Treasure(names[random.Next(names.Count)], random.Next(tierMap[tier].Item1, tierMap[tier].Item2), tier);
        }
    }
}