using System;
using System.Collections.Generic;
using System.Numerics;

public class CubeHelper
{
    private float length;

    public CubeHelper(float length)
    {
        if (length <= 0.0)
        {
            throw new ArgumentOutOfRangeException("length", "length <= 0.0");
        }

        this.length = length;
    }

    public IEnumerable<Tuple<Vector2, Vector2>> GetEdges(Matrix4x4 transformation)
    {
        List<Tuple<Vector2, Vector2>> result = new List<Tuple<Vector2, Vector2>>();

        Vector2 first = new Vector2();
        Vector2 prev = new Vector2();
        int index = 0;
        foreach (Vector2 vertex in GetVertices(transformation))
        {
            if (index == 0)
            {
                first = vertex;
            }
            else
            {
                result.Add(Tuple.Create(prev, vertex));
            }

            prev = vertex;
            index++;
        }

        result.Add(Tuple.Create(prev, first));
        return result;
    }

    public IEnumerable<Vector2> GetVertices(Matrix4x4 transformation)
    {
        Matrix4x4 unit = new Matrix4x4(
            0.0f, 1.0f, 1.0f, 0.0f,
            0.0f, 0.0f, 1.0f, 1.0f,
            0.0f, 0.0f, 0.0f, 0.0f,
            1.0f, 1.0f, 1.0f, 1.0f
        );

        Matrix4x4 lengthScaling = new Matrix4x4(
            length, 0.0f, 0.0f, 0.0f,
            0.0f, length, 0.0f, 0.0f,
            0.0f, 0.0f, length, 0.0f,
            0.0f, 0.0f, 0.0f, 1.0f
        );
        Matrix4x4 transformed = Matrix4x4.Multiply(Matrix4x4.Multiply(transformation, lengthScaling), unit);

        return new List<Vector2>
        {
            new Vector2(transformed.M11 / transformed.M41, transformed.M21 / transformed.M41),
            new Vector2(transformed.M12 / transformed.M42, transformed.M22 / transformed.M42),
            new Vector2(transformed.M13 / transformed.M43, transformed.M23 / transformed.M43),
            new Vector2(transformed.M14 / transformed.M44, transformed.M24 / transformed.M44),
        };
    }
}

