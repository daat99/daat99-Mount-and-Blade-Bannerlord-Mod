using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using static TaleWorlds.CampaignSystem.CampaignBehaviors.CraftingCampaignBehavior;

namespace daat99.ExtensionMethods
{
    public static class CraftingCampaignBehaviorExtensions
    {
        private static MethodInfo s_GetTownOrderDifficultyMethod;
        private static MethodInfo GetTownOrderDifficultyMethod => s_GetTownOrderDifficultyMethod ?? (s_GetTownOrderDifficultyMethod = typeof(CraftingCampaignBehavior).GetMethodInfo("GetTownOrderDifficulty"));
        public static float GetTownOrderDifficulty(this CraftingCampaignBehavior craftingCampaignBehavior, Town town, int orderSlot) => (float)GetTownOrderDifficultyMethod.Invoke(craftingCampaignBehavior, new object[] { town, orderSlot });

        
        private static MethodInfo s_GetWeaponPiecesMethod;
        private static MethodInfo GetWeaponPiecesMethod => s_GetWeaponPiecesMethod ?? (s_GetWeaponPiecesMethod = typeof(CraftingCampaignBehavior).GetMethodInfo("GetWeaponPieces"));
        public static WeaponDesignElement[] GetWeaponPieces(this CraftingCampaignBehavior craftingCampaignBehavior, CraftingTemplate craftingTemplate, int pieceTier) => (WeaponDesignElement[])GetWeaponPiecesMethod.Invoke(craftingCampaignBehavior, new object[] { craftingTemplate, pieceTier});


        private static FieldInfo s__craftingOrders;
        private static FieldInfo _craftingOrdersField => s__craftingOrders ?? (s__craftingOrders = typeof(CraftingCampaignBehavior).GetFieldInfo("_craftingOrders"));
        public static Dictionary<Town, CraftingOrderSlots> Get_craftingOrders(this CraftingCampaignBehavior __instance) => _craftingOrdersField.GetValue(__instance) as Dictionary<Town, CraftingOrderSlots>;
    }
}
