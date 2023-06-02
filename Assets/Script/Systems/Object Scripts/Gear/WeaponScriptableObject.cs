using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows.Items
{
    [CreateAssetMenu(fileName = "New weapon", menuName = "Assets/Items/Weapon")]
    [System.Serializable]
    public class WeaponScriptableObject : ItemBase , IGear
    {
        public void Equip()
        {
            throw new System.NotImplementedException();
        }
    }
    public enum WeaponType
    {
        Null = 0,
        Book = 1,
        Wand = 2,
        Orb = 3
    }
}
