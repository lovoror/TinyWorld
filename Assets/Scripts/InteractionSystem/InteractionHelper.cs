using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionHelper : MonoBehaviour
{
    //public bool visible = false;
    public float spacing;
    public float duration = 1f;
    public List<Sprite> toolsIcons;
    public GameObject container;
    public InteractionConditionTemplate template;
    public Sprite valid;
    public Sprite invalid;
    private AudioSource audiosource;
    public AudioClip errorSound;
    private IEnumerator timerCoroutine;
    private Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();
    
    void Start()
    {
        foreach (Sprite s in toolsIcons)
        {
            string name = s.name.Substring(0, s.name.IndexOf("Icon"));
            icons.Add(name, s);
        }
        audiosource = GetComponent<AudioSource>();
        audiosource.clip = errorSound;
    }

    public void UpdateContent(Dictionary<string, string> conditionList)
    {
        if (container.activeSelf)
            StopCoroutine(timerCoroutine);
        foreach (Transform child in container.transform)
            Destroy(child.gameObject);
        
        Vector3 position = new Vector3(-0.5f * spacing * (conditionList.Count - 1), 0, 0);
        foreach (KeyValuePair<string, string> entry in conditionList)
        {
            InteractionConditionTemplate go = Instantiate(template, container.transform);
            go.transform.localPosition = position;
            go.gameObject.SetActive(true);

            if (icons.ContainsKey(entry.Key))
                go.toolIcon.sprite = icons[entry.Key];
            else
                Debug.Log("no tool icon for this entry : " + entry.Key);

            if (entry.Value == "ok")
            {
                go.validationIcon.sprite = valid;
                go.specialText.gameObject.SetActive(false);
                go.validationIcon.gameObject.SetActive(true);
            }
            else if (entry.Value == "nok")
            {
                go.validationIcon.sprite = invalid;
                go.specialText.gameObject.SetActive(false);
                go.validationIcon.gameObject.SetActive(true);
            }
            else
            {
                go.specialText.text = entry.Value;
                go.specialText.gameObject.SetActive(true);
                go.validationIcon.gameObject.SetActive(false);
            }
            position.x += spacing;
        }

        timerCoroutine = StopTimer(duration);
        StartCoroutine(timerCoroutine);
    }

    private IEnumerator StopTimer(float t)
    {
        audiosource.Play();
        container.SetActive(true);
        yield return new WaitForSeconds(t);
        container.SetActive(false);
    }
}
