using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace daat99
{
	public class Daat99PrisonerRecruitmentCalculationModel : DefaultPrisonerRecruitmentCalculationModel
	{
		public override bool IsPrisonerRecruitable(PartyBase party, CharacterObject character, out int conformityNeeded)
		{
			if (!character.IsRegular || character.Tier > 7 /*daat99: 6*/)
			{
				conformityNeeded = 0;
				return false;
			}
			int elementXp = party.MobileParty.PrisonRoster.GetElementXp(character);
			conformityNeeded = GetConformityNeededToRecruitPrisoner(character);
			return elementXp >= conformityNeeded;
		}

		public float[] GetDailyRecruitedPrisoners(MobileParty mainParty)
		{
			return Settings.CampaignSettings.AllowRecruitingTopTroops ? new float[7]
			{
				1f,
				0.5f,
				0.3f,
				0.2f,
				0.1f,
				0.05f,
				0.01f
			} : new float[0] { };
		}
	}
}
