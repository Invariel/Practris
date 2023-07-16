using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class SettingsScene : MonoBehaviour
{
    // Game Settings
    public UserInput _userInput;
    Settings _settings { get => _userInput.gameSettings; }

    // Movement
    [SerializeField] private TMP_InputField input_Up;
    [SerializeField] private TMP_InputField input_Down;
    [SerializeField] private TMP_InputField input_Left;
    [SerializeField] private TMP_InputField input_Right;

    // Rotation
    [SerializeField] private TMP_InputField input_RotateLeft;
    [SerializeField] private TMP_InputField input_RotateRight;
    [SerializeField] private TMP_InputField input_ShadowLeft;
    [SerializeField] private TMP_InputField input_ShadowRight;

    // Other Controls

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
        AssignKeys(input_Up, _settings.Up);
        AssignKeys(input_Down, _settings.Down);
        AssignKeys(input_Left, _settings.Left);
        AssignKeys(input_Right, _settings.Right);

        AssignKeys(input_RotateLeft, _settings.SpinLeft);
        AssignKeys(input_RotateRight, _settings.SpinRight);
        AssignKeys(input_ShadowLeft, _settings.RotationLeft);
        AssignKeys(input_ShadowRight, _settings.RotationRight);

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
