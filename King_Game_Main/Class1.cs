using NLith.KingGame.Backend.Models;
using NLith.KingGame.Backend.Services;
using Streamer.bot.Plugin.Interface;
using Streamer.bot.Plugin.Interface.Model;
using System;
using System.Collections.Generic;

public class CPHInline : CPHInlineBase
{
    public ChatterService chatterService;

    /*****************************************
     *                                       *
     * Config was moved to ConfigService.cs! *
     *                                       *
     *****************************************/

    /// <summary>
    ///     Function that manages and returns the ChatterService instance to simulate a singleton
    /// </summary>
    /// <returns>
    ///     An instance of ChatterService
    /// </returns>
    /// <seealso cref="ChatterService"/>
    private ChatterService GetChatterService()
    {
        if (this.chatterService == null)
        {
            this.chatterService = new ChatterService(CPH, ConfigService.PATH_ACTIVE_CHATTERS_FILE, ConfigService.MONITOR_KNOWN_BOTS, ConfigService.MONITOR_BOTS_AS_CHATTER, ConfigService.MONITOR_BROADCASTER_AS_CHATTER, ConfigService.MONITOR_DENY_LIST);
        }
        return this.chatterService;
    }

    /// <summary>
    ///     Used for debugging, so you don't have to remap Commands all the time
    /// </summary>
    /// <returns>
    ///     Boolean required by Streamerbot
    /// </returns>
    public bool Debug()
    {
        AdventureService advService = new AdventureService(CPH);
        UserService usrService = new UserService(CPH);

        string announcement = string.Format("Our Monarch {0} has decided to embark on an Expedition! This is how it turned out.", GetKingUsername());
        AnnounceToAudience(announcement);
        CPH.Wait(6000);
        advService.GenerateAdventure(GetKingUsername());
        return true;
    }

    public bool AddUserToMonitoringFile()
    {
        chatterService = GetChatterService();
        chatterService.AddChatterToMonitoringFile(args["user"].ToString());

        return true;
    }


    /// <summary>
    ///     (WIP) Command to Buy Insurance
    ///     Insurance will protect you from Mining Accidents and other random events
    /// </summary>
    /// <returns>Boolean required by Streamerbot</returns>
    public bool BuyInsurance()
    {
        WalletService walletService = new WalletService(CPH);
        VarService varService = new VarService(CPH);
        UserService userService = new UserService(CPH);

        walletService.FinePlayerAmount(userService.GetCurrentUserName(args), ConfigService.INSURANCE_FEE_AMOUNT);
        varService.SetUserVariable(userService.GetCurrentUserName(args), ConfigService.INSURANCE_VAR_NAME, true);
        return true;
    }

    /// <summary>
    /// Puts a User in Jail
    /// Can only be used by the King
    /// </summary>
    /// <returns></returns>
    public bool Jail()
    {
        MessageService msgService = new MessageService(CPH);
        if (CurrentUserIsKing())
        {
            String userToJail = GetCommandArgumentAtPosition(0);
            UserService userService = new UserService(CPH);
            userToJail = userService.SanitizeUsername(userToJail);

            CPH.LogInfo(String.Format("User To Jail: {0}", userToJail));

            String commandArgument = GetCommandArgument();
            CPH.LogInfo(String.Format("Argument: {0}", commandArgument));
            String reason = "";

            if(commandArgument.Split(' ').Length > 1)
            {
                for (int i = 1; i < commandArgument.Split(' ').Length; i++)
                {
                    reason += commandArgument.Split(' ')[i];
                    reason += ' ';
                }
                CPH.LogInfo(String.Format("Jail-Reason: {0}", reason));
            }
            else 
            {
                reason = "No reason";    
            }
            

            JailUser(userToJail, reason);
            CPH.TwitchAnnounce(String.Format("User {0} has been jailed for: {1}", userToJail, reason));
        }
        else
        {
            msgService.SendTwitchReply("Only the king can jail people!");
        }

        return true;
    }

    /// <summary>
    ///     Internal Function to put a User in Jail
    /// </summary>
    /// <param name="username">Name of the user to Jail</param>
    /// <param name="reason">Optional reason to give</param>
    /// <returns></returns>
    private void JailUser(String username, String reason)
    {
        CPH.TwitchTimeoutUser(username, ConfigService.INITIAL_JAIL_TIME, reason);
        CPH.PlaySoundFromFolder("C:\\Users\\rex\\OneDrive\\Dokumente\\Audacity\\CCC\\KingGame\\Jail", 75, false, true);
        string announcement = string.Format("User {0} has been Jailed for {1} seconds for: {2}", username, ConfigService.INITIAL_JAIL_TIME, reason);
        AnnounceToAudience(announcement);
    }

    /// <summary>
    ///     Wrapper Command to sell Items or the whole inventory
    /// </summary>
    /// <returns></returns>
    public bool SellItem()
    {
        if (GetCommandArgument().ToLower().Equals("inventory")
            || GetCommandArgument().ToLower().Equals("inv")
            || GetCommandArgument().ToLower().Equals("treasures")
            || GetCommandArgument().ToLower().Equals("all"))
        {
            SellAllTreasures();
        }
        else
        {
            SellSingleItem(GetCommandArgument());
        }
        return true;
    }

