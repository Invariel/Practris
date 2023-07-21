using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class TooltipData : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject container;

    public Button _myself;
    public Image _image;
    public TMP_Text _tooltip;
    public string _tooltipText;

    public void ConfigureEvents()
    {
        var mouseOverTrigger = _myself.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry mouseOverTriggerEvent = new EventTrigger.Entry();
        mouseOverTriggerEvent.eventID = EventTriggerType.PointerEnter;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltip.text = _tooltipText;
        _tooltip.color = Color.white;
        _image.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip.text = "";
        _tooltip.color = Color.clear;
        _image.enabled = false;
    }
}
