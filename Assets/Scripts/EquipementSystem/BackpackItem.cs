using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BackpackItem : MonoBehaviour
{
    public enum Type
    {
        None,
        QuiverA,
        QuiverB
    };
    public Type type = Type.None;
}