    /// <summary>
    ///     Convenience method to sell a single Item from the inventory
    /// </summary>
    /// <param name="itemName">Name of the item to sell</param>
    /// <returns></returns>
    private bool SellSingleItem(string itemName)
    {
        MessageService msgService = new MessageService(CPH);
        WalletService walletService = new WalletService(CPH);
        UserService userService = new UserService(CPH);

        Inventory inv = GetPlayerInventory(userService.GetCurrentUserName(args));

        Item item = null;

        item = inv.Items.Find(i => i.Name.ToLower().Equals(itemName.ToLower()));
        // If its not in the Items Stack, it is in one of the other collections
        if (item == null)
        {
            if(inv.EquippedItems != null)
            {
                item = inv.EquippedItems.Find(i => i.Name.ToLower().Contains(itemName.ToLower()));
            }
        }
        else if (item == null)
        {
            if(inv.Treasures != null)
            {
                item = inv.Treasures.Find(i => i.Name.ToLower().Contains(itemName.ToLower()));
            }
        }

        if (item != null)
        {
            switch (item.GetType().Name)
            {
                case "Equipment":
                    inv.RemoveEquipment(item);
                    break;
                case "Treasure":
                    inv.RemoveTreasure(item);
                    break;
            }

            SetPlayerInventory(userService.GetCurrentUserName(args), inv);
            walletService.AwardPlayerAmount(userService.GetCurrentUserName(args), item.Value);
            msgService.SendTwitchReply(string.Format("You sold your {0} for {1} {2}!", itemName, item.Value, ConfigService.CURRENCY_NAME));
            return true;
        }
        else
        {
            msgService.SendTwitchReply(string.Format("You don't have a {0} in your inventory!", itemName));
            return false;
        }
    }

    /// <summary>
    ///     Internal method to sell all Treasures from the inventory
    /// </summary>
    /// <returns></returns>
    private void SellAllTreasures()
    {
        UserService userService = new UserService(CPH);
        WalletService walletService = new WalletService(CPH);
        MessageService msgService = new MessageService(CPH);
        
        Inventory inv = GetPlayerInventory(userService.GetCurrentUserName(args));

        int totalTreasureValue = inv.TotalTreasureWorth;
        inv.Treasures.Clear();
        inv.CrunchNumbers();

        SetPlayerInventory(userService.GetCurrentUserName(args), inv);
        walletService.AwardPlayerAmount(userService.GetCurrentUserName(args), totalTreasureValue);
        msgService.SendTwitchReply(string.Format("You sold all your treasures for {0} {1}!", totalTreasureValue, ConfigService.CURRENCY_NAME));
    }


    /// <summary>
    ///     Nukes all States for maintenance
    /// </summary>
    /// <returns></returns>
    public bool NukeIt()
    {
        CPH.ClearNonPersistedGlobals();
        CPH.ClearNonPersistedUserGlobals();

        CPH.UnsetAllUsersVar(ConfigService.PLAYER_MONEY_VAR_NAME);
        CPH.UnsetAllUsersVar(ConfigService.INVENTORY_VAR_NAME);
        CPH.UnsetGlobalVar(ConfigService.KINGS_NAME_VAR_NAME);
        CPH.UnsetGlobalVar(ConfigService.KINGS_PROTECTION_VAR_NAME);

        return true;
    }

    /// <summary>
    ///     (WIP)
    ///     Prep-Method to prepare and initialize Shops 
    /// </summary>
    /// <returns></returns>
    public bool SetUpShop()
    {
        VarService varService = new VarService(CPH);
        Random rand = new Random();
        int roll = rand.Next(1, 2);

        Shop<Equipment> equipmentShop = new Shop<Equipment>();
        equipmentShop.RestockShop();
        varService.SetGlobalVariable<Shop<Equipment>>(ConfigService.EQUIPMENT_SHOP_VAR_NAME, equipmentShop);

        Shop<Tool> toolShop = new Shop<Tool>();
        toolShop.RestockShop();
        varService.SetGlobalVariable<Shop<Tool>>(ConfigService.TOOL_SHOP_VAR_NAME, toolShop);

        return true;

    }

    /// <summary>
    ///     Timer-Method to restock shops
    /// </summary>
    /// <returns></returns>
    public bool RestockShop()
    {
        SetUpShop();
        string announcement = "The Shop has been restocked! Check it with !shop";
        AnnounceToAudience(announcement);
        return true;
    }


    /// <summary>
    ///     Displays the Shop Inventory, only works for the toolshop as of yet
    /// </summary>
    /// <returns></returns>
    public bool DisplayShopStock()
    {
        VarService varService = new VarService(CPH);
        Shop<Tool> toolShop = varService.GetGlobalVariable<Shop<Tool>>(ConfigService.TOOL_SHOP_VAR_NAME);
        CPH.LogInfo("Shop: " + toolShop);

        String items = "";
        toolShop.Items.ForEach(item => {
            items += string.Format("{0}({1} {2}), ", item.Name, item.Value, ConfigService.CURRENCY_SYMBOL);
        });
        CPH.LogInfo("Items in Shop: " + items);
        CPH.LogInfo("Items in Class: " + toolShop.Items.Count);
        CPH.TwitchAnnounce(string.Format("The Shop has the following things in Stock: {0}", items), true, ConfigService.TWITCH_ANNOUNCE_COLOR_DEFAULT, true);

        return true;
    }


