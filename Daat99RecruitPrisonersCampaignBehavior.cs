using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace daat99
{
    public class Daat99RecruitPrisonersCampaignBehavior : RecruitPrisonersCampaignBehavior
    {
		public override void RegisterEvents()
		{
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this, OnPlayerBattleEnded);
			CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
			Test();
		}

		private void OnDailyTick() => recruitPrisonersAndTrainTroops();

        private void OnPlayerBattleEnded(MapEvent obj)
        {
            if (Settings.CampaignSettings.RecruitPrisonersOnPlayerVictory)
            {
				recruitPrisonersAndTrainTroops();
			}
		}

		private void recruitPrisonersAndTrainTroops()
		{
			MobileParty mainParty = MobileParty.MainParty;
			List<TroopRosterElement> troopsRoster = mainParty.MemberRoster.GetTroopRoster();
			int totalExperienceToDistribute = 0;
			int totalTroopsToTrainCount = 0;
			bool dailyLeadershipTrainingDone = false;
			foreach (TroopRosterElement troopRoster in troopsRoster)
			{
				CharacterObject troop = troopRoster.Character;
				if (troop.IsHero)
				{
					Hero hero = troop.HeroObject;
					if (hero.IsAlive && false == hero.IsChild)
					{
						SkillObject leadership = DefaultSkills.Leadership;
						int skillLevel = hero.GetSkillValue(leadership);
						totalExperienceToDistribute += skillLevel;
						hero.AddSkillXp(leadership, skillLevel);
					}
				}
				else
				{
					++totalTroopsToTrainCount;
				}
			}

			int experienceLeft = recruitPrisoners(mainParty, totalExperienceToDistribute);
			if (experienceLeft > 0)
			{
				dailyLeadershipTrainingDone = true;
				totalExperienceToDistribute += experienceLeft;
			}

			if (Settings.CampaignSettings.EnableDailyTroopsTraining)
			{
				totalExperienceToDistribute = leadersTrainTroops(mainParty, totalExperienceToDistribute, totalTroopsToTrainCount);
				if (totalExperienceToDistribute > 0)
				{
					dailyLeadershipTrainingDone = true;
				}
			}

			if (dailyLeadershipTrainingDone)
            {
				InformationManager.DisplayMessage(new InformationMessage(string.Format("Finished daily leadership drilling."), Colors.Cyan));
			}
		}
		
		private int recruitPrisoners(MobileParty mainParty, int totalConformityToDistribute)
		{
			TroopRoster prisonRoster = mainParty.PrisonRoster;
			int prisonersCount = prisonRoster.Count;
			if (prisonersCount > 0)
			{
				int conformityPerPrisoner = totalConformityToDistribute / prisonersCount;
				bool prisonerWasConformed = true;
				while (prisonerWasConformed && totalConformityToDistribute > 0)
				{
					prisonerWasConformed = false;
					for (int prisonerIndex = 0; prisonerIndex < prisonersCount; ++prisonerIndex)
					{
						CharacterObject prisonerCharacterObject = prisonRoster.GetCharacterAtIndex(prisonerIndex);
						if (prisonerCharacterObject.IsRegular)
						{
							int prisonerTroopCount = mainParty.PrisonRoster.GetElementNumber(prisonerIndex);
							int currentConformity = mainParty.PrisonRoster.GetElementXp(prisonerIndex);
							int maxConformity = prisonerTroopCount * prisonerCharacterObject.ConformityNeededToRecruitPrisoner;
							int missingConformity = maxConformity - currentConformity;
							int conformityForPrisoner = Math.Min(conformityPerPrisoner, missingConformity);
							if (conformityForPrisoner > 0)
							{
								mainParty.PrisonRoster.AddXpToTroop(conformityForPrisoner, prisonerCharacterObject);
								int newConformity = mainParty.PrisonRoster.GetElementXp(prisonerIndex);
								if (newConformity != currentConformity)
								{
									totalConformityToDistribute -= conformityForPrisoner;
									prisonerWasConformed = true;
								}
							}
						}
					}
				}
			}
			return Math.Max(0,totalConformityToDistribute);
		}


		private static FieldInfo s_dataField;
		private static FieldInfo DataField => s_dataField ?? (s_dataField = typeof(TroopRoster).GetFieldInfo("data"));
		private static TroopRosterElement[] GetData(TroopRoster __instance) => DataField.GetValue(__instance) as TroopRosterElement[];


		private int leadersTrainTroops(MobileParty mainParty, int totalExperienceToDistribute, int totalTroopsToTrainCount)
		{
			if (totalTroopsToTrainCount > 0 && totalExperienceToDistribute > 0)
			{
				int experiencePerTroop = totalExperienceToDistribute / totalTroopsToTrainCount;
				bool troopWasTrained = true;
				while (troopWasTrained && totalExperienceToDistribute > 0)
				{
					troopWasTrained = false;
					foreach (TroopRosterElement troopRoster in mainParty.MemberRoster.GetTroopRoster())
					{
						CharacterObject troop = troopRoster.Character;
						if (false == troop.IsHero && true == troop.UpgradeTargets?.Any())
						{
							mainParty.MemberRoster.AddXpToTroop(experiencePerTroop, troop);
							totalExperienceToDistribute -= experiencePerTroop;
							troopWasTrained = true;
						}
					}
				}
			}
			return totalExperienceToDistribute;
		}


		private bool generateConformityForTroop(MobileParty mobileParty, CharacterObject troop, int currentConformity, int maxConformityForTroop, int hours = 1)
		{ 
			var args = new object[] { mobileParty, troop, currentConformity, maxConformityForTroop, hours };
			var resultObject = GenerateConformityForTroopMethod.Invoke(this, args);
			return (bool)resultObject;
		}

		private static MethodInfo s_GenerateConformityForTroopMethod;
		private static MethodInfo GenerateConformityForTroopMethod => s_GenerateConformityForTroopMethod ?? (s_GenerateConformityForTroopMethod = typeof(RecruitPrisonersCampaignBehavior).GetMethodInfo("GenerateConformityForTroop"));

		public static void Test()
        {
			System.Diagnostics.Debug.Assert(DataField != null);
			System.Diagnostics.Debug.Assert(GenerateConformityForTroopMethod != null);
		}
	}
}
