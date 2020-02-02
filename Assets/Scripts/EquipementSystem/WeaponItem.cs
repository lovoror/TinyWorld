using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItem : MonoBehaviour
{
    public enum Type
    {
        None,
        AxeA, AxeB, AxeC, AxeD, AxeE, BigAxeA, BigAxeB,
        Bardiche,
        BroadSwordA, BroadSwordB, ShortSwordA, ShortSwordB, SwordA, SwordB, BigSwordA, BigSwordB,
        Club, BigClub,
        CrossbowA, CrossbowB,
        DaggerA, DaggerB, DaggerC,
        Glaive,
        Halberd,
        Hammer, Warhammer, BigWarhammerA, BigWarhammerB,
        MaceA, MaceB, 
        MaulA, MaulB, BigMaulA, BigMaulB,
        MorningStar,
        Pickaxe,
        Pike,
        Sabre,
        Claymore,
        Spear
    };
    public Type type = Type.None;
}