    /// <summary>
    ///     If a king does not want to be king anymore, they can just abdicate and renounce the crown. 
    ///     A new random chatter will then be crowned. 
    ///     Could maybe incorporate some kind of council of VIPs (other Royalty) that can vote them out?
    /// </summary>
    /// <returns></returns>
    public bool Abdicate()
    {
        if (CurrentUserIsKing())
        {
            string currentKing = GetKingUsername();
            CPH.TwitchAnnounce(string.Format("{0} has abdicated voluntarily, or by force of the council (other VIPs), a new King shall be crowned!", currentKing), true, ConfigService.TWITCH_ANNOUNCE_COLOR_DEFAULT, true);
            CrownRandomChatter();
        }
        return true;
    }
    /// <summary>
    ///     (WIP)
    ///     Crowns a new random Chatter, requires that foundUserName0 is set, which is set by [pwn][TwitchChatters] Get Random Chatter
    /// </summary>
    /// <returns></returns>
    public bool CrownRandomChatter()
    {
        CPH.TwitchAnnounce("Due to absence of a King, a random Chatter shall now be crowned!", true, ConfigService.TWITCH_ANNOUNCE_COLOR_DEFAULT, true);
        if (CPH.TryGetArg("foundUserName0", out string userName))
        {
            UserService userService = new UserService(CPH);
            userName = userService.SanitizeUsername(userName);
            TwitchUserInfo randomChatterInfo = CPH.TwitchGetUserInfoByLogin(userName);
            CrownChatter(randomChatterInfo.UserName, false);
        }
        return true;
    }


    /// <summary>
    ///     Crowns a specific chatter and should only be used by the broadcaster. 
    ///     Is mainly used for testing, the command should be set to moderator-only 
    /// </summary>
    /// <returns></returns>
    public bool CrownSpecificChatter()
    {
        string username = GetTwitchUserFromCommandCall().UserName;
        CrownChatter(username, false);
        return true;
    }

    /// <summary>
    ///     Simulates a duel between two users
    /// </summary>
    /// <returns></returns>
    public bool InitiateDuel()
    {
        UserService userService = new UserService(CPH);
        string target = GetCommandArgument();

        Random rand = new Random();
        int roll = rand.Next(1, 100);

        if (roll > 50)
        {
            DuelWin();
            CPH.SendMessage(string.Format("User {0} has won the duel against {1} and has been rewarded {2} {3}!", userService.GetCurrentUserName(args), target, ConfigService.DUEL_BONUS_AMOUNT, ConfigService.CURRENCY_NAME), true, true);
        }
        else
        {
            DuelFail();
            CPH.SendMessage(string.Format("User {0} has lost the duel against {1} and has to pay {2} {3} in hospital bills!", userService.GetCurrentUserName(args), target, ConfigService.DUEL_BONUS_AMOUNT, ConfigService.CURRENCY_NAME), true, true);
        }

        return true;
    }

    /// <summary>
    ///     Streamerbot-Action that Gifts a player Money. 
    ///     Collects the usernames from the Chat Messages, calls the logic-function and posts messages into chat
    /// </summary>
    /// <returns></returns>
    public bool GiftMoney()
    {
        UserService userService = new UserService(CPH);
        WalletService walletService = new WalletService(CPH);
        walletService.GiftMoney(userService.GetCurrentUserName(args), GetCommandArgumentAtPosition(0), GetCommandArgumentAtPosition(1));
        
        return true;
    }

    

    


    /// <summary>
    ///     Abstracted Helper Method to crown a chatter via their username
    /// </summary>
    /// <param name="username">The username of the Chatter to be crowned</param>
    /// <param name="suppressMessage">Suppresses the Twitch-Announcement if set to true. For cases where you want a different Message but still use the helper to crown a chatter</param>
    /// <returns></returns>
    public bool CrownChatter(string username, bool suppressMessage)
    {
        VarService varService = new VarService(CPH);

        varService.SetGlobalVariable(ConfigService.KINGS_NAME_VAR_NAME, username);        
        string currentKing = GetKingUsername();        
        if (!suppressMessage)
        {
            CPH.TwitchAnnounce(string.Format("Chatter {0} has been crowned King! They have been rewarded VIP, 10.000 {1}, and can be challenged for their crown in 5 minutes with the !regicide command!", 
                currentKing, ConfigService.CURRENCY_NAME), true, ConfigService.TWITCH_ANNOUNCE_COLOR_DEFAULT, true);
        }
        CPH.TwitchAddVip(currentKing);
        WalletService walletService = new WalletService(CPH);
        walletService.AwardPlayerAmount(currentKing, ConfigService.CROWNING_BONUS_AMOUNT);
        SetKingsProtection(true);

        return true;
    }

    /// <summary>
    ///     Displays a players Wallet Balance
    /// </summary>
    /// <returns></returns>
    public bool Account()
    {
        MessageService msgService = new MessageService(CPH);
        VarService varService = new VarService(CPH);
        UserService userService = new UserService(CPH);

        string user = userService.GetCurrentUserName(args);
        CPH.LogDebug("Prepare for boom");
        int balance = varService.GetUserVariable<int>(user, ConfigService.PLAYER_MONEY_VAR_NAME);
        CPH.LogDebug("Current Balance of Player: " + user + " is " + balance);
        msgService.SendTwitchReply(string.Format("Hey {0}, your current {1} Balance is: {2}!", user, ConfigService.CURRENCY_NAME, balance));
        return true;
    }

