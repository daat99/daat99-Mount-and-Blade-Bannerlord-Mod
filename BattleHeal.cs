using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace daat99
{
	public class BattleHealBehavior : CampaignBehaviorBase
	{
		public bool HealedDuringBattle { get; set; } = false;

		public override void RegisterEvents()
		{
			CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this, FindBattle);
			CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this, FinishedBattle);
		}

        public void FindBattle(IMission iMission)
		{
			if ( iMission is Mission mission )
            {
				bool isCombat = mission.CombatType == Mission.MissionCombatType.Combat;
				if (isCombat && mission.Scene != null)
				{
					mission.AddMissionBehavior(new BattleHealLogic());
				}
			}
		}

		private void FinishedBattle(MapEvent obj)
		{
			if (HealedDuringBattle)
			{
				Hero.MainHero.AddSkillXp(DefaultSkills.Medicine, 1);
			}
		}

		public override void SyncData(IDataStore dataStore) { }
    }

	public class BattleHealLogic : MissionLogic
	{
		private static string PASSIVE_HEAL_TEXT = "Passively healed {0} for {1} hit points.";
		private static string LEECH_HEAL_TEXT = "Successfully leeched {1} hit points for {0}.";

		private BattleHealSettings m_settings => Settings.CampaignSettings.BattleHealSettings;

		private MissionTime nextPassiveHeal;

		public override void OnRenderingStarted()
        {
            base.OnRenderingStarted();
			BattleHealSettings.BattleHealBehavior.HealedDuringBattle = false;
			nextPassiveHeal = MissionTime.Zero;
		}

        public override void OnEarlyAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnEarlyAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
			if (affectedAgent.IsHuman && affectorAgent == Mission.MainAgent)
			{
				checkAndHeal(affectorAgent, false);
			}
        }

		public override void OnMissionTick(float dt)
		{
			base.OnMissionTick(dt);
			if (nextPassiveHeal.IsPast)
			{
				checkAndHeal(Mission.MainAgent, true);
			}
		}

		private void checkAndHeal(Agent player, bool isPassive)
        {
			if (player != null && player.IsActive())
            {
				Agent mount = player.MountAgent;
				bool isPlayerWounded = player.Health < player.HealthLimit;
				bool isMountWounded = mount != null && mount.Health < mount.HealthLimit;
				if (isPlayerWounded || isMountWounded)
				{
					int medicalSkill = player.Character.GetSkillValue(DefaultSkills.Medicine);
					if (isPlayerWounded)
					{
						healAgent(player, medicalSkill, isPassive);
					}
					if (isMountWounded)
					{
						healAgent(mount, medicalSkill, isPassive);
					}
					if (isPassive)
					{
						nextPassiveHeal = MissionTime.SecondsFromNow(m_settings.PassiveHealingDelayInSeconds);
					}
					else
                    {
						BattleHealSettings.BattleHealBehavior.HealedDuringBattle = true;
					}
				}
			}
        }

		private void healAgent(Agent agent, int medicalSkill, bool isPassive)
        {
			float percentageToHeal = (isPassive ? 0.5f : 1f)* (medicalSkill/ m_settings.MedicineSkillPerPercentageHealed) / 100f;
			float maxHitPointsToHeal = agent.HealthLimit* percentageToHeal;
			int hitPointsToHeal = (int)Math.Min(agent.HealthLimit - agent.Health, maxHitPointsToHeal);
			if ( hitPointsToHeal < 1)
            {
				hitPointsToHeal = 1;
            }
			agent.Health += hitPointsToHeal;
			if ( false == isPassive )
            {
				Hero.MainHero.AddFixedXpToSkill(DefaultSkills.Medicine, 1);
			}
			string message = string.Format(isPassive ? PASSIVE_HEAL_TEXT : LEECH_HEAL_TEXT, agent.Name.ToString(), hitPointsToHeal);
			InformationManager.DisplayMessage(new InformationMessage(message, Colors.White));
		}
	}
}
