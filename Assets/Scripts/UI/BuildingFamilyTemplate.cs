using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingFamilyTemplate : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public Image background;
    public Image border;
    public bool activated = false;
    public UIHandler handler;
    public string helper;

    private void Start()
    {
        if (!activated)
        {
            image.color = new Color(0.5f, 0.5f, 0.5f);
            background.color = new Color(0.5f, 0.5f, 0.5f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //print("Clicked on " + gameObject.name);
        handler.OnFamilyClick(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!activated)
        {
            image.color = Color.white;
            background.color = Color.white;
        }
        handler.SetHelperText(helper);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!activated)
        {
            image.color = new Color(0.5f, 0.5f, 0.5f);
            background.color = new Color(0.5f, 0.5f, 0.5f);
        }
        handler.SetHelperText("");
    }
}
