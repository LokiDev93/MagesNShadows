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

        [SyncVar] [SerializeField] private CharStatsInfo charStats;
        [SyncVar] [SerializeField] public CharLvlInfo lvlInfo;

        [SerializeField] List<EffectBase> effects;
        

        private void Awake()
        {
            effects = new List<EffectBase>();
            charStats.CurrentHP = charStats.MaxHP;

            charStats.CurrentMana = charStats.MaxMana;
        }
        public int HP => charStats.CurrentHP;

        public void FixedUpdate()
        {
            if(!base.IsOwner)
            { return; }
            if(charStats.CurrentHP <=0)
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
    public struct CharStatsInfo
    {
        public int MaxHP;
        public int CurrentHP;
        public int MaxMana;
        public int CurrentMana;
        public int Str;
        public int Dex;
        public int Int;
        public int Wis;
        public int Luck;
    }
}
