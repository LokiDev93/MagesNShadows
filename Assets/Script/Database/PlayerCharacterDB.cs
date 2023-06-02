using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows
{
    public class PlayerCharacterDB : SerializedMonoBehaviour
    {
        [DictionaryDrawerSettings(KeyLabel = "Custom Key Name", ValueLabel = "Custom Value Label")]
        [SerializeField] private Dictionary<CharOpt, GameObject> characterOptions;

        //public bool SelectCharacter(CharOpt keyName)
        //{
        //    bool r = false;
        //    GameObject s = null;
        //    r = characterOptions.TryGetValue(keyName, out s);
        //    return r;
        //}
        public GameObject SelectCharacter(CharOpt keyName)
        {
            
            GameObject s = null;
            characterOptions.TryGetValue(keyName, out s);
            return s;
        }
    }


    public enum CharOpt : byte
    {
        Mage = 0, //Generic/default option
        Skelly = 1

    }
}
