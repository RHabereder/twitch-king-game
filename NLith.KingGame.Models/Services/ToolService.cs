using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NLith.KingGame.Backend.Models;

namespace NLith.KingGame.Backend.Service
{
    public class ToolService
    {
        // Loot Table
        // Can be changed, at any time
        public static readonly int TIER_1_TOOL_MIN_VALUE = 5;
        public static readonly int TIER_1_TOOL_MAX_VALUE = 400;
        public static readonly int TIER_1_TOOL_ROLL_CHANCE_INCREASE = 5;

        public static readonly int TIER_2_TOOL_MIN_VALUE = 400;
        public static readonly int TIER_2_TOOL_MAX_VALUE = 800;
        public static readonly int TIER_2_TOOL_ROLL_CHANCE_INCREASE = 10;

        public static readonly int TIER_3_TOOL_MIN_VALUE = 800;
        public static readonly int TIER_3_TOOL_MAX_VALUE = 1200;
        public static readonly int TIER_3_TOOL_ROLL_CHANCE_INCREASE = 15;

        public static readonly int TIER_4_TOOL_MIN_VALUE = 1200;
        public static readonly int TIER_4_TOOL_MAX_VALUE = 1600;
        public static readonly int TIER_4_TOOL_ROLL_CHANCE_INCREASE = 20;

        public static readonly int TIER_5_TOOL_MIN_VALUE = 1600;
        public static readonly int TIER_5_TOOL_MAX_VALUE = 5000;
        public static readonly int TIER_5_TOOL_ROLL_CHANCE_INCREASE = 50;

        public static readonly int TIER_6_TOOL_MIN_VALUE = 1600;
        public static readonly int TIER_6_TOOL_MAX_VALUE = 5000;
        public static readonly int TIER_6_TOOL_ROLL_CHANCE_INCREASE = 100;




        private List<Tool> allTools;
        private readonly Dictionary<ItemTier, List<String>> toolTierMapping = new Dictionary<ItemTier, List<String>>();

        public ToolService()
        {
            allTools = new List<Tool>();
            Random randGen = new Random();
            List<Tool> commonTools = new List<Tool>
            { 
                new Tool("Bronze Pickaxe", randGen.Next(TIER_1_TOOL_MIN_VALUE, TIER_1_TOOL_MAX_VALUE), 20, ToolCategory.MultiUse, ItemTier.Common, TIER_1_TOOL_ROLL_CHANCE_INCREASE),
                new Tool("Basic Mining Helmet", randGen.Next(TIER_1_TOOL_MIN_VALUE, TIER_1_TOOL_MAX_VALUE), 10, ToolCategory.MultiUse, ItemTier.Common, TIER_1_TOOL_ROLL_CHANCE_INCREASE),
                new Tool("Torch", randGen.Next(TIER_1_TOOL_MIN_VALUE, TIER_1_TOOL_MAX_VALUE), 3, ToolCategory.MultiUse, ItemTier.Common, TIER_1_TOOL_ROLL_CHANCE_INCREASE),
            };
            List<Tool> uncommonTools = new List<Tool>
            { 
                new Tool("Luck Potion", randGen.Next(TIER_2_TOOL_MIN_VALUE, TIER_2_TOOL_MAX_VALUE), 20, ToolCategory.SingleUse, ItemTier.Uncommon, TIER_2_TOOL_ROLL_CHANCE_INCREASE),
                new Tool("Iron Pickaxe", randGen.Next(TIER_2_TOOL_MIN_VALUE, TIER_2_TOOL_MAX_VALUE), 20, ToolCategory.MultiUse, ItemTier.Uncommon, TIER_2_TOOL_ROLL_CHANCE_INCREASE),
                new Tool("Dynamite", randGen.Next(TIER_2_TOOL_MIN_VALUE, TIER_2_TOOL_MAX_VALUE), 20, ToolCategory.SingleUse, ItemTier.Uncommon, TIER_2_TOOL_ROLL_CHANCE_INCREASE),
            };
            List<Tool> rareTools = new List<Tool>
            { 
                new Tool("Steel Pickaxe", randGen.Next(TIER_3_TOOL_MIN_VALUE, TIER_3_TOOL_MAX_VALUE), 20, ToolCategory.SingleUse, ItemTier.Rare, TIER_3_TOOL_ROLL_CHANCE_INCREASE),
                new Tool("Gloves of Strength", randGen.Next(TIER_3_TOOL_MIN_VALUE, TIER_3_TOOL_MAX_VALUE), 20, ToolCategory.SingleUse, ItemTier.Rare, TIER_3_TOOL_ROLL_CHANCE_INCREASE),
            };
            List<Tool> epicTools = new List<Tool>
            { 
                new Tool("Mithril Pickaxe", randGen.Next(TIER_4_TOOL_MIN_VALUE, TIER_4_TOOL_MAX_VALUE), 15, ToolCategory.SingleUse, ItemTier.Epic, TIER_4_TOOL_ROLL_CHANCE_INCREASE),
            };
            List<Tool> legendaryTools = new List<Tool>
            { 
                new Tool("Adamantite Pickaxe", randGen.Next(TIER_5_TOOL_MIN_VALUE, TIER_5_TOOL_MAX_VALUE), 10, ToolCategory.SingleUse, ItemTier.Legendary, TIER_5_TOOL_ROLL_CHANCE_INCREASE),
            };
            List<Tool> godlikeTools = new List<Tool>
            { 
                new Tool("Gunny's Dynamite-Lobber", randGen.Next(TIER_6_TOOL_MIN_VALUE, TIER_6_TOOL_MAX_VALUE), 5, ToolCategory.SingleUse, ItemTier.Godlike, TIER_6_TOOL_ROLL_CHANCE_INCREASE ),
            };

            allTools.AddRange(commonTools);
            allTools.AddRange(uncommonTools);
            allTools.AddRange(rareTools);
            allTools.AddRange(epicTools);
            allTools.AddRange(legendaryTools);
            allTools.AddRange(godlikeTools);
        }

        /**
         * Picks a random tool from the Tool Table
         **/
        public Tool GenerateTool()
        {
            Random random = new Random();
            return allTools[random.Next(allTools.Count)];
        }

         /**
         * Generates a list of unique Tools 
         **/
        public List<Tool> GenerateTools(int amount)
        {            
            Random random = new Random();
            List<Tool> tools = new List<Tool>();
            while (tools.Count < amount)
            {
                Tool tool = allTools[random.Next(allTools.Count)];
                if(!tools.Contains(tool)) {
                    tools.Add(tool);
                }
            }            
            return tools;
        }

        /**
         * Picks a random tool from the Tool Table
         **/
        public Tool GenerateToolWithTier(ItemTier tier)
        {
            Random random = new Random();
            return allTools[random.Next(allTools.Count)];
        }
    }
}