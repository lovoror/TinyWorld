﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadItem : MonoBehaviour
{
    public enum Type
    {
        None,
        NackedA, NackedB, NackedC, NackedD, NackedE,
        HeadbandA, HeadbandB, HeadbandC, HeadbandD, HeadbandE, HeadbandF,
        HoodA, HoodB, HoodC, HoodD, HoodE,
        ChainmailA, ChainmailB, ChainmailC, ChainmailD, ChainmailE, ChainmailF,
        SpangenhelmA, SpangenhelmB, SpangenhelmC,
        SalletA, SalletB, SalletC, SalletD, SalletE, SalletF, SalletG,
        HornA, HornB, HornC, HornD, HornE, HornF, HornG, HornH, HornI,

        SalletH, SalletI, SalletJ,
        BacinetA, BacinetB, BacinetC, BacinetD, BacinetE,
        HelmA, HelmB, HelmC, HelmD, HelmE, HelmF, HelmG, HelmH, HelmI, HelmJ,
        HatA, HatB, HatC, HatD, HatE,
        CrownA, CrownB, CrownC, CrownD, CrownE,
        HelmK, HelmL
    };
    public Type type = Type.None;

    private void OnValidate()
    {
        this.name = type.ToString();
    }
}