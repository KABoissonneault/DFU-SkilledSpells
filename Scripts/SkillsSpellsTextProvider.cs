using System.Collections.Generic;

using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Utility;

class SkillsSpellsTextProvider : FallbackTextProvider
{
    public SkillsSpellsTextProvider(ITextProvider fallback)
        : base(fallback)
    {

    }

    public override TextFile.Token[] GetSkillSummary(DFCareer.Skills skill, int startPosition)
    {
        PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
        bool highlight = playerEntity.GetSkillRecentlyIncreased(skill);

        List<TextFile.Token> tokens = new List<TextFile.Token>();
        TextFile.Formatting formatting = highlight ? TextFile.Formatting.TextHighlight : TextFile.Formatting.Text;

        TextFile.Token skillNameToken = new TextFile.Token();
        skillNameToken.formatting = formatting;
        skillNameToken.text = DaggerfallUnity.Instance.TextProvider.GetSkillName(skill);

        short skillLevel = playerEntity.Skills.GetLiveSkillValue(skill);
        TextFile.Token skillValueToken = new TextFile.Token();
        skillValueToken.formatting = formatting;
        skillValueToken.text = string.Format("{0}%", skillLevel);

        DFCareer.Stats primaryStat = DaggerfallSkills.GetPrimaryStat(skill);
        TextFile.Token skillPrimaryStatToken = new TextFile.Token();
        skillPrimaryStatToken.formatting = formatting;
        skillPrimaryStatToken.text = DaggerfallUnity.Instance.TextProvider.GetAbbreviatedStatName(primaryStat);

        TextFile.Token positioningToken = new TextFile.Token();
        positioningToken.formatting = TextFile.Formatting.PositionPrefix;

        TextFile.Token tabToken = new TextFile.Token();
        tabToken.formatting = TextFile.Formatting.PositionPrefix;

        if (startPosition != 0) // if this is the second column
        {
            positioningToken.x = startPosition;
            tokens.Add(positioningToken);
        }
        tokens.Add(skillNameToken);
        if (SkilledSpellsMod.IsCasterSkill(skill))
        {
            int level = SkilledSpellsMod.SchoolCasterLevel(skill);
            tokens.Add(new TextFile.Token(formatting, $" ({level})"));
        }

        positioningToken.x = startPosition + 112;
        tokens.Add(positioningToken);
        tokens.Add(skillValueToken);

        return tokens.ToArray();
    }
}
