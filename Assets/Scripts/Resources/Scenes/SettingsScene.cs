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
using System.Windows.Forms;
using Assets.Scripts.Resources;

public class SettingsScene : MonoBehaviour
{
    // Game Settings
    public UserInput _userInput;

    public bool binding = false;
    public bool justClicked = false;
    public TMP_InputField bindingInputField = null;

    Settings _settings { get => _userInput.gameSettings; }

    #region Movement UI Controls
    public GameObject go_Up;
    public GameObject go_Down;
    public GameObject go_Left;
    public GameObject go_Right;
    #endregion

    #region Rotation UI Controls
    public GameObject go_SpinLeft;
    public GameObject go_SpinRight;
    public GameObject go_RotationLeft;
    public GameObject go_RotationRight;
    #endregion

    #region Time Travel UI Controls
    public GameObject go_Rewind;
    public GameObject go_Forward;
    #endregion

    #region Other Controls
    public GameObject go_HoldPiece;
    public GameObject go_Accept;
    public GameObject go_Menu;
    #endregion

    public GameObject BindingTextbox;
    public GameObject SaveTextbox;

    int saveTextboxCooldown = 0;

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

    private List<Tuple<GameObject, string>> settingsMapping = new List<Tuple<GameObject, string>>();

    // Start is called before the first frame update
    void Start()
    {
        InitialSettings();
    }

    public void InitialSettings()
    {
        _userInput.LoadSettingsFromFile();

        SetupMenu();

        InputBinding._scene = this;
        ClearButton._scene = this;

        ConfigureContainer(go_Up, name: "Up", settingName: Constants.SettingsField.Up, tooltipText: "The button(s) used to move the piece up.");
        ConfigureContainer(go_Down, name: "Down", settingName: Constants.SettingsField.Down, tooltipText: "The button(s) used to move the piece down.  Moving a piece down into a blocked space will lock it.");
        ConfigureContainer(go_Left, name: "Left", settingName: Constants.SettingsField.Left, tooltipText: "The button(s) used to move the piece to the left.");
        ConfigureContainer(go_Right, name: "Right", settingName: Constants.SettingsField.Right, tooltipText: "The button(s) used to move the piece to the right.");

        ConfigureContainer(go_SpinLeft, name: "Rotate Left", settingName: Constants.SettingsField.SpinLeft, tooltipText: "The button(s) used to rotate the piece to the left (counter-clockwise).");
        ConfigureContainer(go_SpinRight, name: "Rotate Right", settingName: Constants.SettingsField.SpinRight, tooltipText: "The button(s) used to rotate the piece to the right (clockwise).");
        ConfigureContainer(go_RotationLeft, name: "Shadow Left", settingName: Constants.SettingsField.RotationLeft, tooltipText: "The button(s) used to cycle through the checked counter-clockwise rotation positions.");
        ConfigureContainer(go_RotationRight, name: "Shadow Right", settingName: Constants.SettingsField.RotationRight, tooltipText: "The button(s) used to cycle through the checked clockwise rotation positions.");

        ConfigureContainer(go_Rewind, name: "Rewind", settingName: Constants.SettingsField.Rewind, tooltipText: "The button(s) used to rewind time by one step.  Press \"Accept\" to resume from the chosen point.");
        ConfigureContainer(go_Forward, name: "Forward", settingName: Constants.SettingsField.Forward, tooltipText: "The button(s) used to advance time by one step.  Press \"Accept\" to resume from the chosen point.");

        ConfigureContainer(go_HoldPiece, name: "Hold Piece", settingName: Constants.SettingsField.HoldPiece, tooltipText: "The button(s) used to hold the current piece, swapping with the current held piece if one exists.");
        ConfigureContainer(go_Accept, name: "Accept / Drop", settingName: Constants.SettingsField.Accept, tooltipText: "The button(s) used to accept the game state, and also to fast-drop the current piece.");
        ConfigureContainer(go_Menu, name: "Cancel / Menu", settingName: Constants.SettingsField.Menu, tooltipText: "The button(s) used to cancel out of the session and to back out of menus.");

        ConfigureInputFieldMapping();
    }

    public void ConfigureContainer (GameObject container, Constants.SettingsField settingName, string name, string tooltipText)
    {
        container.GetComponent<ContainerInformation>()._myself = container;
        container.GetComponent<ContainerInformation>()._settingName = settingName;
        container.GetComponent<ContainerInformation>()._name = name;
        ConfigureTooltip(container, tooltipText);
    }

