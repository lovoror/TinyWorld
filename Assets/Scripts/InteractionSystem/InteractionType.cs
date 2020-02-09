using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionType : MonoBehaviour
{
    public enum Type
    {
        pickableBackpack,
        pickableHead,
        pickableWeapon,
        pickableSecond,
        pickableBody,
        pickableShield,

        collectWood,
        collectStone,
        collectIron,
        collectGold,
        collectCrystal,

        collectWheet
    };
    public Type type;
}
