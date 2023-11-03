using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.Core;

namespace daat99
{
    public static class Extensions
    {
        private static IReadOnlyList<SkillObject> s_allSkills;
        public static IReadOnlyList<SkillObject> ALL_SKILLS
        {
            get
            {
                if ( s_allSkills == null)
                {
                    s_allSkills = typeof(DefaultSkills).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(prop => prop.PropertyType == typeof(SkillObject)).Select<PropertyInfo, SkillObject>(pr => (SkillObject)pr.GetValue(null)).ToList();
                }
                return s_allSkills;
            }
        }

        private static CharacterDevelopmentModel s_characterDevelopmentModel;
        public static CharacterDevelopmentModel CharacterDevelopmentModel => s_characterDevelopmentModel ?? (s_characterDevelopmentModel = Campaign.Current.Models.CharacterDevelopmentModel);

        public static void TrainRandomSkill(this Hero hero, float levelUpMultiplier = 0.001f)
        {
            if (levelUpMultiplier > 0 && hero.IsAlive && false == hero.IsChild)
            {
                SkillObject skill = ALL_SKILLS.GetRandomElement<SkillObject>();
                int skillLevel = hero.GetSkillValue(skill);

                float learningRate = CharacterDevelopmentModel.CalculateLearningRate(hero, skill);
                float experienceForMultiplier = CharacterDevelopmentModel.GetXpRequiredForSkillLevel(skillLevel) * levelUpMultiplier;
                int experience = (int)(experienceForMultiplier / learningRate);
                hero.AddSkillXp(skill, experience);
            }
        }

        public static void AddFixedXpToSkill(this Hero hero, SkillObject skill, float desiredFixedXp)
        {
            float learningRate = CharacterDevelopmentModel.CalculateLearningRate(hero, skill);
            int experience = (int)Math.Max(1,desiredFixedXp / learningRate);
            hero.AddSkillXp(skill, experience);
        }

        public static PropertyInfo GetPropertyInfo(this object o, string propertyName) => o.GetType().GetPropertyInfo(propertyName);
        public static PropertyInfo GetPropertyInfo(this Type type, string propertyName) => type?.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        public static MethodInfo GetMethodInfo(this object o, string methodName) => o.GetType().GetMethodInfo(methodName);
        public static MethodInfo GetMethodInfo(this Type type, string methodName) => type?.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        public static FieldInfo GetFieldInfo(this object o, string fieldName) => o.GetType().GetFieldInfo(fieldName);
        public static FieldInfo GetFieldInfo(this Type type, string fieldName) => type?.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        public static T GetFieldData<T>(this object o, FieldInfo field) => (T)field?.GetValue(o);
        public static T GetFieldData<T>(this object o, string fieldName) => (T)o.GetFieldInfo(fieldName)?.GetValue(o);
        public static void SetFieldData<T>(this object o, FieldInfo field, T data) => field?.SetValue(o, data);

    }
}
