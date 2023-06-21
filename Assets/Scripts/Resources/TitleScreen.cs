using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public const string BeginScene = "Begin";
    public const string EditBoardScene = "EditBoard";
    public const string SettingsScene = "Settings";

    void Start() { }
    void Update() { }

    public void Begin() { SceneManager.LoadScene(BeginScene); }
    public void Edit() { /* SceneManager.LoadScene(EditBoardScene); */ }
    public void Settings () { SceneManager.LoadScene(SettingsScene); }
    public void Quit() { Application.Quit(); }
}
