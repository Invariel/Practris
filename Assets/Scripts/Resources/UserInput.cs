using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class UserInput : MonoBehaviour
{
    public Settings gameSettings;

    public static int[] pressDurations = { 1, 20, 10, 10, 5, };

    public int lastKeysPressed = (int)KeyPressed.None;
    public int currentKeysPressed = (int)KeyPressed.None;
    public int relayedKeysPressed = (int)KeyPressed.None;

    public int currentPressDuration = 0;
    public int keyPressDuration = 0;

    private const int moving =
        (int)KeyPressed.Up |
        (int)KeyPressed.Down |
        (int)KeyPressed.Left |
        (int)KeyPressed.Right;

    private const int rotating =
        (int)KeyPressed.SpinLeft |
        (int)KeyPressed.SpinRight;

    public void SendSettings (Settings settings)
    {
        gameSettings = settings;
    }

    public int GetKeysPressed { get { return relayedKeysPressed; } }

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

        if ((currentKeysPressed & (moving | rotating)) > 0)
        {
            ++keyPressDuration;
        }
        else
        {
            keyPressDuration = 0;
            currentPressDuration = 0;
        }

        if (keyPressDuration == pressDurations[currentPressDuration] &&
            (currentKeysPressed & moving) > 0)
        {
            keyPressDuration = 1;
            currentPressDuration = Math.Min(++currentPressDuration, pressDurations.Length - 1);
            relayedKeysPressed = currentKeysPressed;
        }
        else if (keyPressDuration == 1 && (currentKeysPressed & rotating) > 0)
        {
            relayedKeysPressed = currentKeysPressed;
        }
        else
        {
            relayedKeysPressed = ~lastKeysPressed & currentKeysPressed;
        }
    }

    public static int CheckKeys(IEnumerable keyCodes, KeyPressed value)
    {
        int retval = 0;

        foreach (KeyCode keyCode in keyCodes)
        {
            if (Input.GetKey(keyCode))
            {
                retval += (int)value;
                break;
            }
        }

        return retval;
    }

    private bool TestKey(KeyPressed key, int? keysPressed)
    {
        return (keysPressed & (int)key) == (int)key;
    }
}
