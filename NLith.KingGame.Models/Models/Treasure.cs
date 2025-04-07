using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace NLith.KingGame.Backend.Models
{
    [Serializable]
    public class Treasure: Item
    {
        public Treasure ()
        {
        }

        public Treasure(string name, int value, ItemTier tier)
        {
            Name = name;
            Value = value;
            Tier = tier;
            IsEquipment = false;
            HasAugment = false;
        }
    }
}