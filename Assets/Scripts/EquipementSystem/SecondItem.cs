using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondItem : MonoBehaviour
{
    public enum Type
    {
        None,
        LongBow, RecurveBow, ShortBow, 
        StaffA, StaffB, StaffC, StaffD
    };
    public Type type = Type.None;
}
