using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace daat99.Encyclopedia
{
    public class EncyclopediaListClanLivingMemberComparer : TaleWorlds.CampaignSystem.Encyclopedia.Pages.DefaultEncyclopediaClanPage.EncyclopediaListClanComparer
    {
		private static Func<Clan, Clan, int> _comparison = (Clan c1, Clan c2) => c1.Heroes.Count(h=>h.IsAlive).CompareTo(c2.Heroes.Count(h => h.IsAlive));

		public override int Compare(EncyclopediaListItem x, EncyclopediaListItem y) => CompareClans(x, y, _comparison);

		public override string GetComparedValueText(EncyclopediaListItem item)
		{
			Clan clan;
			if ((clan = (item.Object as Clan)) != null)
			{
				return clan.Heroes.Count(h=>h.IsAlive).ToString();
			}
			Debug.FailedAssert("Unable to get members of a non-clan object.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Encyclopedia\\Pages\\DefaultEncyclopediaClanPage.cs", "GetComparedValueText", 241);
			return "";
		}
	}

	[HarmonyPatch(typeof(DefaultEncyclopediaClanPage), "InitializeSortControllers")]
	public class DefaultEncyclopediaClanPage_InitializeSortControllers
	{
		public static void Postfix(ref DefaultEncyclopediaClanPage __instance, ref IEnumerable<EncyclopediaSortController> __result)
		{
			if (Settings.CampaignSettings.AddLivingClanMembersEncyclopediaSorter)
			{
				List<EncyclopediaSortController> newList = new List<EncyclopediaSortController>(__result);
				newList.Add(new EncyclopediaSortController(new TextObject("Living Members"), new EncyclopediaListClanLivingMemberComparer()));
				__result = newList;
			}
		}
		/*
			// TaleWorlds.CampaignSystem.Encyclopedia.Pages.DefaultEncyclopediaClanPage
			using System.Collections.Generic;
			using TaleWorlds.Core;
			using TaleWorlds.Localization;

			protected override IEnumerable<EncyclopediaSortController> InitializeSortControllers()
			{
				return new List<EncyclopediaSortController>
				{
					new EncyclopediaSortController(new TextObject("{=qtII2HbK}Wealth"), new EncyclopediaListClanWealthComparer()),
					new EncyclopediaSortController(new TextObject("{=cc1d7mkq}Tier"), new EncyclopediaListClanTierComparer()),
					new EncyclopediaSortController(GameTexts.FindText("str_strength"), new EncyclopediaListClanStrengthComparer()),
					new EncyclopediaSortController(GameTexts.FindText("str_fiefs"), new EncyclopediaListClanFiefComparer()),
					new EncyclopediaSortController(GameTexts.FindText("str_members"), new EncyclopediaListClanMemberComparer())
				};
			}

		*/
	}
}