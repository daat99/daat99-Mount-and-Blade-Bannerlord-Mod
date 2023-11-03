using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Localization;

namespace daat99
{
    public class Daat99CharacterDevelopmentModel : DefaultCharacterDevelopmentModel
    {
        private static TextObject _skillFocusText = new TextObject("{=MRktqZwu}Skill Focus");

        public override ExplainedNumber CalculateLearningLimit(int attributeValue, int focusValue, TextObject attributeName, bool includeDescriptions = false)
        {
            ExplainedNumber explainedSkillLimit;
            int maxSkill = Settings.CampaignSettings.MaximumSkillOverride;
            if (maxSkill > 0)
            {
                explainedSkillLimit = new ExplainedNumber(0f);
                explainedSkillLimit.Add(maxSkill, new TextObject("max"));
            }
            else
            {
                explainedSkillLimit = base.CalculateLearningLimit(attributeValue, focusValue, attributeName, includeDescriptions);
            }
            return explainedSkillLimit;
        }
    }
}
