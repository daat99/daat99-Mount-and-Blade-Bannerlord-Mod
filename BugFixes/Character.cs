using daat99.ExtensionMethods;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.CraftingSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static TaleWorlds.CampaignSystem.CampaignBehaviors.CraftingCampaignBehavior;

namespace daat99.BugFixes
{
    // E:\Games\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client\TaleWorlds.CampaignSystem.dll
    // namespace TaleWorlds.CampaignSystem.CharacterDevelopment
    //[HarmonyPatch(typeof(DefaultSkillLevelingManager), "OnHeroHealedWhileWaiting")]
    public class DefaultSkillLevelingManager_OnHeroHealedWhileWaiting
    {
        public static bool Prefix(ref DefaultSkillLevelingManager __instance, Hero hero, int healingAmount)
        {
            try
            {
                //original: 1.2.4

                //    public void OnHeroHealedWhileWaiting(Hero hero, int healingAmount)
                //    {
                //        if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.EffectiveSurgeon != null)
                //        {
                //            float num = Campaign.Current.Models.PartyHealingModel.GetSkillXpFromHealingTroop(hero.PartyBelongedTo.Party);
                //            float num2 = (hero.PartyBelongedTo.CurrentSettlement != null && !hero.PartyBelongedTo.CurrentSettlement.IsCastle) ? 0.2f : 0.1f;
                //            num *= (float)healingAmount * num2 * (1f + (float)hero.PartyBelongedTo.EffectiveSurgeon.Level * 0.1f);
                //            OnPartySkillExercised(hero.PartyBelongedTo, DefaultSkills.Medicine, num, SkillEffect.PerkRole.Surgeon);
                //        }
                //    }
                if (hero.PartyBelongedTo != null && hero.PartyBelongedTo.EffectiveSurgeon != null)
                {
                    float num = Campaign.Current.Models.PartyHealingModel.GetSkillXpFromHealingTroop(hero.PartyBelongedTo.Party);
                    float num2 = (hero.PartyBelongedTo.CurrentSettlement != null && !hero.PartyBelongedTo.CurrentSettlement.IsCastle) ? 0.2f : 0.1f;
                    num *= (float)healingAmount * num2 * (1f + (float)hero.PartyBelongedTo.EffectiveSurgeon.Level * 0.1f);
                    ExecuteOnPartySkillExercisedMethod(__instance, hero.PartyBelongedTo, DefaultSkills.Medicine, num, SkillEffect.PerkRole.Surgeon);
                }
            }
            catch(Exception ex)
            {

            }
            return false;
        }

        private static MethodInfo s_OnPartySkillExercisedMethod;
        private static MethodInfo OnPartySkillExercisedMethod => s_OnPartySkillExercisedMethod ?? (s_OnPartySkillExercisedMethod = typeof(DefaultSkillLevelingManager).GetMethodInfo("OnPartySkillExercised"));
        private static void ExecuteOnPartySkillExercisedMethod(DefaultSkillLevelingManager __instance, MobileParty party, SkillObject skill, float skillXp, SkillEffect.PerkRole perkRole = SkillEffect.PerkRole.PartyLeader) => OnPartySkillExercisedMethod.Invoke(__instance, new object[] { party, skill, skillXp, perkRole });
    }

    [HarmonyPatch(typeof(CraftingCampaignBehavior), "CreateTownOrder")]
    public class CraftingCampaignBehavior_CreateTownOrder
    {
        public static bool Prefix(ref CraftingCampaignBehavior __instance, Hero orderOwner, int orderSlot)
        {
            if (orderOwner.CurrentSettlement == null || !orderOwner.CurrentSettlement.IsTown)
            {
                Debug.Print("Order owner: " + orderOwner.StringId + " Settlement" + ((orderOwner.CurrentSettlement == null) ? "null" : orderOwner.CurrentSettlement.StringId) + " Order owner party: " + ((orderOwner.PartyBelongedTo == null) ? "null" : orderOwner.PartyBelongedTo.StringId)); 
            }
            float townOrderDifficulty = __instance.GetTownOrderDifficulty(orderOwner.CurrentSettlement.Town, orderSlot);
            int pieceTier = (int)townOrderDifficulty / 50;
            CraftingTemplate randomElement = CraftingTemplate.All.GetRandomElement();
            WeaponDesign weaponDesignTemplate = new WeaponDesign(randomElement, TextObject.Empty, __instance.GetWeaponPieces(randomElement, pieceTier));

            //crash fix
            try
            {
                //exception: dictionary may not contain key
                __instance.Get_craftingOrders()[orderOwner.CurrentSettlement.Town].AddTownOrder(new CraftingOrder(orderOwner, townOrderDifficulty, weaponDesignTemplate, randomElement, orderSlot));
            }
            catch (KeyNotFoundException ex)
            {
                //exists in 1.2.4.27066 beta
                Dictionary<Town, CraftingOrderSlots> _craftingOrders = __instance.Get_craftingOrders();
                _craftingOrders.TryGetValue(orderOwner.CurrentSettlement.Town, out CraftingOrderSlots craftingOrderSlots);
                if (craftingOrderSlots != null)
                {
                    craftingOrderSlots.AddTownOrder(new CraftingOrder(orderOwner, townOrderDifficulty, weaponDesignTemplate, randomElement, orderSlot));
                }
            }
            return false;
        }
        /*original v1.2.4
        @ TaleWorlds.CampaignSystem.CharacterDevelopment.DefaultSkillLevelingManager.OnHeroHealedWhileWaiting(MobileParty mobileParty, Int32 healingAmount)
           public void CreateTownOrder(Hero orderOwner, int orderSlot)
           {
               if (orderOwner.CurrentSettlement == null || !orderOwner.CurrentSettlement.IsTown)
               {
                   Debug.Print("Order owner: " + orderOwner.StringId + " Settlement" + ((orderOwner.CurrentSettlement == null) ? "null" : orderOwner.CurrentSettlement.StringId) + " Order owner party: " + ((orderOwner.PartyBelongedTo == null) ? "null" : orderOwner.PartyBelongedTo.StringId));
               }
               float townOrderDifficulty = GetTownOrderDifficulty(orderOwner.CurrentSettlement.Town, orderSlot);
               int pieceTier = (int)townOrderDifficulty / 50;
               CraftingTemplate randomElement = CraftingTemplate.All.GetRandomElement();
               WeaponDesign weaponDesignTemplate = new WeaponDesign(randomElement, TextObject.Empty, GetWeaponPieces(randomElement, pieceTier));
               _craftingOrders[orderOwner.CurrentSettlement.Town].AddTownOrder(new CraftingOrder(orderOwner, townOrderDifficulty, weaponDesignTemplate, randomElement, orderSlot));
           }
        */
    }
}