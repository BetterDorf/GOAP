using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace GOAP
{
    public struct GOAPNode
    {
        public GOAPNode(int id, int parentId, WorldState state, int actionId)
        {
            Id = id;
            ParentId = parentId;
            State = state;
            ActionId = actionId;
        }

        public int Id;
        public int ParentId;
        public WorldState State;
        // Register the action taken to reach this world state
        public int ActionId;
    }

    public class ForwardIDA : MonoBehaviour
    {
        [SerializeField]
        List<Action> actions;

        // 0 is exact ; 1 is in the positive direction ; -1 means the value can be below the goal but not above
        [SerializeField]
        int canExceedGoal = 0;

        [SerializeField] private float maxHeuri;
        [SerializeField] private float heuristicMult;
        int nodesCreated = 0;

        private WorldState currentWorld;
        private WorldState goalWorld;

        public void ChangeGoal(WorldState newGoal, int canExceed)
        {
            goalWorld = newGoal;
            canExceedGoal = canExceed;
        }

        public void GatherSensorData()
        {
            // TODO change currentWorld
        }

        public void SetActions(List<Action> newActions)
        {
            actions = newActions;
        }
  
        public List<GOAPNode> Plan()
        {
            return Plan(goalWorld);
        }

        public List<GOAPNode> Plan(WorldState to)
        {
            return Plan(currentWorld, to);
        }

        public List<GOAPNode> Plan(WorldState from, WorldState to)
        {
            GatherSensorData();
            List<GOAPNode> path = new List<GOAPNode>();

            // IDA*
            GOAPNode root = new GOAPNode(nodesCreated++, -1, from, -1);
            path.Add(root);
            float bound = Heuristic(root);

            do
            {
                if (Search(path, 0.0f, bound, out float newBound, to) || TestGoal(path[path.Count - 1], to))
                {
                    return path;
                }

                bound = newBound;
            } while (bound < maxHeuri);

            return null;
        }

        public bool Search(List<GOAPNode> path, float costG, float bound, out float newBound, WorldState goal)
        {
            GOAPNode node = path[path.Count - 1];
        
            newBound = costG + Heuristic(node);

            if (TestGoal(node, goal))
            {
                return true;
            }
            if (newBound > bound)
            {
                return false;
            }

            float min = Mathf.Infinity;
            foreach (var succ in ExpandSuccessors(node))
            {
                if (IsStateOnPath(succ, path))
                {
                    continue;
                }

                path.Add(succ);

                if (Search(path, costG + actions[succ.ActionId].cost, bound, out float successorNewBound, goal))
                    return true;

                if (successorNewBound < min)
                    min = successorNewBound;

                path.Remove(succ);
            }

            newBound = min;

            return false;
        }

        private bool IsStateOnPath(GOAPNode candidate, List<GOAPNode> path)
        {
            foreach (GOAPNode node in path)
            {
                if (node.State.Contains(candidate.State) &&
                    candidate.State.Contains(node.State))
                {
                    return true;
                }
            }

            return false;
        }

        public float Heuristic(GOAPNode node)
        {
            return Heuristic(node.State, goalWorld);
        }

        public float Heuristic(WorldState from,  WorldState to)
        {
            float distance = 0f;
            foreach (var keypair in to.state)
            {
                if (from.state.ContainsKey(keypair.Key))
                {
                    float difference = keypair.Value - from.state[keypair.Key];

                    if (difference * canExceedGoal < 0.0f)
                        difference = 0.0f;

                    distance += Mathf.Abs(difference);
                }
            }

            return distance * heuristicMult;
        }

        public bool TestGoal(GOAPNode node, WorldState goal)
        {
            switch (canExceedGoal)
            {
                case 0: 
                    if (node.State.IntersectEquals(goal))
                    { return true; }
                    else
                        break;
                case 1:
                    if (node.State.IntersectAboveOrEqual(goal))
                    { return true; }
                    else
                        break;
                case -1:
                    if (node.State.IntersectBelowOrEqual(goal))
                    { return true; }
                    else
                        break;
                default:
                    throw new Exception("Invalid case");
            }

            return false;
        }

        public List<GOAPNode> ExpandSuccessors(GOAPNode node)
        {
            List<GOAPNode> succs = new List<GOAPNode>();

            for (int i = 0; i < actions.Count; i++)
            {
                Action action = actions[i];
                if (action.CanBePerformed(node.State))
                {
                    GOAPNode newNode = new GOAPNode(nodesCreated++, node.Id, action.Perform(node.State), i);
                    succs.Add(newNode);
                }
            }

            return succs;
        }
    }
}