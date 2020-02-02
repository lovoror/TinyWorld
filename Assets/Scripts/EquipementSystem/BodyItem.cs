using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyItem : MonoBehaviour
{
    public enum Type
    {
        None,
        NeckedA, NeckedB, NeckedC,
        DressA, DressB, DressC, DressD, DressE,
        CivilianA, CivilianB, CivilianC, CivilianD, CivilianE,
        LeatherA, LeatherB, LeatherC, LeatherD,
        LightA, LightB, LightC, LightD, LightE, LightF, LightG, LightH,
        MediumA, MediumB, MediumC, MediumD, MediumE, MediumF, MediumG, MediumH,
        HeavyA, HeavyB, HeavyC, HeavyD, HeavyE, HeavyF, HeavyG, HeavyH, HeavyI,
        NobleA, NobleB
    };
    public Type type = Type.None;
}
