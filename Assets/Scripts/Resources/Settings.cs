using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Settings
{
    public KeyCode[] Up             { get; set; } = new KeyCode[] { KeyCode.W, KeyCode.UpArrow };
    public KeyCode[] Down           { get; set; } = new KeyCode[] { KeyCode.S, KeyCode.DownArrow };
    public KeyCode[] Left           { get; set; } = new KeyCode[] { KeyCode.A, KeyCode.LeftArrow };
    public KeyCode[] Right          { get; set; } = new KeyCode[] { KeyCode.D, KeyCode.RightArrow };
    public KeyCode[] SpinLeft       { get; set; } = new KeyCode[] { KeyCode.Z };
    public KeyCode[] SpinRight      { get; set; } = new KeyCode[] { KeyCode.X };
    public KeyCode[] HoldPiece      { get; set; } = new KeyCode[] { KeyCode.LeftShift, KeyCode.RightShift };
    public KeyCode[] RotationLeft   { get; set; } = new KeyCode[] { KeyCode.Comma };
    public KeyCode[] RotationRight  { get; set; } = new KeyCode[] { KeyCode.Period };
    public KeyCode[] Rewind         { get; set; } = new KeyCode[] { KeyCode.LeftBracket };
    public KeyCode[] Forward        { get; set; } = new KeyCode[] { KeyCode.RightBracket };
    public KeyCode[] Accept         { get; set; } = new KeyCode[] { KeyCode.Return, KeyCode.Space };
    public KeyCode[] Menu           { get; set; } = new KeyCode[] { KeyCode.Escape };

    public bool HardDrop            { get; set; } = true;   // Accept is a hard drop (locks piece) or a soft drop (plummets without locking).
}
