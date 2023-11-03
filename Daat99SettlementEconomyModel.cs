using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace daat99
{
    public class Daat99SettlementEconomyModel : DefaultSettlementEconomyModel
    {
        public override int GetTownGoldChange(Town town)
        {
            float num = 10000f + town.Prosperity * 12f * Settings.CampaignSettings.TownGoldChangeModifier - (float)town.Gold;
            return TaleWorlds.Library.MathF.Round(0.25f * num);
        }
    }
}
