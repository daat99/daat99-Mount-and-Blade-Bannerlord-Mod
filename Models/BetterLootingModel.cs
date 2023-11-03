using daat99;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;

public class BetterLootingModel : DefaultBattleRewardModel
{
	private static Random s_random = new Random();

	public override EquipmentElement GetLootedItemFromTroop(CharacterObject character, float targetValue)
	{
		EquipmentElement loot;
		if (Settings.CampaignSettings.TroopsDropTheirItems)
		{
			try
			{
				Equipment equipment = character.AllEquipments.GetRandomElement();
				//List<ItemObject> itemObjectList = new List<ItemObject>(Equipment.EquipmentSlotLength);
				List<EquipmentElement> equipmentItemsList = new List<EquipmentElement>();
				double sumValues = 0;
				for (int i = 0; i < Equipment.EquipmentSlotLength; i++)
				{
					if (false == equipment[i].Item?.NotMerchandise)
					{
						equipmentItemsList.Add(equipment[i]);
						//itemObjectList.Add(equipment[i].Item);
						sumValues += equipment[i].Item.Value;
					}
				}
				if (equipmentItemsList.Count > 0)
				{
					equipmentItemsList.Sort((a, b) => b.Item == null ? 0 : a.Item == null ? 0 : b.Item.Value.CompareTo(a.Item.Value)); //sort decending by value
					for (int index = 0; index < equipmentItemsList.Count; ++index)
					{
						EquipmentElement equipmentElement = equipmentItemsList[index];
						double chanceToLoot = 0.75 * (index / 100 + 1.0) / (Equipment.EquipmentSlotLength + 1.0);
						if (s_random.NextDouble() < chanceToLoot)
						{
							ItemObject item = equipmentElement.Item;
							chanceToLoot = item.Value / sumValues;
							if (s_random.NextDouble() > chanceToLoot)
							{
								ItemModifier itemModifier = equipmentElement.Item.ItemComponent?.ItemModifierGroup?.GetRandomItemModifierLootScoreBased();
								return new EquipmentElement(equipmentElement.Item, itemModifier);
							}
						}
					}
				}
				loot = default;
			}
			catch(Exception ex)
            {
				loot = base.GetLootedItemFromTroop(character, targetValue);
			}
		}
		else
        {
			loot = base.GetLootedItemFromTroop(character, targetValue);
        }
		return loot;
	}
}