    public void ConfigureInputFieldMapping()
    {
        settingsMapping.Clear();

        settingsMapping.Add(new Tuple<GameObject, string>(go_Up, "Up"));
        settingsMapping.Add(new Tuple<GameObject, string>(go_Down, "Down"));
        settingsMapping.Add(new Tuple<GameObject, string>(go_Left, "Left"));
        settingsMapping.Add(new Tuple<GameObject, string>(go_Right, "Right"));

        settingsMapping.Add(new Tuple<GameObject, string>(go_SpinLeft, "Rotate Left"));
        settingsMapping.Add(new Tuple<GameObject, string>(go_SpinRight, "Rotate Right"));
        settingsMapping.Add(new Tuple<GameObject, string>(go_RotationLeft, "Shadow Left"));
        settingsMapping.Add(new Tuple<GameObject, string>(go_RotationRight, "Shadow Right"));

        settingsMapping.Add(new Tuple<GameObject, string>(go_Rewind, "Rewind"));
        settingsMapping.Add(new Tuple<GameObject, string>(go_Forward, "Forward"));

        settingsMapping.Add(new Tuple<GameObject, string>(go_HoldPiece, "Hold Piece"));
        settingsMapping.Add(new Tuple<GameObject, string>(go_Accept, "Drop / Accept"));

        // Intentionally not letting Menu/Back/Cancel be remapped.  Sorry everyone.
    }

    public void ConfigureTooltip (GameObject container, string tooltipText)
    {
        var tooltipButton = container.GetComponentInChildren<TooltipData>().GetComponentInParent<Button>();

        var tooltipData = tooltipButton.GetComponent<TooltipData>();
        tooltipData._myself = tooltipButton;

        tooltipData.container = container;
        tooltipData._tooltip = container.GetComponent<ContainerInformation>().GetTooltipText();
        tooltipData._image = container.GetComponent<ContainerInformation>().GetTooltipImage();
        tooltipData._tooltipText = tooltipText;

        tooltipData.ConfigureEvents();
    }

    public void SetupMenu()
    {
        InitialAssignKeys(go_Up, _settings.Up);
        InitialAssignKeys(go_Down, _settings.Down);
        InitialAssignKeys(go_Left, _settings.Left);
        InitialAssignKeys(go_Right, _settings.Right);

        InitialAssignKeys(go_SpinLeft, _settings.SpinLeft);
        InitialAssignKeys(go_SpinRight, _settings.SpinRight);
        InitialAssignKeys(go_RotationLeft, _settings.RotationLeft);
        InitialAssignKeys(go_RotationRight, _settings.RotationRight);

        InitialAssignKeys(go_Rewind, _settings.Rewind);
        InitialAssignKeys(go_Forward, _settings.Forward);

        InitialAssignKeys(go_HoldPiece, _settings.HoldPiece);
        InitialAssignKeys(go_Accept, _settings.Accept);
        InitialAssignKeys(go_Menu, _settings.Menu);

        FillDropDown(drp_Style);
        AssignDropDown(drp_Style, _settings.Style);
    }

    public void InitialAssignKeys(GameObject container, KeyCode[] keys)
    {
        TMP_InputField inputField = container.GetComponentInChildren<TMP_InputField>();

        string input = "";

        foreach (var key in keys)
        {
            input = $"{input} {key},";
        }

        if (input.Length > 0)
        {
            input = input.Substring(0, input.Length - 1);
        }

        if (inputField is not null)
            inputField.text = input;
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
        List<string> emptyFields = new List<string>();

        emptyFields.Add(_settings.Up.Any() ? null : "Up");
        emptyFields.Add(_settings.Down.Any() ? null : "Down");
        emptyFields.Add(_settings.Left.Any() ? null : "Left");
        emptyFields.Add(_settings.Right.Any() ? null : "Right");

        emptyFields.Add(_settings.SpinLeft.Any() ? null : "Rotate Left");
        emptyFields.Add(_settings.SpinRight.Any() ? null : "Rotate Right");
        emptyFields.Add(_settings.RotationLeft.Any() ? null : "Shadow Left");
        emptyFields.Add(_settings.RotationRight.Any() ? null : "Shadow Right");

        emptyFields.Add(_settings.Rewind.Any() ? null : "Rewind");
        emptyFields.Add(_settings.Forward.Any() ? null : "Forward");

        emptyFields.Add(_settings.HoldPiece.Any() ? null : "Hold Piece");
        emptyFields.Add(_settings.Accept.Any() ? null : "Drop / Accept");
        emptyFields.Add(_settings.Menu.Any() ? null : "Back / Cancel");

        emptyFields.RemoveAll(e => e is null);

        if (!emptyFields.Any())
        {
            _userInput.SaveSettingsToFile();

            SaveTextbox.GetComponentInChildren<TMP_Text>().text = "Settings saved.";
        }
        else
        {
            SaveTextbox.GetComponentInChildren<TMP_Text>().text = $"The following field{(emptyFields.Count == 1 ? " is" : "s are")} empty: {string.Join(", ", emptyFields)}";
        }

        SaveTextbox.GetComponentInChildren<TMP_Text>().enabled = true;
        foreach (Image image in SaveTextbox.GetComponentsInChildren<Image>())
        {
            image.enabled = true;
        }

        saveTextboxCooldown = 600;
    }

