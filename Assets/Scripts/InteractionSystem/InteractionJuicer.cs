using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionJuicer : MonoBehaviour
{
    public Transform pivot;
    public TextMesh textMesh;

    public Color wood;
    public Color stone;
    public Color iron;
    public Color gold;
    public Color crystal;

    public float amplitude;
    public int duration;
    public AnimationCurve animCurve;

    private IEnumerator animCoroutine;

    public void LaunchAnim(string text, InteractionType.Type type)
    {
        textMesh.text = text;
        switch(type)
        {
            case InteractionType.Type.collectWood:
                textMesh.color = wood;
                break;
            case InteractionType.Type.collectIron:
                textMesh.color = iron;
                break;
            case InteractionType.Type.collectGold:
                textMesh.color = gold;
                break;
            case InteractionType.Type.collectCrystal:
                textMesh.color = crystal;
                break;
            case InteractionType.Type.collectStone:
                textMesh.color = stone;
                break;
            default:
                textMesh.color = Color.white;
                break;
        }

        animCoroutine = Anim();
        StartCoroutine(animCoroutine);
    }

    private IEnumerator Anim()
    {
        pivot.gameObject.SetActive(true);
        for (int i = 0; i < duration; i++)
        {
            pivot.localPosition = Vector3.Lerp(Vector3.zero, amplitude * Vector3.up, animCurve.Evaluate((float)i / duration));
            pivot.right = Camera.main.transform.right;
            yield return null;
        }
        pivot.gameObject.SetActive(false);
    }
}
