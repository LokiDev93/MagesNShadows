using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows.Effects
{
    [System.Serializable]
    public class EffectBase : ScriptableObject
    {
        [SerializeField] public int Id;
        [SerializeField] public string effectName;
        [SerializeField] string effectInternalName;
        [SerializeField] private bool effectClearer;
        public bool EffectClearer
        {
            get { return effectClearer; }
        }



    }



}
