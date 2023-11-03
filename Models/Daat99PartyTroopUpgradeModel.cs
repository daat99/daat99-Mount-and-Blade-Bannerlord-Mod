using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace daat99
{
    public class Daat99PartyTroopUpgradeModel : DefaultPartyTroopUpgradeModel
    {
        //discipline troop without "disciplinarian" (DefaultPerks.Leadership.VeteransRespect) perk
        private static CharacterObject[] EMPTY = new CharacterObject[] { };
        public override bool DoesPartyHaveRequiredPerksForUpgrade(PartyBase party, CharacterObject character, CharacterObject upgradeTarget, out PerkObject requiredPerk)
        {
            if (Settings.CampaignSettings.UpgradeBanditsWithoutDisciplinarianPerk && (party?.LeaderHero?.IsHumanPlayerCharacter ?? false) && Enumerable.Contains((character?.UpgradeTargets ?? EMPTY), upgradeTarget) )
            {
                requiredPerk = null;
                return true;
            }
            return base.DoesPartyHaveRequiredPerksForUpgrade(party, character, upgradeTarget, out requiredPerk);
        }
    }
}
