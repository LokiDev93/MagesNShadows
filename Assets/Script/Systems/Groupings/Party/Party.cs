using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagesnShadows.GroupSystems;

namespace MagesnShadows
{
    public class Party : GroupBase
    {
        [SerializeField] private CharStats[] members = new CharStats[5];

        public override void InitializeOnce()
        {
            base.Initialized = true;


        }

        public void AddToParty()
        {

        }
    }
}
