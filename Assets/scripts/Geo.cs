using System.Collections.Generic;
using UnityEngine;

namespace geo
{

    public static class Geo
    {
        public static IEnumerable<Vector3> polyhedron(float r, uint n, float z = 0)
        {
            for (int i = 0; i < n; i++)
            {
                yield return new Vector3(r * Mathf.Sin(2 * i * Mathf.PI / n), r * Mathf.Cos(2 * i * Mathf.PI / n), z);
            }
            yield return new Vector3(r * Mathf.Sin(0), r * Mathf.Cos(0), z);
        }

        public static float length(Vector2 pt1, Vector2 pt2)
        {
            return Mathf.Sqrt(Mathf.Pow(pt2[0] - pt1[0], 2) + Mathf.Pow(pt2[1] - pt1[1], 2));
        }

        public static float alpha(Vector2 center, Vector2 A)
        {
            if (A == new Vector2(0, 0))
                return 0;
            else
            {
                A = A - center;
                return Mathf.Asin(length(new Vector2(0, A[1]), A) / length(A, new Vector2(0, 0)));
            }
        }

        public static float betha(Vector2 center, Vector2 pt)
        {
            int q = quarter(center, pt);
            float _alpha = alpha(center, pt);
            if (q == 2)
                return 2 * Mathf.PI - _alpha;
            else if (q == 3)
                return _alpha;
            else if (q == 4)
                return Mathf.PI - _alpha;
            else
                return Mathf.PI + _alpha;
        }

        public static Vector2 additive(float alpha, Vector2 pt, Vector2 add)
        {
            float dx = pt[0] + Mathf.Cos(2 * Mathf.PI - alpha) * add[0] + Mathf.Sin(2 * Mathf.PI - alpha) * add[1];
            float dy = pt[1] + Mathf.Cos(2 * Mathf.PI - alpha) * add[1] - Mathf.Sin(2 * Mathf.PI - alpha) * add[0];
            return new Vector2(dx, dy);
        }

        public static Vector2 additive(Vector2 center, Vector2 pt, Vector2 add)
        {
            return additive(alpha(center, pt),pt,add);
        }

        public static Vector2 additive(Vector2 pt, Vector2 add)
        {
            return additive(alpha(new Vector2(0,0), pt), pt, add);
        }

        public static int quarter(Vector2 center, Vector2 pt)
        {
            pt = pt - center;
            if (pt[0] > 0)
            {
                if (pt[1] > 0)
                    return 1;
                else
                    return 2;
            }
            else
            {
                if (pt[1] > 0)
                    return 4;
                else
                    return 3;
            }
        }

        public static Vector2 quarterDirection(Vector2 center, Vector2 pt)
        {
            int q = quarter(center, pt);
            if (q == 1)
                return new Vector2(1, 1);
            else if (q == 2)
                return new Vector2(1, -1);
            else if (q == 3)
                return new Vector2(-1, -1);
            else if (q == 4)
                return new Vector2(-1, 1);
            else
                return new Vector2(1, 1);
        }

    }

}
