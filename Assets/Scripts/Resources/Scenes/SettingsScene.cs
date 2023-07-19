using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Button = UnityEngine.UI.Button;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Unity.VisualScripting;

public class SettingsScene : MonoBehaviour
{
    // Game Settings
    public UserInput _userInput;
    Settings _settings { get => _userInput.gameSettings; }

    #region Movement UI Controls
    public TMP_InputField Up_inputField;
    public Button Up_tooltipButton;
    public GameObject Up_tooltipContainer;

    public TMP_InputField Down_inputField;
    public Button Down_tooltipButton;
    public GameObject Down_tooltipContainer;

    public TMP_InputField Left_inputField;
    public Button Left_tooltipButton;
    public GameObject Left_tooltipContainer;

    public TMP_InputField Right_inputField;
    public Button Right_tooltipButton;
    public GameObject Right_tooltipContainer;
    #endregion

    #region Rotation UI Controls
    public TMP_InputField RotateLeft_inputField;
    public Button RotateLeft_tooltipButton;
    public GameObject RotateLeft_tooltipContainer;

    public TMP_InputField RotateRight_inputField;
    public Button RotateRight_tooltipButton;
    public GameObject RotateRight_tooltipContainer;

    public TMP_InputField ShadowLeft_inputField;
    public Button ShadowLeft_tooltipButton;
    public GameObject ShadowLeft_tooltipContainer;

    public TMP_InputField ShadowRight_inputField;
    public Button ShadowRight_tooltipButton;
    public GameObject ShadowRight_tooltipContainer;
    #endregion

    #region Time Travel UI Controls
    public TMP_InputField Rewind_inputField;
    public Button Rewind_tooltipButton;
    public GameObject Rewind_tooltipContainer;

    public TMP_InputField Forward_inputField;
    public Button Forward_tooltipButton;
    public GameObject Forward_tooltipContainer;
    #endregion

    #region Other Controls
    public TMP_InputField Hold_inputField;
    public Button Hold_tooltipButton;
    public GameObject Hold_tooltipContainer;

    public TMP_InputField Accept_inputField;
    public Button Accept_tooltipButton;
    public GameObject Accept_tooltipContainer;

    public TMP_InputField Menu_inputField;
    public Button Menu_tooltipButton;
    public GameObject Menu_tooltipContainer;
    #endregion

    // Style
    [SerializeField] private TMP_Dropdown drp_Style;
    [SerializeField] private Image img_mino_Empty;
    [SerializeField] private Image img_mino_Border;
    [SerializeField] private Image img_mino_Preset;
    [SerializeField] private Image img_mino_I;
    [SerializeField] private Image img_mino_J;
    [SerializeField] private Image img_mino_L;
    [SerializeField] private Image img_mino_O;
    [SerializeField] private Image img_mino_S;
    [SerializeField] private Image img_mino_T;
    [SerializeField] private Image img_mino_Z;


    // Start is called before the first frame update
    void Start()
    {
        InitialSettings();
    }

    public void InitialSettings()
    {
        _userInput.LoadSettingsFromFile();

        SetupMenu();

        ConfigureButton(Up_tooltipButton, Up_tooltipContainer, "The button(s) used to move the piece up.");
        ConfigureButton(Down_tooltipButton, Down_tooltipContainer, "The button(s) used to move the piece down.  Moving a piece down into a blocked space will lock it.");
        ConfigureButton(Left_tooltipButton, Left_tooltipContainer, "The button(s) used to move the piece to the left.");
        ConfigureButton(Right_tooltipButton, Right_tooltipContainer, "The button(s) used to move the piece to the right.");

        ConfigureButton(RotateLeft_tooltipButton, RotateLeft_tooltipContainer, "The button(s) used to rotate the piece to the left (counter-clockwise).");
        ConfigureButton(RotateRight_tooltipButton, RotateRight_tooltipContainer, "The button(s) used to rotate the piece to the right (clockwise).");
        ConfigureButton(ShadowLeft_tooltipButton, ShadowLeft_tooltipContainer, "The button(s) used to cycle through the checked counter-clockwise rotation positions.");
        ConfigureButton(ShadowRight_tooltipButton, ShadowRight_tooltipContainer, "The button(s) used to cycle through the checked clockwise rotation positions.");

        ConfigureButton(Rewind_tooltipButton, Rewind_tooltipContainer, "The button(s) used to rewind time by one step.  Press \"Accept\" to resume from the chosen point.");
        ConfigureButton(Forward_tooltipButton, Forward_tooltipContainer, "The button(s) used to advance time by one step.  Press \"Accept\" to resume from the chosen point.");

        ConfigureButton(Hold_tooltipButton, Hold_tooltipContainer, "The button(s) used to hold the current piece, swapping with the current held piece if one exists.");
        ConfigureButton(Accept_tooltipButton, Accept_tooltipContainer, "The button(s) used to accept the game state, and also to fast-drop the current piece.");
        ConfigureButton(Menu_tooltipButton, Menu_tooltipContainer, "The button(s) used to cancel out of the session and to back out of menus.");
    }

