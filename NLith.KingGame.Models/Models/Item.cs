using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NLith.KingGame.Backend.Models
{
    [Serializable]
    public class Item
    {
        public Item()
        {
        }

        public Item(string name, int value, ItemTier tier, bool isEquipment, bool hasAugment, AugmentType augmentType, int augmentValue)
        {
            Name = name;
            Value = value;
            Tier = tier;
            IsEquipment = isEquipment;
            HasAugment = hasAugment;
            AugmentType = augmentType;
            AugmentValue = augmentValue;
        }

        public string Name { get; set; }
        public int Value { get; set; }
        public bool IsEquipment { get; set; }
        public bool HasAugment { get; set; }
        public AugmentType AugmentType  { get; set; }
        public int AugmentValue { get; set; }

        public ItemTier Tier { get; set; }
    }
}