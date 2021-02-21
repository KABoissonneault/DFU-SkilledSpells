using System;
using UnityEngine;

using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Utility.ModSupport;


public class UnleveledSpellsMod : MonoBehaviour
{
    static Mod mod;

    [Invoke(StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        new GameObject(mod.Title).AddComponent<UnleveledSpellsMod>();
    }

    void Awake()
    {
        Debug.Log("Begin mod init: Unleveled Spells");

        FormulaHelper.RegisterOverride<Func<DaggerfallEntity, IEntityEffect, int>>(mod, "CalculateCasterLevel", CalculateCasterLevel);

        Debug.Log("Finished mod init: Unleveled Spells");
    }

    private static int CalculateCasterLevel(DaggerfallEntity caster, IEntityEffect effect)
    {
        // Only handle magic spells from the player
        if (caster != GameManager.Instance.PlayerEntity || effect.Properties.MagicSkill == DFCareer.MagicSkills.None)
            return (caster != null) ? caster.Level : 1;

        PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

        int skillValue = playerEntity.Skills.GetLiveSkillValue((DFCareer.Skills)effect.Properties.MagicSkill);

        // You're eligible for caster level 2 starting at 15 in a given magic skill, then another caster level every 3 skill level after that
        int skillLevel = skillValue / 3 - 3;

        // Your willpower puts a "limit" to how high your skill can bring you
        // A character of average willpower can go up to Caster Level 15 only
        // With 100 Willpower, you can reach the maximum caster level of 30 (Luck aside)
        int willpowerLevel = playerEntity.Stats.LiveWillpower / 5 + 10;

        // Every 10 points of Luck above 50 give an extra point which may raise caster level
        // Every 10 points of Luck below 50 remove a caster level
        // For bonus points, each point is distributed to the lower of the two level values (skill and willpower)
        // If skill level and willpower level are nearly equal, this may result in less increase than simply adding Luck to caster level
        int luckPointsToDistribute = (playerEntity.Stats.LiveLuck - 50) / 10; // (Luck - 50) / 10 
        int luckPointsToSkillLevel = 0;
        int luckPointsToWillpowerLevel = 0;
        if (luckPointsToDistribute > 0)
        {
            DistributePointsToEqualize(skillLevel, willpowerLevel, luckPointsToDistribute, out luckPointsToSkillLevel, out luckPointsToWillpowerLevel);
        }
        else
        {
            if (skillLevel < willpowerLevel)
                luckPointsToSkillLevel = luckPointsToDistribute;
            else
                luckPointsToWillpowerLevel = luckPointsToDistribute;
        }

        // Caster level is always at least 1
        return Mathf.Max(1, Mathf.Min(skillLevel + luckPointsToSkillLevel, willpowerLevel + luckPointsToWillpowerLevel));
    }

    private static void DistributePointsToEqualize(int level1, int level2, int pointsToDistribute, out int additionToLevel1, out int additionToLevel2)
    {
        additionToLevel1 = 0;
        additionToLevel2 = 0;

        while (pointsToDistribute > 0)
        {
            if (level1 + additionToLevel1 < level2 + additionToLevel2)
                additionToLevel1++;
            else
                additionToLevel2++;
            pointsToDistribute--;
        }
    }
}
