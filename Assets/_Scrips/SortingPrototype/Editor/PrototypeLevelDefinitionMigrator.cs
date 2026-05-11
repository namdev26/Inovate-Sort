using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace SortingPrototype.Editor
{
    public static class PrototypeLevelDefinitionMigrator
    {
        private const int DefaultVariantCount = 4;

        [MenuItem("Tools/Sorting Prototype/Migrate Selected Levels To Piece Tokens", priority = 10)]
        private static void MigrateSelected()
        {
            var selected = Selection.objects;
            if (selected == null || selected.Length == 0)
            {
                Debug.LogWarning("Select one or more PrototypeLevelDefinition assets to migrate.");
                return;
            }

            var migratedCount = 0;
            foreach (var obj in selected)
            {
                if (obj == null)
                {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                var serialized = new SerializedObject(obj);
                var branchesProp = serialized.FindProperty("branches");
                if (branchesProp == null || !branchesProp.isArray)
                {
                    continue;
                }

                var variantCounters = new Dictionary<int, int>();
                var anyMigrated = false;
                for (var branchIndex = 0; branchIndex < branchesProp.arraySize; branchIndex++)
                {
                    var branchProp = branchesProp.GetArrayElementAtIndex(branchIndex);
                    var legacyProp = branchProp.FindPropertyRelative("legacyColorIds");
                    var tokensProp = branchProp.FindPropertyRelative("pieceTokens");

                    if (legacyProp == null || tokensProp == null || !legacyProp.isArray || !tokensProp.isArray)
                    {
                        continue;
                    }

                    if (legacyProp.arraySize <= 0)
                    {
                        continue;
                    }

                    tokensProp.arraySize = legacyProp.arraySize;
                    for (var i = 0; i < legacyProp.arraySize; i++)
                    {
                        var colorId = legacyProp.GetArrayElementAtIndex(i).enumValueIndex;
                        if (!variantCounters.TryGetValue(colorId, out var counter))
                        {
                            counter = 0;
                        }

                        var tokenProp = tokensProp.GetArrayElementAtIndex(i);
                        var colorIdProp = tokenProp.FindPropertyRelative("colorId");
                        var variantProp = tokenProp.FindPropertyRelative("variant");

                        if (colorIdProp != null)
                        {
                            colorIdProp.enumValueIndex = colorId;
                        }

                        if (variantProp != null)
                        {
                            variantProp.intValue = counter % DefaultVariantCount;
                        }

                        variantCounters[colorId] = counter + 1;
                    }

                    legacyProp.ClearArray();
                    anyMigrated = true;
                }

                if (!anyMigrated)
                {
                    continue;
                }

                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(obj);
                migratedCount++;
            }

            if (migratedCount > 0)
            {
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"Migrated {migratedCount} level asset(s) to piece tokens.");
        }

        [MenuItem("Tools/Sorting Prototype/Migrate Selected Levels To Piece Tokens", validate = true)]
        private static bool ValidateMigrateSelected()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj == null)
                {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrWhiteSpace(path) || !path.EndsWith(".asset"))
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}

