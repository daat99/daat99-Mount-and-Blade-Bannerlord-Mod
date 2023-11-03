using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;

namespace daat99
{
    public class Daat99TroopSacrificeModel : DefaultTroopSacrificeModel
    {
        public override int GetLostTroopCountForBreakingInBesiegedSettlement(MobileParty party, SiegeEvent siegeEvent) => Settings.CampaignSettings.NoLossesBreakingIntoSiege ? 0 : base.GetLostTroopCountForBreakingInBesiegedSettlement(party, siegeEvent);
    }
}
