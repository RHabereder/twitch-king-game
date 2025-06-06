# NLiths King-Game

[![.NET](https://github.com/RHabereder/twitch-king-game/actions/workflows/build.yml/badge.svg)](https://github.com/RHabereder/twitch-king-game/actions/workflows/build.yml)

This my Twitch Chat-Game that I have been working on for quite a while. I just call it "King-Game". 
It is basically a collection of Commands and code for Streamer-Bot, that enables my Chat to do funny things and engage with the stream, without having to rely on Twitch Channel-Points to do stuff. 

**So, what does this do?**

It's basically a game where you can collect an imaginary currency, find treasures, become King (Twitch VIP) and interact with the stream via special commands. 

## Integrations

Applications and Services the King-Game integrates with:

- [Twitch](https://twitch.tv)
- [Speaker.bot](https://speaker.bot)

## Things you will need additionally: 

- [pwnyy's Chat Activity Bundle](https://extensions.streamer.bot/t/pwn-chat-activity-bundle-random-chatter-active-chatters/1677/1)
- [.Net 4 Runtime](https://www.microsoft.com/en-us/download/details.aspx?id=17718)
- An IDE, I recommend Visual Studio ([Code](https://code.visualstudio.com/download) or [2022 Community](https://visualstudio.microsoft.com/de/downloads/), doesn't really matter)

## General Information About Code Quality and Platform

We are unfortunately restricted to .Net Framework 4.8, because that is what streamer.bot runs on, so there are a lot of convenience features missing compared to .NET Core 9.0.
Additionally, most of the Logic and Interaction runs via a Proxy, which is basically impossible to test reliably. 
Most of the testing happens "live", so unfortunately you have to expect bugs or broken Commands at every release 🙁






## Commands and Roles

The game is split into two main roles, [Peasants](#Peasants) and [Royalty](#Royalty). 
Each role has things they can and can't do, so frequent uprisings are highly recommended! 
Cause some healthy Chaos and light up your chat! 

Obviously you can attempt to use commands you don't have the role for, go and see what happens 😜

### All

These commands can be executed by all Chatters

|  **Command**				|      **Effect**      |
|:--------------------------|:----------------|
| !king						| Everyone can always find out who the current King is																													|
| !account					| Check how much money you have in your account																															|
| !audit					| In Fantasy-Land the IRS always knows how much the king has and every peasant has the right to check their Kings Coffers at any time! (Excludes personal treasures)	|
| !inventory				| Want to see your shiny treasures? Check your inventory with this command!																								|
| !gift  <amount> @<player>	| A friendly chatter needs money? You can gift them some of yours!																										|
| !duel	@<player>			| Want to test your Luck against another chatter? Duel them for a cash reward! (Reward is [configurable](#configuration))												|
| !announce	<text>			| If you have the cash needed, you can trigger a TTS-Announcement! (Price is [configurable](#configuration))															|
| !sell <Item Name>			| You are hard for cash? Sell your treasures!																															|
| !sell all <item Name>		| Sells **ALL** your treasures of a specific Name. Get rid of all those Bat fangs!																																			|

### Royalty

Being Royalty should always be the goal of every player. It grants you special rewards and rights, which are completely configurable.
This special standing should encourage the second class, the [Peasants](#Peasants) to either attempt regicide, so they become king themselves, or work hard to be in your good graces!

Things you can do as King:

|  **Command**				|      **Effect**      |
|:--------------------------|:---------------------|
| !decree  <text>				| Royalty can Issue a royal decree for free, which treakers a few sound-effects and Speakerbot to voice that decree to the audience |
| !taxrate <percentage>			| A kings lifestyle can be expensive, so a King needs money. With the Tax-Rate Command you can set the tax-percentage on your peasants Mining-Hauls (Which can be negative, and hilariously high) |
| !jail @<playerame> <reason>	| You can Jail Peasants and time them out for a set amount of time (See [configuration](#configuration)								|
| !abdicate						| Don't want to be king anymore? Abdicate and let the RNG decide who should be next!												|
| !crown @<player>				| Crown a chatter of your choosing instead!																							|
| !successor @<player>			| Nominate your successor! So in case of your death, that chatter will inherit your crown											|
| !expedition		 			| Embark on a kingly expedition!																																													|

### Peasants

Peasants are the backbone of the game (and the kings coffers), as well as the biggest threat to the Royalty, if treated poorly. 
These commands are exclusively usable by the Peasantry

|  **Command**		|      **Effect**      |
|:------------------|:----------------|
| !mine 			| Mining is the primary way to make money as a peasant. It is (fairly) safe, and can yield Treasures which can be sold Tax-Free! (Don't tell the king) (Reward Range and Injury Rate are [configurable](#configuration))		|
| !taxrate 			| A Peasant can check the current Tax-Rate, to decide if it's worth it to go into the mines today	(Default Tax-Rate is [configurable](#configuration))																		|
| !regicide 		| You are fed up with the current royalty? Attempt to take their crown and a sweet crowning bonus on top of it! But beware, you will be fined and jailed if you fail! (Rewards and Fines are [configurable](#configuration))	|
| !adventure 		| Embark on an adventure to gain Loot and Fame! Or hurt yourself, who knows?																																													|


### Broadcaster

In this section are special commands that only the broadcaster can execute, they are mustly there to Fix Game States or Test other functions

|  **Command**						|      **Effect**      |
|:----------------------------------|:---------------------|
| !reset @<player>					| One of your chatters dueled themselves into trillions of debt? Reset them to 0 with this (**Wipes inventory as well!**)		|
| !crownRandom						| Crown a random chatter																										|
| !moneyhax  <amount> @<player>		| Want to test some announcements? Give yourself the cash for it																|
| !giftItem @<player>				| Points to GiftDebugItem and gifts a debug Item to a specified player															|
| !inventoryOf @<player>			| Prints a players inventory, so you can investigate potential problems with Gifting/Selling									|
| !debug							| Does whatever you define in the Debug Function. For easier debugging, so you don't have to create testcommands all the time	|
| !nukeIt							| Attempts to reset the entire game-state and purges all global State-Vars. This should obviously be the last resort			|

## Configuration

The whole Logic currently resides inside [Class1.cs](King_Game_Main/Class1.cs), since the static CPH Var is handling most of the functions.
The various config values can be found between [Line X](King_Game_Main/Class1.cs#L17) and [Line Y](King_Game_Main/Class1.cs#L68) 


## Deployment

Deploying the code after changes is a bit annoying, because Streamer.bot converts your code into bytecode, which it then saves inside `<streamerbot home directory>/data/actions.json`.
There seems to be no easier way to automate the deployment right now.

### Changes in Class1.cs

1. Open The **Actions** Tab
2. Find the **\* Code** Action, which is located in the **King Game** Group
3. Double Click the **Execute Code (King Game)** Sub-Action
4. Overwrite Everything with your code from Class1.cs
5. Scroll to the Top and Remove the `: CPHInlineBase` so the Class declaration looks like this: `public class CPHInline`

The `: CPHInlineBase` declares an Inheritance, which is needed to enable VS-Code to auto-complete your code. 
I assume that Streamerbot adds this transparently in the background when you save your changes, which is very annoying, but it is what it is.


### Changes in NLith.KingGame.Backend Model

1. Stop Streamer.bot
2. Rebuild the DLL and put it into the Streamerbot Home-Directory


