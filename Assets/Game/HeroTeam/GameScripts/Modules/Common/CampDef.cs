using GameScripts.HeroTeam;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;


namespace GameScripts.HeroTeam
{

    public class CampDef
    {

        public const int BATTLE_CAMP_MONSTER = 1;

        public const int BATTLE_CAMP_HERO = 2;

        public static ulong GetLocalCamp(ulong camp)
        {
            ulong localCamp = camp;
            //localCamp = ( localCamp << 32 ) | ( GameGlobal.RoleAgent.id >> 32 );
            return localCamp;
        }

        public static ulong GetRemoteCamp(ulong camp)
        {
            ulong remoteCamp = camp;
            //remoteCamp = ( remoteCamp << 32 ) | ( GameGlobalEx.HeroTeam.Data.OpponentID >> 32 );
            return remoteCamp;
        }

        public static bool IsLocalCamp(ulong camp)
        {
            ulong roleCamp = GameGlobal.RoleAgent.id >> 32;
            camp = camp & 0x00000000FFFFFFFF;

            return roleCamp == camp;
        }
    }

}