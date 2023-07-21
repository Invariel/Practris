using UnityEngine;
using UnityEngine.SceneManagement;
public class TitleScene : MonoBehaviour
{
    void Start() { Mino.CacheResourceMinos(); }
    void Update() { if (Input.GetKey(KeyCode.Escape)) { Application.Quit(); } }

    public void Begin() => SceneManager.LoadScene(Constants.GetScene(Constants.Scene.PLAYFIELD));
    public void Edit() => SceneManager.LoadScene(Constants.GetScene(Constants.Scene.EDIT));
    public void Settings () => SceneManager.LoadScene(Constants.GetScene(Constants.Scene.SETTINGS));
    public void Quit() => Application.Quit();
}
