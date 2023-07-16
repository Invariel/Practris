using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsScene : MonoBehaviour
{
    // Game Settings
    public UserInput _userInput;
    Settings _settings { get => _userInput.gameSettings; }

    // Game state stuff.
    [SerializeField] private TMP_InputField input_Up;
    [SerializeField] private TMP_InputField input_Down;
    [SerializeField] private TMP_InputField input_Left;
    [SerializeField] private TMP_InputField input_Right;

    [SerializeField] private TMP_Dropdown drp_Style_TMP;
    [SerializeField] private Dropdown drp_Style;


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

        FillDropDown(drp_Style);
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

    public void FillDropDown(Dropdown dropdown)
    {
        drp_Style.ClearOptions();

        drp_Style.AddOptions(Constants._resourceStyles.ToList());

        /*
            string[] styles = Directory.GetDirectories("./Styles/");
            drp_Style.AddOptions(styles.ToList<string>());
        */
    }

    public void ChangedStyle()
    {
        _settings.Style = drp_Style.options[drp_Style.value].text;
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
