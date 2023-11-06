using System.IO;
using System.Reflection;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace daat99
{
	public class Settings : BaseSettings
	{
		private static Settings s_campaignSettings;
		public static Settings CampaignSettings => s_campaignSettings ?? (s_campaignSettings = new Settings());

		private const string CONFIG_FILE_NAME = "settings.xml";
		private static string s_configFilePath;
		public static string ConfigFilePath
		{
			get
			{
				if (string.IsNullOrWhiteSpace(s_configFilePath))
				{
					string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					s_configFilePath = Path.Combine(assemblyPath, CONFIG_FILE_NAME);
				}
				return s_configFilePath;
			}
		}

		public bool PlayerIsGovernor { get; private set; } = true;
		public bool OnlyDayBattles { get; private set; } = true;

		public int FocusPointsPerLevel { get; private set; } = 3;
		public int LevelsPerAttribute { get; private set; } = 1;
		public int MaximumSkillOverride { get; private set; } = 300;
		public float MinimalXpGainFactor { get; private set; } = 0.01f;
		public float BuildingBoostAddMultiplier { get; private set; } = 50.0f;

		public float DailySkillTrainingLevelUpMultiplier { get; private set; } = 0.001f;
		public float ItemDropChanceOnLordExecution { get; private set; } = 0.05f;
		public float EquipmentPriceFactorDeductionRatePerExponent { get; private set; } = 0.75f;
		public float RefinementStaminaCostFactor { get; private set; } = 0.0f;
		public float SmeltingStaminaCostFactor { get; private set; } = 0.0f;
		public float SmithingStaminaCostFactor { get; private set; } = 0.0f;

		public bool EnableFastLoot { get; private set; } = true;
		public bool ManageRemoteCompanions { get; private set; } = true;
		public bool EnableBattleSimulationPatch { get; private set; } = true;
		public bool RecruitPrisonersOnPlayerVictory { get; private set; } = true;
		public bool UnlockAllBlacksmithParts { get; private set; } = false;
		public bool AllowPregnancyInSameParty { get; private set; } = true;
		public bool EnableFrugalCavalry { get; private set; } = true;
		public bool DisableHorseCostForCavalryUpgrade { get; private set; } = true;
		public bool EnableFastExitSettlement { get; private set; } = true;
		public bool HideLockedItemsFromSmelting { get; private set; } = true;
		public bool AllowRecruitingTopTroops { get; private set; } = true;
		public bool HeroSpouseIsAlwaysFertile { get; private set; } = true;
		public bool TroopsDropTheirItems { get; private set; } = true;
		public bool EnableDailyTroopsTraining { get; private set; } = true;
		public bool EnableDailySettlementPrisonerRecruitment { get; private set; } = true;
		public bool EnableLowerPlayerSettlementWages { get; private set; } = true;
		public bool FamilyPartiesAboveLimits { get; private set; } = true;
		public bool EnterTownWithMount { get; private set; } = true;
		public bool UpgradeBanditsWithoutDisciplinarianPerk { get; private set; } = true;

		public float PlayerWageModifier { get; private set; } = -0.5f;
		public float TownGoldChangeModifier { get; private set; } = 1.0f;
		public float PlayerPartySpeedModifier { get; private set; } = 0.2f;
		public float PlayerKhuzaitSpeedModifier { get; private set; } = 0.1f;

		public bool PlayerPartyConsumeFood { get; private set; } = false;
		public bool TrainTradeOnProfit { get; private set; } = true;
		public bool NoLossesBreakingIntoSiege { get; private set; } = true;
		public bool EnableFastSendTroopsToFightBandits { get; private set; } = true;
		public bool BuildAllLevels { get; private set; } = true;
		public bool AllowRecruitGarrison { get; private set; } = true;
		public bool TransferWoundedFirst { get; private set; } = true;
		public bool AnnounceTournamentsOnFinished { get; private set; } = true;
		public bool AddLivingClanMembersEncyclopediaSorter { get; private set; } = true;
		public float TrackerChaseSpeedFactor { get; private set; } = 0.02f;

		public BattleHealSettings BattleHealSettings { get; private set; } = new BattleHealSettings();
		public AgeSettings AgeSettings { get; private set; } = new AgeSettings();

		public Settings(XmlElement settingsElement) => SettingsElement = settingsElement;

		public Settings()
        {
			if (File.Exists(ConfigFilePath))
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(ConfigFilePath);
				SettingsElement = (XmlElement)doc.SelectSingleNode(nameof(Settings));
			}
			else
			{
				InformationManager.DisplayMessage(new InformationMessage(string.Format("[{0}] not found, using default values", ConfigFilePath)));
			}
		}

		public override void ParseSettingsElement()
		{
			if (SettingsElement != null)
			{
				PlayerIsGovernor = SettingsElement.ReadChildTextAs(nameof(PlayerIsGovernor), PlayerIsGovernor);
				OnlyDayBattles = SettingsElement.ReadChildTextAs(nameof(OnlyDayBattles), OnlyDayBattles);

				FocusPointsPerLevel = SettingsElement.ReadChildTextAs(nameof(FocusPointsPerLevel), FocusPointsPerLevel);
				LevelsPerAttribute = SettingsElement.ReadChildTextAs(nameof(LevelsPerAttribute), LevelsPerAttribute);
				MaximumSkillOverride = SettingsElement.ReadChildTextAs(nameof(MaximumSkillOverride), MaximumSkillOverride);
				MinimalXpGainFactor = SettingsElement.ReadChildTextAs(nameof(MinimalXpGainFactor), MinimalXpGainFactor);
				BuildingBoostAddMultiplier = SettingsElement.ReadChildTextAs(nameof(BuildingBoostAddMultiplier), BuildingBoostAddMultiplier);

				DailySkillTrainingLevelUpMultiplier = SettingsElement.ReadChildTextAs(nameof(DailySkillTrainingLevelUpMultiplier), DailySkillTrainingLevelUpMultiplier);
				ItemDropChanceOnLordExecution = SettingsElement.ReadChildTextAs(nameof(ItemDropChanceOnLordExecution), ItemDropChanceOnLordExecution);
				EquipmentPriceFactorDeductionRatePerExponent = SettingsElement.ReadChildTextAs(nameof(EquipmentPriceFactorDeductionRatePerExponent), EquipmentPriceFactorDeductionRatePerExponent);
				RefinementStaminaCostFactor = SettingsElement.ReadChildTextAs(nameof(RefinementStaminaCostFactor), RefinementStaminaCostFactor);
				SmeltingStaminaCostFactor = SettingsElement.ReadChildTextAs(nameof(SmeltingStaminaCostFactor), SmeltingStaminaCostFactor);
				SmithingStaminaCostFactor = SettingsElement.ReadChildTextAs(nameof(SmithingStaminaCostFactor), SmithingStaminaCostFactor);

				EnableFastLoot = SettingsElement.ReadChildTextAs(nameof(EnableFastLoot), EnableFastLoot);
				ManageRemoteCompanions = SettingsElement.ReadChildTextAs(nameof(ManageRemoteCompanions), ManageRemoteCompanions);
				EnableBattleSimulationPatch = SettingsElement.ReadChildTextAs(nameof(EnableBattleSimulationPatch), EnableBattleSimulationPatch);
				RecruitPrisonersOnPlayerVictory = SettingsElement.ReadChildTextAs(nameof(RecruitPrisonersOnPlayerVictory), RecruitPrisonersOnPlayerVictory);
				UnlockAllBlacksmithParts = SettingsElement.ReadChildTextAs(nameof(UnlockAllBlacksmithParts), UnlockAllBlacksmithParts);
				AllowPregnancyInSameParty = SettingsElement.ReadChildTextAs(nameof(AllowPregnancyInSameParty), AllowPregnancyInSameParty);
				EnableFrugalCavalry = SettingsElement.ReadChildTextAs(nameof(EnableFrugalCavalry), EnableFrugalCavalry);
				DisableHorseCostForCavalryUpgrade = SettingsElement.ReadChildTextAs(nameof(DisableHorseCostForCavalryUpgrade), DisableHorseCostForCavalryUpgrade);
				EnableFastExitSettlement = SettingsElement.ReadChildTextAs(nameof(EnableFastExitSettlement), EnableFastExitSettlement);
				HideLockedItemsFromSmelting = SettingsElement.ReadChildTextAs(nameof(HideLockedItemsFromSmelting), HideLockedItemsFromSmelting);
				AllowRecruitingTopTroops = SettingsElement.ReadChildTextAs(nameof(AllowRecruitingTopTroops), AllowRecruitingTopTroops);
				HeroSpouseIsAlwaysFertile = SettingsElement.ReadChildTextAs(nameof(HeroSpouseIsAlwaysFertile), HeroSpouseIsAlwaysFertile);
				TroopsDropTheirItems = SettingsElement.ReadChildTextAs(nameof(TroopsDropTheirItems), TroopsDropTheirItems);
				EnableDailyTroopsTraining = SettingsElement.ReadChildTextAs(nameof(EnableDailyTroopsTraining), EnableDailyTroopsTraining);
				EnableDailySettlementPrisonerRecruitment = SettingsElement.ReadChildTextAs(nameof(EnableDailySettlementPrisonerRecruitment), EnableDailySettlementPrisonerRecruitment);
				EnableLowerPlayerSettlementWages = SettingsElement.ReadChildTextAs(nameof(EnableLowerPlayerSettlementWages), EnableLowerPlayerSettlementWages);
				FamilyPartiesAboveLimits = SettingsElement.ReadChildTextAs(nameof(FamilyPartiesAboveLimits), FamilyPartiesAboveLimits);
				EnterTownWithMount = SettingsElement.ReadChildTextAs(nameof(EnterTownWithMount), EnterTownWithMount);
				UpgradeBanditsWithoutDisciplinarianPerk = SettingsElement.ReadChildTextAs(nameof(UpgradeBanditsWithoutDisciplinarianPerk), UpgradeBanditsWithoutDisciplinarianPerk);

				PlayerWageModifier = SettingsElement.ReadChildTextAs<float>(nameof(PlayerWageModifier), PlayerWageModifier);
				TownGoldChangeModifier = SettingsElement.ReadChildTextAs<float>(nameof(TownGoldChangeModifier), TownGoldChangeModifier);
				PlayerPartySpeedModifier = SettingsElement.ReadChildTextAs<float>(nameof(PlayerPartySpeedModifier), PlayerPartySpeedModifier);
				PlayerKhuzaitSpeedModifier = SettingsElement.ReadChildTextAs<float>(nameof(PlayerKhuzaitSpeedModifier), PlayerKhuzaitSpeedModifier);
				
				PlayerPartyConsumeFood = SettingsElement.ReadChildTextAs(nameof(PlayerPartyConsumeFood), PlayerPartyConsumeFood);
				TrainTradeOnProfit = SettingsElement.ReadChildTextAs(nameof(TrainTradeOnProfit), TrainTradeOnProfit);
				NoLossesBreakingIntoSiege = SettingsElement.ReadChildTextAs<bool>(nameof(NoLossesBreakingIntoSiege), NoLossesBreakingIntoSiege);
				EnableFastSendTroopsToFightBandits = SettingsElement.ReadChildTextAs<bool>(nameof(EnableFastSendTroopsToFightBandits), EnableFastSendTroopsToFightBandits);
				BuildAllLevels = SettingsElement.ReadChildTextAs<bool>(nameof(BuildAllLevels), BuildAllLevels);
				AllowRecruitGarrison = SettingsElement.ReadChildTextAs<bool>(nameof(AllowRecruitGarrison), AllowRecruitGarrison);
				TransferWoundedFirst = SettingsElement.ReadChildTextAs<bool>(nameof(TransferWoundedFirst), TransferWoundedFirst);
				AnnounceTournamentsOnFinished = SettingsElement.ReadChildTextAs<bool>(nameof(AnnounceTournamentsOnFinished), AnnounceTournamentsOnFinished);
				AddLivingClanMembersEncyclopediaSorter = SettingsElement.ReadChildTextAs<bool>(nameof(AddLivingClanMembersEncyclopediaSorter), AddLivingClanMembersEncyclopediaSorter);
				TrackerChaseSpeedFactor = SettingsElement.ReadChildTextAs<float>(nameof(TrackerChaseSpeedFactor), TrackerChaseSpeedFactor);

				BattleHealSettings = new BattleHealSettings(SettingsElement.GetFirstElementByName(nameof(BattleHealSettings)));
				AgeSettings = new AgeSettings(SettingsElement.GetFirstElementByName(nameof(AgeSettings)));
			}
		}
	}
}
