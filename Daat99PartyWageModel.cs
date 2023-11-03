using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace daat99
{
    public class Daat99PartyWageModel : DefaultPartyWageModel
    {
        public override ExplainedNumber GetTotalWage(MobileParty mobileParty, bool includeDescriptions = false)
        {

            ExplainedNumber result = new ExplainedNumber();
            try
            {
                result = base.GetTotalWage(mobileParty, includeDescriptions);
                if (Settings.CampaignSettings.PlayerWageModifier != 1.0f)
                {
                    result.AddFactor(Settings.CampaignSettings.PlayerWageModifier);
                }
            }
            catch(System.Exception ex)
            {
                result.AddFactor(0);
            }
            return result;
        }
    }
}
