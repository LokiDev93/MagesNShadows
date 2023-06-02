using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagesnShadows.GroupSystems;

namespace MagesnShadows.GroupSystems.Guild
{
    public class Guild : GroupBase
    {
        [SerializeField] private Dictionary<GRank, GRankPerm> rankPermSets;
        //ulong is the character ID;
        [SerializeField] private Dictionary<ulong, GRank> members;
        [SerializeField] private List<ulong> applicationList;

        [SerializeField] private GStatus applicationStatus;
        [SerializeField] private string description;
        [SerializeField] private GuildPerms guildPerms;

        public override void InitializeOnce()
        {
            base.Initialized = true;


        }


    }


    public enum GRank
    {
        Recruit = 0,
        Member = 1,
        Veteran = 2,
        Officer = 3,
        Leader = 4
    }

    public struct GRankPerm
    {
        public bool canInvite; //can invite others
        public bool canKick; //can kick others
        public bool rankChanger; //can change ranks
        public bool changeStatus; //can change application status
        public bool changeDescription; //can change description
        public bool changeNews; //Can set the news
        public bool changePerms; //can change perms
    }

    public struct GuildPerms
    {

    }
    public enum GStatus
    {
        Open = 0,
        Closed = 1,
        InviteOnly = 2
    }
}
