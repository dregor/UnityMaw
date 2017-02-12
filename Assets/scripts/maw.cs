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
    public float dotRadius = 0.03f;
    private List<Vector3> allVertices = new List<Vector3>();
    private List<Vector2> uv = new List<Vector2>();
    private Texture texture;

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

    private void MakeAText(Vector3 position, String str)
    {
        GameObject text = new GameObject("TextMesh: " + str);
        text.AddComponent<TextMesh>();
        text.transform.position = position;
        text.transform.parent = this.transform;
        text.GetComponent<TextMesh>().text = str;
        text.GetComponent<TextMesh>().richText = false;
        text.GetComponent<TextMesh>().fontSize = 20;
    }

    private void Generate()//private IEnumerator Generate()// 
    {
        WaitForSeconds wait = new WaitForSeconds(0.001f);
        mesh = new Mesh();
        mesh.name = "polyhedron";
        GetComponent<MeshFilter>().mesh = mesh;
        int[] triangles = new int[layers * angles * 12 + angles * 12];
        uint index = 0;

        float height = texture.height;
        float oneProcH = height / 100;
        float partH = height / (float)angles;
        float H = partH / oneProcH / 100;

        float width = texture.width;
        float oneProcW = width / 100;
        float partW = width / ((float)layers * 2);
        float W = partW / oneProcW / 100;

        Debug.Log("width");
        Debug.Log(width);
        Debug.Log(W);

        Debug.Log("height");
        Debug.Log(height);
        Debug.Log(H);
        for (uint layer = 0; layer < layers*2; layer++)
        {
            if (layer == 0)
            {
                foreach (Vector3 vertice in layerOfPolyhedron(layer, radiusIn))
                {
                    allVertices.Add(vertice);
                    uv.Add(new Vector2(W * (float)layer, H * ((float)index - layer * angles)));
                    //MakeAText(vertice, index.ToString()+" - "+uv[uv.Count-1].x.ToString()+"/"+uv[uv.Count - 1].y.ToString());
                    //MakeAText(vertice, index.ToString());
                    index++;
                    mesh.vertices = allVertices.ToArray();
                    //yield return false;
                }
            }
            if (layer <= layers)
            {
                foreach (Vector3 vertice in layerOfPolyhedron(layer, radiusOut))
                {
                    allVertices.Add(vertice);
                    uv.Add(new Vector2(W * (float)layer, H * ((float)index - layer * angles)));
                    //MakeAText(vertice, index.ToString()+" - "+uv[uv.Count - 1].x.ToString()+"/"+uv[uv.Count - 1].y.ToString());
                    //MakeAText(vertice, index.ToString());
                    index++;
                    mesh.vertices = allVertices.ToArray();
                    //yield return false;
                }
                mesh.vertices = allVertices.ToArray();
            }
            if (layer >= layers)
            {
                uint antiLayer = layers;
                if (layer != layers) { antiLayer = layers - (layer - layers); }

                foreach (Vector3 vertice in layerOfPolyhedron(antiLayer, radiusIn))
                {
                    allVertices.Add(vertice);
                    uv.Add(new Vector2(W * (float)layer, H * ((float)index - layer * angles)));
                    //MakeAText(vertice, index.ToString()+" - "+uv[uv.Count - 1].x.ToString()+"/"+uv[uv.Count - 13].y.ToString());
                    //MakeAText(vertice, index.ToString());
                    index++;
                    mesh.vertices = allVertices.ToArray();
                    //yield return false;
                }
            }


            
        }
        mesh.vertices = allVertices.ToArray();

        int start = 0;
        int j = 0;
        for (int layer = 0; layer <= layers*2 + 1; layer++)
        {
            for (int i = start; i < (start + (int)angles); i++)
            {

                triangles[j] = i;
                triangles[j + 3] = triangles[j + 2] = i == start + angles - 1 ? start : i + 1;

                if (layer < layers * 2 + 1)
                {
                    triangles[j + 4] = triangles[j + 1] = i + (int)angles;
                    triangles[j + 5] = i == start + angles - 1 ? start + (int)angles : i + (int)angles + 1;
                }
                else 
                if (layer == layers * 2 + 1)
                {
                    triangles[j + 4] = triangles[j + 1] = i - start;
                    triangles[j + 5] = i == start + angles - 1 ? 0 : i - start + 1;
                }

                //Debug.Log("layer: " + layer.ToString());
                //Debug.Log(triangles[j].ToString() + triangles[j + 1].ToString() + triangles[j + 2].ToString());
                //Debug.Log(triangles[j+3].ToString() + triangles[j + 4].ToString() + triangles[j + 5].ToString());
                mesh.triangles = triangles;
                j += 6;
                //yield return wait;

            }
            start += (int)angles;
        }
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();


    }



    private void OnDrawGizmos()
    {
        if (mesh == null || mesh.vertices.Length <= 0) { return; }
        Gizmos.color = dotColor;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Gizmos.DrawSphere(transform.TransformPoint(mesh.vertices[i]), dotRadius);
        }
    }

    // Use this for initialization
    void Start()
    {
        if (radiusIn > radiusOut) { radiusIn = radiusOut / 2; }
        if (radiusIn < 0.1f) { radiusIn = 0.1f; }
        if (radiusOut < 0.2f) { radiusOut = 0.2f; }
        texture = gameObject.GetComponent<MeshRenderer>().material.mainTexture;  
        //StartCoroutine(Generate());
        Generate();
    }

    // Update is called once per frame
    void Update()
    {

    }

}
