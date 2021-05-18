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

        RegisterCostOverrides();

        FormulaHelper.RegisterOverride<Func<DaggerfallEntity, IEntityEffect, int>>(mod, "CalculateCasterLevel", CalculateCasterLevel);
        FormulaHelper.RegisterOverride<Func<IEntityEffect, EffectSettings, DaggerfallEntity, FormulaHelper.SpellCost>>(mod, "CalculateEffectCosts", CalculateEffectCosts);

        ConsoleCommandsDatabase.RegisterCommand("print_caster_levels", "Prints the caster level for all spell schools", "PRINT_CASTER_LEVELS", PrintCasterLevels);
#if UNITY_EDITOR
        ConsoleCommandsDatabase.RegisterCommand("verify_spell_costs", "Verifies the Skilled Spells overriden spell costs against a verification table, in the Persistent Path folder", "VERIFY_SPELL_COSTS [filename]", VerifyCostOverrides);
#endif

        Debug.Log("Finished mod init: Skilled Spells");
    }

    void RegisterCostOverrides()
    {
        var attributeKeys = new string[] { "Agility", "Endurance", "Intelligence", "Luck", "Personality", "Speed", "Strength", "Willpower" };

        // Alteration
        durationCostOverride.Add(Climbing.EffectKey, new EffectCosts { CostA = 8, CostB = 60 });

        {
            var elementKeys = new string[] { "Fire", "Frost", "Shock", "Magicka" };
            foreach(var elementKey in elementKeys.Select(key => $"ElementalResistance-{key}"))
            {
                durationCostOverride.Add(elementKey, new EffectCosts { CostA = 4, CostB = 30 });
                chanceCostOverride.Add(elementKey, new EffectCosts { CostA = 4, CostB = 30 });
            }

            durationCostOverride.Add("ElementalResistance-Poison", new EffectCosts { CostA = 6, CostB = 45 });
            chanceCostOverride.Add("ElementalResistance-Poison", new EffectCosts { CostA = 6, CostB = 45 });
        }

        durationCostOverride.Add(Jumping.EffectKey, new EffectCosts { CostA = 2, CostB = 15 });
        durationCostOverride.Add(Paralyze.EffectKey, new EffectCosts { CostA = 12, CostB = 100, OffsetGold = 25 });
        chanceCostOverride.Add(Paralyze.EffectKey, new EffectCosts { CostA = 12, CostB = 100 });
        durationCostOverride.Add(Shield.EffectKey, new EffectCosts { CostA = 2, CostB = 15 });
        magnitudeCostOverride.Add(Shield.EffectKey, new EffectCosts { CostA = 4, CostB = 30 });
        durationCostOverride.Add(Slowfall.EffectKey, new EffectCosts { CostA = 10, CostB = 75 });
        durationCostOverride.Add(WaterBreathing.EffectKey, new EffectCosts { CostA = 4, CostB = 30 });

        // Destruction
        durationCostOverride.Add(ContinuousDamageFatigue.EffectKey, new EffectCosts { CostA = 6, CostB = 57 });
        magnitudeCostOverride.Add(ContinuousDamageFatigue.EffectKey, new EffectCosts { CostA = 10, CostB = 95 });
        durationCostOverride.Add(ContinuousDamageHealth.EffectKey, new EffectCosts { CostA = 6, CostB = 57 });
        magnitudeCostOverride.Add(ContinuousDamageHealth.EffectKey, new EffectCosts { CostA = 10, CostB = 95 });
        durationCostOverride.Add(ContinuousDamageSpellPoints.EffectKey, new EffectCosts { CostA = 6, CostB = 57 });
        magnitudeCostOverride.Add(ContinuousDamageSpellPoints.EffectKey, new EffectCosts { CostA = 10, CostB = 95 });

        magnitudeCostOverride.Add(DamageFatigue.EffectKey, new EffectCosts { CostA = 8, CostB = 60 });
        magnitudeCostOverride.Add(DamageHealth.EffectKey, new EffectCosts { CostA = 8, CostB = 60 });
        magnitudeCostOverride.Add(DamageSpellPoints.EffectKey, new EffectCosts { CostA = 8, CostB = 60 });

        foreach(string attributeKey in attributeKeys.Select(key => $"Drain-{key}"))
        {
            magnitudeCostOverride.Add(attributeKey, new EffectCosts { CostA = 12, CostB = 100 });
        }

        foreach (string attributeKey in attributeKeys.Select(key => $"Transfer-{key}"))
        {
            magnitudeCostOverride.Add(attributeKey, new EffectCosts { CostA = 12, CostB = 100, OffsetGold = 40 });
        }
        magnitudeCostOverride.Add(TransferFatigue.EffectKey, new EffectCosts { CostA = 12, CostB = 100, OffsetGold = 40 });
        magnitudeCostOverride.Add(TransferHealth.EffectKey, new EffectCosts { CostA = 12, CostB = 100, OffsetGold = 40 });

        chanceCostOverride.Add(Disintegrate.EffectKey, new EffectCosts { CostA = 18, CostB = 140 });

        // Illusion
        durationCostOverride.Add(ChameleonNormal.EffectKey, new EffectCosts { CostA = 10, CostB = 75 });
        durationCostOverride.Add(ChameleonTrue.EffectKey, new EffectCosts { CostA = 14, CostB = 120 });
        durationCostOverride.Add(InvisibilityNormal.EffectKey, new EffectCosts { CostA = 14, CostB = 120 });
        durationCostOverride.Add(InvisibilityTrue.EffectKey, new EffectCosts { CostA = 18, CostB = 140 });
        durationCostOverride.Add(ShadowNormal.EffectKey, new EffectCosts { CostA = 10, CostB = 75 });
        durationCostOverride.Add(ShadowTrue.EffectKey, new EffectCosts { CostA = 14, CostB = 120 });

        // Mysticism
        durationCostOverride.Add(ComprehendLanguages.EffectKey, new EffectCosts { CostA = 10, CostB = 95 });
        chanceCostOverride.Add(ComprehendLanguages.EffectKey, new EffectCosts { CostA = 4, CostB = 38 });
        durationCostOverride.Add(CreateItem.EffectKey, new EffectCosts { CostA = 10, CostB = 75 });
        chanceCostOverride.Add(DispelDaedra.EffectKey, new EffectCosts { CostA = 22, CostB = 180 });
        chanceCostOverride.Add(DispelMagic.EffectKey, new EffectCosts { CostA = 22, CostB = 180 });
        chanceCostOverride.Add(DispelUndead.EffectKey, new EffectCosts { CostA = 18, CostB = 140 });
        chanceCostOverride.Add(Lock.EffectKey, new EffectCosts { CostA = 8, CostB = 60 });
        durationCostOverride.Add(Silence.EffectKey, new EffectCosts { CostA = 14, CostB = 120 });
        chanceCostOverride.Add(Silence.EffectKey, new EffectCosts { CostA = 4, CostB = 38 });
        durationCostOverride.Add(SoulTrap.EffectKey, new EffectCosts { CostA = 10, CostB = 80, OffsetGold = 40 });
        chanceCostOverride.Add(SoulTrap.EffectKey, new EffectCosts { CostA = 4, CostB = 30 });

        // Restoration
        chanceCostOverride.Add(CureDisease.EffectKey, new EffectCosts { CostA = 8, CostB = 100 });
        chanceCostOverride.Add(CureParalyzation.EffectKey, new EffectCosts { CostA = 8, CostB = 100 });
        chanceCostOverride.Add(CurePoison.EffectKey, new EffectCosts { CostA = 8, CostB = 100 });
        foreach (string attributeKey in attributeKeys.Select(key => $"Fortify-{key}"))
        {
            durationCostOverride.Add(attributeKey, new EffectCosts { CostA = 8, CostB = 60 });
            magnitudeCostOverride.Add(attributeKey, new EffectCosts { CostA = 14, CostB = 120 });
        }
        durationCostOverride.Add(FreeAction.EffectKey, new EffectCosts { CostA = 14, CostB = 120 });

        foreach (string attributeKey in attributeKeys.Select(key => $"Heal-{key}"))
        {
            magnitudeCostOverride.Add(attributeKey, new EffectCosts { CostA = 4, CostB = 30 });
        }
        magnitudeCostOverride.Add(HealFatigue.EffectKey, new EffectCosts { CostA = 4, CostB = 30 });
        magnitudeCostOverride.Add(HealHealth.EffectKey, new EffectCosts { CostA = 4, CostB = 30 });

        durationCostOverride.Add(Regenerate.EffectKey, new EffectCosts { CostA = 4, CostB = 30 });
        durationCostOverride.Add(SpellAbsorption.EffectKey, new EffectCosts { CostA = 18, CostB = 140 });
        chanceCostOverride.Add(SpellAbsorption.EffectKey, new EffectCosts { CostA = 18, CostB = 140 });

        // Thaumaturgy
        chanceCostOverride.Add(CharmEffect.EffectKey, new EffectCosts { CostA = 8, CostB = 76 });
        {
            var typeKeys = new string[] { "Enemy", "Magic", "Treasure" };
            foreach (string attributeKey in typeKeys.Select(key => $"Detect-{key}"))
            {
                durationCostOverride.Add(attributeKey, new EffectCosts { CostA = 4, CostB = 30 });
            }
        }
        durationCostOverride.Add(Levitate.EffectKey, new EffectCosts { CostA = 12, CostB = 100 });
        durationCostOverride.Add("Pacify-Animal", new EffectCosts { CostA = 12, CostB = 100, OffsetGold = 36 });
        durationCostOverride.Add("Pacify-Daedra", new EffectCosts { CostA = 14, CostB = 120, OffsetGold = 160 });
        durationCostOverride.Add("Pacify-Humanoid", new EffectCosts { CostA = 12, CostB = 100, OffsetGold = 60 });
        durationCostOverride.Add("Pacify-Undead", new EffectCosts { CostA = 12, CostB = 100 });
        durationCostOverride.Add(SpellReflection.EffectKey, new EffectCosts { CostA = 12, CostB = 100 });
        chanceCostOverride.Add(SpellReflection.EffectKey, new EffectCosts { CostA = 12, CostB = 114 });
        durationCostOverride.Add(SpellResistance.EffectKey, new EffectCosts { CostA = 10, CostB = 95 });
        chanceCostOverride.Add(SpellResistance.EffectKey, new EffectCosts { CostA = 8, CostB = 76 });
        durationCostOverride.Add(WaterWalking.EffectKey, new EffectCosts { CostA = 8, CostB = 60 });
    }

