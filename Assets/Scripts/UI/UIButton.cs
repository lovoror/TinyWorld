using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UIHandler handler;
    public string helper;
    public EventTrigger.TriggerEvent callback;

    public void OnPointerClick(PointerEventData eventData)
    {
        callback.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        handler.SetHelperText(helper);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        handler.SetHelperText("");
    }
}
