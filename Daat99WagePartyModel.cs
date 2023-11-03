using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace daat99
{
    public class Daat99WagePartyModel : DefaultPartyWageModel
    {
        public override ExplainedNumber GetTotalWage(MobileParty mobileParty, bool includeDescriptions = false)
        {
            ExplainedNumber explainedWage = base.GetTotalWage(mobileParty, includeDescriptions);
            if (Settings.CampaignSettings.EnableLowerPlayerSettlementWages && mobileParty.Equals(mobileParty.HomeSettlement?.Town?.GarrisonParty) && mobileParty.HomeSettlement?.Town.OwnerClan == Hero.MainHero.Clan)
            {
                explainedWage.AddFactor(0.5f);
            }
            return explainedWage;
        }
    }
}
