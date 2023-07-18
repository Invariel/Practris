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

public class SettingsScene : MonoBehaviour
{
    // Game Settings
    public UserInput _userInput;
    Settings _settings { get => _userInput.gameSettings; }

    #region Movement UI Controls
    public TMP_InputField Up_inputField;
    public TMP_Text Up_tooltipField;
    public Button Up_tooltipButton;

    public TMP_InputField Down_inputField;
    public TMP_Text Down_tooltipField;
    public Button Down_tooltipButton;

    public TMP_InputField Left_inputField;
    public TMP_Text Left_tooltipField;
    public Button Left_tooltipButton;

    public TMP_InputField Right_inputField;
    public TMP_Text Right_tooltipField;
    public Button Right_tooltipButton;
    #endregion

    #region Rotation UI Controls
    public TMP_InputField RotateLeft_inputField;
    public TMP_Text RotateLeft_tooltipField;
    public Button RotateLeft_tooltipButton;

    public TMP_InputField RotateRight_inputField;
    public TMP_Text RotateRight_tooltipField;
    public Button RotateRight_tooltipButton;

    public TMP_InputField ShadowLeft_inputField;
    public TMP_Text ShadowLeft_tooltipField;
    public Button ShadowLeft_tooltipButton;

    public TMP_InputField ShadowRight_inputField;
    public TMP_Text ShadowRight_tooltipField;
    public Button ShadowRight_tooltipButton;
    #endregion

    #region Time Travel UI Controls
    public TMP_InputField Rewind_inputField;
    public TMP_Text Rewind_tooltipField;
    public Button Rewind_tooltipButton;

    public TMP_InputField Forward_inputField;
    public TMP_Text Forward_tooltipField;
    public Button Forward_tooltipButton;
    #endregion

    #region Other Controls
    public TMP_InputField Hold_inputField;
    public TMP_Text Hold_tooltipField;
    public Button Hold_tooltipButton;

    public TMP_InputField Accept_inputField;
    public TMP_Text Accept_tooltipField;
    public Button Accept_tooltipButton;

    public TMP_InputField Menu_inputField;
    public TMP_Text Menu_tooltipField;
    public Button Menu_tooltipButton;
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
