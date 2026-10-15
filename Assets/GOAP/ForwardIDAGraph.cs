using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using GOAP;
namespace GOAPVisualAide
{
    public struct Node
    {
        public Node(int id, int parentId, WorldState state, int actionId)
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
        List<GOAP.Action> actions;

        [SerializeField]
        WorldState initialWorld;

        [SerializeField]
        WorldState goalWorld;
        // 0 is exact ; 1 is in the positive direction ; -1 means the value can be below the goal but not above
        [SerializeField]
        int canExceedGoal = 0;

        [SerializeField] private float maxHeuri;
        [SerializeField] private float heuristicMult;

        List<Node> nodes = new List<Node>();

        // Graph drawing
        [SerializeField] private bool drawGraph = true;
        [SerializeField] private int graphColumns = 12;
        [SerializeField] private float nodeSpacing = 1.5f;
        [SerializeField] private float nodeVerticaldeviation = 0.5f;
        [SerializeField] private float nodeRadius = 0.25f;

        private void Start()
        {
            Stack<Node> path = new Stack<Node>();

            // IDA*
            Node root = new Node(0, -1, initialWorld, -1);
            nodes.Add(root);
            float bound = Heuristic(root);
            path.Push(root);

            do
            {
                if (Search(path, 0.0f, bound, out float newBound) || TestGoal(path.Peek()))
                {
                    return;
                }

                bound = newBound;
            } while (bound < maxHeuri);

            Debug.Log(nodes.Count);
        }

        public bool Search(Stack<Node> path, float costG, float bound, out float newBound)
        {
            Node node = path.Peek();

            newBound = costG + Heuristic(node);

            if (TestGoal(node))
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
                if (IsStateOnPath(nodes[succ], path))
                {
                    nodes.RemoveAt(succ);
                    continue;
                }

                path.Push(nodes[succ]);

                if (Search(path, costG + actions[nodes[succ].ActionId].cost, bound, out float successorNewBound))
                    return true;

                if (successorNewBound < min)
                    min = successorNewBound;

                path.Pop();
            }

            newBound = min;

            return false;
        }

        private bool IsStateOnPath(Node candidate, Stack<Node> path)
        {
            foreach (Node node in path)
            {
                if (node.State.Contains(candidate.State) &&
                    candidate.State.Contains(node.State))
                {
                    return true;
                }
            }

            return false;
        }

        public float Heuristic(Node node)
        {
            return Heuristic(node.State, goalWorld);
        }

        public float Heuristic(WorldState from, WorldState to)
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

        public bool TestGoal(Node node)
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

        public List<int> ExpandSuccessors(Node node)
        {
            List<int> succs = new List<int>();

            for (int i = 0; i < actions.Count; i++)
            {
                GOAP.Action action = actions[i];
                if (action.CanBePerformed(node.State))
                {
                    Node newNode = new Node(nodes.Count, node.Id, action.Perform(node.State), i);
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

        private Vector3 GetNodePosition(int id)
        {
            int columns = Mathf.Max(1, graphColumns);

            return transform.position + new Vector3(
                (id % columns) * nodeSpacing,
                -(id / columns) * nodeSpacing - (id % 2) * nodeVerticaldeviation,
                0f
            );
        }

        private void OnDrawGizmos()
        {
            if (!drawGraph || nodes == null || nodes.Count == 0)
                return;

            foreach (Node node in nodes)
            {
                if (node.ParentId < 0 || node.ParentId >= nodes.Count)
                    continue;

                Gizmos.color = Color.gray;
                Gizmos.DrawLine(
                    GetNodePosition(node.ParentId),
                    GetNodePosition(node.Id)
                );
            }

            foreach (Node node in nodes)
            {
                if (node.Id == 0)
                    Gizmos.color = Color.cyan;
                else if (TestGoal(node))
                    Gizmos.color = Color.green;
                else
                    Gizmos.color = Color.white;

                Vector3 position = GetNodePosition(node.Id);
                Gizmos.DrawSphere(position, nodeRadius);

#if UNITY_EDITOR
                Handles.Label(
                    position + Vector3.up * nodeRadius,
                    $"ID:{node.Id} (A{node.ActionId})"
                );
#endif
            }
        }
    }
}