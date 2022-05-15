using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FactoryFramework.Editor
{
    public class EditorUtils
    {
        public static T LoadFirstAssetByFilter<T>(string assetFilter, string[] searchInFolders = null) where T : UnityEngine.Object
        {
            string filter = $"{assetFilter} t:{typeof(T)}";
            var guids = AssetDatabase.FindAssets(filter, searchInFolders);
            if (guids.Length > 0)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<T>(assetPath);
            }

            Debug.LogError($"Unable to find asset '{assetFilter}'");

            return null;
        }

    }
}