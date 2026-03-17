using System;
using Unity.Mathematics;

namespace VAutomationCore.Utils
{
    /// <summary>
    /// Mathematical operations for game development including distance calculations,
    /// positioning, random generation, and geometric operations optimized for Unity ECS.
    /// </summary>
    public static class MathUtility
    {
        // ============== Distance Calculations ==============
        
        /// <summary>
        /// Calculate 2D distance between two positions (ground-based, ignores Y).
        /// </summary>
        public static float Distance2D(float3 a, float3 b)
        {
            var dx = b.x - a.x;
            var dz = b.z - a.z;
            return math.sqrt(dx * dx + dz * dz);
        }
        
        /// <summary>
        /// Calculate 2D distance between two float2 positions.
        /// </summary>
        public static float Distance2D(float2 a, float2 b)
        {
            var dx = b.x - a.x;
            var dy = b.y - a.y;
            return math.sqrt(dx * dx + dy * dy);
        }
        
        /// <summary>
        /// Calculate 3D distance between two positions.
        /// </summary>
        public static float Distance3D(float3 a, float3 b)
        {
            return math.distance(a, b);
        }
        
        // ============== Range Checks ==============
        
        /// <summary>
        /// Check if position is within range (2D, ground-based).
        /// </summary>
        public static bool IsInRange2D(float3 position, float3 target, float range)
        {
            return Distance2D(position, target) <= range;
        }
        
        /// <summary>
        /// Check if position is within range (3D).
        /// </summary>
        public static bool IsInRange3D(float3 position, float3 target, float range)
        {
            return Distance3D(position, target) <= range;
        }
        
        /// <summary>
        /// Check if position is within min/max range (2D).
        /// </summary>
        public static bool IsInRange2D(float3 position, float3 target, float minRange, float maxRange)
        {
            var dist = Distance2D(position, target);
            return dist >= minRange && dist <= maxRange;
        }
        
        // ============== Random Positioning ==============
        
        /// <summary>
        /// Generate random position in a circle around a center (2D, ground-based).
        /// </summary>
        public static float2 RandomPositionInCircle(float2 center, float radius)
        {
            var angle = UnityEngine.Random.Range(0f, math.PI * 2f);
            var r = math.sqrt(UnityEngine.Random.Range(0f, 1f)) * radius;
            return new float2(
                center.x + r * math.cos(angle),
                center.y + r * math.sin(angle)
            );
        }
        
        /// <summary>
        /// Generate random position in a ring (annulus) around center.
        /// </summary>
        public static float2 RandomPositionInRing(float2 center, float minRadius, float maxRadius)
        {
            var angle = UnityEngine.Random.Range(0f, math.PI * 2f);
            var r = math.sqrt(UnityEngine.Random.Range(minRadius * minRadius, maxRadius * maxRadius));
            return new float2(
                center.x + r * math.cos(angle),
                center.y + r * math.sin(angle)
            );
        }
        
        /// <summary>
        /// Generate random 3D position in a sphere.
        /// </summary>
        public static float3 RandomPositionInSphere(float3 center, float radius)
        {
            var randomDir = UnityEngine.Random.onUnitSphere;
            var dist = UnityEngine.Random.Range(0f, radius);
            return center + randomDir * dist;
        }
        
        // ============== Angle Operations ==============
        
        /// <summary>
        /// Calculate angle in degrees between two positions (2D).
        /// </summary>
        public static float Angle2D(float3 from, float3 to)
        {
            var dx = to.x - from.x;
            var dz = to.z - from.z;
            return math.degrees(math.atan2(dz, dx));
        }
        
        /// <summary>
        /// Calculate angle between two float2 positions.
        /// </summary>
        public static float Angle2D(float2 from, float2 to)
        {
            var dx = to.x - from.x;
            var dy = to.y - from.y;
            return math.degrees(math.atan2(dy, dx));
        }
        
        /// <summary>
        /// Normalize angle to [-180, 180] range.
        /// </summary>
        public static float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }
        
        // ============== Interpolation ==============
        
        /// <summary>
        /// Linear interpolation (lerp) for float.
        /// </summary>
        public static float Lerp(float a, float b, float t)
        {
            return math.lerp(a, b, t);
        }
        
        /// <summary>
        /// Linear interpolation for float3.
        /// </summary>
        public static float3 Lerp(float3 a, float3 b, float t)
        {
            return math.lerp(a, b, t);
        }
        
        /// <summary>
        /// Clamp value between min and max.
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            return math.clamp(value, min, max);
        }
        
        /// <summary>
        /// Clamp value to [0, 1] range.
        /// </summary>
        public static float Clamp01(float value)
        {
            return math.saturate(value);
        }
        
        // ============== Direction Vectors ==============
        
        /// <summary>
        /// Get normalized direction vector from one position to another (2D, ground-based).
        /// </summary>
        public static float2 Direction2D(float3 from, float3 to)
        {
            var diff = new float2(to.x - from.x, to.z - from.z);
            return math.normalize(diff);
        }
        
        /// <summary>
        /// Get normalized direction vector from one position to another (3D).
        /// </summary>
        public static float3 Direction3D(float3 from, float3 to)
        {
            return math.normalize(to - from);
        }
        
        // ============== Grid Conversions ==============
        
        /// <summary>
        /// Convert world position to grid coordinates.
        /// </summary>
        public static int2 WorldToGrid(float3 worldPos, float cellSize)
        {
            return new int2(
                (int)math.floor(worldPos.x / cellSize),
                (int)math.floor(worldPos.z / cellSize)
            );
        }
        
        /// <summary>
        /// Convert grid coordinates to world position (center of cell).
        /// </summary>
        public static float3 GridToWorld(int2 gridPos, float cellSize)
        {
            return new float3(
                gridPos.x * cellSize + cellSize * 0.5f,
                0f,
                gridPos.y * cellSize + cellSize * 0.5f
            );
        }
        
        // ============== Rotation Operations ==============
        
        /// <summary>
        /// Rotate a point around a pivot in 2D.
        /// </summary>
        public static float2 RotatePoint2D(float2 point, float2 pivot, float degrees)
        {
            var rad = math.radians(degrees);
            var cos = math.cos(rad);
            var sin = math.sin(rad);
            
            var dx = point.x - pivot.x;
            var dy = point.y - pivot.y;
            
            return new float2(
                pivot.x + dx * cos - dy * sin,
                pivot.y + dx * sin + dy * cos
            );
        }
        
        /// <summary>
        /// Rotate a float3 around a pivot in 3D (around Y axis).
        /// </summary>
        public static float3 RotatePointY(float3 point, float3 pivot, float degrees)
        {
            var rad = math.radians(degrees);
            var cos = math.cos(rad);
            var sin = math.sin(rad);
            
            var dx = point.x - pivot.x;
            var dz = point.z - pivot.z;
            
            return new float3(
                pivot.x + dx * cos - dz * sin,
                point.y,
                pivot.z + dx * sin + dz * cos
            );
        }
    }
}
