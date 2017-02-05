using System;
using System.Collections.Generic;
using UnityEngine;

public class Polyhedron
{
    public IEnumerable<Vector2> Polyhedron(float r, int n)
    {
        for (int i = 0; i < n; i++)
        {
            yield return new Vector2(r * Mathf.Sin(2 * i * Mathf.PI / n), r * Mathf.Cos(2 * i * Mathf.PI / n));
        }
    }
}