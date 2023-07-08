using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using UnityEngine;

public class Mino
{
    private static float _blockSize = 0.625f;
    private static Vector3 _blockScale = new Vector3(_blockSize, _blockSize, _blockSize);

    private static float _sideBlockSize = 0.3125f;
    private static Vector3 _sideBlockScale = new Vector3(_sideBlockSize, _sideBlockSize, _sideBlockSize);

    private static Dictionary<MinoEnum, MinoData> _minoData = new Dictionary<MinoEnum, MinoData>()
    {
        { MinoEnum.Empty, new MinoData() { MinoName = "Empty", MinoSprite = "Mino_Empty", MinoShadowSprite = "Mino_Empty" } },
        { MinoEnum.Border, new MinoData() { MinoName = "Border", MinoSprite = "Mino_Border", MinoShadowSprite = "Mino_Border" } },
        { MinoEnum.Preset, new MinoData() { MinoName = "Preset", MinoSprite = "Mino_Border", MinoShadowSprite = "Mino_Border" } }, // Make another mino for this.
        { MinoEnum.I, new MinoData() { MinoName = "I", MinoSprite = "Mino_I_Cyan", MinoShadowSprite = "Mino_I_Cyan_Shadow" } },
        { MinoEnum.J, new MinoData() { MinoName = "J", MinoSprite = "Mino_J_Blue", MinoShadowSprite = "Mino_J_Blue_Shadow" } },
        { MinoEnum.L, new MinoData() { MinoName = "L", MinoSprite = "Mino_L_Orange", MinoShadowSprite = "Mino_L_Orange_Shadow" } },
        { MinoEnum.O, new MinoData() { MinoName = "O", MinoSprite = "Mino_O_Yellow", MinoShadowSprite = "Mino_O_Yellow_Shadow" } },
        { MinoEnum.S, new MinoData() { MinoName = "S", MinoSprite = "Mino_S_Green", MinoShadowSprite = "Mino_S_Green_Shadow" } },
        { MinoEnum.T, new MinoData() { MinoName = "T", MinoSprite = "Mino_T_Purple", MinoShadowSprite = "Mino_T_Purple_Shadow" } },
        { MinoEnum.Z, new MinoData() { MinoName = "Z", MinoSprite = "Mino_Z_Red", MinoShadowSprite = "Mino_Z_Red_Shadow" } },
    };

    public static MinoData GetMinoData(MinoEnum mino) => _minoData[mino];

    public static GameObject CreateMino (MinoEnum mino, string style, string name, bool isMainBoard)
    {
        GameObject tile = new GameObject();

        SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();

        string filename = GetSpriteFilename(mino, style);
        sr.sprite = Resources.Load<Sprite>(filename);

        tile.transform.localScale = isMainBoard ? _blockScale : _sideBlockScale;
        tile.AddComponent<BoxCollider2D>();

        tile.name = $"{_minoData[mino].MinoName} - {name}";

        return tile;
    }

    public static string GetSpriteFilename (MinoEnum mino, string style)
    {
        string basePath = $"{Application.dataPath}/Sprites/Resources";

        string path = $"{style}/{_minoData[mino].MinoSprite}";
        string backupPath = $"Original/{_minoData[mino].MinoSprite}";

        string outputFilename = File.Exists($"{basePath}/{path}.png") ? path : backupPath;
        return outputFilename;
    }

    public static string GetShadowSpriteFilename (MinoEnum mino, string style)
    {
        string basePath = $"{Application.dataPath}/Sprites/Resources";

        string path = $"{style}/{_minoData[mino].MinoShadowSprite}";
        string backupPath = $"Original/{_minoData[mino].MinoShadowSprite}";

        string outputFilename = File.Exists($"{basePath}/{path}.png") ? path : backupPath;
        return outputFilename;
    }
}

public class MinoData
{
    public string MinoName { get; set; }
    public string MinoSprite { get; set; }
    public string MinoShadowSprite { get; set; }
}

public enum MinoEnum
{
    Empty,
    Border,
    Preset,
    I,
    J,
    L,
    O,
    S,
    T,
    Z,
}
