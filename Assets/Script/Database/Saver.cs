using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MagesnShadows.Inventory.InventoryScript;

namespace MagesnShadows.Assets.Script.Database
{
    public static class Saver
    {
        public static void SaveInventory(List<InventoryObject> items, string name)
        {
            string json = string.Join(',', items);

            string path = $"{Application.persistentDataPath}/Inventory/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path += $"Items_{name}.json";
            if (!File.Exists(path))
                File.Create(path).Dispose();

            File.WriteAllText(path, json);
            string s = "0101";
            
            var b = Encoding.ASCII.GetBytes(s);
            var bs = b.ToString();
        }

        public static List<InventoryObject> LoadInventory(string name)
        {
            string path = $"{Application.persistentDataPath}/Inventory/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path += $"Items_{name}.json";
            if (!File.Exists(path))
                return null;

            string json = File.ReadAllText(path);
            string[] itemStrings = json.Split(',');
            List<InventoryObject> items = new List<InventoryObject>();
            foreach (var item in itemStrings) ;
                //items.Add(InventoryObject.Parse(item));

            return items;
        }
    }
}
