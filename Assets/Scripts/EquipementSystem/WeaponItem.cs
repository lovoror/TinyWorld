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
    public bool forbidSecond = false;
    public bool forbidShield = false;
    public int animationCode = 1;
    public float load = 0f;

    // helpers
    private void OnValidate()
    {
        /*Arsenal.Instance.InitializeTables();
        transform.parent.gameObject.GetComponent<WeaponSlot>().Equip(type);*/
    }

    // special
    public static WeaponItem none
    {
        get
        {
            WeaponItem item = new WeaponItem();
            item.type = Type.None;
            return item;
        }
    }
    public static void Copy(WeaponItem source, WeaponItem destination)
    {
        destination.type = source.type;
        destination.forbidSecond = source.forbidSecond;
        destination.forbidShield = source.forbidShield;
        destination.animationCode = source.animationCode;
        destination.load = source.load;
    }
}
