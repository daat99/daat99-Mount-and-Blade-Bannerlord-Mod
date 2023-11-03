using HarmonyLib;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace daat99.Behaviors
{
    public class Daat99PlayerTownVisitCampaignBehavior : PlayerTownVisitCampaignBehavior
    {
		private static readonly int TroopCost = 1000;

		public override void RegisterEvents()
        {
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OverrideOnAfterNewGameCreated);
        }
        
        public void OverrideOnAfterNewGameCreated(CampaignGameStarter campaignGameSystemStarter)
        {
			if (Settings.CampaignSettings.EnableFastSendTroopsToFightBandits)
			{
				campaignGameSystemStarter.AddGameMenuOption("daat99_fast_send_troops", "daat99_fast_send_troops_to_fight_bandits", "{=!}Fast - Send Troops", null, MenuHelper.EncounterOrderAttackConsequence);
				campaignGameSystemStarter.AddPlayerLine("daat99_fast_send_troops_to_fight_bandits", "bandit_attacker", "close_window", "{=!}Fast - Send Troops", null, delegate
				{
					MobileParty party2 = MobileParty.ConversationParty;
					Campaign.Current.ConversationManager.ConversationEndOneShot += delegate
					{
						PlayerEncounter.InitSimulation(null, null);
						if (PlayerEncounter.Current != null && PlayerEncounter.Current.BattleSimulation != null)
						{
							((MapState)Game.Current.GameStateManager.ActiveState).StartBattleSimulation();
						}
					};
				});

				campaignGameSystemStarter.AddGameMenuOption("daat99_fast_attack", "daat99_fast_attack_bandits", "{=!}Fast - Attack", game_menu_encounter_attack_on_condition, MenuHelper.EncounterAttackConsequence);
			}
			if (Settings.CampaignSettings.AllowRecruitGarrison)
			{
				campaignGameSystemStarter.AddGameMenuOption("town_keep", "daat99_recruit_garrison", "{=!}Recruit garrison for " + TroopCost + " denars each", game_menu_recruit_garrison_on_condition, game_menu_recruit_garrison_on_consequence);
				campaignGameSystemStarter.AddGameMenuOption("castle", "daat99_recruit_garrison", "{=!}Recruit garrison for " + TroopCost + " denars each", game_menu_recruit_garrison_on_condition, game_menu_recruit_garrison_on_consequence);
			}
		}

        private bool game_menu_encounter_attack_on_condition(MenuCallbackArgs args)
        {
			Traverse tEncounterGameMenuBehavior = Traverse.Create<EncounterGameMenuBehavior>();
			Traverse t_game_menu_encounter_attack_on_condition = tEncounterGameMenuBehavior.Method("game_menu_encounter_attack_on_condition");
			return (bool)t_game_menu_encounter_attack_on_condition.GetValue(new object[] { args });
		}

		private bool game_menu_recruit_garrison_on_condition(MenuCallbackArgs args)
		{
			Settlement currentSettlement = Settlement.CurrentSettlement;
			if (currentSettlement.OwnerClan == Clan.PlayerClan && currentSettlement.MapFaction == Hero.MainHero.MapFaction)
			{
				return currentSettlement.IsFortification;
			}
			return false;
		}

		private void game_menu_recruit_garrison_on_consequence(MenuCallbackArgs args)
		{
			Settlement currentSettlement = Hero.MainHero.CurrentSettlement;
			MobileParty garrisonParty = currentSettlement.Town.GarrisonParty;
			if (garrisonParty != null)
			{
				fillParty(garrisonParty);
				fillParty(MobileParty.MainParty);
			}
		}

		private void fillParty(MobileParty party)
		{
			CharacterObject character = Hero.MainHero.Culture.EliteBasicTroop;
			int garrisonSizeLimit = (int)Campaign.Current.Models.PartySizeLimitModel.GetPartyMemberSizeLimit(party.Party).ResultNumber;
			int currentGarrrisonSize = party.MemberRoster.TotalManCount;
			int troopsAmount = Math.Min(garrisonSizeLimit - currentGarrrisonSize, (int)(Hero.MainHero.Gold / TroopCost));
			if (troopsAmount > 0)
			{
				Hero.MainHero.ChangeHeroGold(-TroopCost * troopsAmount);
				party.MemberRoster.AddToCounts(character, troopsAmount);
				InformationManager.DisplayMessage(new InformationMessage(string.Format("Finished recruiting {0} troops for {1}.", troopsAmount, party.Name?.ToString() ?? "unknown"), Colors.Magenta));
			}
		}
	}
}
