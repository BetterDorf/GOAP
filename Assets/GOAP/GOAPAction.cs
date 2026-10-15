using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

namespace GOAP
{
    [System.Serializable]
    public class Action
    {
        [SerializeField]
        WorldState conditions;
        [SerializeField]
        WorldState aboveConds;
        [SerializeField]
        WorldState belowConds;
        [SerializeField]
        WorldState additiveEffects;
        [SerializeField]
        WorldState setEffects;

        public float cost;

        // reference actor's actions
        public int actionRef;

        public bool CanBePerformed(WorldState worldState)
        {
            if (!worldState.Contains(conditions) || !worldState.ContainsAbove(aboveConds) || 
                !worldState.ContainsBelow(belowConds))
                return false;

            return true;
        }

        // Return a new WorldState affected by the action's effect
        public WorldState Perform(WorldState worldState)
        {
            WorldState world = new(worldState);
            world.SetStates(setEffects);
            world.AdditionStates(additiveEffects);

            return world;
        }

        // Return a worldstate corresponding the inverse of the action's effect
        public WorldState ReverseEffects(WorldState worldState)
        {
            WorldState world = new(worldState);
            world.SubstractStates(additiveEffects);
            return world;
        }

        // Return a worldstate that satisfies the condition given a base state
        public WorldState SolveForConditions(WorldState worldState)
        {
            WorldState world = new(worldState);
            world.SetStates(conditions);
            world.SetIfGreater(aboveConds);
            world.SetIfLesser(belowConds);

            return world;
        }
    }
}