using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using FactoryFramework;

public class ResourceSpawn : Resource
{
    public float radius = .5f;
    public Vector2 region = new Vector2(5,5);

    private List<Vector2> Points;

    // Start is called before the first frame update
    void Awake()
    {
        if (item == null)
        {
            Debug.LogError("Resource Spawn Area does not have assigned resource type");
            return;
        }

        RegeneratePoints();
        // teleport to origin
        Vector3 oldPos = transform.position;
        Quaternion oldRot = transform.rotation;
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;
        // create meshes
        foreach(Vector2 point in Points)
        {
            Vector3 pos = transform.TransformPoint(new Vector3(point.x, 0f, point.y));
            GameObject obj = Instantiate(item.prefab, transform);
            obj.name = "Resource Model";
            obj.transform.position = pos;
            obj.transform.rotation = transform.rotation * Quaternion.AngleAxis(Random.value * 360f, transform.up);
        }
        // combine meshes
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        // grab material
        Material m = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterial;
        // add meshes to combine
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            //meshFilters[i].gameObject.SetActive(false);
            Destroy(meshFilters[i].gameObject);
        }
        // setup rendering and combine into one mesh
        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = m;
        filter.mesh = new Mesh();
        filter.mesh.CombineMeshes(combine);
        // reset position and rotation
        transform.position = oldPos;
        transform.rotation = oldRot;

        // add a trigger 
        BoxCollider bc = gameObject.AddComponent<BoxCollider>();
        bc.size = new Vector3(region.x, 2f, region.y);
        bc.center = Vector3.up * 1f;
    }

    public List<Vector2> GeneratePoints(float radius, Vector2 regionSize, int numSamplesBeforeRejection=30)
    {
        // sebastian lague possion disc sampling https://www.youtube.com/watch?v=7WcmyxyFO7o
        float cellSize = radius / Mathf.Sqrt(2);
        int[,] grid = new int[Mathf.CeilToInt(regionSize.x / cellSize), Mathf.CeilToInt(regionSize.y / cellSize)];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(regionSize / 2);
        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCetner = spawnPoints[spawnIndex];

            bool accepted = false;
            for (int i = 0; i <numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCetner + dir * Random.Range(radius, 2 * radius);
                if (isValid(candidate, regionSize, cellSize, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x/cellSize),(int)(candidate.y/cellSize)] = points.Count;
                    accepted = true;
                    break;
                }
            }
            if (!accepted)
            {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
        return points.Select(p => p - regionSize/2f).ToList();

    }

    [ContextMenu("Regenerate Points")]
    public void RegeneratePoints()
    {
        Points = GeneratePoints(radius, region);
    }

    bool isValid(Vector2 candidate, Vector2 regionSize, float cellsSize, List<Vector2> points, int[,] grid)
    {
        if (candidate.x >= 0 && candidate.x < regionSize.x && candidate.y >= 0 && candidate.y < regionSize.y)
        {
            int cellx = (int)(candidate.x / cellsSize);
            int celly = (int)(candidate.y / cellsSize);
            int searchStartX = Mathf.Max(0, cellx - 2);
            int searchEndX = Mathf.Min(cellx + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, celly - 2);
            int searchEndY = Mathf.Min(celly + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex!= -1)
                    {
                        float dist = (candidate - points[pointIndex]).sqrMagnitude;
                        if (dist < radius*radius) return false;

                    }
                }
            }
            return true;
        }
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color c = (item == null) ? Color.black : item.DebugColor;
        c.a = .25f;
        Gizmos.color = c;
        Handles.color = c;
        Gizmos.matrix = transform.localToWorldMatrix;
        Handles.matrix = transform.localToWorldMatrix;

        Rect rect = new Rect(Vector2.zero, region);
        Gizmos.DrawCube(Vector3.zero, new Vector3(region.x,.15f,region.y));

        c.a = 1f;
        Handles.color = c;
        if (Points == null) RegeneratePoints();
        foreach(Vector3 point in Points)
        {
            Handles.DrawSolidDisc(new Vector3(point.x,0f,point.y), transform.up, 0.1f);
        }
        
    }
#endif
}