    public void StartInputBinding(Button whichButton)
    {
        TMP_Text bindingTextbox = BindingTextbox.GetComponentInChildren<TMP_Text>();
        foreach (Image image in BindingTextbox.GetComponentsInChildren<Image>())
        {
            image.enabled = true;
        }

        bindingTextbox.text = $"Press a key to bind to {whichButton.GetComponentInParent<ContainerInformation>()._name}.";
        bindingTextbox.enabled = true;

        binding = true;
        justClicked = true;
        bindingInputField = whichButton.GetComponentInParent<ContainerInformation>().GetInputField();
    }

    // Update is called once per frame
    void Update()
    {
        if (saveTextboxCooldown == 0)
        {
            SaveTextbox.GetComponentInChildren<TMP_Text>().enabled = false;
            foreach (Image image in SaveTextbox.GetComponentsInChildren<Image>())
            {
                image.enabled = false;
            }
        }

        if (saveTextboxCooldown >= 0)
        {
            --saveTextboxCooldown;
        }

        int pressedKeys = _userInput.currentKeysPressed;

        if (justClicked)
        {
            if (!Input.anyKey)
            {
                justClicked = false;
            }
        }

        if (UserInput.TestKey(KeyPressed.Menu, pressedKeys))
        {
            if (binding)
            {
                BindingComplete();
            }
            else
            {
                SceneManager.LoadSceneAsync(Constants.GetScene(Constants.Scene.TITLE));
            }
        }
        else if (binding && !justClicked)
        {
            ReceiveInput();
        }
    }

