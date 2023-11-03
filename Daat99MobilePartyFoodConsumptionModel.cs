using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace daat99
{
    public class Daat99MobilePartyFoodConsumptionModel : DefaultMobilePartyFoodConsumptionModel
    {
        public override bool DoesPartyConsumeFood(MobileParty mobileParty)
        {
            if (false == Settings.CampaignSettings.PlayerPartyConsumeFood && MobileParty.MainParty == mobileParty)
            {
                return false;
            }
            return base.DoesPartyConsumeFood(mobileParty);
        }
    }
}
