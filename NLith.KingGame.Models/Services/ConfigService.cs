using NLith.KingGame.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLith.KingGame.Backend.Services
{
    public static class ConfigService
    {

        // Configuration has moved to Models/cs
        // Streamer-Level Configuration
        // Name of your currency
        public static string CURRENCY_SYMBOL = "NL$";
        public static string CURRENCY_NAME = "NLithium";
        // Should all values be persistent, or should everything start over every stream?
        public static bool IS_GAME_PERSISTENT = true;
        // Time in Minutes that a King is protected after Crowning or a failed Regicide attempt
        public static double KINGS_PROTECTION_LENGTH = 5d;

        // Controls Integrations and integral rewards
        public static bool ENABLE_TTS = true;
        public static bool GIVE_VIP_ON_CROWNING = true;

        // Reduction of hard-coded strings
        public static string TWITCH_ANNOUNCE_COLOR_DEFAULT = "Default";
        public static string TWITCH_ANNOUNCE_COLOR_BLUE = "Blue";
        public static string TWITCH_ANNOUNCE_COLOR_GREEN = "Green";
        public static string TWITCH_ANNOUNCE_COLOR_ORANGE = "Orange";
        public static string TWITCH_ANNOUNCE_COLOR_PURPLE = "Purple";


        // Var Names
        // These can be anything and are only used to reduce Hard-coded strings
        // They shouldn't be changed while a game is in progress, otherwise it might screw with peoples Wallets 
        public static string TOOL_SHOP_VAR_NAME = "toolShop";
        public static string EQUIPMENT_SHOP_VAR_NAME = "equipmentShop";
        public static string PLAYER_PROFILE_VAR_NAME = "profile";
        public static string PLAYER_MONEY_VAR_NAME = "loyaltyPoints";
        public static string CUSTOM_TAX_RATE_VAR_NAME = "kingsSetTax";
        public static string KINGS_PROTECTION_VAR_NAME = "isKingProtected";
        public static string KINGS_NAME_VAR_NAME = "king";
        public static string KINGS_SUCCESSOR_VAR_NAME = "successor";
        public static string INVENTORY_VAR_NAME = "inventory";
        public static string INSURANCE_VAR_NAME = "hasInsurance";


        // Economy Vars
        public static int INSURANCE_FEE_AMOUNT = 1000;
        public static int INITIAL_TAX_RATE = 10;               // Can be overriden by the king via !taxrate <any number>
        public static int REGICIDE_REWARD_AMOUNT = 5000;
        public static int REGICIDE_FAILURE_AMOUNT = 2500;
        public static int CROWNING_BONUS_AMOUNT = 10000;
        public static int PAID_ANNOUNCEMENT_PRICE = 10000;
        public static int DUEL_BONUS_AMOUNT = 1000;
        public static int MINING_MINIMUM_REWARD_AMOUNT = 100;
        public static int MINING_MAXIMUM_REWARD_AMOUNT = 800;
        public static int MINING_MINIMUM_FINE_AMOUNT = 100;
        public static int MINING_MAXIMUM_FINE_AMOUNT = 800;

        public static int KINGS_PROTECTION_FEE = 1000;
        public static int KINGS_PROTECTION_TIME = 5;

        // Loot Table
        // Definition in Nlith.KingGame.Models.ItemGenerator#L19

        // RNG Vars
        public static int MINING_INITIAL_ACCIDENT_RATE = 10;
        public static int INITIAL_JAIL_TIME = 30;
        public static int MINING_TREASURE_RATE = 20;


        //TTS Vars
        public static Dictionary<VoiceTypes, string> VOICE_TYPE_VOICE_ID_MAPPING = new Dictionary<VoiceTypes, string>()
    {
        { VoiceTypes.QUEEN, "Queen Announcer" },
        { VoiceTypes.KING, "King Announcer" },
        { VoiceTypes.REGULAR, "Peasant Announcer" },
        { VoiceTypes.EXPEDITION, "King Announcer" },
        { VoiceTypes.ADVENTURE, "Adventure Narrator" },
        { VoiceTypes.ASSASSIN, "Assassin Voice" },
        { VoiceTypes.NEWS, "News Narrator" },

    };

        // Chatter Monitoring
        // Should the broadcaster be monitored as chatter? Is used for random drawings
        public static bool MONITOR_BROADCASTER_AS_CHATTER = true;
        // Should known bots be monitored as chatter? Is used for random drawings
        public static bool MONITOR_BOTS_AS_CHATTER = false;
        // If you want to exclude specific users, they have to be specified here
        public static string[] MONITOR_DENY_LIST = new string[] { };
        // Known Bots
        public static List<string> MONITOR_KNOWN_BOTS = new List<string>()
    {
        "nlithbot",
        "moobot",
        "sery_bot",
        "streamelements"
    };


        // Filepaths
        // Path to File where active Chatters should be written to (should be truncated on every start)
        public static string PATH_ACTIVE_CHATTERS_FILE = "./activeChatters.txt";
        public static string PATH_EVENTS_FILE = "./events.json";
    }
}