    public void ReceiveInput()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    if (!_settings.Menu.Contains(key))
                    {
                        DeleteKey(key);
                        AssignNewKey(key);
                        BindingComplete();
                    }
                }
            }
        }
    }

    public void ClearKeys (Constants.SettingsField setting)
    {
        ClearSettings(setting);
        BindingComplete();
    }

    public void AssignNewKey(KeyCode key)
    {
        List<KeyCode> keys;

        var container = bindingInputField.GetComponentInParent<ContainerInformation>()._myself;

        switch (container.GetComponent<ContainerInformation>()._settingName)
        {
            case Constants.SettingsField.Up:
                keys = _settings.Up.ToList();
                keys.Add(key);
                _settings.Up = keys.Distinct().ToArray();
                break;

            case Constants.SettingsField.Down:
                keys = _settings.Down.ToList();
                keys.Add(key);
                _settings.Down = keys.Distinct().ToArray();
                break;

            case Constants.SettingsField.Left:
                keys = _settings.Left.ToList();
                keys.Add(key);
                _settings.Left = keys.Distinct().ToArray();
                break;

            case Constants.SettingsField.Right:
                keys = _settings.Right.ToList();
                keys.Add(key);
                _settings.Right = keys.Distinct().ToArray();
                break;



            case Constants.SettingsField.SpinLeft:
                keys = _settings.SpinLeft.ToList();
                keys.Add(key);
                _settings.SpinLeft = keys.Distinct().ToArray();
                break;

            case Constants.SettingsField.SpinRight:
                keys = _settings.SpinRight.ToList();
                keys.Add(key);
                _settings.SpinRight = keys.Distinct().ToArray();
                break;

            case Constants.SettingsField.RotationLeft:
                keys = _settings.RotationLeft.ToList();
                keys.Add(key);
                _settings.RotationLeft = keys.Distinct().ToArray();
                break;

            case Constants.SettingsField.RotationRight:
                keys = _settings.RotationRight.ToList();
                keys.Add(key);
                _settings.RotationRight = keys.Distinct().ToArray();
                break;



            case Constants.SettingsField.Rewind:
                keys = _settings.Rewind.ToList();
                keys.Add(key);
                _settings.Rewind = keys.Distinct().ToArray();
                break;

            case Constants.SettingsField.Forward:
                keys = _settings.Forward.ToList();
                keys.Add(key);
                _settings.Forward = keys.Distinct().ToArray();
                break;



            case Constants.SettingsField.HoldPiece:
                keys = _settings.HoldPiece.ToList();
                keys.Add(key);
                _settings.HoldPiece = keys.Distinct().ToArray();
                break;

            case Constants.SettingsField.Accept:
                keys = _settings.Accept.ToList();
                keys.Add(key);
                _settings.Accept = keys.Distinct().ToArray();
                break;

            case Constants.SettingsField.Menu:
                keys = _settings.Menu.ToList();
                keys.Add(key);
                _settings.Menu = keys.Distinct().ToArray();
                break;
        }
    }

    public void DeleteKey (KeyCode key)
    {
        List<KeyCode> keys;

        keys = _settings.Up.ToList();
        keys.Remove(key);
        _settings.Up = keys.Distinct().ToArray();

        keys = _settings.Down.ToList();
        keys.Remove(key);
        _settings.Down = keys.Distinct().ToArray();

        keys = _settings.Left.ToList();
        keys.Remove(key);
        _settings.Left = keys.Distinct().ToArray();

        keys = _settings.Right.ToList();
        keys.Remove(key);
        _settings.Right = keys.Distinct().ToArray();



        keys = _settings.SpinLeft.ToList();
        keys.Remove(key);
        _settings.SpinLeft = keys.Distinct().ToArray();

        keys = _settings.SpinRight.ToList();
        keys.Remove(key);
        _settings.SpinRight = keys.Distinct().ToArray();

        keys = _settings.RotationLeft.ToList();
        keys.Remove(key);
        _settings.RotationLeft = keys.Distinct().ToArray();

        keys = _settings.RotationRight.ToList();
        keys.Remove(key);
        _settings.RotationRight = keys.Distinct().ToArray();



        keys = _settings.Rewind.ToList();
        keys.Remove(key);
        _settings.Rewind = keys.Distinct().ToArray();

        keys = _settings.Forward.ToList();
        keys.Remove(key);
        _settings.Forward = keys.Distinct().ToArray();



        keys = _settings.HoldPiece.ToList();
        keys.Remove(key);
        _settings.HoldPiece = keys.Distinct().ToArray();

        keys = _settings.Accept.ToList();
        keys.Remove(key);
        _settings.Accept = keys.Distinct().ToArray();
    }

    public void ClearSettings (Constants.SettingsField setting)
    {
        switch (setting)
        {
            case Constants.SettingsField.Up:
                _settings.Up = new KeyCode[] { };
                break;
            case Constants.SettingsField.Down:
                _settings.Down = new KeyCode[] { };
                break;
            case Constants.SettingsField.Left:
                _settings.Left = new KeyCode[] { };
                break;
            case Constants.SettingsField.Right:
                _settings.Right = new KeyCode[] { };
                break;
            case Constants.SettingsField.SpinLeft:
                _settings.SpinLeft = new KeyCode[] { };
                break;
            case Constants.SettingsField.SpinRight:
                _settings.SpinRight = new KeyCode[] { };
                break;
            case Constants.SettingsField.RotationLeft:
                _settings.RotationLeft = new KeyCode[] { };
                break;
            case Constants.SettingsField.RotationRight:
                _settings.RotationRight = new KeyCode[] { };
                break;
            case Constants.SettingsField.Rewind:
                _settings.Rewind = new KeyCode[] { };
                break;
            case Constants.SettingsField.Forward:
                _settings.Forward = new KeyCode[] { };
                break;
            case Constants.SettingsField.HoldPiece:
                _settings.HoldPiece = new KeyCode[] { };
                break;
            case Constants.SettingsField.Accept:
                _settings.Accept = new KeyCode[] { };
                break;
        }
    }

    public void BindingComplete()
    {
        binding = false;
        bindingInputField = null;

        BindingTextbox.GetComponentInChildren<TMP_Text>().text = "";
        BindingTextbox.GetComponentInChildren<TMP_Text>().enabled = false;
        foreach (Image image in BindingTextbox.GetComponentsInChildren<Image>())
        {
            image.enabled = false;
        }

        SetupMenu();
    }
}
