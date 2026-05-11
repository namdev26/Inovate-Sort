using System;
using System.Collections.Generic;
using SortingPrototype.Core;
using UnityEditor;
using UnityEngine;

namespace SortingPrototype.Editor
{
    public static class PrototypeLevelDefinitionGenerator
    {
        private const int BranchCount = 12;
        private const int EmptyBranchCount = 4;
        private const int FilledBranchCount = BranchCount - EmptyBranchCount;
        private const int PiecesPerFilledBranch = 4;
        private const int VariantsPerGroup = 4;

        // 8 groups matching current enum (excluding None)
        private static readonly PieceColorId[] DefaultGroups =
        {
            PieceColorId.Red,
            PieceColorId.Blue,
            PieceColorId.Green,
            PieceColorId.Yellow,
            PieceColorId.Purple,
            PieceColorId.Orange,
            PieceColorId.Pink,
            PieceColorId.Cyan
        };

        [MenuItem("Tools/Sorting Prototype/Generate Level (12 branches, 4 empty)", priority = 11)]
        private static void GenerateSelected()
        {
            var selected = Selection.objects;
            if (selected == null || selected.Length == 0)
            {
                Debug.LogWarning("Select one or more PrototypeLevelDefinition assets to generate.");
                return;
            }

            var generatedCount = 0;
            foreach (var obj in selected)
            {
                if (obj == null)
                {
                    continue;
                }

                var serialized = new SerializedObject(obj);
                if (serialized.FindProperty("branchCapacity") == null || serialized.FindProperty("branches") == null)
                {
                    continue;
                }

                GenerateInto(serialized);
                serialized.ApplyModifiedProperties();
                EditorUtility.SetDirty(obj);
                generatedCount++;
            }

            if (generatedCount > 0)
            {
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"Generated {generatedCount} level asset(s).");
        }

        private static void GenerateInto(SerializedObject level)
        {
            var branchCapacityProp = level.FindProperty("branchCapacity");
            var branchesProp = level.FindProperty("branches");

            if (branchCapacityProp != null)
            {
                branchCapacityProp.intValue = PiecesPerFilledBranch;
            }

            branchesProp.arraySize = BranchCount;

            var tokens = BuildTokenBag();
            Shuffle(tokens, seed: 1337);

            var tokenIndex = 0;
            for (var branchIndex = 0; branchIndex < BranchCount; branchIndex++)
            {
                var branchProp = branchesProp.GetArrayElementAtIndex(branchIndex);
                var legacyProp = branchProp.FindPropertyRelative("legacyColorIds");
                if (legacyProp != null && legacyProp.isArray)
                {
                    legacyProp.ClearArray();
                }

                var tokensProp = branchProp.FindPropertyRelative("pieceTokens");
                if (tokensProp == null || !tokensProp.isArray)
                {
                    continue;
                }

                var isEmptyBranch = branchIndex >= FilledBranchCount;
                if (isEmptyBranch)
                {
                    tokensProp.ClearArray();
                    continue;
                }

                tokensProp.arraySize = PiecesPerFilledBranch;
                for (var slot = 0; slot < PiecesPerFilledBranch; slot++)
                {
                    var token = tokens[tokenIndex++];
                    var tokenProp = tokensProp.GetArrayElementAtIndex(slot);

                    var colorIdProp = tokenProp.FindPropertyRelative("colorId");
                    var variantProp = tokenProp.FindPropertyRelative("variant");

                    if (colorIdProp != null)
                    {
                        colorIdProp.enumValueIndex = (int)token.ColorId;
                    }

                    if (variantProp != null)
                    {
                        variantProp.intValue = token.Variant;
                    }
                }
            }
        }

        private static List<PieceToken> BuildTokenBag()
        {
            var bag = new List<PieceToken>(FilledBranchCount * PiecesPerFilledBranch);
            for (var groupIndex = 0; groupIndex < DefaultGroups.Length; groupIndex++)
            {
                for (var variant = 0; variant < VariantsPerGroup; variant++)
                {
                    bag.Add(new PieceToken(DefaultGroups[groupIndex], variant));
                }
            }

            return bag;
        }

        private static void Shuffle<T>(IList<T> list, int seed)
        {
            var rng = new System.Random(seed);
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        [MenuItem("Tools/Sorting Prototype/Generate Level (12 branches, 4 empty)", validate = true)]
        private static bool ValidateGenerateSelected()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj == null)
                {
                    continue;
                }

                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrWhiteSpace(path) || !path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var temp = new SerializedObject(obj);
                if (temp.FindProperty("branchCapacity") != null && temp.FindProperty("branches") != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

