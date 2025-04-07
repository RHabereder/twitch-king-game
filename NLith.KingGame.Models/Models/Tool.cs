using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace NLith.KingGame.Backend.Models
{
    [Serializable]
    public class Tool: Item
    {
        public Tool ()
        {
        }

        public Tool(string name, int value, int usages, ToolCategory category, ItemTier tier, int augmentBoost)
        {
            Name = name;
            Value = value;
            Usages = usages;
            Category = category;
            Tier = tier;
            IsEquipment = false;
            HasAugment = true;
            Augment = AugmentType.RollBoost;
            AugmentValue = augmentBoost;
        }

        public int Usages { get; set; }
        public ToolCategory Category { get; set; }
        public AugmentType Augment { get; set; }

    }
}