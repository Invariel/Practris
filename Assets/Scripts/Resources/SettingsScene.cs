using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

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

    [SerializeField] private TMP_Dropdown drp_Style;


    // Start is called before the first frame update
    void Start()
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

    public void FillDropDown(TMP_Dropdown dropdown)
    {
        string[] styles = Directory.GetDirectories("./Styles/");
        drp_Style.AddOptions(styles.ToList<string>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReceiveInput()
    {

    }
}
