using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VAuto.Zone.Models
{
    /// <summary>
    /// Represents a zone definition for JSON configuration.
    /// </summary>
    public class ZoneDefinition
    {
        /// <summary>
        /// Unique identifier for the zone.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display name for the zone.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Shape of the zone (e.g., "Circle").
        /// </summary>
        public string Shape { get; set; } = "Circle";

        /// <summary>
        /// Center X coordinate of the zone.
        /// </summary>
        public float CenterX { get; set; }

        /// <summary>
        /// Center Z coordinate of the zone.
        /// </summary>
        public float CenterZ { get; set; }

        /// <summary>
        /// Radius of the zone (for circular zones).
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Kit ID to apply when entering this zone.
        /// </summary>
        public string KitToApplyId { get; set; }

        /// <summary>
        /// Glow effect color in hex format (e.g., "#FF0000").
        /// </summary>
        public string GlowEffectColorHex { get; set; }

        /// <summary>
        /// Prefab ID for the glow effect.
        /// </summary>
        public int GlowPrefabId { get; set; }

        /// <summary>
        /// Prefab name for the glow effect.
        /// </summary>
        public string GlowPrefab { get; set; }

        /// <summary>
        /// Prefab ID for the border glow effect.
        /// </summary>
        public int BorderGlowPrefabId { get; set; }

        /// <summary>
        /// Prefab name for the border glow effect.
        /// </summary>
        public string BorderGlowPrefab { get; set; }

        /// <summary>
        /// Height at which to spawn glow effects.
        /// </summary>
        public float GlowSpawnHeight { get; set; }

        /// <summary>
        /// Whether to automatically apply glow with zone.
        /// </summary>
        public bool AutoGlowWithZone { get; set; }

        /// <summary>
        /// Message to display when entering the zone.
        /// </summary>
        public string EnterMessage { get; set; }

        /// <summary>
        /// Message to display when exiting the zone.
        /// </summary>
        public string ExitMessage { get; set; }

        /// <summary>
        /// Whether to teleport player on zone enter.
        /// </summary>
        public bool TeleportOnEnter { get; set; }

        /// <summary>
        /// X coordinate for teleport destination.
        /// </summary>
        public float TeleportX { get; set; }

        /// <summary>
        /// Y coordinate for teleport destination.
        /// </summary>
        public float TeleportY { get; set; }

        /// <summary>
        /// Z coordinate for teleport destination.
        /// </summary>
        public float TeleportZ { get; set; }

        /// <summary>
        /// Whether to return player to original position on zone exit.
        /// </summary>
        public bool ReturnOnExit { get; set; }

        /// <summary>
        /// List of schematic IDs granted in this zone.
        /// </summary>
        public List<string> Schematics { get; set; } = new List<string>();

        /// <summary>
        /// List of build template IDs for this zone.
        /// </summary>
        public List<string> Templates { get; set; } = new List<string>();

        /// <summary>
        /// Check if a position is inside this zone.
        /// </summary>
        public bool IsInside(float x, float z)
        {
            if (Shape.Equals("Circle", StringComparison.OrdinalIgnoreCase))
            {
                var dx = x - CenterX;
                var dz = z - CenterZ;
                return (dx * dx + dz * dz) <= (Radius * Radius);
            }
            // Default to circle for unknown shapes
            var dist = math.distance(new float2(x, z), new float2(CenterX, CenterZ));
            return dist <= Radius;
        }
    }
}
