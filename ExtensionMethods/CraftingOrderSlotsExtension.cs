using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.CraftingSystem;
using static TaleWorlds.CampaignSystem.CampaignBehaviors.CraftingCampaignBehavior;

namespace daat99.ExtensionMethods
{
    public static class CraftingOrderSlotsExtension
    {
        private static MethodInfo s_AddTownOrderMethod;
        private static MethodInfo AddTownOrderMethod => s_AddTownOrderMethod ?? (s_AddTownOrderMethod = typeof(CraftingOrderSlots).GetMethodInfo("AddTownOrder"));
        public static void AddTownOrder(this CraftingOrderSlots craftingOrderSlots, CraftingOrder craftingOrder) => AddTownOrderMethod.Invoke(craftingOrderSlots, new object[] { craftingOrder });
    }
}
