using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mino
{
    public delegate void MinoEventHandler(object sender, MinoEventArgs e);

    private static float _blockSize = 0.625f;
    private static Vector3 _blockScale = new Vector3(_blockSize, _blockSize, _blockSize);

    private static float _sideBlockSize = 0.3125f;
    private static Vector3 _sideBlockScale = new Vector3(_sideBlockSize, _sideBlockSize, _sideBlockSize);

    private static Dictionary<MinoEnum, MinoData> _minoData = new Dictionary<MinoEnum, MinoData>()
    {
        { MinoEnum.Empty, new MinoData() { MinoName = "Empty", MinoSpriteName = "Mino_Empty", MinoShadowName = "Mino_Empty" } },
        { MinoEnum.Border, new MinoData() { MinoName = "Border", MinoSpriteName = "Mino_Border", MinoShadowName = "Mino_Border" } },
        { MinoEnum.Preset, new MinoData() { MinoName = "Preset", MinoSpriteName = "Mino_Preset", MinoShadowName = "Mino_Preset" } },
        { MinoEnum.I, new MinoData() { MinoName = "I", MinoSpriteName = "Mino_I_Cyan", MinoShadowName = "Mino_I_Cyan_Shadow" } },
        { MinoEnum.J, new MinoData() { MinoName = "J", MinoSpriteName = "Mino_J_Blue", MinoShadowName = "Mino_J_Blue_Shadow" } },
        { MinoEnum.L, new MinoData() { MinoName = "L", MinoSpriteName = "Mino_L_Orange", MinoShadowName = "Mino_L_Orange_Shadow" } },
        { MinoEnum.O, new MinoData() { MinoName = "O", MinoSpriteName = "Mino_O_Yellow", MinoShadowName = "Mino_O_Yellow_Shadow" } },
        { MinoEnum.S, new MinoData() { MinoName = "S", MinoSpriteName = "Mino_S_Green", MinoShadowName = "Mino_S_Green_Shadow" } },
        { MinoEnum.T, new MinoData() { MinoName = "T", MinoSpriteName = "Mino_T_Purple", MinoShadowName = "Mino_T_Purple_Shadow" } },
        { MinoEnum.Z, new MinoData() { MinoName = "Z", MinoSpriteName = "Mino_Z_Red", MinoShadowName = "Mino_Z_Red_Shadow" } },
    };

    private static Dictionary<string, Dictionary<MinoEnum, MinoSpriteData>> _minoSpriteData = new();

    public static void CacheResourceMinos()
    {
        foreach (string style in Constants._resourceStyles)
        {
            CacheResourceStyle(style);
        }
    }

    public static List<string> GetCachedStyles() => _minoSpriteData.Keys.ToList();

    private static void CacheResourceStyle(string style)
    {
        if (_minoSpriteData.ContainsKey(style))
        {
            return;
        }

        _minoSpriteData.Add(style, new());

        foreach (KeyValuePair<MinoEnum, MinoData> data in _minoData)
        {
            Sprite mainSprite = Resources.Load<Sprite>($"{style}/{data.Value.MinoSpriteName}");
            Sprite shadowSprite = Resources.Load<Sprite>($"{style}/{data.Value.MinoShadowName}");

            Sprite backupSprite;
            Sprite backupShadowSprite;

            if (_minoSpriteData.ContainsKey("Original") && _minoSpriteData["Original"].ContainsKey(data.Key))
            {
                backupSprite = _minoSpriteData["Original"][data.Key].MinoSprite;
                backupShadowSprite = _minoSpriteData["Original"][data.Key].MinoShadowSprite;
            }
            else
            {
                backupSprite = Resources.Load<Sprite>($"{style}/{data.Value.MinoSpriteName}");
                backupShadowSprite = Resources.Load<Sprite>($"{style}/{data.Value.MinoShadowName}");
            }

            _minoSpriteData[style].Add(data.Key, new MinoSpriteData() { MinoSprite = mainSprite ?? backupSprite, MinoShadowSprite = shadowSprite ?? backupShadowSprite });
        }
    }

    public static MinoData GetMinoData(MinoEnum mino) => _minoData[mino];

    public static GameObject CreateMino (MinoEnum mino, string style, int x, int y, bool isMainBoard)
    {
        GameObject tile = new GameObject();

        SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
        sr.sprite = GetSprite(mino, style);

        tile.transform.localScale = isMainBoard ? _blockScale : _sideBlockScale;
        tile.AddComponent<BoxCollider2D>();
        tile.AddComponent<MinoClicked>();
        tile.AddComponent<MinoHovered>();

        tile.name = $"{x} {y}";
        return tile;
    }

    public static Sprite GetSprite (MinoEnum mino, string style)
    {
        if (!_minoSpriteData.ContainsKey(style))
        {
            style = "Original";
        }

        return _minoSpriteData[style][mino].MinoSprite;
    }

    public static Sprite GetShadowSprite(MinoEnum mino, string style)
    {
        if (!_minoSpriteData.ContainsKey(style))
        {
            style = "Original";
        }

        return _minoSpriteData[style][mino].MinoShadowSprite;
    }

    public class MinoClicked : MonoBehaviour
    {
        public event MinoEventHandler OnClick;
        private void OnMouseDown()
        {
            OnClick?.Invoke(this, new MinoEventArgs() { Name = gameObject.name, GameObject = gameObject });
        }
    }

    public class MinoHovered : MonoBehaviour
    {
        public event MinoEventHandler OnHover;
        private void OnMouseOver()
        {
            if (Input.GetMouseButton(0))
            {
                OnHover?.Invoke(this, new MinoEventArgs() { Name = gameObject.name, GameObject = gameObject });
            }
        }
    }
}

public class MinoEventArgs : EventArgs
{
    public string Name { get; set; }
    public GameObject GameObject { get; set; }
}

public class MinoData
{
    public string MinoName { get; set; }
    public string MinoSpriteName { get; set; }
    public string MinoShadowName { get; set; }
}

public class MinoSpriteData
{
    public Sprite MinoSprite { get; set; }
    public Sprite MinoShadowSprite { get; set; }
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