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
    public static int[] timesToRegister_Movement = { 1, 30, 30, 25, 25, 20, 20, 15, 10, 5, 3, };
    public static int[] timesToRegister_TimeTravel = { 1, 15, 15, 15, 10, 10, 5, 5, 3 };

    public int lastKeysPressed = (int)KeyPressed.None;
    public int currentKeysPressed = (int)KeyPressed.None;
    public int relayedKeysPressed = (int)KeyPressed.None;

    public int keysHeldDuration_Movement = 0;
    public int keysHeldDuration_TimeTravel = 0;

    public int timeToRegisterIndex_Movement = 0;
    public int timeToRegisterIndex_TimeTravel = 0;

    public const int framesBeforeLockingPiece = 45;
    public int lockingPieceFrames = 0;

    public string filename = "./settings.json";

    private const int moving =
        (int)KeyPressed.Up |
        (int)KeyPressed.Down |
        (int)KeyPressed.Left |
        (int)KeyPressed.Right;

    private const int rotation =
        (int)KeyPressed.SpinLeft |
        (int)KeyPressed.SpinRight;

    private const int timeTravel =
        (int)KeyPressed.Rewind |
        (int)KeyPressed.Forward;

    private const int noRepeatKeys =
        (int)KeyPressed.Accept |
        (int)KeyPressed.HoldPiece |
        (int)KeyPressed.RotationLeft |
        (int)KeyPressed.RotationRight |
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
        CheckKeys(ref keysPressed, gameSettings.Down, KeyPressed.Down, null);
        CheckKeys(ref keysPressed, gameSettings.Up, KeyPressed.Up, new List<KeyPressed>() { KeyPressed.Down });
        CheckKeys(ref keysPressed, gameSettings.Left, KeyPressed.Left, null);
        CheckKeys(ref keysPressed, gameSettings.Right, KeyPressed.Right, new List<KeyPressed>() { KeyPressed.Left });
        CheckKeys(ref keysPressed, gameSettings.Accept, KeyPressed.Accept, new List<KeyPressed>() { KeyPressed.Up, KeyPressed.Down, KeyPressed.Left, KeyPressed.Right });

        // Rotation.
        CheckKeys(ref keysPressed, gameSettings.SpinLeft, KeyPressed.SpinLeft, null);
        CheckKeys(ref keysPressed, gameSettings.SpinRight, KeyPressed.SpinRight, new List<KeyPressed>() { KeyPressed.SpinLeft });
        
        CheckKeys(ref keysPressed, gameSettings.RotationLeft, KeyPressed.RotationLeft, new List<KeyPressed>() { KeyPressed.SpinLeft, KeyPressed.SpinRight });
        CheckKeys(ref keysPressed, gameSettings.RotationRight, KeyPressed.RotationRight, new List<KeyPressed>() { KeyPressed.SpinLeft, KeyPressed.SpinRight });

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

        int currentTimeTravelPressed = keysPressed & timeTravel;
        int lastTimeTravelPressed = lastKeysPressed & timeTravel;

        int currentNoRepeat = keysPressed & noRepeatKeys;
        int lastNoRepeat = lastKeysPressed & noRepeatKeys;

        if (currentMovementPressed > 0)
        {
            ++keysHeldDuration_Movement;
        }
        else
        {
            keysHeldDuration_Movement = 0;
            timeToRegisterIndex_Movement = 0;
        }

        if (currentMovementPressed == 0 && currentTimeTravelPressed > 0)
        {
            ++keysHeldDuration_TimeTravel;
        }
        else
        {
            keysHeldDuration_TimeTravel = 0;
            timeToRegisterIndex_TimeTravel = 0;
        }

        // Split out storing information about moving (should charge) and rotating (should not charge).
        if (keysHeldDuration_Movement == timesToRegister_Movement[timeToRegisterIndex_Movement])
        {
            timeToRegisterIndex_Movement = Math.Min(++timeToRegisterIndex_Movement, timesToRegister_Movement.Length - 1);
            keysHeldDuration_Movement = 1;
            relayedKeysPressed += currentMovementPressed;
        }
        else if (keysHeldDuration_TimeTravel == timesToRegister_TimeTravel[timeToRegisterIndex_TimeTravel])
        {
            timeToRegisterIndex_TimeTravel = Math.Min(++timeToRegisterIndex_TimeTravel, timesToRegister_TimeTravel.Length - 1);
            keysHeldDuration_TimeTravel = 1;
            relayedKeysPressed += currentTimeTravelPressed;
        }

        if ((lastKeysPressed & rotation) == 0 && (currentKeysPressed & rotation) > 0)
        {
            relayedKeysPressed += (currentKeysPressed & rotation);
        }

        relayedKeysPressed |= (~lastNoRepeat & currentNoRepeat);
    }

    public bool PieceShouldLock ()
    {
        return lockingPieceFrames >= framesBeforeLockingPiece;
    }
}
