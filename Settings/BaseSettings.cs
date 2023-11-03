using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace daat99
{
	public abstract class BaseSettings
	{
		private XmlElement m_settingsElement;
		public XmlElement SettingsElement
		{
			get => m_settingsElement;
			set
			{
				if (m_settingsElement != value)
				{
					m_settingsElement = value;
					ParseSettingsElement();
				}
			}
		}

		public BaseSettings() { }
		public BaseSettings(XmlElement settingsElement) => SettingsElement = settingsElement;

		public abstract void ParseSettingsElement();
	}
}
