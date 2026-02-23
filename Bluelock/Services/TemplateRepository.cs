using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BepInEx;
using BepInEx.Logging;
using Stunlock.Core;
using Unity.Collections;
using Unity.Mathematics;
using VAuto.Zone;
using VAuto.Zone.Core;
using VAuto.Zone.Models;

namespace VAuto.Zone.Services
{
    public static class TemplateRepository
    {
        private static readonly string TemplatesRoot = Path.Combine(Paths.ConfigPath, "Bluelock", "templates");
        private static readonly JsonSerializerOptions JsonOptions = new(ZoneJsonOptions.WithUnityMathConverters)
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public static bool TryLoadTemplate(string templateName, out TemplateSnapshot template)
        {
            template = null;
            if (string.IsNullOrWhiteSpace(templateName))
            {
                return false;
            }

            Directory.CreateDirectory(TemplatesRoot);
            var sanitized = SanitizeTemplateName(templateName);
            var path = Path.Combine(TemplatesRoot, $"{sanitized}.json");

            EnsureDefaultTemplateExists(templateName, path);

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                var json = File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<TemplateSnapshot>(json, JsonOptions);
                if (loaded == null)
                {
                    return false;
                }

                loaded.Name = loaded.Name.Length == 0 ? templateName : loaded.Name;
                template = loaded;
                return true;
            }
            catch (Exception ex)
            {
                ZoneCore.LogWarning($"[TemplateRepository] Failed to load template '{templateName}': {ex.Message}");
                return false;
            }
        }

        private static void EnsureDefaultTemplateExists(string templateName, string path)
        {
            if (File.Exists(path))
            {
                return;
            }

            if (!string.Equals(templateName, "arena_default", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                var template = CreateArenaDefaultTemplate();
                File.WriteAllText(path, JsonSerializer.Serialize(template, JsonOptions));
                ZoneCore.LogInfo($"[TemplateRepository] Seeded default template '{templateName}' at '{path}'.");
            }
            catch (Exception ex)
            {
                ZoneCore.LogWarning($"[TemplateRepository] Failed to seed default template '{templateName}': {ex.Message}");
            }
        }

        private static TemplateSnapshot CreateArenaDefaultTemplate()
        {
            return new TemplateSnapshot
            {
                Name = "arena_default",
                Entities = new List<TemplateEntityEntry>
                {
                    CreateEntry("CHAR_Vampire_Dracula_VBlood", -327335305, 0f, 0f, 0f, 0f),
                    CreateEntry("CHAR_Vampire_HighLord_VBlood", -496360395, 10f, 0f, 0f, 180f),
                    CreateEntry("CHAR_Vampire_BloodKnight_VBlood", 495971434, -10f, 0f, 0f, 0f),
                    CreateEntry("CHAR_Winter_Yeti_VBlood", -1347412392, 0f, 0f, 10f, 180f),
                    CreateEntry("CHAR_Wendigo_VBlood", 24378719, 0f, 0f, -10f, 0f),
                    CreateEntry("CHAR_Spider_Queen_VBlood", -548489519, 14f, 0f, 14f, 225f),
                    CreateEntry("CHAR_VHunter_Leader_VBlood", -1449631170, -14f, 0f, 14f, 135f),
                    CreateEntry("CHAR_Bandit_Stalker_VBlood", 1106149033, 14f, 0f, -14f, 315f)
                }
            };
        }

        private static TemplateEntityEntry CreateEntry(string prefabName, int prefabGuid, float x, float y, float z, float rotationDegrees)
        {
            return new TemplateEntityEntry
            {
                PrefabName = prefabName,
                PrefabGuid = prefabGuid,
                Offset = new float3(x, y, z),
                RotationDegrees = rotationDegrees
            };
        }

        private static string SanitizeTemplateName(string templateName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                templateName = templateName.Replace(c, '_');
            }

            return templateName;
        }
    }
}