    /// <summary>
    ///     Displays the kings Wallet Balance
    /// </summary>
    /// <returns></returns>
    public bool Coffers()
    {
        UserService userService = new UserService(CPH);
        MessageService msgService = new MessageService(CPH);
        string currentKing = GetKingUsername();
        int balance = CPH.GetTwitchUserVar<int>(currentKing, ConfigService.PLAYER_MONEY_VAR_NAME, true);
        msgService.SendTwitchReply(string.Format("Hey {0}, King {1} currently has {2} {3}!", userService.GetCurrentUserName(args), currentKing, balance, ConfigService.CURRENCY_NAME));

        return true;
    }

    /// <summary>
    ///     Displays the name of the King
    /// </summary>
    /// <returns></returns>
    public bool GetKing()
    {
        MessageService msgService = new MessageService(CPH);
        if (!string.IsNullOrEmpty(GetKingUsername()))
        {            
            msgService.SendTwitchReply(string.Format("The user {0} is currently King! You can challenge them with the !regicide command for VIP Status!", GetKingUsername()));

        }
        else
        {
            msgService.SendTwitchReply("There is currently no King crowned! Random Chatter will be crowned now!");
            CrownRandomChatter();
        }
        return true;
    }


    /// <summary>
    ///     Challenges the King in Regicide
    /// </summary>
    /// <returns></returns>
    public bool ChallengeKing()
    {
        UserService userService = new UserService(CPH);

        Random rand = new Random();
        int roll = rand.Next(1, 100);
        // Equals a success and initiates a regicide
        if (roll > 50)
        {
            CPH.SendMessage(string.Format("User {0} murdered King {1} in cold blood and will be crowned King!", userService.GetCurrentUserName(args), GetKingUsername()), true, true);
            RegicideSuccess();
        }
        // Equals a failure and will get you fined and possibly jailed later on (timeout?)
        else
        {
            CPH.SendMessage(string.Format("User {0} failed in their attempt to murder King {1}!", userService.GetCurrentUserName(args), GetKingUsername()), true, true);
            RegicideFailure();
        }
        return true;
    }

    /// <summary>
    ///     Handles a regicide failure and jails the Player
    /// </summary>
    private void RegicideFailure()
    {
        UserService userService = new UserService(CPH);
        WalletService walletService = new WalletService(CPH);

        walletService.FinePlayerAmount(userService.GetCurrentUserName(args), ConfigService.REGICIDE_REWARD_AMOUNT);
        JailUser(userService.GetCurrentUserName(args), "Attempted Murder of the King!");        
    }
    /// <summary>
    ///     Handles a regicide success and rewards the player. 
    ///     First the King is fined, since getKingUsername() is still set to the old King. 
    ///     Then the killer is rewarded that same amount tax-free (murder-robbery is not taxed after all). 
    ///     Then the new King is crowned and receives their Crowning bonus on top
    /// </summary>
    private void RegicideSuccess()
    {
        WalletService walletService = new WalletService(CPH);
        UserService userService = new UserService(CPH);
        walletService.FinePlayerAmount(GetKingUsername(), ConfigService.REGICIDE_FAILURE_AMOUNT);
        walletService.AwardPlayerAmount(userService.GetCurrentUserName(args), ConfigService.REGICIDE_FAILURE_AMOUNT);
        KillKing();
        CrownChatter(userService.GetCurrentUserName(args), false);
    }

    /// <summary>
    ///     Handles a Duel Win and awards the Player their Duel Bonus
    /// </summary>
    private void DuelWin()
    {
        UserService userService = new UserService(CPH);
        WalletService walletService = new WalletService(CPH);
        walletService.AwardPlayerAmount(userService.GetCurrentUserName(args), ConfigService.DUEL_BONUS_AMOUNT);
    }

    /// <summary>
    ///     Handles a Duel Failure and fines the Player for the Duel Bonus
    /// </summary>
    private void DuelFail()
    {
        UserService userService = new UserService(CPH);
        WalletService walletService = new WalletService(CPH);
        walletService.FinePlayerAmount(userService.GetCurrentUserName(args), ConfigService.DUEL_BONUS_AMOUNT);
    }

    /// <summary>
    ///     Handles the death of the king, which currently only strips them off of their VIP Status
    /// </summary>
    private void KillKing()
    {
        CPH.TwitchRemoveVip(GetKingUsername());
    }

    /// <summary>
    ///     Abstracted decision tree to either display the tax-rate if you are a peasant. 
    ///     Or set the tax-rate if you are the king. 
    ///     Reduces the amount of commands in exchange for more complexity
    /// </summary>
    /// <returns></returns>
    public bool Taxrate()
    {
        if (CurrentUserIsKing())
        {
            if (GetCommandArgument() != null && GetCommandArgument().Trim().Length > 0)
            {
                // Strip out possible Percentage-Signs and trim it right after
                string rateParam = GetCommandArgument().Replace('%', ' ').Trim();
                if (float.TryParse(rateParam, out float rate))
                {
                    SetTax(rate);
                }
                else
                {
                    MessageService msgService = new MessageService(CPH);
                    msgService.SendTwitchReply("Can't parse your tax-rate. Make sure it is a number, like this: !taxrate 20");
                }
            }
            else
            {
                GetTax();
            }
        }
        else
        {
            GetTax();
        }

        return true;
    }

