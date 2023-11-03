using daat99.Helpers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace daat99
{
	//simulation
	[HarmonyPatch(typeof(MapEvent), "SimulatePlayerEncounterBattle")]
	internal class PlayerBattleSimulationPatch
	{
		private static int s_maxVulnerableTier = 0;
		internal enum SimulationTroopState
		{
			Alive,
			Wounded,
			Killed,
			Routed
		}

		internal static bool Prefix(MapEvent __instance)
		{
			if (Settings.CampaignSettings.EnableBattleSimulationPatch)
			{
				s_maxVulnerableTier = 0;
				//(int defenderRounds, int attackerRounds) 
				var a = __instance.DefenderSide;
				(int defenderRounds, int attackerRounds) simulationRoundsForBattle = Campaign.Current.Models.CombatSimulationModel.GetSimulationRoundsForBattle(__instance, __instance.DefenderSide.NumRemainingSimulationTroops, __instance.AttackerSide.NumRemainingSimulationTroops);
				int item = simulationRoundsForBattle.defenderRounds;
				int item2 = simulationRoundsForBattle.attackerRounds;
				SimulateBattleForRounds(__instance, item, item2);
				return false;
			}
			return true;
		}
		private static void SimulateBattleForRounds(MapEvent __instance, int simulationRoundsDefender, int simulationRoundsAttacker)
		{
			bool isRoundWinnerDetermined = false;
			SetBattleState(__instance, GetCalculateWinner(__instance, ref isRoundWinnerDetermined));
			(float defenderAdvantage, float attackerAdvantage) battleAdvantage = Campaign.Current.Models.CombatSimulationModel.GetBattleAdvantage(__instance.DefenderSide.LeaderParty, __instance.AttackerSide.LeaderParty, __instance.EventType, __instance.MapEventSettlement);
			float item = battleAdvantage.defenderAdvantage;
			float item2 = battleAdvantage.attackerAdvantage;
			int num = 0;
			while (0 < simulationRoundsAttacker + simulationRoundsDefender && __instance.BattleState == BattleState.None)
			{
				int attackerMaxTier = GetAllocatedTroops(__instance.AttackerSide).Keys.Select<UniqueTroopDescriptor, CharacterObject>(desc => __instance.AttackerSide.GetAllocatedTroop(desc)).Max<CharacterObject>(c => c.Tier);
				int defenderMaxTier = GetAllocatedTroops(__instance.DefenderSide).Keys.Select<UniqueTroopDescriptor, CharacterObject>(desc => __instance.DefenderSide.GetAllocatedTroop(desc)).Max<CharacterObject>(c => c.Tier);
				s_maxVulnerableTier = Math.Min(attackerMaxTier, defenderMaxTier);
				
				float num2 = (float)simulationRoundsAttacker / (float)(simulationRoundsAttacker + simulationRoundsDefender);
				if (MBRandom.RandomFloat < num2)
				{
					simulationRoundsAttacker--;
					SimulateBattleForRound(__instance, BattleSideEnum.Attacker, item2);
				}
				else
				{
					simulationRoundsDefender--;
					SimulateBattleForRound(__instance, BattleSideEnum.Defender, item);
				}
				num++;
			}
		}

		private static void SimulateBattleForRound(MapEvent __instance, BattleSideEnum side, float advantage)
		{
			if (__instance.AttackerSide.NumRemainingSimulationTroops == 0 || __instance.DefenderSide.NumRemainingSimulationTroops == 0 || SimulateSingleHit(__instance, (int)side, (int)(1 - side), advantage))
			{
				bool isRoundWinnerDetermined = false;
				BattleState calculateWinner = GetCalculateWinner(__instance, ref isRoundWinnerDetermined);
				if (calculateWinner != 0)
				{
					SetBattleState(__instance, calculateWinner);
				}
				else if (isRoundWinnerDetermined)
				{
					IBattleObserver battleObserver = GetBattleObserver(__instance);
					battleObserver?.BattleResultsReady();
				}
			}
		}

		private static bool SimulateSingleHit(MapEvent __instance, int strikerSideIndex, int strikedSideIndex, float strikerAdvantage)
		{
			MapEventSide mapEventSide = strikerSideIndex == 1 ? __instance.AttackerSide : __instance.DefenderSide;
			MapEventSide mapEventSide2 = strikedSideIndex == 1 ? __instance.AttackerSide : __instance.DefenderSide;
			UniqueTroopDescriptor uniqueTroopDescriptor = mapEventSide.SelectRandomSimulationTroop();
			UniqueTroopDescriptor uniqueTroopDescriptor2 = mapEventSide2.SelectRandomSimulationTroop();
			CharacterObject allocatedTroop = mapEventSide.GetAllocatedTroop(uniqueTroopDescriptor);
			CharacterObject allocatedTroop2 = mapEventSide2.GetAllocatedTroop(uniqueTroopDescriptor2);
			PartyBase allocatedTroopParty = mapEventSide.GetAllocatedTroopParty(uniqueTroopDescriptor);
			PartyBase allocatedTroopParty2 = mapEventSide2.GetAllocatedTroopParty(uniqueTroopDescriptor2);
			int num = GetSimulatedDamage(__instance, allocatedTroop, allocatedTroop2, allocatedTroopParty, allocatedTroopParty2, strikerAdvantage);
			if (num > 0)
			{
				if (__instance.IsPlayerSimulation && allocatedTroopParty2 == PartyBase.MainParty)
				{
					float playerTroopsReceivedDamageMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
					num = MBRandom.RoundRandomized((float)num * playerTroopsReceivedDamageMultiplier);
				}
				DamageTypes damageType = (MBRandom.RandomFloat < 0.3f) ? DamageTypes.Blunt : DamageTypes.Cut;
				SimulationTroopState troopState;
				IBattleObserver battleObserver = GetBattleObserver(__instance);
				ApplySimulationDamageToSelectedTroop(mapEventSide2, battleObserver, num, damageType, out troopState, allocatedTroopParty);
				bool flag = troopState == SimulationTroopState.Killed || troopState == SimulationTroopState.Wounded;
				mapEventSide.ApplySimulatedHitRewardToSelectedTroop(allocatedTroop, allocatedTroop2, num, flag);
				return flag;
			}
			return false;
		}

		private static bool ApplySimulationDamageToSelectedTroop(MapEventSide __instance, IBattleObserver BattleObserver, int damage, DamageTypes damageType, out SimulationTroopState troopState, PartyBase strikerParty)
		{
			troopState = SimulationTroopState.Alive;
			bool flag = false;
			CharacterObject _selectedSimulationTroop = GetSelectedSimulationTroop(__instance);
			UniqueTroopDescriptor _selectedSimulationTroopDescriptor = Get_selectedSimulationTroopDescriptorField(__instance);
			if (_selectedSimulationTroop.IsHero)
			{
				__instance.AddHeroDamage(_selectedSimulationTroop.HeroObject, damage);
				if (_selectedSimulationTroop.HeroObject.IsWounded)
				{
					flag = true;
					troopState = SimulationTroopState.Wounded;
					BattleObserver?.TroopNumberChanged(__instance.MissionSide, __instance.GetAllocatedTroopParty(_selectedSimulationTroopDescriptor), _selectedSimulationTroop, -1, 0, 1);
				}
			}
			else if (MBRandom.RandomInt(_selectedSimulationTroop.MaxHitPoints()) < damage)
			{
				PartyBase party = GetAllocatedTroops(__instance)[_selectedSimulationTroopDescriptor].Party;
				float survivalChance = Campaign.Current.Models.PartyHealingModel.GetSurvivalChance(party, _selectedSimulationTroop, damageType, false, strikerParty);
				if (MBRandom.RandomFloat < survivalChance || (_selectedSimulationTroop.Tier > s_maxVulnerableTier && party == PartyBase.MainParty))
				{
					__instance.OnTroopWounded(_selectedSimulationTroopDescriptor);
					troopState = SimulationTroopState.Wounded;
					BattleObserver?.TroopNumberChanged(__instance.MissionSide, __instance.GetAllocatedTroopParty(_selectedSimulationTroopDescriptor), _selectedSimulationTroop, -1, 0, 1);
					SkillsHelper.AddSkillXp(party, DefaultSkills.Medicine, SkillEffect.PerkRole.Surgeon, _selectedSimulationTroop.Tier);
				}
				else
				{
					__instance.OnTroopKilled(_selectedSimulationTroopDescriptor);
					troopState = SimulationTroopState.Killed;
					BattleObserver?.TroopNumberChanged(__instance.MissionSide, __instance.GetAllocatedTroopParty(_selectedSimulationTroopDescriptor), _selectedSimulationTroop, -1, 1);
					SkillsHelper.AddSkillXp(party, DefaultSkills.Medicine, SkillEffect.PerkRole.Surgeon, _selectedSimulationTroop.Tier);
				}
				flag = true;
			}
			if (flag)
			{
				ExecuteRemoveSelectedTroopFromSimulationListMethod(__instance);
			}
			return flag;
		}
		private static int GetSimulatedDamage(MapEvent __instance, CharacterObject strikerTroop, CharacterObject strikedTroop, PartyBase strikerParty, PartyBase strikedParty, float strikerAdvantage)
		{
			return Campaign.Current.Models.CombatSimulationModel.SimulateHit(strikerTroop, strikedTroop, strikerParty, strikedParty, strikerAdvantage, __instance);
		}
		private static BattleState GetCalculateWinner(MapEvent __instance, ref bool isRoundWinnerDetermined)
		{
			BattleState result = BattleState.None;
			int num = __instance.AttackerSide.NumRemainingSimulationTroops;
			int num2 = __instance.DefenderSide.NumRemainingSimulationTroops;
			if (__instance.IsPlayerSimulation && !Hero.MainHero.IsWounded && __instance.InvolvedParties.Contains(PartyBase.MainParty))
			{
				if (PartyBase.MainParty.Side == BattleSideEnum.Attacker)
				{
					if (num == 0)
					{
						isRoundWinnerDetermined = true;
					}
					num++;
				}
				else if (PartyBase.MainParty.Side == BattleSideEnum.Defender)
				{
					if (num2 == 0)
					{
						isRoundWinnerDetermined = true;
					}
					num2++;
				}
			}
			if (num == 0)
			{
				result = BattleState.DefenderVictory;
			}
			else if (num2 == 0)
			{
				result = BattleState.AttackerVictory;
			}
			return result;
		}


		private static MethodInfo s_RemoveSelectedTroopFromSimulationListMethod;
		private static MethodInfo RemoveSelectedTroopFromSimulationListMethod => s_RemoveSelectedTroopFromSimulationListMethod ?? (s_RemoveSelectedTroopFromSimulationListMethod = typeof(MapEventSide).GetMethodInfo("RemoveSelectedTroopFromSimulationList"));
		private static void ExecuteRemoveSelectedTroopFromSimulationListMethod(MapEventSide __instance) => RemoveSelectedTroopFromSimulationListMethod.Invoke(__instance, null);


		private static FieldInfo s__selectedSimulationTroopDescriptorField;
		private static FieldInfo _selectedSimulationTroopDescriptorField => s__selectedSimulationTroopDescriptorField ?? (s__selectedSimulationTroopDescriptorField = typeof(MapEventSide).GetFieldInfo("_selectedSimulationTroopDescriptor"));
		private static UniqueTroopDescriptor Get_selectedSimulationTroopDescriptorField(MapEventSide __instance) => (UniqueTroopDescriptor)_selectedSimulationTroopDescriptorField.GetValue(__instance);

		private static FieldInfo s_AllocatedTroopsField;
		private static FieldInfo AllocatedTroopsField => s_AllocatedTroopsField ?? (s_AllocatedTroopsField = typeof(MapEventSide).GetFieldInfo("_allocatedTroops"));
		private static Dictionary<UniqueTroopDescriptor, MapEventParty> GetAllocatedTroops(MapEventSide __instance) => AllocatedTroopsField.GetValue(__instance) as Dictionary<UniqueTroopDescriptor, MapEventParty>;

		private static FieldInfo s__selectedSimulationTroopField;
		private static FieldInfo _selectedSimulationTroopField => s__selectedSimulationTroopField ?? (s__selectedSimulationTroopField = typeof(MapEventSide).GetFieldInfo("_selectedSimulationTroop"));
		private static CharacterObject GetSelectedSimulationTroop(MapEventSide __instance) => _selectedSimulationTroopField.GetValue(__instance) as CharacterObject;


		private static PropertyInfo s_battleStateProperty;
		private static PropertyInfo BattleStateProperty => s_battleStateProperty ?? (s_battleStateProperty = typeof(MapEvent).GetPropertyInfo("BattleState"));
		private static void SetBattleState(MapEvent __instance, BattleState battleState) => BattleStateProperty.SetValue(__instance, battleState, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, null, null);

		private static PropertyInfo s_battleObserverProperty;
		private static PropertyInfo BattleObserverProperty => s_battleObserverProperty ?? (s_battleObserverProperty = typeof(MapEvent).GetPropertyInfo("BattleObserver"));
		private static IBattleObserver GetBattleObserver(MapEvent __instance) => BattleObserverProperty.GetValue(__instance) as IBattleObserver;

	}
}
