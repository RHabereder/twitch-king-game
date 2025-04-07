using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace NLith.KingGame.Backend.Models
{
    [Serializable]
    public class Equipment: Item
    {
        public Equipment ()
        {
        }

        public Equipment(string name, int value, int augmentBoost)
        {
            Name = name;
            Value = value;
            IsEquipment = true;
            HasAugment = true;
            Augment = AugmentType.InjuryReduction;
            AugmentValue = augmentBoost;
        }

        public AugmentType Augment { get; set; }
    }
}