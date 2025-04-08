using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Models
{
    public class Config
    {
        //// Streamer-Level Configuration 
        //// Name of your currency
        //public static readonly string CURRENCY_SYMBOL = "NL$";
        //public static readonly string CURRENCY_NAME = "NLithium";
        //// Should all values be persistent, or should everything start over every stream?
        //public static readonly bool IS_GAME_PERSISTENT = true;
        //// Time in Minutes that a King is protected after Crowning or a failed Regicide attempt
        //public static readonly double KINGS_PROTECTION_LENGTH = 5d;

        //// Controls Integrations and integral rewards
        //public static readonly bool ENABLE_TTS = true;
        //public static readonly bool GIVE_VIP_ON_CROWNING = true;

        //// Reduction of hard-coded strings
        //public static readonly string TWITCH_ANNOUNCE_COLOR_DEFAULT = "Default";
        //public static readonly string TWITCH_ANNOUNCE_COLOR_BLUE = "Blue";
        //public static readonly string TWITCH_ANNOUNCE_COLOR_GREEN = "Green";
        //public static readonly string TWITCH_ANNOUNCE_COLOR_ORANGE = "Orange";
        //public static readonly string TWITCH_ANNOUNCE_COLOR_PURPLE = "Purple";


        //// Var Names
        //// These can be anything and are only used to reduce Hard-coded strings
        //// They shouldn't be changed while a game is in progress, otherwise it might screw with peoples Wallets 
        //public static readonly string TOOL_SHOP_VAR_NAME = "toolShop";
        //public static readonly string EQUIPMENT_SHOP_VAR_NAME = "equipmentShop";
        //public static readonly string PLAYER_PROFILE_VAR_NAME = "profile";
        //public static readonly string PLAYER_MONEY_VAR_NAME = "loyaltyPoints";
        //public static readonly string CUSTOM_TAX_RATE_VAR_NAME = "kingsSetTax";
        //public static readonly string KINGS_PROTECTION_VAR_NAME = "isKingProtected";
        //public static readonly string KINGS_NAME_VAR_NAME = "king";
        //public static readonly string INVENTORY_VAR_NAME = "inventory";
        //public static readonly string INSURANCE_VAR_NAME = "hasInsurance";


        //// Economy Vars
        //public static readonly int INSURANCE_FEE_AMOUNT = 1000;
        //public static readonly int INITIAL_TAX_RATE = 10;               // Can be overriden by the king via !taxrate <any number>
        //public static readonly int REGICIDE_REWARD_AMOUNT = 5000;
        //public static readonly int REGICIDE_FAILURE_AMOUNT = 2500;
        //public static readonly int CROWNING_BONUS_AMOUNT = 10000;
        //public static readonly int PAID_ANNOUNCEMENT_PRICE = 10000;
        //public static readonly int DUEL_BONUS_AMOUNT = 1000;
        //public static readonly int MINING_MINIMUM_REWARD_AMOUNT = 100;
        //public static readonly int MINING_MAXIMUM_REWARD_AMOUNT = 800;
        //public static readonly int MINING_MINIMUM_FINE_AMOUNT = 100;
        //public static readonly int MINING_MAXIMUM_FINE_AMOUNT = 800;

        //// Loot Table
        //// Definition in Nlith.KingGame.Models.ItemGenerator#L19

        //// RNG Vars
        //public static readonly int MINING_INITIAL_ACCIDENT_RATE = 10;
        //public static readonly int INITIAL_JAIL_TIME = 30;
        //public static readonly int MINING_TREASURE_RATE = 20;


        //// TTS Vars
        //public static readonly Dictionary<VoiceTypes, string> VOICE_TYPE_VOICE_ID_MAPPING = new Dictionary<VoiceTypes, string>()
        //{
        //    { VoiceTypes.KING, "Queen Announcer" },
        //    { VoiceTypes.KING, "King Announcer" },
        //    { VoiceTypes.REGULAR, "Peasant Announcer" },
        //};

        //// Filepaths
        //public static readonly string PATH_ACTIVE_CHATTERS_FILE = "./activeChatters.txt";          // Path to File where active Chatters should be written to (should be truncated on every start)

    }
}
