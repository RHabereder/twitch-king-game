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
        return true;
    }

    public bool KinglyExpedition()
    {
        MessageService msgService = new MessageService(CPH);
        RoyaltyService royaltySvc = new RoyaltyService(CPH);
        UserService usrSvc = new UserService(CPH);
        string redeemer = usrSvc.GetCurrentUserName(args);
        
        if (royaltySvc.UserIsKing(redeemer))
        {
            AdventureService advService = new AdventureService(CPH);
            UserService usrService = new UserService(CPH);

            string currentKing = new RoyaltyService(CPH).GetKingUsername();
            string announcement = string.Format("Our Monarch {0} has decided to embark on an Expedition! This is how it turned out.", currentKing);
            new AnnouncementService(CPH).AnnounceToAudience(announcement);
            CPH.Wait(6000);
            advService.GenerateExpedition(currentKing);
        } else
        {
            msgService.SendTwitchReply("Only Royalty can go on expeditions! (Adventurers for peasants coming soon)");
        }

        return true;
    }

    public bool Adventure()
    {
        MessageService msgSvc = new MessageService(CPH);
        UserService usrSvc = new UserService(CPH);
        RoyaltyService royaltySvc = new RoyaltyService(CPH);
        string redeemer = usrSvc.GetCurrentUserName(args);
        if (!royaltySvc.UserIsKing(redeemer))
        {
            AdventureService advService = new AdventureService(CPH);
            string announcement = string.Format("Peasant {0} has decided to embark on an Adventure! This is how it turned out.", usrSvc.GetCurrentUserName(args));
            new AnnouncementService(CPH).AnnounceToAudience(announcement);
            CPH.Wait(6000);
            advService.GenerateAdventure(usrSvc.GetCurrentUserName(args));
        }
        else
        {
            msgSvc.SendTwitchReply("Only Peasants can go on Adventures! (Kings do !expedition)");
        }

        return true;
    }

    public bool AddUserToMonitoringFile()
    {
        chatterService = GetChatterService();
        chatterService.AddChatterToMonitoringFile(args["user"].ToString());

        return true;
    }


    

    /// <summary>
    /// Puts a User in Jail
    /// Can only be used by the King
    /// </summary>
    /// <returns></returns>
    public bool Jail()
    {
        string redeemer = new UserService(CPH).GetCurrentUserName(args);
        MessageService msgService = new MessageService(CPH);
        RoyaltyService royaltyService = new RoyaltyService(CPH);
        if (royaltyService.UserIsKing(redeemer))
        {
            string inmate = GetCommandArgumentAtPosition(0);
            String reason = "";
            string commandArgument = GetCommandArgument();

            if (commandArgument.Split(' ').Length > 1)
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
            royaltyService.JailUser(inmate, reason);
        }
        else
        {
            msgService.SendTwitchReply("Only the king can jail people!");
        }
        return true;
    }

    

    /// <summary>
    ///     Wrapper Command to sell Items or the whole inventory
    /// </summary>
    /// <returns></returns>
    public bool SellItem()
    {
        InventoryService invService = new InventoryService(CPH);
        UserService userService = new UserService(CPH);
        string redeemer = userService.GetCurrentUserName(args);
        if (GetCommandArgument().ToLower().Equals("inventory")
            || GetCommandArgument().ToLower().Equals("inv")
            || GetCommandArgument().ToLower().Equals("treasures")
            || GetCommandArgument().ToLower().Equals("all"))
        {
            invService.SellAllTreasures(redeemer);
        }
        else
        {
            invService.SellSingleItem(GetCommandArgument(), redeemer);
        }
        return true;
    }

    /// <summary>
    ///     Nukes all States for maintenance
    /// </summary>
    /// <returns></returns>
    public bool NukeIt()
    {
        new AdminService(CPH).NukeAllState();
        return true;
    }

    


    /// <summary>
    ///     Displays the Shop Inventory, only works for the toolshop as of yet
    /// </summary>
    /// <returns></returns>
    public bool DisplayShopStock()
    {
        new ShopService(CPH).DisplayShopStock();
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
        RoyaltyService royaltyService = new RoyaltyService(CPH);
        string username = new UserService(CPH).GetCurrentUserName(args);
        if (royaltyService.UserIsKing(username))
        {
            string currentKing = royaltyService.GetKingUsername();
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
        UserService usrSvc = new UserService(CPH);
        RoyaltyService royaltySvc = new RoyaltyService(CPH); 


        string redeemer = usrSvc.GetCurrentUserName(args);
        TwitchUserInfo broadcasterUserInfo = CPH.TwitchGetBroadcaster();

        string crowningTarget = usrSvc.SanitizeUsername(GetCommandArgument());
        
        
        if (royaltySvc.UserIsKing(redeemer) 
            || redeemer.Equals(broadcasterUserInfo.UserName) 
            && !redeemer.Equals(crowningTarget))
        {
            CrownChatter(crowningTarget, false);
        }
        
        return true;
    }

    /// <summary>
    ///     Streamerbot Command that enables a King to set their own Successor. 
    ///     In case of Death the successor automatically gets crowned King.
    ///     If there is no successor set, the crown passes on randomly
    /// </summary>
    /// <returns></returns>
    public bool SetSuccessor()
    {
        RoyaltyService royaltySvc = new RoyaltyService(CPH);
        MessageService msgSvc = new MessageService(CPH);
        string redeemer = new UserService(CPH).GetCurrentUserName(args);
        string chosenSuccessor = GetCommandArgument();

        if (royaltySvc.UserIsKing(redeemer))
        {
            if (!chosenSuccessor.Equals(redeemer))
            {
                royaltySvc.SetKingsSuccessor(chosenSuccessor);
            } else
            {
                msgSvc.SendTwitchReply("Nice try, but no! Choose a new successor");
            }
        }
        else
        {
            msgSvc.SendTwitchReply("Only the king can set their successor!");
        }
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
        string currentKing = new RoyaltyService(CPH).GetKingUsername();        
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
        string currentKing = new RoyaltyService(CPH).GetKingUsername();
        int balance = CPH.GetTwitchUserVar<int>(currentKing, ConfigService.PLAYER_MONEY_VAR_NAME, true);
        msgService.SendTwitchReply(string.Format("Hey {0}, King {1} currently has {2} {3}!", userService.GetCurrentUserName(args), currentKing, balance, ConfigService.CURRENCY_NAME));

        return true;
    }

    /// <summary>
    ///     Displays the name of the King
    /// </summary>
    /// <returns></returns>
    public bool GetKing() { 
        MessageService msgService = new MessageService(CPH);
        string currentKing = new RoyaltyService(CPH).GetKingUsername();

        if (!string.IsNullOrEmpty(currentKing))
        {            
            msgService.SendTwitchReply(string.Format("The user {0} is currently King! You can challenge them with the !regicide command for VIP Status!", currentKing));

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
        RoyaltyService royaltyService = new RoyaltyService(CPH);

        Random rand = new Random();
        int roll = rand.Next(1, 100);
        // Equals a success and initiates a regicide
        if (roll > 50)
        {
            CPH.SendMessage(string.Format("User {0} murdered King {1} in cold blood and will be crowned King!", userService.GetCurrentUserName(args), royaltyService.GetKingUsername()), true, true);
            RegicideSuccess();
        }
        // Equals a failure and will get you fined and possibly jailed later on (timeout?)
        else
        {
            CPH.SendMessage(string.Format("User {0} failed in their attempt to murder King {1}!", userService.GetCurrentUserName(args), royaltyService.GetKingUsername()), true, true);
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
        new RoyaltyService(CPH).JailUser(userService.GetCurrentUserName(args), "Attempted Murder of the King!");        
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
        walletService.FinePlayerAmount(new RoyaltyService(CPH).GetKingUsername(), ConfigService.REGICIDE_FAILURE_AMOUNT);
        walletService.AwardPlayerAmount(userService.GetCurrentUserName(args), ConfigService.REGICIDE_FAILURE_AMOUNT);
        KillKing();

        // Now we need to check if the King has a successor set, if so, we need to crown them
        RoyaltyService royaltySvc = new RoyaltyService(CPH);
        if (royaltySvc.KingHasSuccessor())
        {
            CrownChatter(royaltySvc.GetKingsSuccessor(), false);
            // Since the successor has been crowned, we need to unset the value to make sure we don't loop here
            new VarService(CPH).SetGlobalVariable<string>(ConfigService.KINGS_SUCCESSOR_VAR_NAME, "");
        }
        else
        {
            CrownChatter(userService.GetCurrentUserName(args), false);             
        }
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
        CPH.TwitchRemoveVip(new RoyaltyService(CPH).GetKingUsername());                
    }

    /// <summary>
    ///     Abstracted decision tree to either display the tax-rate if you are a peasant. 
    ///     Or set the tax-rate if you are the king. 
    ///     Reduces the amount of commands in exchange for more complexity
    /// </summary>
    /// <returns></returns>
    public bool Taxrate()
    {
        string username = new UserService(CPH).GetCurrentUserName(args);
        if (new RoyaltyService(CPH).UserIsKing(username))
        {
            if (CommandHasValidArgument())
            {
                // Strip out possible Percentage-Signs and trim it right after
                string rateParam = GetCommandArgument().Replace('%', ' ').Trim();
                if (float.TryParse(rateParam, out float rate))
                {
                    TaxService taxSvc = new TaxService(CPH);
                    taxSvc.SetTax(rate);
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

    private bool CommandHasValidArgument()
    {
        return GetCommandArgument() != null && GetCommandArgument().Trim().Length > 0;
    }

    /// <summary>
    ///     Displays the current Tax-Rate
    /// </summary>
    private void GetTax()
    {
        MessageService msgService = new MessageService(CPH);
        msgService.SendTwitchReply(string.Format("The current Tax-Rate is {0}%", Math.Floor(new TaxService(CPH).GetTaxRate()).ToString()));
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
    ///     Triggers a mining event where you can earn money
    /// </summary>
    /// <returns></returns>
    public bool Mine()
    {
        UserService userSvc = new UserService(CPH);
        MessageService msgSvc = new MessageService(CPH);
        string redeemer = userSvc.GetCurrentUserName(args);

        // Since the king doesn't mine, we gotta check it here
        if (new RoyaltyService(CPH).UserIsKing(redeemer))
        {
            msgSvc.SendTwitchReply("The King does not mine!");
        }
        else
        {
            MiningService miningSvc = new MiningService(CPH);
            miningSvc.Mine(redeemer);
        }
        return true;
    }

    

    /// <summary>
    ///     Resets an account to 0 Currency. 
    ///     Used mainly for testing, but can also be used by other Commands like a "debt forgiveness", but should then have to come out of the kings coffers
    /// </summary>
    /// <returns></returns>
    public bool ResetAccountOfChatter()
    {
        string username = GetCommandArgumentAtPosition(0);
        new AdminService(CPH).ResetAccountOfChatter(username);
        return true;
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
        string key = "input" + position;

        if (args.ContainsKey(key))
        {
            if (args[key] != null)
                return args["input" + position].ToString();
        } else
        {
            CPH.LogError(string.Format("Key {0} does not exist!", key));
        }

        return null;
    }

    /// <summary>
    ///     Shows the User their treasures in a message
    /// </summary>
    /// <returns></returns>
    public bool ListPlayersTreasures()
    {
        InventoryService inv = new InventoryService(CPH);
        UserService userService = new UserService(CPH);
        inv.ListPlayersTreasure(userService.GetCurrentUserName(args));

        return true;
    }

    /// <summary>
    ///     Shows the User their Equipment in a message
    /// </summary>
    /// <returns></returns>
    public bool ListPlayersEquipment()
    {
        InventoryService inv = new InventoryService(CPH);
        UserService userService = new UserService(CPH);
        inv.ListPlayersEquipment(userService.GetCurrentUserName(args));
        return true;
    }  

    /// <summary>
    ///     Shows a player their Inventory in a message
    /// </summary>
    /// <returns></returns>
    public bool ListPlayersInventory()
    {
        UserService userService = new UserService(CPH);
        InventoryService inv = new InventoryService(CPH);
        inv.ListPlayersInventory(userService.GetCurrentUserName(args));
        
        return true;
    }

    /// <summary>
    ///     Issues a royal decree via a Twitch Announcement and calls the TTS
    /// </summary>
    /// <returns></returns>
    public bool IssueRoyalDecree()
    {
        RoyaltyService royaltyService = new RoyaltyService(CPH);
        UserService userService = new UserService(CPH);

        bool isKing = royaltyService.UserIsKing(userService.GetCurrentUserName(args));

        if (isKing)
        {
            String decree = GetCommandArgument();
            AnnouncementService announcementService = new AnnouncementService(CPH);
            announcementService.IssueRoyalDecree(decree);
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
        AnnouncementService announceSvc = new AnnouncementService(CPH);
        string redeemer = userService.GetCurrentUserName(args);

        if (walletService.UserHasEnoughMoneyToGift(redeemer, ConfigService.PAID_ANNOUNCEMENT_PRICE))
        {            
            String announcement = string.Format("User {0} paid {1} {2} for the following announcement: {3}", redeemer, ConfigService.PAID_ANNOUNCEMENT_PRICE, ConfigService.CURRENCY_NAME, GetCommandArgument());
            walletService.FinePlayerAmount(redeemer, ConfigService.PAID_ANNOUNCEMENT_PRICE);
            announceSvc.AnnounceToAudience(announcement, redeemer); 
        }
        else
        {
            msgService.SendTwitchReply(string.Format("Announcements cost {0} {1}, which you can't afford right now!", ConfigService.PAID_ANNOUNCEMENT_PRICE, ConfigService.CURRENCY_SYMBOL));
        }

        return true;
    }

    /// <summary>
    ///     Debugging Method to grant a player a sum of money
    /// </summary>
    /// <returns></returns>
    public bool Moneyhax()
    {        
        int.TryParse(GetCommandArgumentAtPosition(1), out int amount);
        string username = GetCommandArgumentAtPosition(0);
        new AdminService(CPH).MoneyHax(username, amount);
        return true;
    }
}
