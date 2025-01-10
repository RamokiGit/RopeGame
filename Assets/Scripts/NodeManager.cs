using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NodeManager : MonoBehaviour
{
    public Action<TypeComplete> OnTypeComplete;
    public static NodeManager Instance; 
    
    [Header("Node Settings")]
    public GameObject nodePrefab;
    public int nodeCount = 5;
    public float spawnRadius = 5f;
    public Material lineMaterial;
    public float lineWidth = 0.1f;
    public float offsetDistance = 0.2f;
    
    
    private List<GameObject> _nodes = new();
    private List<LineRenderer> _ropes = new();
    private Dictionary<LineRenderer, (GameObject, GameObject)> _ropeToNodes = new();
    
    [Header("Materials Checker")]
    [SerializeField] Material _lineMaterialGreen;
    [SerializeField] Material _lineMaterialRed;
    [Header("Area")]
    [SerializeField] float _areaX;
    [SerializeField] float _areaY;
    
    [Header("SkipLevelParam")]
    [SerializeField] int _maxIterations = 3000; 
    [SerializeField] float _attractionStrength = 0.45f; 
    [SerializeField] float _repulsionStrength = 0.3f;  
    [SerializeField] float _stepSize = 0.2f;
    private void Awake()
    {
       Instance = this;
    }

    public void SetupLevel()
    {
        GenerateNodes();
        ConnectNodes();
        UpdateRopes();
        CheckForIntersections();
    }

    void Update()
    {
        if (GameStatus.CurrentGameState != GameState.Game) return;
        UpdateRopes();
        
    }
    
    void GenerateNodes()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            Vector2 position = Random.insideUnitCircle * spawnRadius;
            GameObject node = Instantiate(nodePrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
            node.name = $"Node_{i}";
            node.GetComponent<DraggableNode>().SetArea(_areaX, _areaY);
            _nodes.Add(node);
        }
    }
    void ConnectNodes()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            GameObject nodeA = _nodes[i];
            GameObject nodeB = _nodes[(i + 1) % _nodes.Count];

            LineRenderer rope1 = CreateRope(nodeA, nodeB);
            _ropes.Add(rope1);
            _ropeToNodes[rope1] = (nodeA, nodeB);

            LineRenderer rope2 = CreateRope(nodeB, nodeA);
            _ropes.Add(rope2);
            _ropeToNodes[rope2] = (nodeB, nodeA);
        }
    }

   
    LineRenderer CreateRope(GameObject startNode, GameObject endNode)
    {
        GameObject ropeObject = new GameObject("Rope");
        ropeObject.transform.parent = startNode.transform;

        LineRenderer lineRenderer = ropeObject.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;

        Vector3 startOffset = (endNode.transform.position - startNode.transform.position).normalized * offsetDistance;
        lineRenderer.SetPosition(0, startNode.transform.position + startOffset);
        lineRenderer.SetPosition(1, endNode.transform.position);

        return lineRenderer;
    }

   
    void UpdateRopes()
    {
        foreach (var rope in _ropes)
        {
            var (startNode, endNode) = _ropeToNodes[rope];
            Vector3 startOffset = (endNode.transform.position - startNode.transform.position).normalized * offsetDistance;

            rope.SetPosition(0, startNode.transform.position + startOffset);
            rope.SetPosition(1, endNode.transform.position);
        }
    }

    
    public bool CheckForIntersections(bool _isSkipingLevel = false)
    {
       
        bool hasIntersections = false;
        foreach (var rope in _ropes)
        {
            rope.material = _lineMaterialGreen;
        }

        for (int i = 0; i < _ropes.Count; i++)
        {
            for (int j = i + 1; j < _ropes.Count; j++)
            {
                if (!AreRopesFromSameNode(_ropes[i], _ropes[j]) && DoRopesIntersect(_ropes[i], _ropes[j]))
                {
                    _ropes[i].material = _lineMaterialRed;
                    _ropes[j].material = _lineMaterialRed;
                    hasIntersections = true;
                }
            }
        }
    
        if (hasIntersections == false && _isSkipingLevel == false)
        {
            GameStatus.CurrentGameState = GameState.CompleteLevel;
            OnTypeComplete?.Invoke(TypeComplete.SelfComplete);
            return true;
        }
        else if (hasIntersections == false && _isSkipingLevel == true)
        {
            GameStatus.CurrentGameState = GameState.CompleteLevel;
            OnTypeComplete?.Invoke(TypeComplete.Skip);
            return true;
        }

        return false;
    }

   
    bool DoRopesIntersect(LineRenderer rope1, LineRenderer rope2)
    {
        Vector3 a = rope1.GetPosition(0);
        Vector3 b = rope1.GetPosition(1);

        Vector3 c = rope2.GetPosition(0);
        Vector3 d = rope2.GetPosition(1);

        return AreSegmentsIntersecting(a, b, c, d);
    }

    bool AreRopesFromSameNode(LineRenderer rope1, LineRenderer rope2)
    {
        var (nodeA1, nodeB1) = _ropeToNodes[rope1];
        var (nodeA2, nodeB2) = _ropeToNodes[rope2];

        return nodeA1 == nodeA2 || nodeA1 == nodeB2 || nodeB1 == nodeA2 || nodeB1 == nodeB2;
    }

    bool AreSegmentsIntersecting(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        float cross1 = CrossProduct(c, d, a) * CrossProduct(c, d, b);
        float cross2 = CrossProduct(a, b, c) * CrossProduct(a, b, d);

        return cross1 <= 0 && cross2 <= 0;
    }

    float CrossProduct(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x);
    }

   
    [Button]
    public void ArrangeNodesToAvoidIntersections()
{
            
    bool hasIntersections;

    for (int iteration = 0; iteration < _maxIterations; iteration++)
    {
        hasIntersections = false;

        
        foreach (var node in _nodes)
        {
            Vector3 displacement = Vector3.zero;

            
            foreach (var otherNode in _nodes)
            {
                if (node == otherNode) continue;

                Vector3 direction = node.transform.position - otherNode.transform.position;
                float distance = direction.magnitude;
                if (distance < 0.1f) distance = 0.1f; 

                displacement += direction.normalized * (_repulsionStrength / (distance * distance));
            }

            
            foreach (var rope in _ropes)
            {
                var (startNode, endNode) = _ropeToNodes[rope];
                if (startNode == node)
                {
                    Vector3 direction = endNode.transform.position - node.transform.position;
                    displacement += direction * _attractionStrength;
                }
                else if (endNode == node)
                {
                    Vector3 direction = startNode.transform.position - node.transform.position;
                    displacement += direction * _attractionStrength;
                }
            }

            
            node.transform.position += displacement * _stepSize;
        }

        
        UpdateRopes();
        if (CheckForIntersections(true)) return;
    }
}
    

    
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position,new Vector3(_areaX,_areaY));
    }
}

