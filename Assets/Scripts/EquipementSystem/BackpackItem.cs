using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BackpackItem : MonoBehaviour
{
    public enum Type
    {
        None,
        QuiverA,
        QuiverB,
        RessourceContainer
    };
    public Type type = Type.None;
    public string toolFamily = "None";
    public float load = 0f;

    public void Clear()
    {
        type = Type.None;
        toolFamily = "None";
        load = 0f;
    }
    public static void Copy(BackpackItem source, BackpackItem destination)
    {
        destination.type = source.type;
        destination.load = source.load;
        destination.toolFamily = source.toolFamily;
    }
}
