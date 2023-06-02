using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows.GroupSystems
{
    public class GroupBase
    {
        [SerializeField] public bool Initialized { get; set; }
        //public Server Server { get; private set; }
        [SerializeField] public ServerManager ServerManager { get; private set; }

        [SerializeField] private ulong GroupID;
        [SerializeField] private string GroupName;

        public virtual void InitializeOnce()
        {
            
        }

    }
}
