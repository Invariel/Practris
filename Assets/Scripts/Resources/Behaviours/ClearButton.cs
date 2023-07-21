using Assets.Scripts.Resources;
using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;

public class ClearButton : MonoBehaviour, IPointerDownHandler
{
    public static SettingsScene _scene;
    public Button _myself;

    public void OnPointerDown(PointerEventData eventData)
    {
        _scene.ClearKeys(_myself.GetComponentInParent<ContainerInformation>()._settingName);
    }
}
