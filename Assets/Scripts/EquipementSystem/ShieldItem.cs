using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldItem : MonoBehaviour
{
    public enum Type
    {
        None,
        RoundA, RoundB,
        CavaleryA, CavaleryB, CavaleryC, CavaleryD, CavaleryE, CavaleryF, CavaleryG,
        RectangleA, RectangleB, RectangleC,
        KiteA, KiteB, KiteC, KiteD, KiteE, KiteF, KiteG, KiteH, KiteI, KiteJ, KiteK
    };
    public Type type = Type.None;
    public float load = 0f;

    public static ShieldItem none
    {
        get
        {
            ShieldItem item = new ShieldItem();
            item.type = Type.None;
            return item;
        }
    }
    public static void Copy(ShieldItem source, ShieldItem destination)
    {
        destination.type = source.type;
        destination.load = source.load;
    }
}
