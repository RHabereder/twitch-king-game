using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace NLith.KingGame.Backend.Models
{
    [Serializable]
    public class Wallet
    {

        public int Money { get; set; }

        public Wallet()
        {
            Money = 0;
        }

    }
}