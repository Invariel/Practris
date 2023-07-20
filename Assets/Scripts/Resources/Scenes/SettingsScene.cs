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
    public TMP_InputField Up_inputField;
    public Button Up_tooltipButton;
    public GameObject Up_tooltipContainer;
    public Button Up_bindButton;

    public TMP_InputField Down_inputField;
    public Button Down_tooltipButton;
    public GameObject Down_tooltipContainer;
    public Button Down_bindButton;

    public TMP_InputField Left_inputField;
    public Button Left_tooltipButton;
    public GameObject Left_tooltipContainer;
    public Button Left_bindButton;

    public TMP_InputField Right_inputField;
    public Button Right_tooltipButton;
    public GameObject Right_tooltipContainer;
    public Button Right_bindButton;
    #endregion

    #region Rotation UI Controls
    public TMP_InputField RotateLeft_inputField;
    public Button RotateLeft_tooltipButton;
    public GameObject RotateLeft_tooltipContainer;
    public Button RotateLeft_bindButton;

    public TMP_InputField RotateRight_inputField;
    public Button RotateRight_tooltipButton;
    public GameObject RotateRight_tooltipContainer;
    public Button RotateRight_bindButton;

    public TMP_InputField ShadowLeft_inputField;
    public Button ShadowLeft_tooltipButton;
    public GameObject ShadowLeft_tooltipContainer;
    public Button ShadowLeft_bindButton;

    public TMP_InputField ShadowRight_inputField;
    public Button ShadowRight_tooltipButton;
    public GameObject ShadowRight_tooltipContainer;
    public Button ShadowRight_bindButton;
    #endregion

    #region Time Travel UI Controls
    public TMP_InputField Rewind_inputField;
    public Button Rewind_tooltipButton;
    public GameObject Rewind_tooltipContainer;
    public Button Rewind_bindButton;

    public TMP_InputField Forward_inputField;
    public Button Forward_tooltipButton;
    public GameObject Forward_tooltipContainer;
    public Button Forward_bindButton;
    #endregion

    #region Other Controls
    public TMP_InputField Hold_inputField;
    public Button Hold_tooltipButton;
    public GameObject Hold_tooltipContainer;
    public Button Hold_bindButton;

    public TMP_InputField Accept_inputField;
    public Button Accept_tooltipButton;
    public GameObject Accept_tooltipContainer;
    public Button Accept_bindButton;

    public TMP_InputField Menu_inputField;
    public Button Menu_tooltipButton;
    public GameObject Menu_tooltipContainer;
    public Button Menu_bindButton;
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

    private List<Tuple<Button, TMP_InputField, KeyCode[], string>> settingsMapping = new List<Tuple<Button, TMP_InputField, KeyCode[], string>>();

    // Start is called before the first frame update
    void Start()
    {
        InitialSettings();
    }

    public void InitialSettings()
    {
        _userInput.LoadSettingsFromFile();

        SetupMenu();

        InputBinding.scene = this;

        ConfigureTooltip(Up_tooltipButton, Up_tooltipContainer, "The button(s) used to move the piece up.");
        ConfigureTooltip(Down_tooltipButton, Down_tooltipContainer, "The button(s) used to move the piece down.  Moving a piece down into a blocked space will lock it.");
        ConfigureTooltip(Left_tooltipButton, Left_tooltipContainer, "The button(s) used to move the piece to the left.");
        ConfigureTooltip(Right_tooltipButton, Right_tooltipContainer, "The button(s) used to move the piece to the right.");

        ConfigureTooltip(RotateLeft_tooltipButton, RotateLeft_tooltipContainer, "The button(s) used to rotate the piece to the left (counter-clockwise).");
        ConfigureTooltip(RotateRight_tooltipButton, RotateRight_tooltipContainer, "The button(s) used to rotate the piece to the right (clockwise).");
        ConfigureTooltip(ShadowLeft_tooltipButton, ShadowLeft_tooltipContainer, "The button(s) used to cycle through the checked counter-clockwise rotation positions.");
        ConfigureTooltip(ShadowRight_tooltipButton, ShadowRight_tooltipContainer, "The button(s) used to cycle through the checked clockwise rotation positions.");

        ConfigureTooltip(Rewind_tooltipButton, Rewind_tooltipContainer, "The button(s) used to rewind time by one step.  Press \"Accept\" to resume from the chosen point.");
        ConfigureTooltip(Forward_tooltipButton, Forward_tooltipContainer, "The button(s) used to advance time by one step.  Press \"Accept\" to resume from the chosen point.");

        ConfigureTooltip(Hold_tooltipButton, Hold_tooltipContainer, "The button(s) used to hold the current piece, swapping with the current held piece if one exists.");
        ConfigureTooltip(Accept_tooltipButton, Accept_tooltipContainer, "The button(s) used to accept the game state, and also to fast-drop the current piece.");
        ConfigureTooltip(Menu_tooltipButton, Menu_tooltipContainer, "The button(s) used to cancel out of the session and to back out of menus.");

        foreach (Button button in new[] { Up_bindButton, Down_bindButton, Left_bindButton, Right_bindButton,
            RotateLeft_bindButton, RotateRight_bindButton, ShadowLeft_bindButton, ShadowRight_bindButton,
            Rewind_bindButton, Forward_bindButton, Hold_bindButton, Accept_bindButton })
        {
            ConfigureBindButton(button);
        }

        ConfigureInputFieldMapping();
    }

    public void ConfigureInputFieldMapping()
    {
        settingsMapping.Clear();

        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(Up_bindButton, Up_inputField, _settings.Up, "Up"));
        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(Down_bindButton, Down_inputField, _settings.Down, "Down"));
        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(Left_bindButton, Left_inputField, _settings.Left, "Left"));
        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(Right_bindButton, Right_inputField, _settings.Right, "Right"));

        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(RotateLeft_bindButton, RotateLeft_inputField, _settings.SpinLeft, "Rotate Left"));
        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(RotateRight_bindButton, RotateRight_inputField, _settings.SpinRight, "Rotate Right"));
        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(ShadowLeft_bindButton, ShadowLeft_inputField, _settings.RotationLeft, "Shadow Left"));
        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(ShadowRight_bindButton, ShadowRight_inputField, _settings.RotationRight, "Shadow Right"));

        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(Rewind_bindButton, Rewind_inputField, _settings.Rewind, "Rewind"));
        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(Forward_bindButton, Forward_inputField, _settings.Forward, "Forward"));

        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(Hold_bindButton, Hold_inputField, _settings.HoldPiece, "Hold"));
        settingsMapping.Add(new Tuple<Button, TMP_InputField, KeyCode[], string>(Accept_bindButton, Accept_inputField, _settings.Accept, "Drop / Accept"));

        // Intentionally not letting Menu/Back/Cancel be remapped.  Sorry everyone.
    }

    public void ConfigureTooltip (Button button, GameObject tooltipContainer, string tooltipText)
    {
        var tooltipData = button.GetComponent<TooltipData>();
        tooltipData.myself = button;

        tooltipData.tooltip = tooltipContainer.GetComponentInChildren<TMP_Text>();
        tooltipData.image = tooltipContainer.GetComponentInChildren<Image>();
        tooltipData.tooltipText = tooltipText;

        tooltipData.ConfigureEvents();
    }

    public void ConfigureBindButton(Button button)
    {
        var inputBinding = button.GetComponent<InputBinding>();
        inputBinding.myself = button;
    }

    public void SetupMenu()
    {
        InitialAssignKeys(Up_inputField, _settings.Up);
        InitialAssignKeys(Down_inputField, _settings.Down);
        InitialAssignKeys(Left_inputField, _settings.Left);
        InitialAssignKeys(Right_inputField, _settings.Right);

        InitialAssignKeys(RotateLeft_inputField, _settings.SpinLeft);
        InitialAssignKeys(RotateRight_inputField, _settings.SpinRight);
        InitialAssignKeys(ShadowLeft_inputField, _settings.RotationLeft);
        InitialAssignKeys(ShadowRight_inputField, _settings.RotationRight);

        InitialAssignKeys(Rewind_inputField, _settings.Rewind);
        InitialAssignKeys(Forward_inputField, _settings.Forward);

        InitialAssignKeys(Hold_inputField, _settings.HoldPiece);
        InitialAssignKeys(Accept_inputField, _settings.Accept);
        InitialAssignKeys(Menu_inputField, _settings.Menu);

        FillDropDown(drp_Style);
        AssignDropDown(drp_Style, _settings.Style);
    }

    public void InitialAssignKeys(TMP_InputField textField, KeyCode[] keys)
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

        bindingTextbox.text = $"Press a key to bind to {GetDescriptionFromButton(whichButton)}.";
        bindingTextbox.enabled = true;

        binding = true;
        justClicked = true;
        bindingInputField = GetSettingsFieldFromButton(whichButton);
    }

    public TMP_InputField GetSettingsFieldFromButton(Button whichButton)
        => settingsMapping.First(t => t.Item1 == whichButton).Item2;

    public KeyCode[] GetSettingsFromButton(Button whichButton)
        => settingsMapping.First(t => t.Item1 == whichButton).Item3;

    public string GetDescriptionFromButton(Button whichButton)
        => settingsMapping.First(t => t.Item1 == whichButton).Item4;

    public KeyCode[] GetSettingsFromField(TMP_InputField whichField)
        => settingsMapping.First(t => t.Item2 == whichField).Item3;

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

    public void AssignNewKey(KeyCode key)
    {
        List<KeyCode> keys;

        if (bindingInputField == Up_inputField)
        {
            keys = _settings.Up.ToList();
            keys.Add(key);
            _settings.Up = keys.Distinct().ToArray();
        }
        else if (bindingInputField == Down_inputField)
        {
            keys = _settings.Down.ToList();
            keys.Add(key);
            _settings.Down = keys.Distinct().ToArray();
        }
        else if (bindingInputField == Left_inputField)
        {
            keys = _settings.Left.ToList();
            keys.Add(key);
            _settings.Left = keys.Distinct().ToArray();
        }
        else if (bindingInputField == Right_inputField)
        {
            keys = _settings.Right.ToList();
            keys.Add(key);
            _settings.Right = keys.Distinct().ToArray();
        }

        else if (bindingInputField == RotateLeft_inputField)
        {
            keys = _settings.SpinLeft.ToList();
            keys.Add(key);
            _settings.SpinLeft = keys.Distinct().ToArray();
        }
        else if (bindingInputField == RotateRight_inputField)
        {
            keys = _settings.SpinRight.ToList();
            keys.Add(key);
            _settings.SpinRight = keys.Distinct().ToArray();
        }
        else if (bindingInputField == ShadowLeft_inputField)
        {
            keys = _settings.RotationLeft.ToList();
            keys.Add(key);
            _settings.RotationLeft = keys.Distinct().ToArray();
        }
        else if (bindingInputField == ShadowRight_inputField)
        {
            keys = _settings.RotationRight.ToList();
            keys.Add(key);
            _settings.RotationRight = keys.Distinct().ToArray();
        }

        else if (bindingInputField == Rewind_inputField)
        {
            keys = _settings.Rewind.ToList();
            keys.Add(key);
            _settings.Rewind = keys.Distinct().ToArray();
        }
        else if (bindingInputField == Forward_inputField)
        {
            keys = _settings.Forward.ToList();
            keys.Add(key);
            _settings.Forward = keys.Distinct().ToArray();
        }

        else if (bindingInputField == Hold_inputField)
        {
            keys = _settings.HoldPiece.ToList();
            keys.Add(key);
            _settings.HoldPiece = keys.Distinct().ToArray();
        }
        else if (bindingInputField == Accept_inputField)
        {
            keys = _settings.Accept.ToList();
            keys.Add(key);
            _settings.Accept = keys.Distinct().ToArray();
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
