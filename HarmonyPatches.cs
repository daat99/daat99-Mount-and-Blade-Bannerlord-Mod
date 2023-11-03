using HarmonyLib;
using Helpers;
using SandBox.Missions.MissionLogics;
using SandBox.View.Map;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.CraftingSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Recruitment;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.Smelting;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace daat99
{
	/*
	// Note: the following is very simplified and only used to illustrate
	// the difference between prefix/postfix and a transpiler.
	REPLACEMENT()
	{
		if (Prefix_1() == false) return
		// ...
		if (Prefix_n() == false) return

		// replacement_IL_here

		Postfix_1()
		// ...
		Postfix_n()
	}
	*/

	//BuildingBoostAddMultiplier
	[HarmonyPatch(typeof(BuildingHelper), "BoostBuildingProcessWithGold")]
	public class BuildingHelper_BoostBuildingProcessWithGold
	{
		private static float cBuildingBoostAddMultiplier = Settings.CampaignSettings.BuildingBoostAddMultiplier;
		private static bool m_isMultiplying = false;
		public static bool Prefix(int gold, Town town)
		{
			if (false == m_isMultiplying && town.OwnerClan == Clan.PlayerClan && cBuildingBoostAddMultiplier != 1.0f)
			{
				int boostAmount = gold - town.BoostBuildingProcess;
				boostAmount = (int)(boostAmount * cBuildingBoostAddMultiplier);
				if ( Hero.MainHero.Gold > boostAmount*10 )
                {
					m_isMultiplying = true;
					BuildingHelper.BoostBuildingProcessWithGold(boostAmount + town.BoostBuildingProcess, town);
					return false; //skip original
				}
			}
			m_isMultiplying = false;
			return true; //perform original
		}
		/*
			// Helpers.BuildingHelper
			public static void BoostBuildingProcessWithGold(int gold, Town town)
		 */
	}



	[HarmonyPatch(typeof(HeroDeveloper), "AddSkillXp")]
	public class HeroDeveloper_AddSkillXp
	{
		private const float MINIMAL_XP = 0.01f;
		private static float sMinimalXpGainFactor = Settings.CampaignSettings.MinimalXpGainFactor;
		public static bool Prefix(HeroDeveloper __instance, SkillObject skill, float rawXp, bool isAffectedByFocusFactor = true, bool shouldNotify = true)
		{
			if (isAffectedByFocusFactor && rawXp > 0 && sMinimalXpGainFactor > 0 && __instance.GetFocusFactor(skill) <= MINIMAL_XP)
			{
				float minimalXp = Math.Max(MINIMAL_XP, rawXp * sMinimalXpGainFactor);
				__instance.AddSkillXp(skill, minimalXp, false, shouldNotify);
				return false; //skip original
			}
			return true; //perform original
		}
		/*
			// TaleWorlds.CampaignSystem.CharacterDevelopment.HeroDeveloper
			using TaleWorlds.Core;

			public void AddSkillXp(SkillObject skill, float rawXp, bool isAffectedByFocusFactor = true, bool shouldNotify = true)

		 */
	}

	//OnlyDayBattles
	[HarmonyPatch(typeof(DefaultMapWeatherModel), "GetEnvironmentMultiplier")]
	public class DefaultMapWeatherModel_CreateMission
	{
		public static bool Prefix(DefaultMapWeatherModel __instance, object sunPos, ref float __result)
		{
			if (Settings.CampaignSettings.OnlyDayBattles)
			{
				Traverse defaultMapWeatherModel = Traverse.Create(__instance);
				Traverse _sunIsMoonField = defaultMapWeatherModel.Field("_sunIsMoon");
				InformationManager.DisplayMessage(new InformationMessage($"Setting _sunIsMoon from {_sunIsMoonField.GetValue()} to {__result}."));
				_sunIsMoonField.SetValue(false);
			}
			return true; //perform original
		}
	}


	// Helpers.PerkHelper
	//PlayerIsGovernor
	[HarmonyPatch(typeof(PerkHelper), "AddPerkBonusForTown")]
	public class PerkHelper_AddPerkBonusForTown
	{
		public static bool Prefix(PerkObject perk, Town town, ref ExplainedNumber bonuses)
        {
			if (Settings.CampaignSettings.PlayerIsGovernor && town != null &&/* town.Governor == null &&*/ town.OwnerClan == Clan.PlayerClan)
			{
				if ( Hero.MainHero.GetPerkValue(perk) )
                {
					ExecuteAddToStatMethod(ref bonuses, perk.PrimaryIncrementType, perk.PrimaryBonus, perk.Name);
				}
				return false; //skip original
			}
			return true; //perform original
		}

		private static MethodInfo s_addToStatMethod;
		private static MethodInfo AddToStatMethod => s_addToStatMethod ?? (s_addToStatMethod = typeof(PerkHelper).GetMethodInfo("AddToStat"));
		private static void ExecuteAddToStatMethod(ref ExplainedNumber stat, SkillEffect.EffectIncrementType effectIncrementType, float number, TextObject text)
		{
			//reflection with ref parameter
			object[] arguments = new object[4] { stat, effectIncrementType, number, text };
			AddToStatMethod.Invoke(null, arguments);
			stat = (ExplainedNumber)arguments[0];
		}
		/*
			// Helpers.PerkHelper
			using TaleWorlds.CampaignSystem;
			using TaleWorlds.CampaignSystem.CharacterDevelopment;
			using TaleWorlds.CampaignSystem.Settlements;

			public static void AddPerkBonusForTown(PerkObject perk, Town town, ref ExplainedNumber bonuses)
			{
				bool flag = perk.PrimaryRole == SkillEffect.PerkRole.Governor;
				bool flag2 = perk.SecondaryRole == SkillEffect.PerkRole.Governor;
				if (!(flag | flag2))
				{
					return;
				}
				Hero governor = town.Governor;
				if (governor != null && governor.GetPerkValue(perk) && governor.CurrentSettlement != null && governor.CurrentSettlement == town.Settlement)
				{
					if (flag)
					{
						AddToStat(ref bonuses, perk.PrimaryIncrementType, perk.PrimaryBonus, perk.Name);
					}
					else
					{
						AddToStat(ref bonuses, perk.SecondaryIncrementType, perk.SecondaryBonus, perk.Name);
					}
				}
			}
		 */
	}



	//PlayerIsGovernor
	[HarmonyPatch(typeof(Town), "get_Governor")]
	public class Town_Governor
	{
		public static void Prefix(Town __instance)
		{
			if (Settings.CampaignSettings.PlayerIsGovernor && __instance.GetFieldData<Hero>(_governorField) == null && __instance?.OwnerClan == Clan.PlayerClan)
			{
				__instance.SetFieldData<Hero>(_governorField, Hero.MainHero);
			}
		}

		private static FieldInfo s_governor;
		private static FieldInfo _governorField => s_governor ?? (s_governor = typeof(Town).GetFieldInfo("_governor"));
	}
	//PlayerIsGovernor bug fix
	[HarmonyPatch(typeof(ChangeGovernorAction), "RemoveGovernorOfIfExists")]
	public class ChangeGovernorAction_RemoveGovernorOfIfExists
	{
		public static bool Prefix(Town town)
		{
			if (town.Governor != null && town.Governor != Hero.MainHero)
			{
				ExecuteApplyGiveUpInternalMethod(town.Governor);
				return false; //skip original
			}
			return false; //skip original
		}

		private static MethodInfo s_ApplyGiveUpInternal;
		private static MethodInfo ApplyGiveUpInternalMethod => s_ApplyGiveUpInternal ?? (s_ApplyGiveUpInternal = typeof(ChangeGovernorAction).GetMethodInfo("ApplyGiveUpInternal"));
		private static void ExecuteApplyGiveUpInternalMethod(Hero governor) => ApplyGiveUpInternalMethod.Invoke(null, new object[] { governor });
	}

	[HarmonyPatch(typeof(TownManagementVM), "OnGovernorSelectionDone")]
	public class TownManagementVM_OnGovernorSelectionDone
	{
		public static bool Prefix(TownManagementVM __instance, Hero selectedGovernor)
		{
			if (Settings.CampaignSettings.PlayerIsGovernor && selectedGovernor == null && Settlement.CurrentSettlement?.Town?.OwnerClan == Clan.PlayerClan)
            {
				//selectedGovernor = Hero.MainHero;
				//ExecuteOnGovernorSelectionDoneMethod(__instance, selectedGovernor);
				//return false;
            }
			return true; //run original method
		}


		private static MethodInfo s_OnGovernorSelectionDone;
		private static MethodInfo OnGovernorSelectionDoneMethod => s_OnGovernorSelectionDone ?? (s_OnGovernorSelectionDone = typeof(TownManagementVM).GetMethodInfo("OnGovernorSelectionDone"));
		private static void ExecuteOnGovernorSelectionDoneMethod(TownManagementVM __instance, Hero selectedGovernor) => OnGovernorSelectionDoneMethod.Invoke(__instance, new object[] { selectedGovernor });
	}

	//BuildAllLevels
	[HarmonyPatch(typeof(Town), "TickCurrentBuilding")]
	public class BuildAllLevelsPatch
	{
		public static bool Prefix(Town __instance)
		{
			if (Settings.CampaignSettings.BuildAllLevels)
			{
				if (__instance.BuildingsInProgress.Peek().CurrentLevel == 3)
				{
					__instance.BuildingsInProgress.Dequeue();
				}
				if (__instance.Owner.Settlement.IsUnderSiege || TaleWorlds.Core.Extensions.IsEmpty(__instance.BuildingsInProgress))
				{
					return false;
				}
				Building building = __instance.BuildingsInProgress.Peek();
				building.BuildingProgress += __instance.Construction;
				int num = (__instance.IsCastle ? 250 : 500);
				if (__instance.BoostBuildingProcess > 0)
				{
					__instance.BoostBuildingProcess -= num;
					if (__instance.BoostBuildingProcess < 0)
					{
						__instance.BoostBuildingProcess = 0;
					}
				}
				if ((float)building.GetConstructionCost() <= building.BuildingProgress)
				{
					if (building.CurrentLevel < 3)
					{
						building.LevelUp();
					}
					if (building.CurrentLevel == 3)
					{
						building.BuildingProgress = building.GetConstructionCost();
						__instance.BuildingsInProgress.Dequeue();
					}
				}
				return false;
			}
			return true;
		}
	}

	//Main hero spouse is always fertile
	[HarmonyPatch(typeof(DefaultPregnancyModel), "IsHeroAgeSuitableForPregnancy")]
	public class IsHeroAgeSuitableForPregnancyPatch
	{
		public static bool Prefix(ref bool __result, Hero hero)
		{
			if (Settings.CampaignSettings.HeroSpouseIsAlwaysFertile && (hero?.Spouse == Hero.MainHero || hero == Hero.MainHero))
			{
				__result = true;
			}
			return false;
		}
	}

	//use mount in towns
	[HarmonyPatch(typeof(MissionAgentHandler), "SpawnPlayer")]
	internal class EnterTownOnMountPatch
	{
		public static bool Prefix(MissionAgentHandler __instance, bool civilianEquipment = false, bool noHorses = false, bool noWeapon = false, bool wieldInitialWeapons = false, bool isStealth = false, string spawnTag = "")
        {
			if ( noHorses)
            {
				__instance.SpawnPlayer(civilianEquipment, false, noWeapon, wieldInitialWeapons, isStealth, spawnTag);
				return false;
            }
			return true;
        }
	}

	//add remote companion to character list
	[HarmonyPatch(typeof(SPInventoryVM), "CharacterList", MethodType.Getter)]
	public class SPInventoryVMPatchv2
	{
		private static void Postfix(SelectorVM<InventoryCharacterSelectorItemVM> ____characterList)
		{
			if (Settings.CampaignSettings.ManageRemoteCompanions)
			{
				foreach (Hero hero in Clan.PlayerClan.Heroes)
				{
					bool isAlreadyOnList = Enumerable.Any(Enumerable.Where(____characterList.ItemList, delegate (SelectorItemVM e)
					{
						return e.StringItem == hero.Name.ToString();
					}));
					if (hero.IsAlive && hero.IsActive && !hero.IsChild && hero != Hero.MainHero && !isAlreadyOnList)
					{
						try
						{
							InventoryCharacterSelectorItemVM heroSelector = new InventoryCharacterSelectorItemVM(hero.CharacterObject.StringId, hero, hero.Name);
							____characterList.AddItem(heroSelector);
						}
						catch(Exception ex)
                        {

                        }
					}
				}
			}
		}
	}
	//add remote companion to inventory list
	[HarmonyPatch(typeof(InventoryLogic), "InitializeRosters")]
	internal class PatchInventoryInit
	{
		public static void Prefix(InventoryLogic __instance, ItemRoster leftItemRoster, ItemRoster rightItemRoster, ref TroopRoster rightMemberRoster, CharacterObject initialCharacterOfRightRoster)
        {
			if (Settings.CampaignSettings.ManageRemoteCompanions && rightMemberRoster.Contains(Hero.MainHero.CharacterObject))
			{
				TroopRoster newRoster = TroopRoster.CreateDummyTroopRoster();
				newRoster.Add(rightMemberRoster);
				foreach (Hero hero2 in Clan.PlayerClan.Heroes)
				{
					if (hero2.IsAlive && !hero2.IsChild && !newRoster.Contains(hero2.CharacterObject))
					{
						newRoster.AddToCounts(hero2.CharacterObject, 1);
					}
				}
				foreach (Hero hero in Clan.PlayerClan.Companions)
				{
					if (hero.IsAlive && hero.IsPlayerCompanion && !newRoster.Contains(hero.CharacterObject))
					{
						newRoster.AddToCounts(hero.CharacterObject, 1);
					}
				}
				rightMemberRoster = newRoster;
			}
		}
		/*public static void Prefix(ref TroopRoster rightMemberRoster)
		{
			if ( Settings.CampaignSettings.ManageRemoveCompanions && rightMemberRoster.Contains(Hero.MainHero.CharacterObject))
			{
				TroopRoster newRoster = TroopRoster.CreateDummyTroopRoster();
				newRoster.Add(rightMemberRoster);
				foreach (Hero hero2 in Clan.PlayerClan.Heroes)
				{
					if (hero2.IsAlive && !hero2.IsChild && hero2 != Hero.MainHero && !newRoster.Contains(hero2.CharacterObject))
					{
						newRoster.AddToCounts(hero2.CharacterObject, 1);
					}
				}
				foreach (Hero hero in Clan.PlayerClan.Companions)
				{
					if (hero.IsAlive && hero.IsPlayerCompanion && !newRoster.Contains(hero.CharacterObject))
					{
						newRoster.AddToCounts(hero.CharacterObject, 1);
					}
				}
				rightMemberRoster = newRoster;
			}
		}*/
	}

	//allow add attributes to non-party heroes
	[HarmonyPatch(typeof(CharacterAttributeItemVM), "RefreshWithCurrentValues")]
	internal class PatchCanAddAttributePoint
	{
		public static void Postfix(CharacterAttributeItemVM __instance)
		{
			if (Settings.CampaignSettings.ManageRemoteCompanions)
			{
				__instance.CanAddPoint = __instance.AttributeValue < 10 && __instance.UnspentAttributePoints > 0;
			}
		}
	}

	//allow add focus to non-party heroes
	[HarmonyPatch(typeof(CharacterVM), "CanAddFocusToSkillWithFocusAmount")]
	internal class PatchCanAddFocusPoint
	{
		public static void Postfix(ref CharacterVM __instance, int currentFocusAmount, ref bool __result)
		{
			if (false == __result && Settings.CampaignSettings.ManageRemoteCompanions && __instance.UnspentCharacterPoints > 0 && currentFocusAmount < 5)
			{
				__result = true;
			}
		}
	}

	//allow perk selection for non-party heroes
	[HarmonyPatch(typeof(SkillVM), "IsPerkAvailable")]
	internal class PatchIsPerkAvailable
	{
		public static void Postfix(SkillVM __instance, PerkObject perk, ref bool __result)
		{
			if (Settings.CampaignSettings.ManageRemoteCompanions && __instance.Level >= perk.RequiredSkillValue)
			{
				__result = true;
			}
		}
	}

	//open all blacksmith parts
	[HarmonyPatch(typeof(CraftingCampaignBehavior), "OpenPart")]
	public class BlacksmithOpenPartPatch
	{
		private static bool Prefix() => false == Settings.CampaignSettings.UnlockAllBlacksmithParts;
	}

	//open all blacksmith parts
	[HarmonyPatch(typeof(CraftingCampaignBehavior), "IsOpened")]
	public class BlacksmithIsOpenedPatch
	{
		private static bool Prefix(ref bool __result)
		{
			if (Settings.CampaignSettings.UnlockAllBlacksmithParts)
			{
				__result = true;
				return false;
			}
			return true;
		}
	}

	//auto sell
	#region auto sell
	[HarmonyPatch(typeof(SPInventoryVM))]
	[HarmonyPatch(new Type[]
	{
		typeof(InventoryLogic),
		typeof(bool),
		typeof(Func<WeaponComponentData, ItemObject.ItemUsageSetFlags>),
		typeof(string),
		typeof(string)
	})]
	[HarmonyPatch(MethodType.Constructor)]
	internal class AutoSellGetInventoryVMPostfix
	{
		private static SPInventoryVM Instance;
		private static FieldInfo s_inventoryLogicField;
		private static FieldInfo InventoryLogicField => s_inventoryLogicField ?? (s_inventoryLogicField = typeof(SPInventoryVM).GetFieldInfo("_inventoryLogic"));
		private static InventoryLogic InventoryLogic => InventoryLogicField.GetValue(Instance) as InventoryLogic;

		public static void Postfix(SPInventoryVM __instance)
		{
			Instance = __instance;
			if (InventoryManager.Instance.CurrentMode == InventoryMode.Trade)
			{
				int itemAmountDivider = 5;
				ItemRoster itemRoster = PartyBase.MainParty.ItemRoster;
				IViewDataTracker campaignBehavior = Campaign.Current.GetCampaignBehavior<IViewDataTracker>();
				IEnumerable<string> lockedItemIDs = campaignBehavior.GetInventoryLocks().ToList();
				foreach (ItemRosterElement item in Enumerable.ToList(itemRoster))
				{
					int num = item.Amount;
					bool flag = false;
					if (item.EquipmentElement.IsQuestItem)
					{
						Debugger.Log(1, "AutoSell", $"Quest Item {item}{Environment.NewLine}");
						continue;
					}
					bool isLocked = false;
					//is item locked start
					string text = item.EquipmentElement.Item.StringId;
					if (item.EquipmentElement.ItemModifier != null)
					{
						text += item.EquipmentElement.ItemModifier.StringId;
					}
					isLocked = lockedItemIDs!= null && lockedItemIDs.Contains(text);
					//is item locked end
					if (isLocked)
					{
						Debugger.Log(1, "AutoSell", $"Locked Item {item}{Environment.NewLine}");
						continue;
					}
					int amount = 0;
					ItemObject itemObject = item.EquipmentElement.Item;
					if ( itemObject == null)
                    {
						Debugger.Log(1, "AutoSell", $"Null Item Object {item}{Environment.NewLine}");
						continue;
					}
					ItemObject.ItemTypeEnum itemType = itemObject.Type;
					if (itemType == ItemObject.ItemTypeEnum.Invalid)
                    {
						Debugger.Log(1, "AutoSell", $"Invalid Item Type {item}{Environment.NewLine}");
						continue;
                    }
					switch (itemType)
                    {
						case ItemObject.ItemTypeEnum.Horse:
							amount = item.Amount - MobileParty.MainParty?.Party?.NumberOfAllMembers / itemAmountDivider ?? 100;
							Debugger.Log(1, "AutoSell", $"Selling Horses Item: {item}, amount: {amount}, type: {itemType}{Environment.NewLine}");
							break;
						case ItemObject.ItemTypeEnum.Goods:
							amount = item.Amount - MobileParty.MainParty?.Party?.NumberOfAllMembers / itemAmountDivider ?? 100;
							if (itemObject.IsFood && item.Amount >= 1)
                            {
								amount = item.Amount - 1;
                            }
							Debugger.Log(1, "AutoSell", $"Selling Goods Item: {item}, amount: {amount}, type: {itemType}{Environment.NewLine}");
							break;
						case ItemObject.ItemTypeEnum.Arrows:
						case ItemObject.ItemTypeEnum.Bolts:
						case ItemObject.ItemTypeEnum.Shield:
						case ItemObject.ItemTypeEnum.Bow:
						case ItemObject.ItemTypeEnum.Crossbow:
						case ItemObject.ItemTypeEnum.HeadArmor:
						case ItemObject.ItemTypeEnum.BodyArmor:
						case ItemObject.ItemTypeEnum.LegArmor:
						case ItemObject.ItemTypeEnum.HandArmor:
						case ItemObject.ItemTypeEnum.Bullets:
						case ItemObject.ItemTypeEnum.Animal:
						case ItemObject.ItemTypeEnum.Book:
						case ItemObject.ItemTypeEnum.ChestArmor:
						case ItemObject.ItemTypeEnum.Cape:
						case ItemObject.ItemTypeEnum.HorseHarness:
							Debugger.Log(1, "AutoSell", $"Selling Item: {item}, amount: {item.Amount}, type: {itemType}{Environment.NewLine}");
							amount = item.Amount;
							break;
						case ItemObject.ItemTypeEnum.Banner:
						case ItemObject.ItemTypeEnum.Polearm:
						case ItemObject.ItemTypeEnum.Thrown:
						case ItemObject.ItemTypeEnum.Pistol:
						case ItemObject.ItemTypeEnum.Musket:
						case ItemObject.ItemTypeEnum.OneHandedWeapon:
						case ItemObject.ItemTypeEnum.TwoHandedWeapon:
						default:
							Debugger.Log(1, "AutoSell", $"Keeping Item: {item}, amount: {item.Amount}, type: {itemType}{Environment.NewLine}");
							continue;
					}
					if (amount > 0)
					{
						int merchantGold = InventoryLogic.InventoryListener.GetGold() + InventoryLogic.TotalAmount;
						int itemPrice = InventoryLogic.GetItemPrice(item.EquipmentElement, false);
						Debugger.Log(1, "AutoSell", $"Testing Sell Command Item: {item}, amount: {amount}/{item.Amount}, type: {itemType}, merchant gold: {merchantGold}{Environment.NewLine}");
						amount = Math.Min(amount, merchantGold / itemPrice);
						Debugger.Log(1, "AutoSell", $"Sending Sell Command Item: {item}, amount: {amount}/{item.Amount}, type: {itemType}, merchant gold: {merchantGold}{Environment.NewLine}");
						if (amount > 0)
						{
							TransferCommand command = TransferCommand.Transfer(amount, InventoryLogic.InventorySide.PlayerInventory, InventoryLogic.InventorySide.OtherInventory, item, EquipmentIndex.None, EquipmentIndex.None, Hero.MainHero.CharacterObject, false);
							InventoryLogic.AddTransferCommand(command);
						}
					}
				}
			}
		}
	}
	#endregion

	//fast loot patches start
	#region fast loot
	[HarmonyPatch(typeof(SPInventoryVM))]
	[HarmonyPatch(new Type[]
	{
		typeof(InventoryLogic),
		typeof(bool),
		typeof(Func<WeaponComponentData, ItemObject.ItemUsageSetFlags>),
		typeof(string),
		typeof(string)
	})]
	[HarmonyPatch(MethodType.Constructor)]
	internal class FastLootGetInventoryVMPostfix
	{
		public static void Postfix(SPInventoryVM __instance)
		{
			if (Settings.CampaignSettings.EnableFastLoot && InventoryManager.Instance.CurrentMode == InventoryMode.Loot)
			{
				ActiveViewModels.InventoryVM = __instance;
			}
		}
	}
	[HarmonyPatch(typeof(SPInventoryVM), "ExecuteCompleteTranstactions")]
	internal class RemoveOldInventoryVMPostfix
	{
		public static void Postfix() => ActiveViewModels.InventoryVM = null;
	}

	[HarmonyPatch(typeof(MenuContext))]
	[HarmonyPatch(new Type[] { })]
	[HarmonyPatch(MethodType.Constructor)]
	internal class GetMenuContextPostfix
	{
		public static void Postfix(MenuContext __instance) => ActiveViewModels.MenuContext = __instance;
	}
	[HarmonyPatch(typeof(MenuContext), "Destroy")]
	internal class RemoveMenuContextPostfix
	{
		public static void Postfix() => ActiveViewModels.MenuContext = null;
	}

	// E:\Games\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.CampaignSystem.ViewModelCollection.dll
	// namespace TaleWorlds.CampaignSystem.ViewModelCollection.Party
	[HarmonyPatch(typeof(PartyVM))]
	[HarmonyPatch(new Type[]
	{
		typeof(PartyScreenLogic)
	})]
	[HarmonyPatch(MethodType.Constructor)]
	internal class GetPartyVMPostfix
	{
		public static void Postfix(PartyVM __instance)
		{
			if (Settings.CampaignSettings.EnableFastLoot && PartyScreenManager.Instance != null && PartyScreenManager.Instance.CurrentMode == PartyScreenMode.Loot)
			{
				ActiveViewModels.partyVM = __instance;
			}
		}
	}
	[HarmonyPatch(typeof(PartyVM), "ExecuteDone")]
	internal class RemoveOldPartyVMPostfix
	{
		public static void Postfix() => ActiveViewModels.partyVM = null;
	}

	[HarmonyPatch(typeof(RecruitmentVM))]
	[HarmonyPatch(new Type[] { })]
	[HarmonyPatch(MethodType.Constructor)]
	internal class GetRecruitmentVMPostfix
	{
		public static void Postfix(RecruitmentVM __instance) => ActiveViewModels.RecruitmentVM = __instance;
	}
	[HarmonyPatch(typeof(RecruitmentVM), "ExecuteDone")]
	internal class RemoveRecruitmentVMPostfix
	{
		public static void Postfix() => ActiveViewModels.RecruitmentVM = null;
	}

	[HarmonyPatch(typeof(WeaponDesignVM))]
	[HarmonyPatch(new Type[]
	{
		typeof(Crafting),
		typeof(ICraftingCampaignBehavior),
		typeof(Action),
		typeof(Action),
		typeof(Func<CraftingAvailableHeroItemVM>),
		typeof(Action<CraftingOrder>), 
		typeof(Func<WeaponComponentData, ItemObject.ItemUsageSetFlags>)
	})]
	[HarmonyPatch(MethodType.Constructor)]
	internal class GetWeaponDesignVMPostfix
	{
		public static void Postfix(WeaponDesignVM __instance) => ActiveViewModels.WeaponDesignVM = __instance;
	}
	[HarmonyPatch(typeof(WeaponDesignVM), "ExecuteFinalizeCrafting")]
	internal class RemoveOldWeaponDesignVMPostfix
	{
		public static void Postfix() => ActiveViewModels.WeaponDesignVM = null;
	}
    #endregion fast loot
    //fast loot patches end

    //loot executed lord
    [HarmonyPatch(typeof(PartyScreenLogic), "ExecuteTroop")]
	public class ExecuteTroopLootLord
	{
		public static void Postfix(PartyScreenLogic.PartyCommand command)
		{
			float dropChance = Settings.CampaignSettings.ItemDropChanceOnLordExecution;
			if (dropChance > 0)
			{
				Random random = new Random();
				CharacterObject character = command.Character;

				List<ItemObject> lootItems = new List<ItemObject>();
				for (int i = 0; i < Equipment.EquipmentSlotLength; i++)
				{
					ItemObject item = character?.HeroObject?.BattleEquipment[i].Item;
					if (item != null)
					{
						lootItems.Add(item);
					}
				}
				lootItems.Sort((a, b) => a.Value.CompareTo(b.Value));
				foreach (ItemObject item in lootItems)
				{
					if (random.NextDouble() <= dropChance)
					{
						PartyBase.MainParty.ItemRoster.AddToCounts(item, 1);
						InformationManager.DisplayMessage(new InformationMessage(item.Name.ToString() + " Added - Drop Rate: " + dropChance * 100f + " %"));
					}
				}
			}
		}
	}

	//player sells item for cheaper
	[HarmonyPatch(typeof(DefaultTradeItemPriceFactorModel), "GetPriceFactor")]
	public class GetPriceFactorPatch
	{
		public static void Postfix(DefaultTradeItemPriceFactorModel __instance, ItemObject item, MobileParty tradingParty, PartyBase merchant, float inStoreValue, float supply, float demand, bool isSelling, ref float __result)
		{

			float deductionRate = Settings.CampaignSettings.EquipmentPriceFactorDeductionRatePerExponent;
			if (deductionRate > 0 && isSelling && item.IsCraftedWeapon)
			{
				int itemValue = item.Value;
				while ( itemValue > 0 )
                {
					__result *= deductionRate;
					itemValue /= 10;
                }
			}
		}
	}

	//pregnancy nearby check
	[HarmonyPatch(typeof(PregnancyCampaignBehavior), "CheckAreNearby")]
	public class PregnancyCheckAreNearby
	{
		private static void Postfix(Hero hero, Hero spouse, ref bool __result)
		{
			if (Settings.CampaignSettings.AllowPregnancyInSameParty && (hero == Hero.MainHero || hero?.Spouse == Hero.MainHero) && hero?.PartyBelongedTo != null && hero.PartyBelongedTo == hero?.Spouse?.PartyBelongedTo)
			{
				__result = true;
				InformationManager.DisplayMessage(new InformationMessage(string.Format("{0} pregnancy check in the same party as spouse.", hero.Name?.ToString())));
			}
		}
	}


	//focus points per level
	[HarmonyPatch(typeof(DefaultCharacterDevelopmentModel), "get_FocusPointsPerLevel")]
	public class GetFocusPointsPerLevelPatch
	{
		private static bool Prefix(ref int __result) 
		{
			int focusPointsPerLevel = Settings.CampaignSettings.FocusPointsPerLevel;
			if (focusPointsPerLevel > 0)
			{
				__result = focusPointsPerLevel;
				return false;
			}
			return true;
		}
	}

	//attribute points per level
	[HarmonyPatch(typeof(DefaultCharacterDevelopmentModel), "get_LevelsPerAttributePoint")]
	public class GetLevelsPerAttributePointPatch
	{
		private static bool Prefix(ref int __result) 
		{
			int levelsPerAttribute = Settings.CampaignSettings.LevelsPerAttribute;
			if (levelsPerAttribute > 0)
			{
				__result = levelsPerAttribute; 
				return false;
			}
			return true;
		}
	}

	//frugal cavalry
	[HarmonyPatch(typeof(CharacterObject), "get_UpgradeRequiresItemFromCategory")]
	public class GetNoMoreUpgradeItems
	{
		private static void Postfix(ref ItemCategory __result) => __result = (Settings.CampaignSettings.DisableHorseCostForCavalryUpgrade && true == __result?.StringId?.ToLowerInvariant()?.Contains("horse")) ? null : __result;
	}

	[HarmonyPatch(typeof(PartyScreenLogic), "RemoveItemFromItemRoster")]
	public class PatchPartyScreenLogic
	{
		public static bool Prefix(PartyScreenLogic __instance, ItemCategory itemCategory, int numOfItemsLeftToRemove)
		{
			if (Settings.CampaignSettings.EnableFrugalCavalry)
			{
				int val = numOfItemsLeftToRemove;
				ItemRosterElement[] copyOfAllElements = GetItemRoster(__instance.RightOwnerParty.ItemRoster);
				int count = __instance.RightOwnerParty.ItemRoster.Count;
				ItemRosterElement[] array = new ItemRosterElement[count];
				ItemRosterElement[] itemRosterElementArray = array;
				int length = count;
				Array.Copy(copyOfAllElements, 0, itemRosterElementArray, 0, length);
				Array.Sort(array, (ItemRosterElement left, ItemRosterElement right) => left.EquipmentElement.ItemValue.CompareTo(right.EquipmentElement.ItemValue));
				ItemRosterElement[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					ItemRosterElement itemRosterElement = array2[i];
					if (itemRosterElement.EquipmentElement.Item?.ItemCategory == itemCategory)
					{
						int num = Math.Min(val, itemRosterElement.Amount);
						__instance.RightOwnerParty.ItemRoster.AddToCounts(itemRosterElement.EquipmentElement, -num);
						val -= num;
						if (val == 0)
						{
							break;
						}
					}
				}
				return false;
			}
			return true;
		}

		private static FieldInfo s_itemRosterField;
		private static FieldInfo ItemRosterField => s_itemRosterField ?? (s_itemRosterField = typeof(ItemRoster).GetFieldInfo("_data"));
		private static ItemRosterElement[] GetItemRoster(ItemRoster __instance) => ItemRosterField.GetValue(__instance) as ItemRosterElement[];
	}

	//exit settlment
	[HarmonyPatch(typeof(MapScreen), "HandleLeftMouseButtonClick")]
	public class MapScreenHandleLeftMouseButtonClickPatch
	{
		private static AccessTools.FieldRef<MapScreen, MapState> mapState = AccessTools.FieldRefAccess<MapScreen, MapState>("_mapState");

		private static void Prefix(MapScreen __instance, PathFaceRecord mouseOverFaceIndex)
		{
			if (Settings.CampaignSettings.EnableFastExitSettlement && mouseOverFaceIndex.IsValid() && PlayerSiege.PlayerSiegeEvent == null && MobileParty.MainParty.SiegeEvent == null && mapState(__instance) != null && mapState(__instance).AtMenu)
			{
				Settlement currentSettlement = MobileParty.MainParty.CurrentSettlement;
				if (currentSettlement != null && (currentSettlement.IsCastle || currentSettlement.IsFortification || currentSettlement.IsTown || currentSettlement.IsVillage))
				{
					PlayerEncounter.LeaveSettlement();
					GameMenu.ExitToLast();
					PlayerEncounter.Finish();
				}
				else if (MobileParty.MainParty.TargetSettlement != null)
				{
					PlayerEncounter.Finish();
				}
			}
		}
	}

	//do not smelt locked weapons
	[HarmonyPatch(typeof(SmeltingVM))]
	public class SmeltingVMPatch
	{
		[HarmonyPostfix]
		[HarmonyPatch("RefreshList")]
		private static void Postfix(SmeltingVM __instance)
		{
			if (Settings.CampaignSettings.HideLockedItemsFromSmelting)
			{
				var list = __instance.SmeltableItemList;
				if (list != null)
				{
					MBBindingList<SmeltingItemVM> filteredList = new MBBindingList<SmeltingItemVM>();
					foreach (SmeltingItemVM smeltableItem in list)
					{
						if (false == smeltableItem.IsLocked)
						{
							filteredList.Add(smeltableItem);
						}
					}
					__instance.SmeltableItemList = filteredList;
					if (__instance.SmeltableItemList.Count == 0)
					{
						__instance.CurrentSelectedItem = null;
					}
				}
            }
		}
	}

	//transfer wounded first
	[HarmonyPatch(typeof(PartyVM), "OnTransferTroop")]
	public class TransferWoundedFirstPatch
	{
		private static AccessTools.FieldRef<MapScreen, MapState> mapState = AccessTools.FieldRefAccess<MapScreen, MapState>("_mapState");

		private static bool Prefix(PartyVM __instance, PartyCharacterVM troop, int newIndex, int transferAmount, PartyScreenLogic.PartyRosterSide fromSide)
		{
			if (troop.Side == PartyScreenLogic.PartyRosterSide.None || fromSide == PartyScreenLogic.PartyRosterSide.None)
			{
				return false;
			}
			_ = troop.Side;
			ExecuteSetSelectedCharacterMethod(__instance, troop);
			PartyScreenLogic.PartyCommand partyCommand = new PartyScreenLogic.PartyCommand();
			if (transferAmount > 0)
			{
				int numberOfWoundedTroopNumberForSide = ExecuteGetNumberOfWoundedTroopNumberForSideMethod(__instance, troop.Troop.Character, fromSide, troop.IsPrisoner);
				partyCommand.FillForTransferTroop(fromSide, troop.Type, troop.Character, transferAmount, (numberOfWoundedTroopNumberForSide >= transferAmount) ? transferAmount : numberOfWoundedTroopNumberForSide, newIndex);
				__instance.PartyScreenLogic.AddCommand(partyCommand);
			}
			return false;
		}

		private static MethodInfo s_GetNumberOfWoundedTroopNumberForSideMethod;
		private static MethodInfo GetNumberOfWoundedTroopNumberForSideMethod => s_GetNumberOfWoundedTroopNumberForSideMethod ?? (s_GetNumberOfWoundedTroopNumberForSideMethod = typeof(PartyVM).GetMethodInfo("GetNumberOfWoundedTroopNumberForSide"));
		private static int ExecuteGetNumberOfWoundedTroopNumberForSideMethod(PartyVM __instance, CharacterObject character, PartyScreenLogic.PartyRosterSide fromSide, bool isPrisoner) => (int)GetNumberOfWoundedTroopNumberForSideMethod.Invoke(__instance, new object[] { character, fromSide, isPrisoner });

		private static MethodInfo s_SetSelectedCharacterMethod;
		private static MethodInfo SetSelectedCharacterMethod => s_SetSelectedCharacterMethod ?? (s_SetSelectedCharacterMethod = typeof(PartyVM).GetMethodInfo("SetSelectedCharacter"));
		private static void ExecuteSetSelectedCharacterMethod(PartyVM __instance, PartyCharacterVM partyCharacterVM) => SetSelectedCharacterMethod.Invoke(__instance, new object[]{partyCharacterVM});
	}

	//smith forever
	[HarmonyPatch(typeof(DefaultSmithingModel), "GetEnergyCostForRefining")]
	public class RefineForever
	{
		private static bool Prefix(DefaultSmithingModel __instance, ref Crafting.RefiningFormula refineFormula, Hero hero, ref int __result)  
		{ 
			__result = (int)(__result * Settings.CampaignSettings.RefinementStaminaCostFactor); 
			return false; 
		}
	}
	[HarmonyPatch(typeof(DefaultSmithingModel), "GetEnergyCostForSmelting")]
	public class SmeltForever
	{
		private static bool Prefix(DefaultSmithingModel __instance, ItemObject item, Hero hero, ref int __result)
		{
			__result = (int)(__result * Settings.CampaignSettings.SmeltingStaminaCostFactor);
			return false;
		}
	}
	[HarmonyPatch(typeof(DefaultSmithingModel), "GetEnergyCostForSmithing")]
	public class SmithForever
	{
		private static bool Prefix(DefaultSmithingModel __instance, ItemObject item, Hero hero, ref int __result)
		{
			__result = (int)(__result * Settings.CampaignSettings.SmithingStaminaCostFactor);
			return false;
		}
	}

	public static class Tester
	{
		public static void Test()
        {
            System.Diagnostics.Debug.Assert(typeof(RecruitPrisonersCampaignBehavior).GetMethodInfo("HourlyTickMainParty") != null);
		}
	}
}
