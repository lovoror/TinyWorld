using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public ConstructionCamera mainController;
    public List<BuildingFamilyTemplate> familyList;
    public BuildingIconTemplate iconTemplate;
    public Sprite selectedBorder;
    public Sprite notSelectedBorder;
    public float selectedScale = 1.04f;
    public Color selectedColor;
    public Color notSelectedColor;
    public Text helperText;
    public GameObject helperPanel;
    public Transform iconContainer;
    public Slider slider;
    public List<Sprite> optionList;
    public Dictionary<string, Sprite> options;

    public AudioClip selectedSound;
    public AudioClip nokSound;
    public AudioSource audiosource;

    public int iconSpacing;
    private int lineCount;
    public string toolName;

    private void Start()
    {
        audiosource = GetComponent<AudioSource>();
        helperPanel.SetActive(false);
        options = new Dictionary<string, Sprite>();
        foreach(Sprite s in optionList)
            options.Add(s.name, s);
        Reset();
    }

    public void Reset()
    {
        toolName = "";
        foreach (BuildingFamilyTemplate icon in familyList)
        {
            icon.activated = false;
            icon.border.sprite = notSelectedBorder;
            icon.border.transform.localScale = new Vector3(1.02f, 1.02f, 1.02f);
            icon.OnPointerExit(null);
            icon.border.color = notSelectedColor;
        }
        foreach (Transform t in iconContainer)
            Destroy(t.gameObject);
        audiosource.clip = selectedSound;
    }
    public void OnFamilyClick(BuildingFamilyTemplate click)
    {
        string family = "";
        foreach(BuildingFamilyTemplate icon in familyList)
        {
            if(icon == click)
            {
                icon.activated = true;
                icon.border.sprite = selectedBorder;
                icon.border.transform.localScale = new Vector3(selectedScale, selectedScale, selectedScale);
                icon.border.color = selectedColor;
                SetHelperText(icon.helper);
                family = icon.name;
            }
            else
            {
                icon.activated = false;
                icon.border.sprite = notSelectedBorder;
                icon.border.transform.localScale = new Vector3(1.02f, 1.02f, 1.02f);
                icon.OnPointerExit(null);
                icon.border.color = notSelectedColor;
            }
        }
        audiosource.clip = selectedSound;
        audiosource.Play();
        Initialize(family);

        toolName = (family == "Terrain") ? "terrain" : "building";
    }
    public void OnIconClick(BuildingIconTemplate click)
    {
        if(!click.nok.enabled)
            mainController.SelectedBuilding(click.gameObject);
        audiosource.clip = click.nok.enabled ? nokSound : selectedSound;
        audiosource.Play();
    }
    public void OnSliderMove()
    {
        float v = iconSpacing * (lineCount - 2.5f) * slider.value;
        iconContainer.localPosition = new Vector3(v, iconContainer.localPosition.y, iconContainer.localPosition.z);
    }
    public void SetHelperText(string text)
    {
        if (text.Length != 0)
        {
            helperPanel.SetActive(true);
            helperText.text = text;
        }
        else helperPanel.SetActive(false);
    }
    public void Exit()
    {
        mainController.quit = true;
    }
    public void BuildingDelete()
    {
        toolName = "delete";
        audiosource.clip = selectedSound;
        audiosource.Play();
    }
    private void Initialize(string family)
    {
        foreach (Transform t in iconContainer)
            Destroy(t.gameObject);

        if(family == "Terrain")
        {
            List<ScriptableTile> terrainTile = Map.Instance.tileList;
            Vector3 p = new Vector3(95, 236, 0);
            int column = 0;
            lineCount = 1;
            foreach (ScriptableTile st in terrainTile)
            {
                if(st.isTerrain)
                {
                    BuildingIconTemplate go = Instantiate(iconTemplate);
                    go.transform.localPosition = p;
                    go.transform.SetParent(iconContainer, false);
                    go.transform.localEulerAngles = new Vector3(0, 0, -90);
                    go.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);

                    go.image.sprite = st.optionalSprite;
                    go.helper = st.helperText;
                    go.name = st.name;
                    go.handler = this;
                    go.nok.enabled = false;
                    go.option.enabled = false;

                    column++;
                    if (column >= 4)
                    {
                        p.y = 236;
                        p.x -= iconSpacing;
                        column = 0;
                        lineCount++;
                    }
                    else p.y -= iconSpacing;
                }
            }
            slider.gameObject.SetActive(lineCount > 2);
            slider.value = 0f;
        }
        else
        {
            Dictionary<string, ConstructionTemplate> list = ConstructionDictionary.Instance.templateDictionary;
            Vector3 p = new Vector3(95, 236, 0);
            int column = 0;
            lineCount = 1;
            foreach(KeyValuePair<string, ConstructionTemplate> entry in list)
            {
                if(entry.Value.buildingFamily == family)
                {
                    BuildingIconTemplate go = Instantiate(iconTemplate);
                    go.transform.localPosition = p;
                    go.transform.SetParent(iconContainer, false);
                    go.transform.localEulerAngles = new Vector3(0, 0, -90);
                    go.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);

                    go.image.sprite = entry.Value.sprite;
                    go.helper = entry.Key;
                    go.name = entry.Key;
                    go.handler = this;
                    go.nok.enabled = false;

                    if (entry.Value.additionalIcon)
                    {
                        go.option.enabled = true;
                        go.option.sprite = entry.Value.additionalIcon;
                    }
                    else go.option.enabled = false;

                    column++;
                    if(column >= 4)
                    {
                        p.y = 236;
                        p.x -= iconSpacing;
                        column = 0;
                        lineCount++;
                    }
                    else p.y -= iconSpacing;
                }
            }
            slider.gameObject.SetActive(lineCount > 2);
            slider.value = 0f;
        }
        
    }
}
