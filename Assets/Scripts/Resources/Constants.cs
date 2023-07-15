using System.Collections.Generic;

public static class Constants
{
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
}