    public void ConfigureButton (Button button, GameObject tooltipContainer, string tooltipText)
    {
        var tooltipData = button.GetComponent<TooltipData>();
        tooltipData.myself = button;

        tooltipData.tooltip = tooltipContainer.GetComponentInChildren<TMP_Text>();
        tooltipData.image = tooltipContainer.GetComponentInChildren<Image>();
        tooltipData.tooltipText = tooltipText;

        tooltipData.ConfigureEvents();
    }

    public void SetupMenu()
    {
        AssignKeys(Up_inputField, _settings.Up);
        AssignKeys(Down_inputField, _settings.Down);
        AssignKeys(Left_inputField, _settings.Left);
        AssignKeys(Right_inputField, _settings.Right);

        AssignKeys(RotateLeft_inputField, _settings.SpinLeft);
        AssignKeys(RotateRight_inputField, _settings.SpinRight);
        AssignKeys(ShadowLeft_inputField, _settings.RotationLeft);
        AssignKeys(ShadowRight_inputField, _settings.RotationRight);

        AssignKeys(Rewind_inputField, _settings.Rewind);
        AssignKeys(Forward_inputField, _settings.Forward);

        AssignKeys(Hold_inputField, _settings.HoldPiece);
        AssignKeys(Accept_inputField, _settings.Accept);
        AssignKeys(Menu_inputField, _settings.Menu);

        FillDropDown(drp_Style);
        AssignDropDown(drp_Style, _settings.Style);
    }

    public void AssignKeys(TMP_InputField textField, KeyCode[] keys)
    {
        string input = "";

        foreach (var key in keys)
        {
            input = $"{input} {key},";
        }

        input = input.Substring(0, input.Length - 1);

        if (textField is not null)
            textField.text = input;
    }

    public void FillDropDown(TMP_Dropdown dropdown)
    {
        dropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> optionData = new List<TMP_Dropdown.OptionData>();

        foreach (string style in Constants._resourceStyles)
        {
            optionData.Add(new TMP_Dropdown.OptionData
            {
                text = style,
                image = Mino.GetSprite(MinoEnum.Empty, style)
            });
        }

        dropdown.AddOptions(optionData);

        /*
            string[] styles = Directory.GetDirectories("./Styles/");
            drp_Style.AddOptions(styles.ToList<string>());
        */
    }

    public void AssignDropDown(TMP_Dropdown dropdown, string style)
    {
        TMP_Dropdown.OptionData dropdownValue = dropdown.options.FirstOrDefault(data => data.text.Equals(style, StringComparison.OrdinalIgnoreCase));
        if (dropdownValue != null)
        {
            dropdown.value = dropdown.options.IndexOf(dropdownValue);
        }
    }

    public void ChangedStyle()
    {
        _settings.Style = drp_Style.options[drp_Style.value].text;

        UpdateMinoPreview(_settings.Style);
    }

    public void UpdateMinoPreview (string style)
    {
        img_mino_Empty.sprite = Mino.GetSprite(MinoEnum.Empty, style);
        img_mino_Border.sprite = Mino.GetSprite(MinoEnum.Border, style);
        img_mino_Preset.sprite = Mino.GetSprite(MinoEnum.Preset, style);
        img_mino_I.sprite = Mino.GetSprite(MinoEnum.I, style);
        img_mino_J.sprite = Mino.GetSprite(MinoEnum.J, style);
        img_mino_L.sprite = Mino.GetSprite(MinoEnum.L, style);
        img_mino_O.sprite = Mino.GetSprite(MinoEnum.O, style);
        img_mino_S.sprite = Mino.GetSprite(MinoEnum.S, style);
        img_mino_T.sprite = Mino.GetSprite(MinoEnum.T, style);
        img_mino_Z.sprite = Mino.GetSprite(MinoEnum.Z, style);
    }

    public void SaveSettings()
    {
        _userInput.SaveSettingsToFile();
    }

    // Update is called once per frame
    void Update()
    {
        int pressedKeys = _userInput.currentKeysPressed;
        if (UserInput.TestKey(KeyPressed.Menu, pressedKeys))
        {
            SceneManager.LoadSceneAsync(Constants.GetScene(Constants.Scene.TITLE));
        }
    }

    public void ReceiveInput()
    {

    }
}
