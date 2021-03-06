﻿using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using geo;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class maw : MonoBehaviour
{
    [SerializeField, Candlelight.PropertyBackingField]
    private int m_layers = 2;    
    public int layers
    {
        get { return m_layers; }
        set { m_layers = Mathf.Clamp(value,1,100); Generate(); }
    }

    [SerializeField, Candlelight.PropertyBackingField]
    private int m_angles = 5;
    public int angles
    {
        get { return m_angles; }
        set { m_angles = Mathf.Clamp(value,3,100); Generate(); }
    }
    [SerializeField, Candlelight.PropertyBackingField]
    private float m_radiusOut = 20f;
    public float radiusOut
    {
        get { return m_radiusOut; }
        set { m_radiusOut = Mathf.Clamp(value,0,1000); Generate(); }
    }
    [SerializeField, Candlelight.PropertyBackingField]
    private float m_radiusIn = 10f;
    public float radiusIn
    {
        get { return m_radiusIn; }
        set { m_radiusIn = Mathf.Clamp(value, 0, 1000); Generate(); }
    }
    [SerializeField, Candlelight.PropertyBackingField]
    private float m_depth = 10f;
    public float depth
    {
        get { return m_depth; }
        set { m_depth = Mathf.Clamp(value, 0, 1000); Generate(); }
    }

    private Texture2D texture;
    private Mesh mesh;

    private IEnumerable<Vector3> layerOfPolyhedron(uint layer, float radius)
    {
        float z = 0f;
        if ((layer <= 1)||(layer == layers * 2))
            z = 0;
        else if (layer < layers + 1)
            z = (layer - 1) * depth / layers;
        else if (layer == layers + 1)
            z = (layer - 2) * depth / layers;
        else if (layer > layers + 1)
            z = ( layers - (layer - layers) ) * depth / layers;

        foreach (Vector3 vertice in Geo.polyhedron(radius, angles, z))
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
        mesh.Clear();
        //float deltaRad = radiusOut - radiusIn;
        //WaitForSeconds wait = new WaitForSeconds(0.001f);
        int[] triangles = new int[(layers+2) * angles * 12];
        List<Vector3> allVertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();

        float height = texture.height;
        float oneProcH = height / 100;
        float partH = height / ((float)angles);
        float H = partH / oneProcH / 100;
        float width = texture.width;
        float oneProcW = width / 100;
        float partW = width / ((float)layers * 2);
        float W = partW / oneProcW / 100;
        int X = 0;
        int Y = 0;
    

        int boxSizeX = 10;
        int boxSizeY = 10;
        Color[] greenBox = new Color[boxSizeX*boxSizeY];
        for (int i = 0; i < boxSizeX * boxSizeY; i++) { greenBox[i] = new Color(0, 200, 0); }

        Texture2D textureToSave = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
        textureToSave.SetPixels(texture.GetPixels());

        for (uint layer = 0; layer < layers*2 +1; layer++)
        {
            uint index = 0;
            float radius = 0;
            if ((layer == 0)||(layer > layers))
                radius = radiusIn;
            else if ((layer > 0)&&(layer<=layers))
                radius = radiusOut;

            foreach (Vector3 vertice in layerOfPolyhedron(layer, radius))
            {
                X = (int)Mathf.Clamp(((uint)partW * layer),0, width - boxSizeX);
                Y = (int)Mathf.Clamp((partH * index),0, height - boxSizeY);            
                textureToSave.SetPixels(X, Y, boxSizeX, boxSizeY, greenBox);

                Vector2 Addiction = Geo.additive(new Vector2(vertice.x, vertice.y), new Vector2(0, 7 * Mathf.PerlinNoise(vertice.x/100 * (layer == layers * 2? 0 : layer), (vertice.y / 100 * (layer == layers * 2 ? 0 : layer)))));

                allVertices.Add(new Vector3(Addiction.x, Addiction.y, vertice.z));
                uv.Add(new Vector2(W * (float)layer, H * (float)index));
                index++;
                //mesh.vertices = allVertices.ToArray();
                //yield return false;
            }
        }

        mesh.vertices = allVertices.ToArray();
        mesh.uv = uv.ToArray();

        byte[] bytes = textureToSave.EncodeToJPG();
        Destroy(textureToSave);
        File.WriteAllBytes(Application.dataPath + "/../SavedImage.jpg", bytes);

        int start = 0;
        int j = 0;
        for (int layer = 0; layer < layers * 2 ; layer++)
        {
            for (int i = start; i < (start + (int)angles); i++)
            {
                triangles[j] = i;
                triangles[j + 3] = triangles[j + 2] = i + 1;
                triangles[j + 4] = triangles[j + 1] = i + (int)angles+1;
                triangles[j + 5] = i + (int)angles + 2;
                //mesh.triangles = triangles;
                j += 6;
                //yield return wait;

            }
            start += (int)angles + 1;
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void Start()
    {
        if (radiusIn > radiusOut) { radiusIn = radiusOut / 2; }
        if (radiusIn < 0.1f) { radiusIn = 0.1f; }
        if (radiusOut < 0.2f) { radiusOut = 0.2f; }
        texture = gameObject.GetComponent<MeshRenderer>().material.mainTexture as Texture2D;
        mesh = new Mesh();
        mesh.name = "polyhedron";
        GetComponent<MeshFilter>().mesh = mesh;
        //StartCoroutine(Generate());
        Generate();
    }

    void Update()
    {

    }

}
