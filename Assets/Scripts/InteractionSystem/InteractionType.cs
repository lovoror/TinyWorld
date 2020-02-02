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
        pickableShield
    };
    public Type type;
}