    /// <summary>
    ///     WIP, Unused
    ///     Internal Method that checks the kings protection Status 
    ///     Not in Use yet
    /// </summary>
    /// <returns></returns>
    private bool CheckKingsProtection()
    {
        VarService varService = new VarService(CPH);
        return varService.GetGlobalVariable<bool>(ConfigService.KINGS_PROTECTION_VAR_NAME);
    }
    /// <summary>
    ///     Internal Method to set the Kings Protection Status
    /// </summary>
    /// <param name="isProtected"></param>
    private void SetKingsProtection(bool isProtected)
    {
        VarService varService = new VarService(CPH);
        varService.SetGlobalVariable(ConfigService.KINGS_PROTECTION_VAR_NAME, DateTime.Now.AddMinutes(5d));
    }

    /// <summary>
    ///     Displays the current Tax-Rate
    /// </summary>
    private void GetTax()
    {
        MessageService msgService = new MessageService(CPH);
        msgService.SendTwitchReply(string.Format("The current Tax-Rate is {0}%", Math.Floor(GetTaxRate()).ToString()));
    }

    /// <summary>
    ///     Sets the Tax-Rate and announces the Change to other players
    /// </summary>
    /// <param name="newRate">New Tax-Rate in Percent</param>
    /// <returns></returns>
    private bool SetTax(float newRate)
    {
        VarService varService = new VarService(CPH);

        float oldRate = varService.GetGlobalVariable<float>(ConfigService.CUSTOM_TAX_RATE_VAR_NAME);
        // In case the rate has never been changed yet, this might throw an error, so we set the default just to be sure     
        if (oldRate == 0)
        {
            oldRate = ConfigService.INITIAL_TAX_RATE;
        }
        string announcement = string.Format("Hear ye, hear ye! King {0} changed the taxes from {1}% to {2}%, effective immediately!", GetKingUsername(), oldRate, newRate);
        AnnounceToAudience(announcement);
        

        varService.SetGlobalVariable(ConfigService.CUSTOM_TAX_RATE_VAR_NAME, newRate);
        return true;
    }

    /// <summary>
    ///     Triggers a mining event where you can earn money
    /// </summary>
    /// <returns></returns>
    public bool Mine()
    {
        
        UserService userService = new UserService(CPH);
        MessageService msgService = new MessageService(CPH);
        // Inventory inventory = getPlayerInventory(userService.GetCurrentUserName(args));
        // Since the king doesn't mine, we gotta check it here
        if (CurrentUserIsKing())
        {
            msgService.SendTwitchReply("The King does not mine!");
        }
        else
        {
            Random randGen = new Random();

            // Get Players inventory, it holds some important values like roll-boosts and injury-reduction
            Inventory inv = GetPlayerInventory(userService.GetCurrentUserName(args));

            // Roll for the Haul-Amount
            int haul = randGen.Next(ConfigService.MINING_MINIMUM_REWARD_AMOUNT, ConfigService.MINING_MAXIMUM_REWARD_AMOUNT);

            // Apply the Rollboosts            
            if (randGen.Next(100) < (ConfigService.MINING_TREASURE_RATE + inv.CurrentRollBost))
            {
                TreasureService gen = new TreasureService();
                Item item = gen.GenerateTreasure();
                inv.AddItem(item);
                SetPlayerInventory(userService.GetCurrentUserName(args), inv);

                string announcement = string.Format("User {0} just found a {1} {2}! Congratulations on your find!!", userService.GetCurrentUserName(args), item.Tier.ToString(), item.Name);
                msgService.SendTwitchReply(string.Format("You found a hidden treasure! You found a {0} {1} which is worth {2} {3} (tax-free)!", item.Tier.ToString(), item.Name, item.Value, ConfigService.CURRENCY_NAME));
                AnnounceToAudience(announcement);
            }


            // Mining has a chance to result in Injury
            if (new Random().Next(100) < (ConfigService.MINING_INITIAL_ACCIDENT_RATE - inv.CurrentInjuryReduction))
            {
                MiningAccident();
                inv.ReduceEquipmentDurability();
            }
            // Otherwise you have to pay taxes and get your haul!
            else
            {
                int paidTaxes = PayTaxes(haul);
                int paidSalary = PayMiner(userService.GetCurrentUserName(args), haul);

                msgService.SendTwitchReply(string.Format("@{0} you mined {1} {2}! You paid {3} {2} in Taxes and were rewarded the remaining {4} {2}!",
                        userService.GetCurrentUserName(args), haul, ConfigService.CURRENCY_NAME, paidTaxes, paidSalary));
            }
        }

        return true;
    }

