using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LightTransport;


namespace GOAP
{
    [System.Serializable]
    public class WorldState
    {
        [SerializeField]
        public Dictionary<string, int> state;

        public WorldState()
        {
            state = new Dictionary<string, int> ();
        }

        public WorldState(WorldState world)
        {
            state = new Dictionary<string, int>();
            state = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> pair in world.state)
            {
                state.Add(pair.Key, pair.Value);
            }
        }

        public WorldState(Dictionary<string, int> initialisationState)
        {
            state = new Dictionary<string, int>();

            foreach (KeyValuePair<string, int> pair in initialisationState)
            {
                state.Add(pair.Key, pair.Value);
            }
        }

        public void SetValue(string name, int value)
        {
            if (state.ContainsKey(name))
            {
                state[name] = value;
            }
            else
            {
                state.Add(name, value);
            }
        }

        public void AdditionStates(WorldState world)
        {
            foreach (var pair in world.state)
            {
                if (state.ContainsKey(pair.Key))
                {
                    state[pair.Key] += pair.Value;
                }
                else
                {
                    state.Add(pair.Key, pair.Value);
                }
            }
        }
        public void SubstractStates(WorldState world)
        {
            foreach (var pair in world.state)
            {
                if (state.ContainsKey(pair.Key))
                {
                    state[pair.Key] -= pair.Value;
                }
                else
                {
                    state.Add(pair.Key, -pair.Value);
                }
            }
        }

        public void SetStates(WorldState world)
        {
            foreach (var pair in world.state)
            {
                if (state.ContainsKey(pair.Key))
                {
                    state[pair.Key] = pair.Value;
                }
                else
                {
                    state.Add(pair.Key, pair.Value);
                }
            }
        }

        public void SetIfGreater(WorldState values)
        {
            foreach (var pair in values.state)
            {
                if (state.ContainsKey(pair.Key))
                {
                    if (state[pair.Key] < pair.Value)
                        state[pair.Key] = pair.Value;
                }
                else
                {
                    state.Add(pair.Key, pair.Value);
                }
            }
        }

        public void SetIfLesser(WorldState values)
        {
            foreach (var pair in values.state)
            {
                if (state.ContainsKey(pair.Key))
                {
                    if (state[pair.Key] > pair.Value)
                        state[pair.Key] = pair.Value;
                }
                else
                {
                    state.Add(pair.Key, pair.Value);
                }
            }
        }

        public bool IntersectAboveOrEqual(WorldState testValues)
        {
            foreach (var pair in testValues.state)
            {
                if (!state.ContainsKey(pair.Key))
                {
                    continue;
                }
                if (state[pair.Key] < pair.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public bool ContainsAbove(WorldState world)
        {
            foreach (var pair in world.state)
            {
                if (!state.ContainsKey(pair.Key) || state[pair.Key] < pair.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IntersectBelowOrEqual(WorldState testValues)
        {
            foreach (var pair in testValues.state)
            {
                if (!state.ContainsKey(pair.Key))
                {
                    continue;
                }
                if (state[pair.Key] > pair.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public bool ContainsBelow(WorldState world)
        {
            foreach (var pair in world.state)
            {
                if (!state.ContainsKey(pair.Key) || state[pair.Key] > pair.Value)
                {
                    return false;
                }
            }

            return true;
        }


        public bool IntersectEquals(WorldState world)
        {
            if (IntersectEquals(world.state, state))
            {
                return true;
            }
            return false;
        }

        public bool Contains(WorldState world)
        {
            foreach (var pair in world.state)
            {
                if (!state.ContainsKey(pair.Key) || state[pair.Key] != pair.Value)
                {
                    return false;
                }
            }

            return true;
        }

        // check if the state of both worlds is the same where they intersect
        static bool IntersectEquals(Dictionary<string, int> state1, Dictionary<string, int> state2)
        {
            foreach (var pair in state1)
            {
                if (state2.ContainsKey(pair.Key) && state2[pair.Key] != pair.Value)
                {
                    return false;
                }
            }

            return true;
        }
    }
}