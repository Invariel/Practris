using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class UserInput : MonoBehaviour
{
    public Settings gameSettings;
    public static int[] timesToRegister = { 1, 30, 30, 30, 20, 20, 15, 10, 5, };

    public int lastKeysPressed = (int)KeyPressed.None;
    public int currentKeysPressed = (int)KeyPressed.None;
    public int relayedKeysPressed = (int)KeyPressed.None;

    public int keysHeldDuration = 0;
    public int timeToRegisterIndex = 0;

    public string filename = "./settings.json";

    private const int moving =
        (int)KeyPressed.Up |
        (int)KeyPressed.Down |
        (int)KeyPressed.Left |
        (int)KeyPressed.Right;

    private const int rotation =
        (int)KeyPressed.SpinLeft |
        (int)KeyPressed.SpinRight;

    private const int otherKeys =
        (int)KeyPressed.Accept |
        (int)KeyPressed.HoldPiece |
        (int)KeyPressed.RotationLeft |
        (int)KeyPressed.RotationRight |
        (int)KeyPressed.Rewind |
        (int)KeyPressed.Forward |
        (int)KeyPressed.Menu;

    public void LoadSettingsFromFile ()
    {
        gameSettings = new Settings ();
        if (File.Exists(filename))
        {
            string settings = File.ReadAllText(filename);
            gameSettings = JsonSerializer.Deserialize<Settings>(settings);

            // Verify no control overlap.  That would be not ideal.
        }
    }

    public void SaveSettingsToFile ()
    {
        if (File.Exists(filename))
        {
            File.Delete(filename);
        }

        string settings = JsonSerializer.Serialize (gameSettings, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(filename, settings, System.Text.Encoding.UTF8);
    }

    public int GetKeysPressed { get => relayedKeysPressed; }

    public void CheckKeys(ref int keysPressed, IEnumerable keyCodes, KeyPressed value, List<KeyPressed> exclude = null)
    {
        int kp = keysPressed;
        if (exclude is null || !exclude.Select(e => (kp & (int)e)).Where(e => e > 0).Any())
        {
            foreach (KeyCode keyCode in keyCodes)
            {
                if (Input.GetKey(keyCode))
                {
                    keysPressed += (int)value;
                    break;
                }
            }
        }
    }

    public static bool TestKey(KeyPressed pressedKey, int keysPressed)
    {
        return (keysPressed & (int)pressedKey) == (int)pressedKey;
    }

    // Update is called once per frame
    void Update()
    {
        int keysPressed = 0;

        // Movement.
        CheckKeys(ref keysPressed, gameSettings.Up, KeyPressed.Up, null);
        CheckKeys(ref keysPressed, gameSettings.Down, KeyPressed.Down, new List<KeyPressed>() { KeyPressed.Up });
        CheckKeys(ref keysPressed, gameSettings.Left, KeyPressed.Left, null);
        CheckKeys(ref keysPressed, gameSettings.Right, KeyPressed.Right, new List<KeyPressed>() { KeyPressed.Left });
        CheckKeys(ref keysPressed, gameSettings.Accept, KeyPressed.Accept, new List<KeyPressed>() { KeyPressed.Up, KeyPressed.Down, KeyPressed.Left, KeyPressed.Right });

        // Rotation.
        CheckKeys(ref keysPressed, gameSettings.SpinLeft, KeyPressed.SpinLeft, null);
        CheckKeys(ref keysPressed, gameSettings.SpinRight, KeyPressed.SpinRight, new List<KeyPressed>() { KeyPressed.SpinLeft });
        CheckKeys(ref keysPressed, gameSettings.RotationLeft, KeyPressed.RotationLeft, new List<KeyPressed>() { KeyPressed.SpinLeft, KeyPressed.SpinRight });
        CheckKeys(ref keysPressed, gameSettings.RotationRight, KeyPressed.RotationRight, new List<KeyPressed>() { KeyPressed.SpinLeft, KeyPressed.SpinRight, KeyPressed.RotationRight });

        // Hold Piece
        CheckKeys(ref keysPressed, gameSettings.HoldPiece, KeyPressed.HoldPiece, null);

        // Time Travel
        CheckKeys(ref keysPressed, gameSettings.Rewind, KeyPressed.Rewind, null);
        CheckKeys(ref keysPressed, gameSettings.Forward, KeyPressed.Forward, new List<KeyPressed>() { KeyPressed.Rewind });

        CheckKeys(ref keysPressed, gameSettings.Menu, KeyPressed.Menu, null);

        lastKeysPressed = currentKeysPressed;
        currentKeysPressed = keysPressed;
        relayedKeysPressed = 0;

        int currentMovementPressed = keysPressed & moving;

        int currentRotationPressed = keysPressed & rotation;
        int lastRotationPressed = lastKeysPressed & rotation;

        int currentOtherPressed = keysPressed & otherKeys;
        int lastOtherPressed = lastKeysPressed & otherKeys;

        if (currentMovementPressed > 0)
        {
            ++keysHeldDuration;
        }
        else
        {
            keysHeldDuration = 0;
            timeToRegisterIndex = 0;
        }

        // Split out storing information about moving (should charge) and rotating (should not charge).
        if (keysHeldDuration == timesToRegister[timeToRegisterIndex])
        {
            timeToRegisterIndex = Math.Min(++keysHeldDuration, timesToRegister.Length - 1);
            keysHeldDuration = 1;
            relayedKeysPressed += currentMovementPressed;
        }

        if ((lastKeysPressed & rotation) == 0 && (currentKeysPressed & rotation) > 0)
        {
            relayedKeysPressed += (currentKeysPressed & rotation);
        }

        relayedKeysPressed |= (~lastOtherPressed & currentOtherPressed);
    }
}
