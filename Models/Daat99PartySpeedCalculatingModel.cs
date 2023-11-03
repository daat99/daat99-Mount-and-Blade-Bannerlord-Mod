using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace daat99
{
    public class Daat99PartySpeedCalculatingModel : DefaultPartySpeedCalculatingModel
    {
        private static readonly TextObject s_khuzaitBonusExplanation = new TextObject("daat99 Khuzait Bonus");
        private static readonly TextObject s_speedBonusExplanation = new TextObject("daat99 Speed Bonus");

        public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
        {
            ExplainedNumber speed = base.CalculateFinalSpeed(mobileParty, finalSpeed);
            if ( mobileParty.IsMainParty )
            {
                if ( mobileParty.LeaderHero?.Culture?.GetCultureCode() == TaleWorlds.Core.CultureCode.Khuzait)
                {
                    speed.AddFactor(Settings.CampaignSettings.PlayerKhuzaitSpeedModifier, s_khuzaitBonusExplanation);
                }
                speed.AddFactor(Settings.CampaignSettings.PlayerPartySpeedModifier, s_speedBonusExplanation);
            }
            return speed;
        }
    }
}
