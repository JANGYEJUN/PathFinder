using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Yejun.Tool
{
    public class Terrain2D : MonoBehaviour
    {
        [SerializeField]
        private GameObject tile;

        [SerializeField]
        private List<SpriteRenderer> walls;

        [SerializeField]
        private GameObject start;

        [SerializeField]
        private GameObject target;

        [SerializeField]
        private int WIDTH = 100;

        [SerializeField]
        private int HEIGHT = 100;


        private PathFinder m_pathFinder;
        private Dictionary<Vector2Int, bool> m_walkable;
        private DateTime m_time;

        void Start()
        {
            m_time = DateTime.UtcNow;

            m_walkable = new Dictionary<Vector2Int, bool>();
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    Vector2Int posInt = new Vector2Int(x, y);
                    Vector2 pos = posInt;
                    m_walkable.Add(posInt, !walls.Any(t => t.bounds.Contains(pos)));
                }
            }

            m_pathFinder = new PathFinder(WIDTH, HEIGHT);
            m_pathFinder.VerifyTerrain += PathFinder_VerifyTerrain;

            Debug.Log($"Finish init: {(DateTime.UtcNow - m_time).TotalMilliseconds}");
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(Vector2.zero, new Vector2(150, 50)), "Find Path"))
            {
                m_time = DateTime.UtcNow;

                var suc = m_pathFinder.FindPath(Vector2Int.CeilToInt(start.transform.position), Vector2Int.CeilToInt(target.transform.position));

                Debug.Log($"Finish find path: {(DateTime.UtcNow - m_time).TotalMilliseconds}");
                m_time = DateTime.UtcNow;

                if (suc)
                {
                    var objects = GameObject.FindGameObjectsWithTag("Respawn");
                    foreach (var obj in objects)
                    {
                        DestroyImmediate(obj);
                    }

                    foreach (var pos in m_pathFinder.Path)
                    {
                        Vector2 localPos = pos;
                        GameObject go = Instantiate(tile);
                        go.transform.localPosition = localPos;
                        go.GetComponent<SpriteRenderer>().color = Color.green;
                        go.name = "Path";
                        go.SetActive(true);
                    }

                    Debug.Log($"Finish draw path: {(DateTime.UtcNow - m_time).TotalMilliseconds}");
                }
            }
        }

        private bool PathFinder_VerifyTerrain(Vector2Int arg)
        {
            if (m_walkable.TryGetValue(arg, out var value))
            {
                return value;
            }

            return false;
        }
    }
}