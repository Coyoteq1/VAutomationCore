using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace VAuto.Core.Services
{
    public static class GlowTileGeometry
    {
        public static List<float3> GeneratePoints(float centerX, float centerY, float centerZ, float radius, float spacing, float rotation)
        {
            var positions = new List<float3>();
            var circumference = 2f * MathF.PI * radius;
            var pointCount = Math.Max(1, (int)(circumference / spacing));

            for (int i = 0; i < pointCount; i++)
            {
                var angle = (float)i / pointCount * MathF.PI * 2f + rotation * MathF.PI / 180f;
                var x = centerX + MathF.Cos(angle) * radius;
                var z = centerZ + MathF.Sin(angle) * radius;
                positions.Add(new float3(x, centerY, z));
            }

            return positions;
        }
    }
}