    /// <summary>
    ///     WIP, Unused
    ///     Lists Items and their remaining durability
    /// </summary>
    /// <returns></returns>
    public bool ListDurability()
    {
        UserService userService = new UserService(CPH);
        string messageString = "";
        Inventory inv = GetPlayerInventory(userService.GetCurrentUserName(args));
        foreach (Item item in inv.EquippedItems)
        {
            if (item.IsEquipment)
            {
                Tool tool = (Tool)item;

                messageString += string.Format("{0} has {1} Durability left, ", tool.Name, tool.Usages);
                
                MessageService msgService = new MessageService(CPH);
                msgService.SendTwitchReply(messageString);
            }
        }
        return true;
    }


    /// <summary>
    ///     WIP, Unused
    ///     Internal Method to add an item to a players inventory
    /// </summary>
    /// <param name="username"></param>
    /// <param name="item"></param>
    private void AddItemToPlayerInventory(string username, Item item)
    {
        Inventory inventory = GetPlayerInventory(username);
        inventory.AddItem(item);
        SetPlayerInventory(username, inventory);
    }

    /// <summary>
    ///     Convenience Method to get a players Inventory
    /// </summary>
    /// <param name="user">User to pull the Inventory for</param>
    /// <returns>Inventory of the Player</returns>
    private Inventory GetPlayerInventory(string user)
    {
        VarService varService = new VarService(CPH);
        Inventory inv = varService.GetUserVariable<Inventory>(user, ConfigService.INVENTORY_VAR_NAME);
        if (inv == null)
        {
            inv = new Inventory();
            varService.SetUserVariable<Inventory>(user, ConfigService.INVENTORY_VAR_NAME, inv);
        }
        return inv;
    }

    /// <summary>
    ///     Internal Method to set a players Inventory
    /// </summary>
    /// <param name="inv"></param>
    private void SetPlayerInventory(string playerName, Inventory inv)
    {
        VarService varService = new VarService(CPH);
        varService.SetUserVariable(playerName, ConfigService.INVENTORY_VAR_NAME, inv);
    }


    /// <summary>
    ///     Resets an account to 0 Currency. 
    ///     Used mainly for testing, but can also be used by other Commands like a "debt forgiveness", but should then have to come out of the kings coffers
    /// </summary>
    /// <returns></returns>
    public bool ResetAccountOfChatter()
    {
        MessageService msgService = new MessageService(CPH);
        VarService varService = new VarService(CPH);

        string username = GetTwitchUserFromCommandCall().UserName;

        CPH.LogInfo("Resetting Account of " + username);
        Inventory inventory = new Inventory();
        CPH.LogInfo("Resetting inventory of " + username);
        varService.SetUserVariable(username, ConfigService.INVENTORY_VAR_NAME, inventory);

        CPH.LogInfo("Resetting wallet of " + username);
        varService.SetUserVariable(username, ConfigService.PLAYER_MONEY_VAR_NAME, 0);
        msgService.SendTwitchReply(string.Format("@{0} your points and inventory were reset!", username));
        return true;
    }



    /// <summary>
    ///     Internal Method to Check if the Redeeming user Is King
    /// </summary>
    /// <returns></returns>
    private bool CurrentUserIsKing()
    {
        UserService userService = new UserService(CPH);        
        return userService.GetCurrentUserName(args).ToLower().Equals(GetKingUsername().ToLower());
    }

    /// <summary>
    ///     Helper to get the TwitchUserInfo Object from the Redeeming User
    /// </summary>
    /// <returns>TwitchUserInfo Object for the redeeming user</returns>
    public TwitchUserInfo GetTwitchUserFromCommandCall()
    {
        UserService userService = new UserService(CPH);
        string user = GetCommandArgument();
        user = userService.SanitizeUsername(user);
        if (!string.IsNullOrEmpty(user))
        {
            TwitchUserInfo userInfo = CPH.TwitchGetUserInfoByLogin(user);
            if (userInfo != null)
            {
                return CPH.TwitchGetUserInfoByLogin(user);
            }
        }
        return null;
    }

    /// <summary>
    ///     Gets the input from the redeemed Command
    /// </summary>
    /// <returns></returns>
    public string GetCommandArgument()
    {
        if (args["rawInputEscaped"] != null)
            return args["rawInputEscaped"].ToString();
        return null;
    }

    /// <summary>
    ///     Streamerbot translates a command into an array and every position is available in the args-Dictionary
    ///     Gets a positional input from the Command
    /// </summary>
    /// <param name="position">Position of the String Token to return</param>
    /// <returns></returns>
    private string GetCommandArgumentAtPosition(int position)
    {
        if (args["input" + position] != null)
            return args["input" + position].ToString();
        return null;
    }

    /// <summary>
    ///     Simulates a mining Accident and fines a Player. 
    ///     Should maybe get different Messages along with heights of fines (like stubbed your toe, vs broke your arm)
    /// </summary>
    /// <returns></returns>
    public bool MiningAccident()
    {
        UserService userService = new UserService(CPH);
        WalletService walletService = new WalletService(CPH);
        MessageService msgService = new MessageService(CPH);

        Random randomGen = new Random();

        int fine = randomGen.Next(ConfigService.MINING_MINIMUM_FINE_AMOUNT, ConfigService.MINING_MAXIMUM_FINE_AMOUNT);
        walletService.FinePlayerAmount(userService.GetCurrentUserName(args), fine);

        CallTTS(VoiceTypes.REGULAR, string.Format("Weeeee you weeeee you, here comes the ambulance to rescue {0}!", userService.GetCurrentUserName(args)), false);
        msgService.SendTwitchReply(string.Format("Oh no! You had a terrible accident while mining! ({0}% chance). The treatment cost you {1} {2}", ConfigService.MINING_INITIAL_ACCIDENT_RATE, fine, ConfigService.CURRENCY_NAME));
        return true;
    }



