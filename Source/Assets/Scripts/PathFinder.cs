using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yejun.Tool
{
    public class PathFinder
    {
        private class Node
        {
            public enum State
            {
                None,
                Opened,
                Closed
            }

            public State CurState { get; set; }

            public Vector2Int Pos { get; private set; }

            public int CostG { get; private set; }

            public int CostH { get; private set; }

            public int TotalCost => CostG + CostH;

            public Node Parent { get; set; }

            public Node(int x, int y)
            {
                Pos = new Vector2Int(x, y);
            }

            public void Clear()
            {
                CurState = State.None;
                CostG = 0;
                CostH = 0;
                Parent = default;
            }

            public void CalcCost(Vector2Int target, Node parent = default)
            {
                Vector2Int deltaToTarget = Pos - target;

                deltaToTarget.x = Math.Abs(deltaToTarget.x);
                deltaToTarget.y = Math.Abs(deltaToTarget.y);

                CostH = (deltaToTarget.x + deltaToTarget.y) * 10;

                int costG = 0;

                if (parent != default)
                {
                    int distToParent = (int)(Vector2Int.Distance(Pos, parent.Pos) * 10);

                    if (distToParent > 10)
                    {
                        costG = parent.CostG + 14;
                    }
                    else if (distToParent == 10)
                    {
                        costG = parent.CostG + 10;
                    }
                }

                if (CostG == 0 || costG < CostG)
                {
                    Parent = parent;
                    CostG = costG;
                }
            }
        }

        private const int LIMIT_COUNT = 100000;

        public event Func<Vector2Int, bool> VerifyTerrain;

        public List<Vector2Int> Path { get; private set; }

        private readonly Dictionary<Vector2Int, Node> m_nodes;
        private readonly List<Node> m_opened;
        private readonly List<Node> m_closed;
        private readonly Vector2Int[] m_childrenOffsetPos;

        public PathFinder(int width, int height)
        {
            m_nodes = new Dictionary<Vector2Int, Node>();
            m_opened = new List<Node>();
            m_closed = new List<Node>();
            Path = new List<Vector2Int>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Node node = new Node(x, y);
                    m_nodes.Add(node.Pos, node);
                }
            }

            m_childrenOffsetPos = new Vector2Int[] {
                Vector2Int.up,
                Vector2Int.one,
                Vector2Int.right,
                Vector2Int.right + Vector2Int.down,
                Vector2Int.down,
                Vector2Int.down + Vector2Int.left,
                Vector2Int.left,
                Vector2Int.left + Vector2Int.up
            };
        }

        private void Clear()
        {
            m_opened.Clear();
            m_closed.Clear();

            foreach (var kv in m_nodes)
            {
                kv.Value.Clear();
            }
        }

        /// <summary>
        /// 길찾기
        /// </summary>
        /// <param name="startPos">시작 위치</param>
        /// <param name="targetPos">도착 위치</param>
        /// <returns>성공 여부</returns>
        public bool FindPath(Vector2Int startPos, Vector2Int targetPos)
        {
            Clear();

            Node startNode = m_nodes[startPos];
            startNode.CalcCost(targetPos);
            startNode.CurState = Node.State.Closed;
            m_closed.Add(startNode);

            Node curNode = startNode;
            for (int i = 0; i < LIMIT_COUNT; i++)
            {
                if (curNode == default)
                {
                    break;
                }

                if (curNode.Pos == targetPos)
                {
                    break;
                }

                OpenChildNode(curNode, targetPos);

                curNode = GetLowestCostOpenedNode();
                curNode.CurState = Node.State.Closed;
                m_closed.Add(curNode);
            }

            Path.Clear();
            while (curNode != default)
            {
                Path.Insert(0, curNode.Pos);
                curNode = curNode.Parent;
            }

            return Path.Count > 0;
        }

        private Node GetLowestCostOpenedNode()
        {
            Node node = m_opened.FirstOrDefault();

            if (node != default)
            {
                node.CurState = Node.State.None;
                m_opened.Remove(node);
            }

            return node;
        }

        private void OpenChildNode(Node parent, Vector2Int targetPos)
        {
            foreach (var pos in m_childrenOffsetPos)
            {
                Vector2Int childPos = parent.Pos + pos;

                if (m_nodes.TryGetValue(childPos, out var childNode))
                {
                    if (childNode.CurState != Node.State.None)
                    {
                        continue;
                    }

                    if (!VerifyTerrain?.Invoke(childPos) ?? false)
                    {
                        continue;
                    }

                    childNode.CalcCost(targetPos, parent);
                    childNode.CurState = Node.State.Opened;

                    if (m_opened.Count > 0 && childNode.TotalCost < m_opened[0].TotalCost)
                    {
                        m_opened.Insert(0, childNode);
                    }
                    else
                    {
                        m_opened.Add(childNode);
                    }
                }
            }
        }
    }
}