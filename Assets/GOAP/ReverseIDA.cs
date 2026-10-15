using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace GOAP
{
    public class ReverseIDA : MonoBehaviour
    {
        [SerializeField]
        List<Action> actions;

        [SerializeField]
        WorldState initialWorld;

        [SerializeField]
        WorldState goalWorld;
        // 0 is exact ; 1 is in the positive direction ; -1 means the value can be below the goal but not above
        [SerializeField]
        int canExceedGoal = 0;

        [SerializeField] private float maxHeuri;
        [SerializeField] private float heuristicMult;

        List<GOAPNode> nodes = new List<GOAPNode>();

        private void Start()
        {
            Stack<GOAPNode> path = new Stack<GOAPNode>();

            // IDA*
            GOAPNode root = new GOAPNode(0, -1, goalWorld, -1);
            nodes.Add(root);
            float bound = Heuristic(root);
            Debug.Log(bound);
            path.Push(root);

            do
            {
                Search(path, 0.0f, bound, out bound);
                Debug.Log(bound);
                if (TestGoal(path.Peek()))
                {
                    foreach (var n in path)
                    {
                        string s = "";
                        foreach (var a in n.State.state.Values)
                            s += a.ToString() + " ";
                        Debug.Log(n.ActionId + " ; " + s);
                    }
                    return;
                }
            } while (bound < maxHeuri);
        }

        public bool Search(Stack<GOAPNode> path, float costG, float bound, out float newBound)
        {
            GOAPNode node = path.Peek();
            newBound = costG + Heuristic(node);
            if (newBound > bound)
            {
                return false;
            }
            else if (TestGoal(node))
            {
                return true;
            }

            float min = Mathf.Infinity;
            foreach (var succ in ExpandSuccessors(node))
            {
                if (!IsStateOnPath(nodes[succ], path))
                {
                    path.Push(nodes[succ]);

                    float nextG = costG + actions[nodes[succ].ActionId].cost;

                    if (Search(path, nextG, bound, out newBound))
                        return true;

                    if (newBound < min)
                        min = newBound;

                    path.Pop();
                    //path.Push(nodes[succ]);
                    //if (TestGoal(path.Peek()) || Search(path, costG + actions[nodes[succ].ActionId].cost, bound, out newBound))
                    //    return true;
                    //if (newBound < min)
                    //    min = newBound;
                    //path.Pop();
                }
                else
                {
                    nodes.RemoveAt(succ);
                }
            }
            newBound = min;
            return false;
        }

        private bool IsStateOnPath(GOAPNode candidate, Stack<GOAPNode> path)
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
                    distance += Math.Abs(keypair.Value - from.state[keypair.Key]);
                }
            }

            return distance * heuristicMult;
        }

        public bool TestGoal(GOAPNode node)
        {
            switch (canExceedGoal)
            {
                case 0: 
                    if (node.State.IntersectEquals(goalWorld))
                    { return true; }
                    else
                        break;
                case 1:
                    if (node.State.IntersectAboveOrEqual(goalWorld))
                    { return true; }
                    else
                        break;
                case -1:
                    if (node.State.IntersectBelowOrEqual(goalWorld))
                    { return true; }
                    else
                        break;
                default:
                    throw new Exception("Invalid case");
            }

            return false;
        }

        public List<int> ExpandSuccessors(GOAPNode node)
        {
            List<int> succs = new List<int>();

            for (int i = 0; i < actions.Count; i++)
            {
                Action action = actions[i];
                if (action.CanBePerformed(node.State))
                {
                    GOAPNode newNode = new GOAPNode(nodes.Count, node.Id, action.Perform(node.State), i);
                    nodes.Add(newNode);
                    succs.Add(newNode.Id);
                }
            }

            //succs.Sort(delegate (int x, int y)
            //{
            //    float x2 = Heuristic(x);
            //    float y2 = Heuristic(y);
            //    if (x2 < y2)
            //        return -1;
            //    else if (x2 > y2)
            //        return 1;
            //    else
            //        return 0;
            //});

            return succs;
        }
    }
}