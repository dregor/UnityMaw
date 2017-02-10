using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class maw : MonoBehaviour
{
    public uint layers = 1;
    public uint angles = 5;
    public float radiusOut = 20f;
    public float radiusIn = 10f;
    public float depth = 10f;

    public Color dotColor = new Color(150, 150, 150);
    private List<Vector3> allVertices = new List<Vector3>();
    private Mesh mesh;

    private static IEnumerable<Vector3> polyhedron(float r, uint n, float z)
    {
        for (int i = 0; i < n; i++)
        {
            yield return new Vector3(r * Mathf.Sin(2 * i * Mathf.PI / n), r * Mathf.Cos(2 * i * Mathf.PI / n), z);
        }
    }

    private IEnumerable<Vector3> layerOfPolyhedron(uint layer, float radius)
    {
        float z = 0f;
        if (layer > 0) { z = layer * depth / layers; }
        foreach (Vector3 vertice in polyhedron(radius, angles, z))
        {
            yield return vertice;
        }
    }

    private IEnumerator Generate()
    {
        mesh = new Mesh();
        mesh.name = "polyhedron";
        GetComponent<MeshFilter>().mesh = mesh;
        int[] triangles = new int[layers * angles * 6];
        uint index = 0;
        for (uint layer = 0; layer < layers; layer++)
        {
            foreach (Vector3 vertice in layerOfPolyhedron(layer, radiusOut))
            {
                MakeAText(vertice, index.ToString());
                allVertices.Add(vertice);
                index++;
            }
            foreach (Vector3 vertice in layerOfPolyhedron(layer, radiusIn))
            {
                MakeAText(vertice, index.ToString());
                allVertices.Add(vertice);
                index++;
            }
            mesh.vertices = allVertices.ToArray();

            yield return false;

        }
        mesh.vertices = allVertices.ToArray();

        for (int i = 0, j = 0; i < angles-1; i++)
        {
            triangles[j] = i;
            triangles[j + 3] = triangles[j + 2] = i + (int)angles;
            triangles[j + 4] = triangles[j + 1] = i + 1;
            triangles[j + 5] = i + (int)angles +1;
            j += 6;
            mesh.triangles = triangles;
            yield return false;
        }
        mesh.RecalculateNormals();

    }

    private void MakeAText( Vector3 position, String str)
    {
        GameObject text = new GameObject("TextMesh: "+ str);
        text.AddComponent<TextMesh>();
        text.transform.position = position;
        text.transform.parent = this.transform;
        text.GetComponent<TextMesh>().text = str;
        text.GetComponent<TextMesh>().richText = false;
        text.GetComponent<TextMesh>().fontSize = 20;
    }

    private void OnDrawGizmos()
    {
        if (mesh == null || mesh.vertices.Length <= 0) { return; }
        Gizmos.color = dotColor;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Gizmos.DrawSphere(transform.TransformPoint(mesh.vertices[i]), 0.08f);
        }
    }

    // Use this for initialization
    void Start()
    {
        if (radiusIn > radiusOut) { radiusIn = radiusOut / 2; }
        if (radiusIn < 0.1f) { radiusIn = 0.1f; }
        if (radiusOut < 0.2f) { radiusOut = 0.2f; }

        StartCoroutine(Generate());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Awake()
    {

    }
}