#if UNITY_EDITOR
    string VerifyCostOverrides(params string[] args)
    {
        string tablePath = Path.Combine(DaggerfallUnity.Settings.PersistentDataPath, "SkilledSpellsCosts.csv");
        if (!File.Exists(tablePath))
            return $"Could not find '{tablePath}'";

        Dictionary<string, EffectCosts> durationCosts = new Dictionary<string, EffectCosts>();
        Dictionary<string, EffectCosts> chanceCosts = new Dictionary<string, EffectCosts>();
        Dictionary<string, EffectCosts> magnitudeCosts = new Dictionary<string, EffectCosts>();

        using(StreamReader tableFile = File.OpenText(tablePath))
        {
            tableFile.ReadLine(); // Strip header

            while(!tableFile.EndOfStream)
            {
                string line = tableFile.ReadLine();

                string[] tokens = line.Split(';');
                string effect = tokens[0];

                int ValueOrZero(string token)
                {
                    return !string.IsNullOrEmpty(token) ? int.Parse(token) : 0;
                }

                int durationA = ValueOrZero(tokens[2]);
                int durationB = ValueOrZero(tokens[3]);
                int durationOffset = ValueOrZero(tokens[4]);

                int chanceA = ValueOrZero(tokens[5]);
                int chanceB = ValueOrZero(tokens[6]);
                int chanceOffset = ValueOrZero(tokens[7]);

                int magnitudeA = ValueOrZero(tokens[8]);
                int magnitudeB = ValueOrZero(tokens[9]);
                int magnitudeOffset = ValueOrZero(tokens[10]);

                durationCosts.Add(effect, new EffectCosts { CostA = durationA, CostB = durationB, OffsetGold = durationOffset });
                chanceCosts.Add(effect, new EffectCosts { CostA = chanceA, CostB = chanceB, OffsetGold = chanceOffset });
                magnitudeCosts.Add(effect, new EffectCosts { CostA = magnitudeA, CostB = magnitudeB, OffsetGold = magnitudeOffset });
            }
        }

        foreach(KeyValuePair<string, EffectCosts> costOverride in durationCostOverride)
        {
            EffectCosts validationCost = durationCosts[costOverride.Key];
            EffectCosts overrideCost = costOverride.Value;
            if (validationCost.CostA != overrideCost.CostA)
                Debug.LogError($"[Skilled Spells] Validation error for spell effect '{costOverride.Key}':" +
                    $"duration override CostA '{overrideCost.CostA}' does not match validation '{validationCost.CostA}'");

            if (validationCost.CostB != overrideCost.CostB)
                Debug.LogError($"[Skilled Spells] Validation error for spell effect '{costOverride.Key}':" +
                    $"duration override CostB '{overrideCost.CostB}' does not match validation '{validationCost.CostB}'");

            if (validationCost.OffsetGold != overrideCost.OffsetGold)
                Debug.LogError($"[Skilled Spells] Validation error for spell effect '{costOverride.Key}':" +
                    $"duration override OffsetGold '{overrideCost.OffsetGold}' does not match validation '{validationCost.OffsetGold}'");
        }

        foreach (KeyValuePair<string, EffectCosts> costOverride in chanceCostOverride)
        {
            EffectCosts validationCost = chanceCosts[costOverride.Key];
            EffectCosts overrideCost = costOverride.Value;
            if (validationCost.CostA != overrideCost.CostA)
                Debug.LogError($"[Skilled Spells] Validation error for spell effect '{costOverride.Key}':" +
                    $"chance override CostA '{overrideCost.CostA}' does not match validation '{validationCost.CostA}'");

            if (validationCost.CostB != overrideCost.CostB)
                Debug.LogError($"[Skilled Spells] Validation error for spell effect '{costOverride.Key}':" +
                    $"chance override CostB '{overrideCost.CostB}' does not match validation '{validationCost.CostB}'");

            if (validationCost.OffsetGold != overrideCost.OffsetGold)
                Debug.LogError($"[Skilled Spells] Validation error for spell effect '{costOverride.Key}':" +
                    $"chance override OffsetGold '{overrideCost.OffsetGold}' does not match validation '{validationCost.OffsetGold}'");
        }

        foreach (KeyValuePair<string, EffectCosts> costOverride in magnitudeCostOverride)
        {
            EffectCosts validationCost = magnitudeCosts[costOverride.Key];
            EffectCosts overrideCost = costOverride.Value;
            if (validationCost.CostA != overrideCost.CostA)
                Debug.LogError($"[Skilled Spells] Validation error for spell effect '{costOverride.Key}':" +
                    $"magnitude override CostA '{overrideCost.CostA}' does not match validation '{validationCost.CostA}'");

            if (validationCost.CostB != overrideCost.CostB)
                Debug.LogError($"[Skilled Spells] Validation error for spell effect '{costOverride.Key}':" +
                    $"magnitude override CostB '{overrideCost.CostB}' does not match validation '{validationCost.CostB}'");

            if (validationCost.OffsetGold != overrideCost.OffsetGold)
                Debug.LogError($"[Skilled Spells] Validation error for spell effect '{costOverride.Key}':" +
                    $"magnitude override OffsetGold '{overrideCost.OffsetGold}' does not match validation '{validationCost.OffsetGold}'");
        }

        return "Verification done";
    }
#endif

    private static int SchoolCasterLevelBonus(DFCareer.Skills skill)
    {
        PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

        if (playerEntity.GetPrimarySkills().Contains(skill))
            return 2;

        if (playerEntity.GetMajorSkills().Contains(skill))
            return 1;

        return 0;
    }

    private static int SchoolCasterLevel(DFCareer.Skills skill)
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
