using HarmonyLib;
using System;
using System.Linq;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Recruitment;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.TournamentGames;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem.GameMenus;
using daat99.BugFixes;
using daat99.Behaviors;
using System.Diagnostics;

namespace daat99
{
    public class daat99SubModule : MBSubModuleBase
	{
		public static InputKey FastLootInputKey = InputKey.F;
		
		private static CharacterDevelopmentModel s_characterDevelopmentModel;
		public static CharacterDevelopmentModel CharacterDevelopmentModel => s_characterDevelopmentModel ?? (s_characterDevelopmentModel = Campaign.Current.Models.CharacterDevelopmentModel);

		private static PrisonerRecruitmentCalculationModel s_prisonerRecruitmentCalculationModel;
		public static PrisonerRecruitmentCalculationModel PrisonerRecruitmentCalculationModel => s_prisonerRecruitmentCalculationModel ?? (s_prisonerRecruitmentCalculationModel = Campaign.Current.Models.PrisonerRecruitmentCalculationModel);

		private static PartyWageModel s_daat99WagePartyModel;
		public static PartyWageModel Daat99WagePartyModel => s_daat99WagePartyModel ?? (s_daat99WagePartyModel = Campaign.Current.Models.PartyWageModel);

		private static Random s_random;
		public static Random Random => s_random ?? (s_random = new Random());

		protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
			if (game.GameType is Campaign)
			{
				gameStarterObject.AddModel(new BetterLootingModel());
				gameStarterObject.AddModel(new Daat99PrisonerRecruitmentCalculationModel());
				gameStarterObject.AddModel(new Daat99CharacterDevelopmentModel());
				gameStarterObject.AddModel(new Daat99WagePartyModel());
				gameStarterObject.AddModel(new Daat99ClanTierModel());
				gameStarterObject.AddModel(new Daat99MobilePartyFoodConsumptionModel());
				gameStarterObject.AddModel(new Daat99PartyTroopUpgradeModel());
				gameStarterObject.AddModel(new Daat99PartyWageModel());
				gameStarterObject.AddModel(new Daat99SettlementEconomyModel());
				gameStarterObject.AddModel(new Daat99AgeModel());
				gameStarterObject.AddModel(new Daat99PartySpeedCalculatingModel());
				gameStarterObject.AddModel(new Daat99TroopSacrificeModel());

				//bugfix
				gameStarterObject.AddModel(new Daat99DiplomacyModel());

				CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarterObject;
				campaignGameStarter.AddBehavior(new BattleHealBehavior());
				campaignGameStarter.AddBehavior(new Daat99RecruitPrisonersCampaignBehavior());
				campaignGameStarter.AddBehavior(new Daat99PlayerTownVisitCampaignBehavior());


				if (Settings.CampaignSettings.DailySkillTrainingLevelUpMultiplier > 0 || Settings.CampaignSettings.EnableDailyTroopsTraining || Settings.CampaignSettings.EnableDailySettlementPrisonerRecruitment)
				{
					CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, dailyTick);
				}

				if (Settings.CampaignSettings.AnnounceTournamentsOnFinished)
				{
					CampaignEvents.TournamentFinished.AddNonSerializedListener(this, tournamentEndedEvent);
					CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, leftSettelmentEvent);
				}


