using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class Mino
{
    private static float _blockSize = 0.625f;
    private static Vector3 _blockScale = new Vector3(_blockSize, _blockSize, _blockSize);

    private static float _sideBlockSize = 0.3125f;
    private static Vector3 _sideBlockScale = new Vector3(_sideBlockSize, _sideBlockSize, _sideBlockSize);

    private static Dictionary<MinoEnum, MinoData> _minoData = new Dictionary<MinoEnum, MinoData>()
    {
        { MinoEnum.Empty, new MinoData() { MinoName = "Empty", MinoSprite = "Mino_Empty" } },
        { MinoEnum.Border, new MinoData() { MinoName = "Border", MinoSprite = "Mino_Border" } },
        { MinoEnum.Preset, new MinoData() { MinoName = "Preset", MinoSprite = "Mino_Border" } }, // Make another mino for this.
        { MinoEnum.I, new MinoData() { MinoName = "I", MinoSprite = "Mino_I_Cyan" } },
        { MinoEnum.J, new MinoData() { MinoName = "J", MinoSprite = "Mino_J_Blue" } },
        { MinoEnum.L, new MinoData() { MinoName = "L", MinoSprite = "Mino_L_Orange" } },
        { MinoEnum.O, new MinoData() { MinoName = "O", MinoSprite = "Mino_O_Yellow" } },
        { MinoEnum.S, new MinoData() { MinoName = "S", MinoSprite = "Mino_S_Green" } },
        { MinoEnum.T, new MinoData() { MinoName = "T", MinoSprite = "Mino_T_Purple" } },
        { MinoEnum.Z, new MinoData() { MinoName = "Z", MinoSprite = "Mino_Z_Red" } },
    };

    public static MinoData GetMinoData(MinoEnum mino) => _minoData[mino];

    public static GameObject CreateMino (MinoEnum mino, string name, bool isMainBoard)
    {
        GameObject tile = new GameObject();

        SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>(_minoData[mino].MinoSprite);

        tile.transform.localScale = isMainBoard ? _blockScale : _sideBlockScale;
        tile.AddComponent<BoxCollider2D>();
        tile.AddComponent<MinoMouseDownHandler>();

        tile.name = $"{_minoData[mino].MinoName} - {name}";

        return tile;
    }

    public class MinoMouseDownHandler : MonoBehaviour
    {
        private void OnMouseDown()
        {
            Debug.Log("Clicked on " + gameObject.name);
        }
    }
}

public class MinoData
{
    public string MinoName { get; set; }
    public string MinoSprite { get; set; }
}

public enum MinoEnum
{
    Empty = 0,
    Shadow = 1,
    Border = 2,
    Preset = 3,
    I,
    J,
    L,
    O,
    S,
    T,
    Z
}
