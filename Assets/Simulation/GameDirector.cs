using GOAP;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ArenaNode
{
    public Vector3 Position;
    public GameObject Character;
    public int Id;
}

public class GameDirector : MonoBehaviour
{
    [SerializeField] List<GameObject> charactersPrefab;
    List<GameObject> charactersInstance;

    [SerializeField] int spacing;

    [SerializeField] GameObject nodeVisual;

    public List<ArenaNode> arena;
    [SerializeField] int arenaSize;
    [SerializeField] float nodeSpacing;

    private void Awake()
    {
        arena = new List<ArenaNode>();
        charactersInstance = new List<GameObject>();

        for (int i = 0; i < arenaSize; ++i)
        {
            ArenaNode node = new ArenaNode();
            node.Position = transform.position + Vector3.left * ((arenaSize / 2) - i - 0.5f) * nodeSpacing;
            node.Id = 0;

            Instantiate(nodeVisual, node.Position, Quaternion.identity);

            arena.Add(node);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int pos = arenaSize / charactersPrefab.Count - spacing / 2;

        pos = Math.Max(pos - 1, 0);

        for (int i = 0; i < charactersPrefab.Count; i++)
        {
            charactersInstance.Add(Instantiate(charactersPrefab[i], arena[pos].Position, Quaternion.identity));
            if (charactersInstance[i].TryGetComponent<EnemyAI>(out var enemyAI))
                enemyAI.SetGameDirector(this);
            arena[pos].Character = charactersInstance[i];

            pos = Math.Min(pos + spacing, arenaSize - 1);
        }
    }

    private void Update()
    {
        PlayTurn();
    }

    void PlayTurn()
    {
        foreach (var character in charactersInstance)
        {
            if (character.TryGetComponent<EnemyAI>(out var enemyAI))
            {
                enemyAI.PlayTurn();
            }
        }
    }

    public List<ArenaNode> CharaterPositions()
    {
        List<ArenaNode> nodes = new List<ArenaNode>();

        foreach (var node in arena)
        {
            if (node.Character != null)
            {
                nodes.Add(node);
            }
        }

        return nodes;
    }
}
