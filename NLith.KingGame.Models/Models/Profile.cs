using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace NLith.KingGame.Backend.Models
{
    [Serializable]
    public class Profile
    {

        public int CurrentInjuryReduction { get; set; }
        public int CurrentRollBoost { get; set; }
        public Inventory Inventory { get; set; }

        public Wallet Wallet { get; set; }

        public Profile()
        {
            Inventory = new Inventory();
            Wallet = new Wallet();
        }
    }
}