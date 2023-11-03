using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.Recruitment;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.WeaponCrafting.WeaponDesign;

namespace daat99
{
    public class ActiveViewModels
    {
        public static PartyVM partyVM;

        public static SPInventoryVM InventoryVM;

        public static WeaponDesignVM WeaponDesignVM;

        public static RecruitmentVM RecruitmentVM;

        public static MenuContext MenuContext;
    }
}
