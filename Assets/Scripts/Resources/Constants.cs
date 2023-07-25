using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class Constants
{
    public static string[] _resourceStyles
    {
        get
        {
            List<string> styles = new List<string> { "Original", "v2", "v3", "v4", "Way Too Grey", "Hearts", "Mostly Dark" };
            return styles.OrderBy(a => a.ToLowerInvariant()).ToArray();
        }
    }

    private static Dictionary<Scene, string> SceneNames = new Dictionary<Scene, string>()
    {
        { Scene.EDIT, "EditBoard" },
        { Scene.PLAYFIELD, "Playfield" },
        { Scene.SETTINGS, "Settings" },
        { Scene.TITLE, "Title" },
    };

    public enum Scene
    {
        EDIT,
        PLAYFIELD,
        SETTINGS,
        TITLE,
    }

    public static string GetScene(Scene scene) => SceneNames[scene] ?? string.Empty;

    public enum SettingsField
    {
        Up,
        Down,
        Left,
        Right,
        SpinLeft,
        SpinRight,
        RotationLeft,
        RotationRight,
        Rewind,
        Forward,
        HoldPiece,
        Accept,
        Menu
    }
}