				if (Settings.CampaignSettings.TrainTradeOnProfit)
				{
					//CampaignEvents.OnPlayerTradeProfitEvent.AddNonSerializedListener(this, playerTradeProfitEvent);
					CampaignEvents.HeroOrPartyTradedGold.AddNonSerializedListener(this, heroOrPartyTradedGold);
				}
				Message(string.Format("Press {0} for fast loot.", FastLootInputKey.ToString()));
			}
		}
		
		private void leftSettelmentEvent(MobileParty arg1, Settlement arg2)
		{
			if (arg1.IsMainParty)
			{
				announceTournaments();
			}
		}

        private void tournamentEndedEvent(CharacterObject arg1, MBReadOnlyList<CharacterObject> arg2, Town arg3, ItemObject arg4) => announceTournaments(true);

		private void announceTournaments(bool showInquery = false)
		{
			Dictionary<string, float> distanceTowns = new Dictionary<string, float>();
			Vec3 playerLocation = Hero.MainHero.GetPosition();
			foreach (Town town in Town.AllTowns)
			{
				if ( town.HasTournament && town.Settlement != null)
                {
					float distance = playerLocation.DistanceSquared(town.Settlement.GetPosition());
					//TournamentGame tournament = Campaign.Current.TournamentManager.GetTournamentGame(town);
					distanceTowns[$"{town.Name.ToString()} [{distance}]"] = distance;
                }
			}
			var sortedDistanceTowns = from entry in distanceTowns orderby entry.Value ascending select entry;
			string message = "No tournaments found!";
			int count = 0;
			if (sortedDistanceTowns.Any())
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("Tournaments found in towns: ");
				foreach (var entry in sortedDistanceTowns)
				{
					if (count++ < 3)
					{
						sb.Append("\r\n        ");
					}
					else
					{
						message = sb.ToString();
						break;
					}
					sb.Append(entry.Key);
				}
			}
			Message(message);
			if (showInquery && false)
			{
				InformationManager.ShowInquiry(new InquiryData("Tournament Announcement", message, true, false, "OK", "", (Action)null, (Action)null, ""), false);
			}
		}

        private void heroOrPartyTradedGold((Hero, PartyBase) arg1, (Hero, PartyBase) arg2, (int, string) arg3, bool arg4)
        {
            if ( arg1.Item1 == null && arg1.Item2 == null && arg2.Item1 == Hero.MainHero && arg3.Item1 > 0 && arg4)
            {
				playerTradeProfitEvent(arg3.Item1);
            }
        }

        private void playerTradeProfitEvent(int profit)
        {
			if ( profit > 0 )
            {
				int magnitude = (int)Math.Floor(Math.Log10(profit));
				Hero.MainHero.AddFixedXpToSkill(DefaultSkills.Trade, magnitude*Hero.MainHero.Level);
			}
		}

        private void dailyTick()
        {
			float dailyLevelUpMultiplier = Settings.CampaignSettings.DailySkillTrainingLevelUpMultiplier;
			if (dailyLevelUpMultiplier > 0)
			{
				Clan.PlayerClan?.Heroes?.Do(hero => hero.TrainRandomSkill(dailyLevelUpMultiplier));
				Clan.PlayerClan?.Companions?.Do(hero => hero.TrainRandomSkill(dailyLevelUpMultiplier));
			}
			if ( Settings.CampaignSettings.EnableDailySettlementPrisonerRecruitment)
            {
				settelementRecruitPrisoners();
            }
		}

        private void settelementRecruitPrisoners()
        {
			if (PrisonerRecruitmentCalculationModel is Daat99PrisonerRecruitmentCalculationModel daat99PrisonerRecruitmentCalculationModel)
			{
				foreach (Settlement settlement in Settlement.All.Where(s => s.IsCastle || s.IsTown))
				{
					Hero governor = null;
					if (settlement.IsTown)
					{
						governor = settlement.Town.Governor;
					}
					else if (settlement.IsCastle)
					{
						governor = settlement.Town.Governor;
					}
					if (governor != null)
					{
						int governorMaxRecruitingSkill = Math.Max(1, governor.GetSkillValue(DefaultSkills.Leadership) / 10);
						MobileParty garrison = settlement?.Town?.GarrisonParty;
						if (garrison != null)
						{
							int garrisonSize = garrison.MemberRoster.TotalManCount;
							int garrisonSizeLimit = garrison.Party.PartySizeLimit;
							int availableSpots = Math.Max(0, garrisonSizeLimit - garrisonSize);
							int troopsToRecruit = Math.Min(availableSpots, governorMaxRecruitingSkill);
							while (troopsToRecruit > 0)
							{
								TroopRoster prisoners = garrison?.PrisonRoster;
								if (prisoners?.Count > 0)
								{

									float[] recruitmentOdds = daat99PrisonerRecruitmentCalculationModel.GetDailyRecruitedPrisoners(garrison);
									for (int prisonerIndex = 0; prisonerIndex < prisoners.Count; ++prisonerIndex)
									{
										if (troopsToRecruit == 0)
										{
											break;
										}
										TroopRosterElement prisonerRoster = prisoners.GetElementCopyAtIndex(prisonerIndex);
										CharacterObject prisoner = prisonerRoster.Character;
										int tier = prisoner.Tier;
										if (tier >= 0 && tier < recruitmentOdds.Length)
										{
											int availableCount = prisonerRoster.Number;
											while (availableCount > 0 && Random.NextDouble() < recruitmentOdds[tier])
											{
												garrison.MemberRoster.AddToCounts(prisoner, 1);
												prisoners.RemoveTroop(prisoner, 1);
												governor.AddSkillXp(DefaultSkills.Leadership, 1);
												--availableCount;
												--troopsToRecruit;
												if (availableCount == 0)
												{
													--prisonerIndex;
												}
											}
										}
									}
									--troopsToRecruit;
								}
								else
								{
									break;
								}
							}
						}
						governor.AddSkillXp(DefaultSkills.Leadership, 1);
					}
				}
			}
        }

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();
			try
			{
				//Harmony.DEBUG = true;
				Harmony harmony = new Harmony("daat99HarmonyPatch");
				harmony.PatchAll();
				var methods = harmony.GetPatchedMethods();
				foreach ( var method in methods)
                {
                    Debugger.Log(1, "Harmony", $"patched method: Name: [{method.Name}], DeclaringType: [{method.DeclaringType}], ReflectedType: [{method.ReflectedType}], Module: [{method.Module}], ToString: [{method.ToString()}]");
                }

				Tester.Test();
			}
			catch(Exception ex)
			{
				string error = ex.ToString();
				MessageBox.Show("Unable to patch all using Harmony lib: {0}", error);
            }
		}

		protected override void OnApplicationTick(float dt)
		{
			if (Settings.CampaignSettings.EnableFastLoot)
			{
				try
				{
					fastLoot(dt);
				}
				catch (Exception ex)
				{ 
				}
			}
			base.OnApplicationTick(dt);
		}

		private void fastLoot(float dt)
        {
			if (Input.IsKeyPressed(FastLootInputKey) && Mission.Current?.Scene == null)
			{
				if (PartyScreenManager.Instance != null && PartyScreenManager.Instance.CurrentMode == PartyScreenMode.Loot && ActiveViewModels.partyVM != null && ActiveViewModels.partyVM.OtherPartyPrisoners.Count > 0)
				{
					Message("Grabbed " + ActiveViewModels.partyVM.OtherPartyPrisoners.Count.ToString() + " prisoners!");
					Traverse.Create(ActiveViewModels.partyVM).Method("ExecuteTransferAllOtherPrisoners").GetValue();
				}
				else if (PartyScreenManager.Instance != null && PartyScreenManager.Instance.CurrentMode == PartyScreenMode.Loot && ActiveViewModels.partyVM != null && ActiveViewModels.partyVM.OtherPartyPrisoners.Count == 0)
				{
					Traverse.Create(ActiveViewModels.partyVM).Method("ExecuteDone");//.GetValue();
				}
				else if (InventoryManager.Instance != null && InventoryManager.Instance.CurrentMode == InventoryMode.Loot && ActiveViewModels.InventoryVM != null && ActiveViewModels.InventoryVM.LeftItemListVM.Count > 0)
				{
					Message("Grabbed " + ActiveViewModels.InventoryVM.LeftItemListVM.Count.ToString() + " stacks of items!");
					ActiveViewModels.InventoryVM.ExecuteBuyAllItems();
				}
				else if (InventoryManager.Instance != null && InventoryManager.Instance.CurrentMode == InventoryMode.Loot && ActiveViewModels.InventoryVM != null && ActiveViewModels.InventoryVM.LeftItemListVM.Count == 0)
				{
					Traverse.Create(ActiveViewModels.InventoryVM).Method("ExecuteCompleteTranstactions").GetValue();
				}
				//else if (ActiveViewModels.SkipWeaponNaming && ActiveViewModels.WeaponDesignVM != null && ActiveViewModels.WeaponDesignVM.IsInFinalCraftingStage)
				//{
				//	Traverse.Create(ActiveViewModels.WeaponDesignVM).Method("ExecuteFinalizeCrafting").GetValue();
				//}
				else if (ActiveViewModels.RecruitmentVM != null && ActiveViewModels.RecruitmentVM.CanRecruitAll)
				{
					Traverse.Create(ActiveViewModels.RecruitmentVM).Method("ExecuteRecruitAll").GetValue();
				}
				else if (ActiveViewModels.RecruitmentVM != null && (ActiveViewModels.RecruitmentVM.TroopsInCart.Count > 0 || ActiveViewModels.RecruitmentVM.VolunteerList.Sum((RecruitVolunteerVM volunteerList) => volunteerList.Troops.Sum((RecruitVolunteerTroopVM troop) => (!troop.IsInCart && troop.CanBeRecruited) ? 1 : 0)) == 0))
				{
					Traverse.Create(ActiveViewModels.RecruitmentVM).Method("ExecuteDone").GetValue();
				}
				//else if (ActiveViewModels.WeaponDesignVM == null && ActiveViewModels.RecruitmentVM == null && ActiveViewModels.MenuContext != null && ((MBObjectBase)ActiveViewModels.MenuContext).IsReady && ActiveViewModels.MenuContext.GameMenu != null && (ActiveViewModels.MenuContext.GameMenu.MenuTitle.ToString() == "Village" || ActiveViewModels.MenuContext.GameMenu.MenuTitle.ToString() == "Town Center"))
				//{
				//	ActiveViewModels.MenuContext.OpenRecruitVolunteers();
				//}
			}
			if (Input.IsKeyPressed(FastLootInputKey) && Mission.Current?.Scene == null)
			{
				bool execute = false;
				if (PartyScreenManager.Instance != null && PartyScreenManager.Instance.CurrentMode == PartyScreenMode.Loot && ActiveViewModels.partyVM != null && ActiveViewModels.partyVM.OtherPartyPrisoners.Count > 0)
				{
					if (execute)
					{
						ActiveViewModels.partyVM.ExecuteTransferAllOtherPrisoners();
					}
					else
					{
						Message("Grabbed " + ActiveViewModels.partyVM.OtherPartyPrisoners.Count.ToString() + " prisoners!");
						Traverse.Create(ActiveViewModels.partyVM).Method("ExecuteTransferAllOtherPrisoners").GetValue();
					}
				}
				else if (PartyScreenManager.Instance != null && PartyScreenManager.Instance.CurrentMode == PartyScreenMode.Loot && ActiveViewModels.partyVM != null && ActiveViewModels.partyVM.OtherPartyPrisoners.Count == 0)
				{
					if (execute)
					{
						ActiveViewModels.partyVM.ExecuteDone();
					}
					else
					{
						ActiveViewModels.partyVM.ExecuteDone();
						Traverse.Create(ActiveViewModels.partyVM).Method("ExecuteDone").GetValue();
					}
				}
				else if (InventoryManager.Instance != null && InventoryManager.Instance.CurrentMode == InventoryMode.Loot && ActiveViewModels.InventoryVM != null && ActiveViewModels.InventoryVM.LeftItemListVM.Count > 0)
				{
					Message("Grabbed " + ActiveViewModels.InventoryVM.LeftItemListVM.Count.ToString() + " stacks of items!");
					ActiveViewModels.InventoryVM.ExecuteBuyAllItems();
				}
				else if (InventoryManager.Instance != null && InventoryManager.Instance.CurrentMode == InventoryMode.Loot && ActiveViewModels.InventoryVM != null && ActiveViewModels.InventoryVM.LeftItemListVM.Count == 0)
				{
					if (execute)
					{
						ActiveViewModels.InventoryVM.ExecuteCompleteTranstactions();
					}
					else
					{
						Traverse.Create(ActiveViewModels.InventoryVM).Method("ExecuteCompleteTranstactions").GetValue();
					}
				}
				//else if (ActiveViewModels.SkipWeaponNaming && ActiveViewModels.WeaponDesignVM != null && ActiveViewModels.WeaponDesignVM.IsInFinalCraftingStage)
				//{
				//	Traverse.Create(ActiveViewModels.WeaponDesignVM).Method("ExecuteFinalizeCrafting").GetValue();
				//}
				else if (ActiveViewModels.RecruitmentVM != null && ActiveViewModels.RecruitmentVM.CanRecruitAll)
				{
					if (execute)
					{
						ActiveViewModels.RecruitmentVM.ExecuteRecruitAll();
					}
					else
					{
						Traverse.Create(ActiveViewModels.RecruitmentVM).Method("ExecuteRecruitAll").GetValue();
					}
				}
				else if (ActiveViewModels.RecruitmentVM != null && (ActiveViewModels.RecruitmentVM.TroopsInCart.Count > 0 || ActiveViewModels.RecruitmentVM.VolunteerList.Sum((RecruitVolunteerVM volunteerList) => volunteerList.Troops.Sum((RecruitVolunteerTroopVM troop) => (!troop.IsInCart && troop.CanBeRecruited) ? 1 : 0)) == 0))
				{
					if (execute)
					{
						ActiveViewModels.RecruitmentVM.ExecuteDone();
					}
					else
					{
						Traverse.Create(ActiveViewModels.RecruitmentVM).Method("ExecuteDone").GetValue();
					}
				}
				//else if (ActiveViewModels.WeaponDesignVM == null && ActiveViewModels.RecruitmentVM == null && ActiveViewModels.MenuContext != null && ((MBObjectBase)ActiveViewModels.MenuContext).IsReady && ActiveViewModels.MenuContext.GameMenu != null && (ActiveViewModels.MenuContext.GameMenu.MenuTitle.ToString() == "Village" || ActiveViewModels.MenuContext.GameMenu.MenuTitle.ToString() == "Town Center"))
				//{
				//	ActiveViewModels.MenuContext.OpenRecruitVolunteers();
				//}
			}
		}
		private void Message(string s)
		{
			InformationManager.DisplayMessage(new InformationMessage(s));
		}
	}
}
