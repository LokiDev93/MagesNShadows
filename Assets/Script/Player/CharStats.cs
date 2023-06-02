using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagesnShadows.Effects;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MagesnShadows.PlayerSystems;

namespace MagesnShadows
{
    public class CharStats : NetworkBehaviour
    {
        //Level Info


        //Stats
        [SyncVar] [SerializeField] public ulong playerID;
        [SyncVar][SerializeField] int MaxHP;
        [SyncVar] [SerializeField] int CurrentHP;
        [SyncVar] [SerializeField] int MaxMana;
        [SyncVar] [SerializeField] int CurrentMana;
        [SerializeField] int Str;
        [SerializeField] int Dex;
        [SerializeField] int Int;
        [SerializeField] int Wis;
        [SerializeField] int Luck;

        [SerializeField] List<EffectBase> effects;
        

        private void Awake()
        {
            effects = new List<EffectBase>();
            CurrentHP = MaxHP;
            CurrentMana = MaxMana;
        }
        public int HP => CurrentHP;

        public void FixedUpdate()
        {
            if(!base.IsOwner)
            { return; }
            if(CurrentHP <=0)
            {
                this.gameObject.GetComponent<PlayerMovement>().AliveToggle();
            }    
        }
        private void ClearNegEffects()
        {
            /*
            foreach (var e in effects)
            {
                if (e.effectType == EffectType.Debuff)
                {
                    effects.Remove(e);
                }
            }
            */
        }

        public void AddEffects(List<EffectBase> effects)
        {
            effects.AddRange(effects);
            foreach (var e in effects)
            {
                if (e.EffectClearer == true)
                {
                    ClearNegEffects();
                    return;
                }
            }
        }
    }


    public struct CharLvlInfo
    {
        public byte Level;
        public uint Xp;
    }
}
