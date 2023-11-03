using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace daat99.BugFixes
{
    public class Daat99DiplomacyModel : DefaultDiplomacyModel
    {
        public override void GetHeroesForEffectiveRelation(Hero hero1, Hero hero2, out Hero effectiveHero1, out Hero effectiveHero2)
        {
            effectiveHero1 = hero1;
            effectiveHero2 = hero2;
            try
            {
                base.GetHeroesForEffectiveRelation(hero1, hero2, out effectiveHero1, out effectiveHero2);
                /* original
                // TaleWorlds.CampaignSystem.GameComponents.DefaultDiplomacyModel
                	public override void GetHeroesForEffectiveRelation(Hero hero1, Hero hero2, out Hero effectiveHero1, out Hero effectiveHero2)
	                {
		                effectiveHero1 = ((hero1.Clan != null) ? hero1.Clan.Leader : hero1);
		                effectiveHero2 = ((hero2.Clan != null) ? hero2.Clan.Leader : hero2);
		                if (effectiveHero1 == effectiveHero2 || (hero1.IsPlayerCompanion && hero2.IsHumanPlayerCharacter) || (hero1.IsPlayerCompanion && hero2.IsHumanPlayerCharacter))
		                {
			                effectiveHero1 = hero1;
			                effectiveHero2 = hero2;
		                }
	                }
                */
            }
            catch (Exception ex)
            {

            }
        }
    }
    //[HarmonyPatch(typeof(DefaultDiplomacyModel), "GetHeroesForEffectiveRelation")]
    //public class DefaultDiplomacyModel_GetHeroesForEffectiveRelation
    //{
    //    public static bool Prefix(ref DefaultDiplomacyModel __instance, Hero hero1, Hero hero2, out Hero effectiveHero1, out Hero effectiveHero2)
    //    {
    //        effectiveHero1 = hero1;
    //        effectiveHero2 = hero2;
    //        try
    //        {
    //            Clan neutralFaction = CampaignData.NeutralFaction;
    //            effectiveHero1 = ((hero1.Clan != null && hero1.Clan != neutralFaction) ? hero1.Clan.Leader : hero1);
    //            effectiveHero2 = ((hero2.Clan != null && hero2.Clan != neutralFaction) ? hero2.Clan.Leader : hero2);
    //            if (effectiveHero1 == effectiveHero2 || (hero1.IsPlayerCompanion && hero2.IsHumanPlayerCharacter) || (hero1.IsPlayerCompanion && hero2.IsHumanPlayerCharacter))
    //            {
    //                effectiveHero1 = hero1;
    //                effectiveHero2 = hero2;
    //            }
    //        }
    //        catch (Exception ex)
    //        {

    //        }
    //        return false;
    //    }
    //}
}
