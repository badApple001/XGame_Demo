using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;

namespace GameScripts.HeroTeam
{

    /// <summary>
    /// 纵火者Buff
    /// 
    /// 每秒钟受到固定伤害
    /// 持续N秒
    /// 
    /// </summary>
    public class ArsonistBuff : BuffBase
    {

        public override bool IsDone()
        {
            return m_nTimes >= m_Cfg.count;
        }

        public override void OnStep(float now)
        {
            base.OnStep(now);
            if (null != m_Owner)
            {
                m_Owner.SetHPDelta(m_Cfg.AddHp);
            }
        }

    }

}