    /// <summary>
    ///     Abstracted Method to calculate a miners haul after-tax. 
    ///     Pays it out with awardPlayerAmount(...). 
    /// </summary>
    /// <param name="username">String that represents the user to be paid</param>
    /// <param name="taxableHaul">Integer that represants the taxable Amount</param>
    /// <returns>Integer that represents the salary after Taxes</returns>
    private int PayMiner(string username, int taxableHaul)
    {
        WalletService walletService = new WalletService(CPH);
        float taxRate = GetTaxRate();
        int taxes = (int)Math.Floor(taxableHaul * (taxRate / 100));
        int paidSalary = taxableHaul - taxes;
        walletService.AwardPlayerAmount(username, paidSalary);

        return paidSalary;
    }

    /// <summary>
    ///     Abstracted Method to get the current Tax-Rate. 
    ///     If the king hasn't set a custom rate, it defaults to the INITIAL_TAX_RATE.
    /// </summary>
    /// <returns>Current Tax-Rate as Float</returns>
    private float GetTaxRate()
    {
        VarService varService = new VarService(CPH);

        float taxRate;
        if (varService.GetGlobalVariable<float>(ConfigService.CUSTOM_TAX_RATE_VAR_NAME) == 0)
        {
            taxRate = ConfigService.INITIAL_TAX_RATE;
        }
        else
        {
            taxRate = varService.GetGlobalVariable<float>(ConfigService.CUSTOM_TAX_RATE_VAR_NAME);
        }

        return taxRate;
    }

    /// <summary>
    ///     Abstracted Method to pay a tax on a taxableSum. 
    ///     Pays it out to the king via awardPlayerAmount(...). 
    /// </summary>
    /// <param name="taxableSum">Integer that represents the sum to be taxed</param>
    /// <returns>Integer that represents the paid Tax-Amount</returns>
    private int PayTaxes(int taxableSum)
    {
        WalletService walletService = new WalletService(CPH);
        float taxRate = GetTaxRate();
        int taxes = (int)Math.Floor(taxableSum * (taxRate / 100));
        walletService.AwardPlayerAmount(GetKingUsername(), taxes);

        return taxes;
    }


    /// <summary>
    ///     Abstracted Method to get the Username of the current King. 
    ///     Reduces redundant code and increases resilience towards API-Changes
    /// </summary>
    /// <returns>String that represents Name of the current King</returns>
    private string GetKingUsername()
    {
        VarService varService = new VarService(CPH);
        return varService.GetGlobalVariable<string>(ConfigService.KINGS_NAME_VAR_NAME);
    }

    /// <summary>
    ///     Shows the User their treasures in a message
    /// </summary>
    /// <returns></returns>
    public bool ListPlayersTreasures()
    {
        UserService userService = new UserService(CPH);
        Inventory inv = GetPlayerInventory(userService.GetCurrentUserName(args));

        if (inv.Items.Count > 0)
        {

            string items = "";
            inv.Items.ForEach(item =>
            {
                items += string.Format("{0}({1})", item.Name, item.Value) + ", ";
            });
            MessageService msgService = new MessageService(CPH);
            msgService.SendTwitchReply(string.Format("You have the following treasures in your inventory: {0} and it is worth {1} {2}", items, inv.TotalInventoryWorth, ConfigService.CURRENCY_NAME));
        }
        else
        {
            CPH.SendMessage("Your inventory is currently empty! Go and head into the Mine to spelunk for treasure!");
        }
        return true;
    }

    /// <summary>
    ///     Shows the User their Equipment in a message
    /// </summary>
    /// <returns></returns>
    public bool ListPlayersEquipment()
    {
        UserService userService = new UserService(CPH);
        Inventory inv = GetPlayerInventory(userService.GetCurrentUserName(args));

        if (inv.EquippedItems.Count > 0)
        {
            string items = "";
            inv.EquippedItems.ForEach(item =>
            {
                switch (item.GetType().Name)
                {
                    case "Equipment":
                        items += string.Format("{0}({1} {2})", item.Name, item.Value, ConfigService.CURRENCY_SYMBOL) + ", ";
                        break;
                    case "Tool":
                        Tool tool = (Tool)item;
                        items += string.Format("{0}({1} uses left)", tool.Name, tool.Usages) + ", ";
                        break;
                }
            });
            MessageService msgService = new MessageService(CPH);
            msgService.SendTwitchReply(string.Format("You have the following treasures in your inventory: {0} and it is worth {1} {2}", items, inv.TotalTreasureWorth, ConfigService.CURRENCY_NAME));
        }
        else
        {
            CPH.SendMessage("Your inventory is currently empty! Go and head into the Mine to spelunk for treasure!");
        }
        return true;
    }  

