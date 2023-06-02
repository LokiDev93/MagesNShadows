using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace FishNet.InventorySystem
{

    /// <summary>
    /// Just an example. There are much better ways to do this using a database, or JSON.NET, etc.
    /// </summary>
    public static class Saver 
    {

        // TODO save/load inv size
        public static void SaveInventory(List<NetworkInventoryItem> items, string name)
        {
            string json = string.Join(',', items);

            string path = $"{Application.persistentDataPath}/Inventory/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path += $"Items_{name}.json";
            if (!File.Exists(path))
                File.Create(path).Dispose();

            File.WriteAllText(path, json);
        }

        public static List<NetworkInventoryItem> LoadInventory(string name)
        {
            string path = $"{Application.persistentDataPath}/Inventory/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path += $"Items_{name}.json";
            if (!File.Exists(path))
                return null;

            string json = File.ReadAllText(path);
            string[] itemStrings = json.Split(',');
            List<NetworkInventoryItem> items = new List<NetworkInventoryItem>();
            foreach (var item in itemStrings)
                items.Add(NetworkInventoryItem.Parse(item));

            return items;
        }

    }

}
