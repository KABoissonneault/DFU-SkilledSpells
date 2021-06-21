using System;
using UnityEngine;

using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Utility.ModSupport;

using Wenzil.Console;
using DaggerfallWorkshop;

public class SkilledSpellsMod : MonoBehaviour
{
    static Mod mod;

    [Invoke(StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        new GameObject(mod.Title).AddComponent<SkilledSpellsMod>();
    }

    void Awake()
    {
        Debug.Log("Begin mod init: Skilled Spells");

        DaggerfallUnity.Instance.TextProvider = new SkillsSpellsTextProvider(DaggerfallUnity.Instance.TextProvider);

        FormulaHelper.RegisterOverride<Func<DaggerfallEntity, IEntityEffect, int>>(mod, "CalculateCasterLevel", CalculateCasterLevel);

        ConsoleCommandsDatabase.RegisterCommand("print_caster_levels", "Prints the caster level for all spell schools", "PRINT_CASTER_LEVELS", PrintCasterLevels);

        mod.MessageReceiver = MessageReceiver;

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
        // Also don't use "skilled levels" for magic items and potions
        if (caster != GameManager.Instance.PlayerEntity
            || effect.Properties.MagicSkill == DFCareer.MagicSkills.None
            || effect.ParentBundle == null
            || effect.ParentBundle.bundleType != BundleTypes.Spell
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

    static void MessageReceiver(string message, object data, DFModMessageCallback callBack)
    {
        switch(message)
        {
            case "CasterLevel":
                DFCareer.Skills skill = (DFCareer.Skills)data;
                if(!IsCasterSkill(skill))
                {
                    Debug.LogError($"Could not return caster level for invalid skill '{Enum.GetName(typeof(DFCareer.Skills), skill)}'");
                    break;
                }
                callBack(message, SchoolCasterLevel(skill));
                break;
            case "CasterLevels":
                int[] casterLevels = new int[]
                {
                    SchoolCasterLevel(DFCareer.Skills.Destruction),
                    SchoolCasterLevel(DFCareer.Skills.Restoration),
                    SchoolCasterLevel(DFCareer.Skills.Illusion),
                    SchoolCasterLevel(DFCareer.Skills.Alteration),
                    SchoolCasterLevel(DFCareer.Skills.Thaumaturgy),
                    SchoolCasterLevel(DFCareer.Skills.Mysticism)
                };
                callBack(message, casterLevels);
                break;
        }
    }
}
