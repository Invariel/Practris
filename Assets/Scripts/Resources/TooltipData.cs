using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class TooltipData : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button myself;
    public TMP_Text tooltip;
    public Image image;
    public string tooltipText;

    public void ConfigureEvents()
    {
        var mouseOverTrigger = myself.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry mouseOverTriggerEvent = new EventTrigger.Entry();
        mouseOverTriggerEvent.eventID = EventTriggerType.PointerEnter;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.text = tooltipText;
        tooltip.color = Color.white;
        image.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.text = "";
        tooltip.color = Color.clear;
        image.enabled = false;
    }
}
