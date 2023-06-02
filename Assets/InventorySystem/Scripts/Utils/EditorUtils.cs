#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FishNet.InventorySystem
{

    public static class EditorUtils
    {

        public static List<T> FindScriptableObjects<T>() where T : ScriptableObject
        {
            List<T> list = new List<T>();
            string path;

            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            for (int i = 0; i < guids.Length; i++)
            {
                path = AssetDatabase.GUIDToAssetPath(guids[i]);
                list.Add(AssetDatabase.LoadAssetAtPath<T>(path));
            }

            return list;
        }

    }

}
#endif