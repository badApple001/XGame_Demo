using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;

namespace GameScripts.HeroTeam
{
    public class LeaderBuff : BuffBase
    {

        public override void OnInit(ISpineCreature owner, cfg_HeroTeamBuff cfg)
        {
            base.OnInit(owner, cfg);
            m_trEffect.rotation = Quaternion.Euler(45, 0, 0);
        }
    }
}