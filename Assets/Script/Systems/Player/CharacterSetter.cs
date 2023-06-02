using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Connection;

namespace MagesnShadows.Server.Player
{
    public class CharacterSetter : NetworkBehaviour
    {
        [SerializeField] private PlayerCharacterDB charDB;

        Vector3 pos;
        Quaternion rot;
        GameObject parent;
        private void Awake()
        {
            charDB = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerCharacterDB>();

            parent = this.gameObject;

            pos = parent.transform.position;
            rot = parent.transform.rotation;

        }
        public void OnClick_PSpawn()
        {
            SpawnCharacter();
        } 
        [ServerRpc]
        public void SpawnCharacter()
        {
            var r = Random.Range(1, 10);
            if (r<=5)
            {
                Instantiate(charDB.SelectCharacter(CharOpt.Mage), pos, rot, parent.transform);
            }
            else
            {
                Instantiate(charDB.SelectCharacter(CharOpt.Skelly), pos, rot, parent.transform);
            }

        }
    }
}
