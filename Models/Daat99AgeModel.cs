using TaleWorlds.CampaignSystem.GameComponents;

namespace daat99
{
    public class Daat99AgeModel : DefaultAgeModel
    {
		public override int BecomeInfantAge => Settings.CampaignSettings.AgeSettings.BecomeInfantAge;
		public override int BecomeChildAge => Settings.CampaignSettings.AgeSettings.BecomeChildAge;
		public override int BecomeTeenagerAge => Settings.CampaignSettings.AgeSettings.BecomeTeenagerAge;
		public override int HeroComesOfAge => Settings.CampaignSettings.AgeSettings.HeroComesOfAge;
		public override int BecomeOldAge => Settings.CampaignSettings.AgeSettings.BecomeOldAge;
		public override int MaxAge => Settings.CampaignSettings.AgeSettings.MaxAge;
	}
}
