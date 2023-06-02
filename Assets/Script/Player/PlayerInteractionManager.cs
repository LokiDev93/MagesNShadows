using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows
{
    public class PlayerInteractionManager : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField] private GameObject raySource;


        private void Awake()
        {
            player = this.gameObject;
            
        }
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
