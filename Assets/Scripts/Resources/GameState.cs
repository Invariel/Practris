using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    None,
    SpawnPiece,
    MovePiece,
    PiecePlaced,
    ClearingRows,
    Searching,
    GameOver,
}
