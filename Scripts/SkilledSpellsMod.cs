using System;
using UnityEngine;

using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Utility.ModSupport;

using Wenzil.Console;

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

        FormulaHelper.RegisterOverride<Func<DaggerfallEntity, IEntityEffect, int>>(mod, "CalculateCasterLevel", CalculateCasterLevel);

        ConsoleCommandsDatabase.RegisterCommand("print_caster_levels", "Prints the caster level for all spell schools", "PRINT_CASTER_LEVELS", PrintCasterLevels);

        Debug.Log("Finished mod init: Skilled Spells");
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
        if (caster != GameManager.Instance.PlayerEntity || effect.Properties.MagicSkill == DFCareer.MagicSkills.None)
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
}
