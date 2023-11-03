using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;

namespace daat99
{
    public class BattleHealSettings : BaseSettings
    {
		private static BattleHealBehavior s_battleHealBehavior;
		public static BattleHealBehavior BattleHealBehavior => s_battleHealBehavior ?? (s_battleHealBehavior = Campaign.Current.GetCampaignBehavior<BattleHealBehavior>());
		
		public float PassiveHealingDelayInSeconds { get; private set; } = 5;
		public float MedicineSkillPerPercentageHealed { get; private set; } = 15;

		public BattleHealSettings(XmlElement element = null) : base(element) { }

		public override void ParseSettingsElement()
		{
			if (SettingsElement != null)
			{
				PassiveHealingDelayInSeconds = SettingsElement.ReadChildTextAs(nameof(PassiveHealingDelayInSeconds), PassiveHealingDelayInSeconds);
				MedicineSkillPerPercentageHealed = SettingsElement.ReadChildTextAs(nameof(MedicineSkillPerPercentageHealed), MedicineSkillPerPercentageHealed);
			}
		}
	}
}
