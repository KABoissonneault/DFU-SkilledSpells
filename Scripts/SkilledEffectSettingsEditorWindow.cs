using System;

using UnityEngine;

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallConnect.Arena2;
using static DaggerfallWorkshop.Game.UserInterfaceWindows.DaggerfallMessageBox;
using DaggerfallWorkshop.Game.Formulas;

class SkilledEffectSettingsEditorWindow : DaggerfallEffectSettingsEditorWindow
{
    protected Button confirmButton;
    protected TextLabel goldCostLabel;

    bool entryValid = false;

    #region Constructors

    public SkilledEffectSettingsEditorWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
        : base(uiManager, previous)
    {
    }

    #endregion

    #region Setup Methods
    protected override void Setup()
    {
        // Load all the textures used by effect editor window
        LoadTextures();

        // Setup native panel background
        NativePanel.BackgroundTexture = baseTexture;

        // Setup controls
        SetupEffectDescriptionPanels();

        durationBaseSpinnerRect = new Rect(64, 105, spinnerWidth, spinnerHeight);
        durationPlusSpinnerRect = new Rect(104, 105, spinnerWidth, spinnerHeight);
        durationPerLevelSpinnerRect = new Rect(160, 105, spinnerWidth, spinnerHeight);
        chanceBaseSpinnerRect = new Rect(64, 125, spinnerWidth, spinnerHeight);
        chancePlusSpinnerRect = new Rect(104, 125, spinnerWidth, spinnerHeight);
        chancePerLevelSpinnerRect = new Rect(160, 125, spinnerWidth, spinnerHeight);
        magnitudeBaseMinSpinnerRect = new Rect(64, 145, spinnerWidth, spinnerHeight);
        magnitudeBaseMaxSpinnerRect = new Rect(104, 145, spinnerWidth, spinnerHeight);
        magnitudePlusMinSpinnerRect = new Rect(144, 145, spinnerWidth, spinnerHeight);
        magnitudePlusMaxSpinnerRect = new Rect(184, 145, spinnerWidth, spinnerHeight);
        magnitudePerLevelSpinnerRect = new Rect(235, 145, spinnerWidth, spinnerHeight);

        SetupSpinners();

        SetupButtons();
        InitControlState();

        // Spell cost label
        goldCostLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(274, 102), string.Empty, NativePanel);
        spellCostLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(274, 122), string.Empty, NativePanel);
         
        IsSetup = true;
        UpdateCosts();
    }

    protected override void SetupButtons()
    {
        // Confirm button
        confirmButton = DaggerfallUI.AddButton(new Rect(116, 169, 32, 16), NativePanel);
        confirmButton.OnMouseClick += ConfirmButton_OnMouseClick;

        // Cancel button
        exitButton = DaggerfallUI.AddButton(new Rect(172, 169, 32, 16), NativePanel);
        exitButton.OnMouseClick += CancelButton_OnMouseClick;
    }

    #endregion

    #region Helpers
    public bool IsEntryValid()
    {
        return entryValid;
    }

    private bool AreEffectAttributesValid()
    {
        if (EffectTemplate.Properties.SupportDuration
            && durationBaseSpinner.Value + durationPlusSpinner.Value == 0)
            return false;

        if (EffectTemplate.Properties.SupportChance
            && chanceBaseSpinner.Value + chancePlusSpinner.Value == 0)
            return false;

        if (EffectTemplate.Properties.SupportMagnitude
            && magnitudeBaseMinSpinner.Value + magnitudeBaseMaxSpinner.Value + magnitudePlusMinSpinner.Value + magnitudePlusMaxSpinner.Value == 0)
            return false;

        return true;
    }

    #endregion

    #region Protected Methods

    public override void Update()
    {
        base.Update();

        if (DaggerfallUI.Instance.HotkeySequenceProcessed == HotkeySequence.HotkeySequenceProcessStatus.NotFound
            && Input.GetKeyUp(KeyCode.Return))
        {
            OnConfirm();
        }
    }

    public override void CancelWindow()
    {
        entryValid = false;
        base.CancelWindow();
    }

    protected override void SetupSpinners()
    {
        // Add spinner controls
        durationBaseSpinner = new UpDownSpinner(durationBaseSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
        durationPlusSpinner = new UpDownSpinner(durationPlusSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
        durationPerLevelSpinner = new UpDownSpinner(durationPerLevelSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
        chanceBaseSpinner = new UpDownSpinner(chanceBaseSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
        chancePlusSpinner = new UpDownSpinner(chancePlusSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
        chancePerLevelSpinner = new UpDownSpinner(chancePerLevelSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
        magnitudeBaseMinSpinner = new UpDownSpinner(magnitudeBaseMinSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
        magnitudeBaseMaxSpinner = new UpDownSpinner(magnitudeBaseMaxSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
        magnitudePlusMinSpinner = new UpDownSpinner(magnitudePlusMinSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
        magnitudePlusMaxSpinner = new UpDownSpinner(magnitudePlusMaxSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);
        magnitudePerLevelSpinner = new UpDownSpinner(magnitudePerLevelSpinnerRect, spinnerUpButtonRect, spinnerDownButtonRect, spinnerValueLabelRect, 0, null, NativePanel);

        // Set spinner mouse over colours
        durationBaseSpinner.SetMouseOverBackgroundColor(hotButtonColor);
        durationPlusSpinner.SetMouseOverBackgroundColor(hotButtonColor);
        durationPerLevelSpinner.SetMouseOverBackgroundColor(hotButtonColor);
        chanceBaseSpinner.SetMouseOverBackgroundColor(hotButtonColor);
        chancePlusSpinner.SetMouseOverBackgroundColor(hotButtonColor);
        chancePerLevelSpinner.SetMouseOverBackgroundColor(hotButtonColor);
        magnitudeBaseMinSpinner.SetMouseOverBackgroundColor(hotButtonColor);
        magnitudeBaseMaxSpinner.SetMouseOverBackgroundColor(hotButtonColor);
        magnitudePlusMinSpinner.SetMouseOverBackgroundColor(hotButtonColor);
        magnitudePlusMaxSpinner.SetMouseOverBackgroundColor(hotButtonColor);
        magnitudePerLevelSpinner.SetMouseOverBackgroundColor(hotButtonColor);

        // Set spinner ranges
        durationBaseSpinner.SetRange(0, 60);
        durationPlusSpinner.SetRange(0, 60);
        durationPerLevelSpinner.SetRange(1, 20);
        chanceBaseSpinner.SetRange(0, 100);
        chancePlusSpinner.SetRange(0, 100);
        chancePerLevelSpinner.SetRange(1, 20);
        magnitudeBaseMinSpinner.SetRange(0, 100);
        magnitudeBaseMaxSpinner.SetRange(0, 100);
        magnitudePlusMinSpinner.SetRange(0, 100);
        magnitudePlusMaxSpinner.SetRange(0, 100);
        magnitudePerLevelSpinner.SetRange(1, 20);

        // Set spinner events
        durationBaseSpinner.OnValueChanged += DurationBaseSpinner_OnValueChanged;
        durationPlusSpinner.OnValueChanged += DurationPlusSpinner_OnValueChanged;
        durationPerLevelSpinner.OnValueChanged += DurationPerLevelSpinner_OnValueChanged;
        chanceBaseSpinner.OnValueChanged += ChanceBaseSpinner_OnValueChanged;
        chancePlusSpinner.OnValueChanged += ChancePlusSpinner_OnValueChanged;
        chancePerLevelSpinner.OnValueChanged += ChancePerLevelSpinner_OnValueChanged;
        magnitudeBaseMinSpinner.OnValueChanged += MagnitudeBaseMinSpinner_OnValueChanged;
        magnitudeBaseMaxSpinner.OnValueChanged += MagnitudeBaseMaxSpinner_OnValueChanged;
        magnitudePlusMinSpinner.OnValueChanged += MagnitudePlusMinSpinner_OnValueChanged;
        magnitudePlusMaxSpinner.OnValueChanged += MagnitudePlusMaxSpinner_OnValueChanged;
        magnitudePerLevelSpinner.OnValueChanged += MagnitudePerLevelSpinner_OnValueChanged;
    }

    protected override void UpdateCosts()
    {
        RaiseSettingsChanged();

        // Get spell cost
        (int goldCost, int spellPointCost) = FormulaHelper.CalculateEffectCosts(EffectEntry);
        goldCostLabel.Text = goldCost.ToString();
        spellCostLabel.Text = spellPointCost.ToString();
    }

    #endregion

    #region Private Methods

    void OnConfirm()
    {
        if (!AreEffectAttributesValid())
        {
            DaggerfallMessageBox mb = new DaggerfallMessageBox(uiManager, this);
            mb.SetTextTokens(DaggerfallUnity.TextProvider.CreateTokens(TextFile.Formatting.JustifyLeft, "Effect values are invalid. Do you want to discard?"));
            mb.AddButton(MessageBoxButtons.Yes);
            mb.AddButton(MessageBoxButtons.No);
            mb.OnButtonClick += DiscardWindow_OnButtonClick;
            mb.Show();
        }
        else
        {
            entryValid = true;
            CloseWindow();
        }
    }

    void ConfirmButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
    {
        OnConfirm();
    }

    void DiscardWindow_OnButtonClick(DaggerfallMessageBox sender, MessageBoxButtons messageBoxButton)
    {
        sender.CloseWindow();

        if(messageBoxButton == MessageBoxButtons.Yes)
        {
            entryValid = false;
            CloseWindow();
        }
    }


    void CancelButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
    {
        entryValid = false;
        CloseWindow();
    }

    #endregion
}
