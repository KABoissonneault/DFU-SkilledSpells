using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

class SkilledSpellmakerWindow : DaggerfallSpellMakerWindow
{
    #region Constructors

    public SkilledSpellmakerWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
        : base(uiManager, previous)
    {
    }

    #endregion

    #region Private Methods
    protected override void EffectEditor_OnSettingsChanged()
    {
        
    }

    protected override void AddAndEditSlot(IEntityEffect effectTemplate)
    {
        effectEditor.EffectTemplate = effectTemplate;
        int slot = GetFirstFreeEffectSlotIndex();
        editOrDeleteSlot = slot;
        uiManager.PushWindow(effectEditor);
    }

    protected override void EffectEditor_OnClose()
    {
        var skilledEditor = effectEditor as SkilledEffectSettingsEditorWindow;
        if(skilledEditor == null)
        {
            base.EffectEditor_OnClose();
            return;
        }

        if(skilledEditor.IsEntryValid())
        {
            EffectEntries[editOrDeleteSlot] = effectEditor.EffectEntry;
            UpdateSpellCosts();
            UpdateSlotText(editOrDeleteSlot, effectEditor.EffectTemplate.DisplayName);
        }

        base.EffectEditor_OnClose();
    }
    #endregion
}
