using NLith.KingGame.Backend.Models;
using NLith.KingGame.Backend.Models.CYOAdventure;
using NLith.KingGame.Backend.Services;
using NLith.TwitchLib.Models;
using NLith.TwitchLib.Services;
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

    public bool KinglyExpedition()
    {
        MessageService msgService = new MessageService(CPH);
        RoyaltyService royaltySvc = new RoyaltyService(CPH);
        UserService usrSvc = new UserService(CPH);
        string redeemer = usrSvc.GetCurrentUserName(args);
        
        if (royaltySvc.UserIsKing(redeemer))
        {
            AdventureService advService = new AdventureService(CPH);

            string currentKing = new RoyaltyService(CPH).GetKingUsername();
            string announcement = $"Our Monarch {currentKing} has decided to embark on an Expedition!";
            string voiceID = ConfigService.VOICE_TYPE_VOICE_ID_MAPPING[VoiceTypes.KING];

            new AnnouncementService(CPH).AnnounceToAudience(announcement, voiceID);
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
            string announcement = $"Peasant {usrSvc.GetCurrentUserName(args)} has decided to embark on an Adventure!";
            string voiceID = ConfigService.VOICE_TYPE_VOICE_ID_MAPPING[VoiceTypes.ADVENTURE];
            new AnnouncementService(CPH).AnnounceToAudience(announcement, voiceID);
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
        RoyaltyService royaltySvc = new RoyaltyService(CPH);
        ArgService argSvc = new ArgService(CPH);
        if (royaltySvc.UserIsKing(redeemer))
        {
            string inmate = argSvc.GetCommandArgumentAtPosition(args, 0);
            String reason = "";
            string commandArgument = argSvc.GetCommandArgument(args);

            if (commandArgument.Split(' ').Length > 1)
            {
                for (int i = 1; i < commandArgument.Split(' ').Length; i++)
                {
                    reason += commandArgument.Split(' ')[i];
                    reason += ' ';
                }
                CPH.LogInfo($"Jail-Reason: {reason}");
            }
            else
            {
                reason = "No reason";
            }
            royaltySvc.JailUser(inmate, reason);
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
        InventoryService invSvc = new InventoryService(CPH);
        UserService userSvc = new UserService(CPH);
        ArgService argSvc = new ArgService(CPH);
        string redeemer = userSvc.GetCurrentUserName(args);
        CPH.LogInfo($"Player {redeemer} requested Sell-Action");

        string argument = argSvc.GetCommandArgument(args).ToLower();
        string item = argSvc.GetCommandArgumentAtPosition(args, 1);

        if (argument.StartsWith("all") && item != null)            
        {
            string itemName = argument.Replace("all", "").Trim();
            CPH.LogInfo($"Player {redeemer} attempts selling all their {itemName}");
            //To just make it easier on ourself, we replace the all out of it and get the Item Name
            invSvc.SellAllTreasuresWithName(redeemer, itemName);
        }
        else
        {
            CPH.LogInfo($"Player {redeemer} attempts selling of {argument}");
            invSvc.SellSingleItem(argument, redeemer);

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
            CPH.TwitchAnnounce($"{currentKing} has abdicated voluntarily, or by force of the council (other VIPs), a new King shall be crowned!", true, ConfigService.TWITCH_ANNOUNCE_COLOR_DEFAULT, true);
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
        ArgService argSvc = new ArgService(CPH);    

        string redeemer = usrSvc.GetCurrentUserName(args);
        TwitchUserInfo broadcasterUserInfo = CPH.TwitchGetBroadcaster();

        string crowningTarget = usrSvc.SanitizeUsername(argSvc.GetCommandArgument(args));
        
        
        if (royaltySvc.UserIsKing(redeemer) 
            || redeemer.Equals(broadcasterUserInfo.UserName) 
            && !redeemer.Equals(crowningTarget))
        {
            CrownChatter(crowningTarget, false);
        }
        
        return true;
    }

    /// <summary>
    ///     Streamerbot Command that either gets a successor, if no command arguments have been provided.
    ///     If an argument has been provided, a Role-Check happens, so only King can set their own Successor. 
    ///     In case of Death the successor automatically gets crowned as King.
    ///     If there is no successor set, the crown passes on randomly
    /// </summary>
    /// <returns></returns>
    public bool Successor()
    {
        RoyaltyService royaltySvc = new RoyaltyService(CPH);
        MessageService msgSvc = new MessageService(CPH);
        ArgService argSvc = new ArgService(CPH);
        string redeemer = new UserService(CPH).GetCurrentUserName(args);
        string chosenSuccessor;

        LogArgs();

        CPH.LogInfo("Checking if an argument has been provided");
        if(CommandHasValidArgument())
        {
            CPH.LogInfo("Argument was set, entering Role-Checks");
            chosenSuccessor = argSvc.GetCommandArgumentAtPosition(args,0);
            if (royaltySvc.UserIsKing(redeemer) && !string.IsNullOrEmpty(chosenSuccessor))
            {
                if (!chosenSuccessor.Equals(redeemer))
                {
                    royaltySvc.SetKingsSuccessor(chosenSuccessor);
                }
                else
                {
                    msgSvc.SendTwitchReply("Nice try, but no! Choose a new successor");
                }
            }
            else if (!royaltySvc.UserIsKing(redeemer) && !string.IsNullOrEmpty(chosenSuccessor))
            {
                msgSvc.SendTwitchReply("Only the king can set their successor!");
            }
        }
        else
        {
            royaltySvc.GetSuccessor();
        }


        return true;
    }

    private void LogArgs()
    {
        foreach(KeyValuePair<String, Object> kvp in args)
        {
            CPH.LogDebug($"{kvp.Key}: {kvp.Value}");
        }
    }



    /// <summary>
    ///     Simulates a duel between two users
    /// </summary>
    /// <returns></returns>
    public bool InitiateDuel()
    {
        UserService userService = new UserService(CPH);
        string redeemer = userService.GetCurrentUserName(args);


        string target = new ArgService(CPH).GetCommandArgument(args);

        Random rand = new Random();
        int roll = rand.Next(1, 100);

        if (roll > 50)
        {
            DuelWin();
            CPH.SendMessage($"User {redeemer} has won the duel against {target} and has been rewarded {ConfigService.DUEL_BONUS_AMOUNT} {ConfigService.CURRENCY_NAME}!",
                true, true);
        }
        else
        {
            DuelFail();
            CPH.SendMessage($"User {redeemer} has lost the duel against {target} and has to pay {ConfigService.DUEL_BONUS_AMOUNT} {ConfigService.CURRENCY_NAME} in hospital bills!",
                true, true);
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
        UserService userSvc = new UserService(CPH);
        WalletService walletSvc = new WalletService(CPH);
        ArgService argSvc = new ArgService(CPH);
        walletSvc.GiftMoney(userSvc.GetCurrentUserName(args), argSvc.GetCommandArgumentAtPosition(args, 0), argSvc.GetCommandArgumentAtPosition(args, 1));
        
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

        varService.SetGlobalVariable(ConfigService.KINGS_NAME_VAR_NAME, username, ConfigService.IS_GAME_PERSISTENT);        
        string currentKing = new RoyaltyService(CPH).GetKingUsername();        
        if (!suppressMessage)
        {
            CPH.TwitchAnnounce($"Chatter {currentKing} has been crowned King! They have been rewarded VIP, {ConfigService.CROWNING_BONUS_AMOUNT} " +
                                $"{ConfigService.CURRENCY_NAME}, and can be challenged for their crown in 5 minutes with the !regicide command!",
                true, ConfigService.TWITCH_ANNOUNCE_COLOR_DEFAULT, true);
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
        int balance = varService.GetUserVariable<int>(user, ConfigService.PLAYER_MONEY_VAR_NAME, ConfigService.IS_GAME_PERSISTENT);
        msgService.SendTwitchReply($"Hey {user}, your current Wallet Balance is: {balance}!");
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
        msgService.SendTwitchReply($"Hey {userService.GetCurrentUserName(args)}, King {currentKing} currently has {balance} {ConfigService.CURRENCY_NAME}!");

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
            msgService.SendTwitchReply($"The user {currentKing} is currently King! You can challenge them with the !regicide command for VIP Status!");

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

        string redeemer = userService.GetCurrentUserName(args);
        string currentKing = royaltyService.GetKingUsername();

        Random rand = new Random();
        int roll = rand.Next(1, 100);
        // Equals a success and initiates a regicide
        if (roll > 50)
        {
            CPH.SendMessage($"User {redeemer} murdered King {currentKing} in cold blood and can steal the crown!", true, true);
            RegicideSuccess();
        }
        // Equals a failure and will get you fined and possibly jailed later on (timeout?)
        else
        {
            CPH.SendMessage($"User {redeemer} failed in their attempt to murder King {currentKing}!", true, true);
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
            new VarService(CPH).SetGlobalVariable<string>(ConfigService.KINGS_SUCCESSOR_VAR_NAME, "", ConfigService.IS_GAME_PERSISTENT);
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
                string rateParam = new ArgService(CPH).GetCommandArgument(args).Replace('%', ' ').Trim();
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
        ArgService argSvc = new ArgService(CPH);
        return argSvc.GetCommandArgument(args) != null && argSvc.GetCommandArgument(args).Trim().Length > 0;
    }

    /// <summary>
    ///     Displays the current Tax-Rate
    /// </summary>
    private void GetTax()
    {
        MessageService msgService = new MessageService(CPH);
        msgService.SendTwitchReply($"The current Tax-Rate is {Math.Floor(new TaxService(CPH).GetTaxRate())}%");
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
        varService.SetGlobalVariable(ConfigService.KINGS_PROTECTION_VAR_NAME, DateTime.Now.AddMinutes(5d), ConfigService.IS_GAME_PERSISTENT);
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
        string username = new ArgService(CPH).GetCommandArgumentAtPosition(args, 0);
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
        string user = new ArgService(CPH).GetCommandArgument(args);
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
            String decree = new ArgService(CPH).GetCommandArgument(args);
            AnnouncementService announcementService = new AnnouncementService(CPH);
            royaltyService.IssueRoyalDecree(decree);
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
            string voiceID = ConfigService.VOICE_TYPE_VOICE_ID_MAPPING[VoiceTypes.REGULAR];
            String announcement = $"User {redeemer} paid {ConfigService.PAID_ANNOUNCEMENT_PRICE} {ConfigService.CURRENCY_NAME} for the following announcement: {new ArgService(CPH).GetCommandArgument(args)}";
            walletService.FinePlayerAmount(redeemer, ConfigService.PAID_ANNOUNCEMENT_PRICE);
            announceSvc.AnnounceToAudience(announcement, redeemer, voiceID); 
        }
        else
        {
            msgService.SendTwitchReply($"Announcements cost {ConfigService.PAID_ANNOUNCEMENT_PRICE} {ConfigService.CURRENCY_SYMBOL}, which you can't afford right now!");
        }

        return true;
    }


    /*************************************************************
     *                        DEBUG STUFF                        *
     *************************************************************/

    /// <summary>
    ///     Debugging Method to grant a player a sum of money
    /// </summary>
    /// <returns></returns>
    public bool Moneyhax()
    {        
        int.TryParse(new ArgService(CPH).GetCommandArgumentAtPosition(args, 1), out int amount);
        string username = new ArgService(CPH).GetCommandArgumentAtPosition(args, 0);
        new AdminService(CPH).MoneyHax(username, amount);
        return true;
    }

    /// <summary>
    ///     Debugging Method to grant a player a random Item
    /// </summary>
    /// <returns></returns>
    public bool GiftItem()
    {
        string receiver = new ArgService(CPH).GetCommandArgumentAtPosition(args, 0);
        CPH.LogInfo($"Received {receiver} as Item-Receiver");
        Treasure treasure = new TreasureService().GenerateTreasure();
        CPH.LogInfo($"Generated {treasure.Name} with Value {treasure.Value} as Item to gift");

        new InventoryService(CPH).AddItemToPlayerInventory(receiver, treasure);
        CPH.LogInfo($"Added {treasure.Name} to Characters Inventory");

        return true;
    }

    /// <summary>
    ///     Debugging Method to grant a player a random Item
    /// </summary>
    /// <returns></returns>
    public bool GiftDebugItem()
    {
        string receiver = new ArgService(CPH).GetCommandArgumentAtPosition(args, 0);
        CPH.LogInfo($"Received {receiver} as Item-Receiver");
        Treasure treasure = new Treasure("Debug Item", 1, ItemTier.Common);
        CPH.LogInfo($"Generated {treasure.Name} with Value {treasure.Value} as Item to gift");

        new InventoryService(CPH).AddItemToPlayerInventory(receiver, treasure);
        CPH.LogInfo($"Added {treasure.Name} to Characters Inventory");

        return true;
    }

    /// <summary>
    ///     Prints a players Inventory 
    ///     Should be protected on command-level, so it's only usable by broadcaster/mods
    /// </summary>
    /// <returns></returns>
    public bool PrintInventoryOfPlayer()
    {
        ArgService argSvc = new ArgService(CPH);
        InventoryService inv = new InventoryService(CPH);
        inv.ListPlayersInventory(argSvc.GetCommandArgument(args));

        return true;
    }
}