using System;
using System.IO;
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
    private Texture2D texture;
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
        if (layer <= 1)
            z = 0;
        else if (layer < layers + 1)
            z = (layer - 1) * depth / layers;
        else if (layer == layers + 1)
            z = (layer - 2) * depth / layers;
        else if (layer > layers + 1)
            z = ( layers - (layer - layers) ) * depth / layers;
        //Debug.Log(layer.ToString() + " - " + z);

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
        //WaitForSeconds wait = new WaitForSeconds(0.001f);
        mesh = new Mesh();
        mesh.name = "polyhedron";
        GetComponent<MeshFilter>().mesh = mesh;
        int[] triangles = new int[(layers+1) * angles * 12];
        uint index = 0;

        float deltaRad = radiusOut - radiusIn;

        float height = texture.height;
        float oneProcH = height / 100;
        float partH = height / ((float)angles);
        float H = partH / oneProcH / 100;

        float width = texture.width;
        float oneProcW = width / 100;
        float partW = width / ((float)layers * 2);
        float W = partW / oneProcW / 100;

        int boxSizeX = 10;
        int boxSizeY = 10;
        Color[] greenBox = new Color[boxSizeX*boxSizeY];
        for (int i = 0; i < boxSizeX * boxSizeY; i++) { greenBox[i] = new Color(0, 200, 0); }

        Texture2D textureToSave = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
        textureToSave.SetPixels(texture.GetPixels());

        for (uint layer = 0; layer < layers*2 ; layer++)
        {
            float radius = 0;
            if ((layer == 0)||(layer > layers))
                radius = radiusIn;
            else if ((layer > 0)&&(layer<=layers))
                radius = radiusOut;

            foreach (Vector3 vertice in layerOfPolyhedron(layer, radius))
            {
                allVertices.Add(vertice);
                uv.Add(new Vector2(W * (float)layer, H * ((float)index - (layer + 1) * angles)));
                textureToSave.SetPixels((int)((uint)partW * (layer)), (int)(partH * (index - (layer) * angles)), boxSizeX, boxSizeY, greenBox);
                //Debug.Log(((uint)partW * layer).ToString() + " - " + (partH * (index - (layer + 1) * angles)).ToString());
                //MakeAText(vertice, index.ToString()+" - "+uv[uv.Count - 1].x.ToString()+"/"+uv[uv.Count - 13].y.ToString());
                //MakeAText(vertice, index.ToString());
                index++;
                mesh.vertices = allVertices.ToArray();
                //yield return false;
            }
            //uv.Add(new Vector2(W * (float)layer, H * ((float)index - (layer + 1) * angles)));
        }
       // uv.Add(new Vector2(W * ((float)layers*2), H * ((float)index - (((float)layers * 2) + 1) * angles)));

        mesh.vertices = allVertices.ToArray();

        byte[] bytes = textureToSave.EncodeToJPG();
        Destroy(textureToSave);
        File.WriteAllBytes(Application.dataPath + "/../SavedImage.jpg", bytes);

        int start = 0;
        int j = 0;
        for (int layer = 0; layer <= layers * 2 -1; layer++)
        {
            for (int i = start; i < (start + (int)angles); i++)
            {

                triangles[j] = i;
                triangles[j + 3] = triangles[j + 2] = i == start + angles - 1 ? start : i + 1;

                if (layer < layers * 2 -1)
                {
                    triangles[j + 4] = triangles[j + 1] = i + (int)angles;
                    triangles[j + 5] = i == start + angles - 1 ? start + (int)angles : i + (int)angles + 1;
                }
                else
                if (layer == layers * 2 -1)
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


    void Start()
    {
        if (radiusIn > radiusOut) { radiusIn = radiusOut / 2; }
        if (radiusIn < 0.1f) { radiusIn = 0.1f; }
        if (radiusOut < 0.2f) { radiusOut = 0.2f; }
        texture = gameObject.GetComponent<MeshRenderer>().material.mainTexture as Texture2D;  
        //StartCoroutine(Generate());
        Generate();
    }


    void Update()
    {

    }

}