    /// <summary>
    ///     Shows a player their Inventory in a message
    /// </summary>
    /// <returns></returns>
    public bool ListPlayersInventory()
    {
        UserService userService = new UserService(CPH);
        Inventory inv = GetPlayerInventory(userService.GetCurrentUserName(args));

        if (inv.Items.Count > 0)
        {
            string items = "";
            List<String> itemMessages = new List<string>();

            inv.Items.ForEach(item =>
            {
                items += string.Format("{0}({1})", item.Name, item.Value) + ", ";
            });

            items = string.Format("You have the following treasures in your inventory: {0} and it is worth {1} {2}", items, inv.TotalInventoryWorth, ConfigService.CURRENCY_NAME);
            while (items.Length > 450)
            {
                itemMessages.Add(items.Substring(0, 450));
                items = items.Substring(450);
            }            
            itemMessages.Add(items);            

            itemMessages.ForEach(message =>
            {
                MessageService msgService = new MessageService(CPH);
                msgService.SendTwitchReply(message);
            });
        }
        else
        {
            CPH.SendMessage("Your inventory is currently empty! Go and head into the Mine to spelunk for treasure!");
        }
        return true;
    }

    /// <summary>
    ///     Issues a royal decree via a Twitch Announcement and calls the TTS
    /// </summary>
    /// <returns></returns>
    public bool IssueRoyalDecree()
    {
        if (CurrentUserIsKing())
        {
            CPH.PlaySoundFromFolder("C:\\Users\\rex\\OneDrive\\Dokumente\\Audacity\\CCC\\Fanfare", 75, false, true);
            if (GetKingUsername().Equals("hey_zelfa"))
            {
                CallTTS(VoiceTypes.QUEEN, GetCommandArgument(), false);
            } 
            else
            {
                CallTTS(VoiceTypes.KING, GetCommandArgument(), false);
            }

            CPH.TwitchAnnounce(GetCommandArgument(), true, ConfigService.TWITCH_ANNOUNCE_COLOR_DEFAULT, true);
        }
        else
        {
            MessageService msgService = new MessageService(CPH);
            msgService.SendTwitchReply("Only Royalty may issue decrees, peasant!");
        }

        return true;
    }

    /// <summary>
    ///     Method to call a paid Announcement with the Peasant Voice Announcer
    /// </summary>
    /// <returns></returns>
    public bool PaidAnnouncement()
    {
        UserService userService = new UserService(CPH);
        WalletService walletService = new WalletService(CPH);
        MessageService msgService = new MessageService(CPH);
        if (walletService.UserHasEnoughMoneyToGift(userService.GetCurrentUserName(args), ConfigService.PAID_ANNOUNCEMENT_PRICE))
        {            
            String announcement = string.Format("User {0} paid {1} {2} for the following announcement: {3}", userService.GetCurrentUserName(args), ConfigService.PAID_ANNOUNCEMENT_PRICE, ConfigService.CURRENCY_NAME, GetCommandArgument());
            walletService.FinePlayerAmount(userService.GetCurrentUserName(args), ConfigService.PAID_ANNOUNCEMENT_PRICE);
            AnnounceToAudience(announcement);
        }
        else
        {
            msgService.SendTwitchReply(string.Format("Announcements cost {0} {1}, which you can't afford right now!", ConfigService.PAID_ANNOUNCEMENT_PRICE, ConfigService.CURRENCY_SYMBOL));
        }

        return true;
    }

    /// <summary>
    ///     Wrapper to send an announcement to Chat and the TTS
    /// </summary>
    /// <param name="announcement">String that represents the announcement text</param>
    private void AnnounceToAudience(string announcement)
    {
        //CPH.SendMessage(announcement, true, true);
        //CPH.TwitchAnnounce(announcement, true, ConfigService.TWITCH_ANNOUNCE_COLOR_DEFAULT, false);
        CallTTS(VoiceTypes.KING, announcement, false);
    }

    /// <summary>
    ///     Convenience Function to trigger the Speaker.bot TTS
    /// </summary>
    /// <param name="voice">Voice-ID to use in the TTS</param>
    /// <param name="announcement">Announcement Text</param>
    /// <param name="isPaidAnnouncement">Boolean flag to mark an announcement as paid</param>
    private void CallTTS(VoiceTypes voice, string announcement, bool isPaidAnnouncement)
    {
        if(ConfigService.ENABLE_TTS)
        {
            CPH.TtsSpeak(ConfigService.VOICE_TYPE_VOICE_ID_MAPPING[voice], announcement, true);            
        } 
        else
        {
            // Notify User of TTS not being enabled
            MessageService msgService = new MessageService(CPH);
            UserService userService = new UserService(CPH);
            msgService.SendTwitchReply("TTS is currently not enabled!");
            // If it was a paid announcement, refund the cost
            if(isPaidAnnouncement)
            {
                WalletService walletService = new WalletService(CPH); 
                walletService.AwardPlayerAmount(userService.GetCurrentUserName(args), ConfigService.PAID_ANNOUNCEMENT_PRICE);
            }
        }
    }

    /// <summary>
    ///     Debugging Method to grant a player a sum of money
    /// </summary>
    /// <returns></returns>
    public bool Moneyhax()
    {
        CPH.LogInfo(GetCommandArgument());
        Int32.TryParse(GetCommandArgumentAtPosition(1), out int amount);

        if (amount > 0)
        {
            WalletService walletService = new WalletService(CPH);
            walletService.AwardPlayerAmount(GetCommandArgumentAtPosition(0), amount);
        }
        return true;
    }


    
}
