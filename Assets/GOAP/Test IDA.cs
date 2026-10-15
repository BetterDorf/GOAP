using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace testGOAP
{

    public class TestIDA : MonoBehaviour
    {
        [SerializeField] private int numberNodes;
        [SerializeField] private int cost1;
        [SerializeField] private int cost2;
        [SerializeField] private int maxHeuri;

        List<Node> nodes;
        private void Start()
        {
            nodes = new List<Node>();
            for (int i = 0; i < numberNodes; i++)
            {
                Node n = new Node();
                n.id = i;
                n.connected_nodes = new List<int>() { (i + 1) % numberNodes, (i * 2) % numberNodes };
                nodes.Add(n);
            }

            Stack<Node> path = new Stack<Node>();

            // IDA*
            Node root = nodes[0];
            float bound = Heuristic(root);
            path.Push(root);

            do
            {
                Search(path, 0.0f, bound, out bound);
                if (TestGoal(path.Peek()))
                {
                    Debug.Log(path);
                    foreach (var n in path)
                    {
                        Debug.Log(n.id);
                    }
                    return;
                }
            } while (true);
        }

        public bool Search(Stack<Node> path, float costG, float bound, out float newBound)
        {
            Node node = path.Peek();
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
                if (!path.Contains(nodes[succ]))
                {
                    path.Push(nodes[succ]);
                    if (TestGoal(path.Peek()) || Search(path, costG + Cost(node, nodes[succ]), bound, out newBound))
                        return true;
                    if (newBound < min)
                        min = newBound;
                    path.Pop();
                }
            }
            newBound = min;
            return false;
        }

        public float Heuristic(Node node)
        {
            return Heuristic(node.id);
        }

        public float Heuristic(int id)
        {
            return Mathf.Sqrt(numberNodes - id) * 3;
        }

        public int Cost(Node node, Node succ)
        {
            if (!node.connected_nodes.Contains(succ.id))
            {
                throw new ArgumentOutOfRangeException("node is not a successor");
            }

            for (int i = 0; i < node.connected_nodes.Count; i++)
            {
                if (node.connected_nodes[i] == succ.id)
                {
                    return cost1 + i * cost2;
                }
            }

            Debug.LogError("Shouldn't happen");
            return -1;
        }

        public bool TestGoal(Node node)
        {
            if (node.id == numberNodes - 1)
                return true;
            return false;
        }

        public List<int> ExpandSuccessors(Node node)
        {
            List<int> succs = new List<int>(node.connected_nodes);
            succs.Sort(delegate (int x, int y)
            {
                float x2 = Heuristic(x);
                float y2 = Heuristic(y);
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


    public struct Node
    {
        public int id;
        public List<int> connected_nodes;
    }
}