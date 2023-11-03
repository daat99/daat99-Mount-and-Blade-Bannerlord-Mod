using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.ViewModelCollection;

namespace daat99
{
    public class Daat99ClanTierModel : DefaultClanTierModel
    {
        //public int CommanderLimit => Campaign.Current.Models.ClanTierModel.GetPartyLimitForTier(this, Tier);
        public override int GetPartyLimitForTier(Clan clan, int clanTierToCheck)
        {
            int limit = base.GetPartyLimitForTier(clan, clanTierToCheck);
            if (Settings.CampaignSettings.FamilyPartiesAboveLimits && clan == Clan.PlayerClan)
            {
                //foreach (var a in Clan.NonBanditFactions) System.Diagnostics.Debugger.Log(1,"test",a.Leader?.Name?.ToString() + ", ");
                //var khuzait = Clan.NonBanditFactions.First(a => a.Leader.Name.ToString() == "Monchug");
                int heroesParties = clan.Heroes.Count(a => a.IsPartyLeader && false == a.IsPlayerCompanion && false == a.IsHumanPlayerCharacter);
                limit += heroesParties;
                
                //todo look for a way to deduct siblings and children companies from the total company size limits
                //limit += clan.AllParties.Count(party => Hero.MainHero.Children.Contains(party.LeaderHero) || Hero.MainHero.Siblings.Contains(party.LeaderHero));
            }
            return limit;
        }
    }
}