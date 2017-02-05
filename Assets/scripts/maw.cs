using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class maw : MonoBehaviour
{
    public uint layers = 2;
    public uint angles = 3;
    public float radiusOut = 2f;
    public float radiusIn = 1f;
    public float depth = 1f;

    public Color dotColor = new Color(150, 150, 150);
    private Vector3[,] verticesIn;
    private Vector3[,] verticesOut;
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
        verticesIn = new Vector3[layers, angles];
        verticesOut = new Vector3[layers, angles];
        uint i = 0;
        for (uint layer = 0; layer < layers; layer++)
        {         
            foreach (Vector3 vertice in layerOfPolyhedron(layer, radiusOut))
            {
                verticesOut[layer, i] = vertice;
                allVertices.Add(vertice);
            }
            foreach (Vector3 vertice in layerOfPolyhedron(layer, radiusIn))
            {
                verticesIn[layer, i] = vertice;
                allVertices.Add(vertice);
            }
            mesh.vertices = allVertices.ToArray();
            yield return false;

            i++;
        }

        //mesh.vertices = allVertices.ToArray();



        //vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        //for (int i = 0, y = 0; y <= ySize; y++)
        //{
        //    for (int x = 0; x <= xSize; x++, i++)
        //    {
        //        vertices[i] = new Vector3(x, y);
        //    }
        //}
        //mesh.vertices = vertices;

        //int[] triangles = new int[xSize * ySize * 6];
        //for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        //{
        //    for (int x = 0; x < xSize; x++, ti += 6, vi++)
        //    {
        //        triangles[ti] = vi;
        //        triangles[ti + 3] = triangles[ti + 2] = vi + 1;
        //        triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
        //        triangles[ti + 5] = vi + xSize + 2;
        //    }
        //    mesh.triangles = triangles;
        //    mesh.RecalculateNormals();
        //}
    }

    private void OnDrawGizmos()
    {
        if (mesh == null || mesh.vertices.Length <= 0) { return; }
        Gizmos.color = dotColor;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Gizmos.DrawSphere(transform.TransformPoint(mesh.vertices[i]), 0.03f);
        }
    }

    // Use this for initialization
    void Start()
    {
        if (radiusIn > radiusOut) { radiusIn = radiusOut / 2; }
        if (radiusIn < 0.1f) { radiusIn = 0.1f; }
        if (radiusOut < 0.2f) { radiusOut = 0.2f; }
        mesh = new Mesh();
        mesh.name = "polyhedron";
        GetComponent<MeshFilter>().mesh = mesh;        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void Awake()
    {
      StartCoroutine(Generate());
    }
}
