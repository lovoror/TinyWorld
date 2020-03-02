using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class QuitButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UIHandler handler;
    public string helper;

    public void OnPointerClick(PointerEventData eventData)
    {
        handler.Exit();
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
