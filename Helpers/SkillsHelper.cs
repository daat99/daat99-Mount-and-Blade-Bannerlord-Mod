using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using static TaleWorlds.CampaignSystem.SkillEffect;

namespace daat99.Helpers
{
    public static class SkillsHelper
    {
        public static void AddSkillXp(this PartyBase partyBase, SkillObject skill, PerkRole perkRole, int amount)
        {
            Hero hero = partyBase.MobileParty?.GetEffectiveRoleHolder(perkRole);
            if (hero != null && hero != Hero.MainHero)
            {
                hero.AddSkillXp(skill, amount);
            }
            Hero.MainHero.AddSkillXp(skill, amount);
        }
    }
}
