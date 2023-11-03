using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;

namespace daat99
{
    public class AgeSettings : BaseSettings
    {
		public int BecomeInfantAge { get; private set; } = 3;
		public int BecomeChildAge { get; private set; } = 6;
		public int BecomeTeenagerAge { get; private set; } = 14;
		public int HeroComesOfAge { get; private set; } = 18;
		public int BecomeOldAge { get; private set; } = 47;
		public int MaxAge { get; private set; } = 128;

		public AgeSettings(XmlElement element = null) : base(element) { }

		public override void ParseSettingsElement()
		{
			if (SettingsElement != null)
			{
				BecomeInfantAge = SettingsElement.ReadChildTextAs(nameof(BecomeInfantAge), BecomeInfantAge);
				BecomeChildAge = SettingsElement.ReadChildTextAs(nameof(BecomeChildAge), BecomeChildAge);
				BecomeTeenagerAge = SettingsElement.ReadChildTextAs(nameof(BecomeTeenagerAge), BecomeTeenagerAge);
				HeroComesOfAge = SettingsElement.ReadChildTextAs(nameof(HeroComesOfAge), HeroComesOfAge);
				BecomeOldAge = SettingsElement.ReadChildTextAs(nameof(BecomeOldAge), BecomeOldAge);
				MaxAge = SettingsElement.ReadChildTextAs(nameof(MaxAge), MaxAge);
			}
		}
	}
}
