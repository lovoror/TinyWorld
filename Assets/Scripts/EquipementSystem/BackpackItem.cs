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
    public float load = 0f;

    public static BackpackItem none {
        get
        {
            BackpackItem item = new BackpackItem();
            item.type = Type.None;
            return item;
        }
    }
    public static void Copy(BackpackItem source, BackpackItem destination)
    {
        destination.type = source.type;
    }
}
