using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingIconTemplate : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public Image nok;
    public Image option;
    public UIHandler handler;
    public string helper;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        handler.OnIconClick(this);
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
