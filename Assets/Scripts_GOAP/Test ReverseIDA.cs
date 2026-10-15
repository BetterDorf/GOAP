using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace testGOAP
{
    public class TestReverseIDA : MonoBehaviour
    {
        [SerializeField] private int numberNodes;
        [SerializeField] private int cost1;
        [SerializeField] private int cost2;
        [SerializeField] private int maxHeuri;

        List<ReverseNode> nodes;
        List<Movement> moveSet;
        private void Start()
        {
            // Create the initial nodes
            nodes = new List<ReverseNode>();
            for (int i = 0; i < numberNodes; i++)
            {
                ReverseNode n = new ReverseNode();
                n.id = i;
                nodes.Add(n);
            }

            // Create the relevant actions
            moveSet = new List<Movement>();
            moveSet.Add(new Movement(cost1, (ReverseNode n) => (n.id + 1) % numberNodes,
                (ReverseNode n) => (n.id + numberNodes) % numberNodes));
            moveSet.Add(new Movement(cost1 + cost2 * 1, (ReverseNode n) => (n.id * 2) % numberNodes,
                (ReverseNode n) => (n.id + numberNodes) % numberNodes));

            Stack<ReverseNode> path = new Stack<ReverseNode>();

            // IDA*
            ReverseNode root = nodes[0];
            float bound = Heuristic(root);
            path.Push(root);

            do
            {
                float t = Search(path, 0.0f, bound);
                if (TestGoal(path.Peek()))
                {
                    Debug.Log(path);
                    foreach (var n in path)
                    {
                        Debug.Log(n.id);
                    }
                    return;
                }

                bound = t;

                Debug.Log("bound : " + bound);

                if (bound > maxHeuri)
                {
                    Debug.Log("max exceeded");
                    return;
                }
            } while (true);
        }

        public float Search(Stack<ReverseNode> path, float costG, float bound)
        {
            ReverseNode node = path.Peek();
            float f = costG + Heuristic(node);
            if (f > bound || TestGoal(node))
                return f;

            float min = Mathf.Infinity;
            foreach (var succ in ExpandSuccessors(node))
            {
                if (!path.Contains(nodes[succ.Item1]))
                {
                    path.Push(nodes[succ.Item1]);
                    float x = Search(path, costG + succ.Item2, bound);
                    if (TestGoal(path.Peek()))
                        return f;
                    if (x < min)
                        min = x;
                    path.Pop();
                }
            }
            return min;
        }

        public float Heuristic(ReverseNode node)
        {
            return Heuristic(node.id);
        }

        public float Heuristic(int id)
        {
            return Mathf.Sqrt(numberNodes - id) * cost2;
        }

        public bool TestGoal(ReverseNode node)
        {
            if (node.id == numberNodes - 1)
                return true;
            return false;
        }

        public List<Tuple<int, int>> ExpandSuccessors(ReverseNode node)
        {
            List<Tuple<int, int>> succs = new List<Tuple<int, int>>();
            foreach (var movement in moveSet)
            {
                succs.Add(new Tuple<int, int> (movement.forward(node), movement.cost));
            }
            succs.Sort(delegate (Tuple<int, int> x, Tuple<int, int> y)
            {
                float x2 = Heuristic(x.Item2);
                float y2 = Heuristic(y.Item2);
                if (x2 < y2)
                    return -1;
                else if (x2 > y2)
                    return 1;
                else
                    return 0;
            });

            return succs;
        }
    }


    public struct ReverseNode
    {
        public int id;
    }

    public struct Movement
    {
        public Movement(int _cost, Func<ReverseNode, int> _forward, Func<ReverseNode, int> _backward)
        {
            cost = _cost;
            forward = _forward;
            backward = _backward;
        }

        public int cost;

        public Func<ReverseNode, int> forward;
        public Func<ReverseNode, int> backward;
    }
}