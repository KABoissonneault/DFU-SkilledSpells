using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

using Wenzil.Console;

public class SkilledSpellsMod : MonoBehaviour
{
    static Mod mod;

    private readonly Dictionary<string, EffectCosts> durationCostOverride = new Dictionary<string, EffectCosts>();
    private readonly Dictionary<string, EffectCosts> chanceCostOverride = new Dictionary<string, EffectCosts>();
    private readonly Dictionary<string, EffectCosts> magnitudeCostOverride = new Dictionary<string, EffectCosts>();

    [Invoke(StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        new GameObject(mod.Title).AddComponent<SkilledSpellsMod>();
    }

    void Awake()
    {
        Debug.Log("Begin mod init: Skilled Spells");

        ParseCostOverrides();
		
        DaggerfallUnity.Instance.TextProvider = new SkillsSpellsTextProvider(DaggerfallUnity.Instance.TextProvider);

        FormulaHelper.RegisterOverride<Func<DaggerfallEntity, IEntityEffect, int>>(mod, "CalculateCasterLevel", CalculateCasterLevel);
        FormulaHelper.RegisterOverride<Func<IEntityEffect, EffectSettings, DaggerfallEntity, FormulaHelper.SpellCost>>(mod, "CalculateEffectCosts", CalculateEffectCosts);

        ConsoleCommandsDatabase.RegisterCommand("print_caster_levels", "Prints the caster level for all spell schools", "PRINT_CASTER_LEVELS", PrintCasterLevels);

        UIWindowFactory.RegisterCustomUIWindow(UIWindowType.SpellMaker, typeof(SkilledSpellmakerWindow));
        UIWindowFactory.RegisterCustomUIWindow(UIWindowType.EffectSettingsEditor, typeof(SkilledEffectSettingsEditorWindow));

        Debug.Log("Finished mod init: Skilled Spells");
    }

    public static bool IsCasterSkill(DFCareer.Skills skill)
    {
        return skill == DFCareer.Skills.Alteration
            || skill == DFCareer.Skills.Destruction
            || skill == DFCareer.Skills.Illusion
            || skill == DFCareer.Skills.Mysticism
            || skill == DFCareer.Skills.Restoration
            || skill == DFCareer.Skills.Thaumaturgy;
    }

    void ParseCostOverrides()
    {
        TextAsset costsFile = mod.GetAsset<TextAsset>("SkilledSpellsCosts.csv");
        if (costsFile == null)
            return;

        string[] lines = costsFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines.Skip(1))
        {
            string[] tokens = line.Split(';');
            string key = tokens[0];

            // Duration override
            if(!string.IsNullOrEmpty(tokens[1]))
            {
                EffectCosts effectCosts = new EffectCosts { CostA = int.Parse(tokens[1]), CostB = int.Parse(tokens[2]) };
                if(int.TryParse(tokens[3], out int offsetGold))
                {
                    effectCosts.OffsetGold = offsetGold;
                }

                durationCostOverride.Add(key, effectCosts);
            }

            // Chance override
            if (!string.IsNullOrEmpty(tokens[4]))
            {
                EffectCosts effectCosts = new EffectCosts { CostA = int.Parse(tokens[4]), CostB = int.Parse(tokens[5]) };
                if (int.TryParse(tokens[6], out int offsetGold))
                {
                    effectCosts.OffsetGold = offsetGold;
                }

                chanceCostOverride.Add(key, effectCosts);
            }

            // Magnitude override
            if (!string.IsNullOrEmpty(tokens[7]))
            {
                EffectCosts effectCosts = new EffectCosts { CostA = int.Parse(tokens[7]), CostB = int.Parse(tokens[8]) };
                if (int.TryParse(tokens[9], out int offsetGold))
                {
                    effectCosts.OffsetGold = offsetGold;
                }

                magnitudeCostOverride.Add(key, effectCosts);
            }
        }
    }

    private static int SchoolCasterLevelBonus(DFCareer.Skills skill)
    {
        PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

        if (playerEntity.GetPrimarySkills().Contains(skill))
            return 2;

        if (playerEntity.GetMajorSkills().Contains(skill))
            return 1;

        return 0;
    }

    public static int SchoolCasterLevel(DFCareer.Skills skill)
    {
        PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

        int level = playerEntity.Level;
        int skillValue = playerEntity.Skills.GetLiveSkillValue(skill);

        int skillLevel = Mathf.RoundToInt(level * skillValue / 100.0f);

        // Your willpower puts a "limit" to how high your skill can bring you
        // A character of average willpower can go up to Caster Level 15 only
        // With 100 Willpower, you can reach the maximum caster level of 30
        int willpowerLevel = playerEntity.Stats.LiveWillpower / 5 + 10;

        // Caster level is always at least 1
        return Mathf.Max(1, Mathf.Min(skillLevel, willpowerLevel) + SchoolCasterLevelBonus(skill));
    }

