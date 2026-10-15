using GOAP;
using System;
using UnityEngine;

public delegate void ActionToPerform();

[System.Serializable]
public class SimulAction
{
    [SerializeField]
    public WorldState Conditions;
    [SerializeField]
    public WorldState AboveConds;
    [SerializeField]
    public WorldState BelowConds;
    [SerializeField]
    public WorldState AdditiveEffects;
    [SerializeField]
    public WorldState SetEffects;

    public float Cost;
    
    public ActionToPerform action;
}