    private static int CalculateCasterLevel(DaggerfallEntity caster, IEntityEffect effect)
    {
        // Only handle magic spells from the player
        // Also don't use "skilled levels" for magic items
        if (caster != GameManager.Instance.PlayerEntity
            || effect.Properties.MagicSkill == DFCareer.MagicSkills.None
            || (effect.ParentBundle != null && effect.ParentBundle.castByItem != null)
            )
            return (caster != null) ? caster.Level : 1;

        return SchoolCasterLevel((DFCareer.Skills)effect.Properties.MagicSkill);       
    }

    private static string PrintCasterLevels(params string[] args)
    {
        return $"Destruction: {SchoolCasterLevel(DFCareer.Skills.Destruction)}\n" +
            $"Restoration: {SchoolCasterLevel(DFCareer.Skills.Restoration)}\n" +
            $"Illusion: {SchoolCasterLevel(DFCareer.Skills.Illusion)}\n" +
            $"Alteration: {SchoolCasterLevel(DFCareer.Skills.Alteration)}\n" +
            $"Thaumaturgy: {SchoolCasterLevel(DFCareer.Skills.Thaumaturgy)}\n" +
            $"Mysticism: {SchoolCasterLevel(DFCareer.Skills.Mysticism)}";
    }

    private static int GetEffectComponentCosts(
            EffectCosts costs,
            float starting,
            float increase,
            float perLevel)
    {
        //Calculate effect gold cost, spellpoint cost is calculated from gold cost after adding up for duration, chance and magnitude
        return Mathf.RoundToInt(costs.OffsetGold + costs.CostA * starting + costs.CostB * increase / perLevel);
    }

    private FormulaHelper.SpellCost CalculateEffectCosts(IEntityEffect effect, EffectSettings settings, DaggerfallEntity casterEntity)
    {
        bool activeComponents = false;

        // Duration costs
        int durationGoldCost = 0;
        if (effect.Properties.SupportDuration)
        {
            EffectCosts durationCost = effect.Properties.DurationCosts;
            if(durationCostOverride.TryGetValue(effect.Key, out EffectCosts costOverride))
            {
                durationCost = costOverride;
            }

            activeComponents = true;
            durationGoldCost = GetEffectComponentCosts(
                durationCost,
                settings.DurationBase,
                settings.DurationPlus,
                settings.DurationPerLevel);
        }

        // Chance costs
        int chanceGoldCost = 0;
        if (effect.Properties.SupportChance)
        {
            EffectCosts chanceCosts = effect.Properties.ChanceCosts;
            if (chanceCostOverride.TryGetValue(effect.Key, out EffectCosts costOverride))
            {
                chanceCosts = costOverride;
            }

            activeComponents = true;
            chanceGoldCost = GetEffectComponentCosts(
                chanceCosts,
                settings.ChanceBase,
                settings.ChancePlus,
                settings.ChancePerLevel);
        }

        // Magnitude costs
        int magnitudeGoldCost = 0;
        if (effect.Properties.SupportMagnitude)
        {
            EffectCosts magnitudeCosts = effect.Properties.MagnitudeCosts;
            if (magnitudeCostOverride.TryGetValue(effect.Key, out EffectCosts costOverride))
            {
                magnitudeCosts = costOverride;
            }

            activeComponents = true;
            float magnitudeBase = (settings.MagnitudeBaseMax + settings.MagnitudeBaseMin) / 2.0f;
            float magnitudePlus = (settings.MagnitudePlusMax + settings.MagnitudePlusMin) / 2.0f;
            magnitudeGoldCost = GetEffectComponentCosts(
                magnitudeCosts,
                magnitudeBase,
                magnitudePlus,
                settings.MagnitudePerLevel);
        }

        // If there are no active components (e.g. Teleport) then fudge some costs
        // This gives the same casting cost outcome as classic and supplies a reasonable gold cost
        // Note: Classic does not assign a gold cost when a zero-component effect is the only effect present, which seems like a bug
        int fudgeGoldCost = 0;
        if (!activeComponents)
            fudgeGoldCost = 240;

        // Get related skill
        int skillValue;
        if (casterEntity == null)
        {
            // From player
            skillValue = GameManager.Instance.PlayerEntity.Skills.GetLiveSkillValue((DFCareer.Skills)effect.Properties.MagicSkill);
        }
        else
        {
            // From another entity
            skillValue = casterEntity.Skills.GetLiveSkillValue((DFCareer.Skills)effect.Properties.MagicSkill);
        }

        // Add gold costs together and calculate spellpoint cost from the result
        FormulaHelper.SpellCost effectCost;
        effectCost.goldCost = durationGoldCost + chanceGoldCost + magnitudeGoldCost + fudgeGoldCost;
        effectCost.spellPointCost = Mathf.RoundToInt(effectCost.goldCost / (4.0f + (skillValue / 25.0f)));

        return effectCost;
    }
}
