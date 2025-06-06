using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;
using XClient.Entity;

namespace GameScripts.HeroTeam
{
    public class MonsterIdleState : SpineCreatureStateBase
    {
        private float m_AttackCoolding = 0f;
        private float m_AttackInterval = 1f;
        public override void OnCreate(StateMachine machine)
        {
            base.OnCreate(machine);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            m_Anim.state.SetAnimation(0, m_ActorAnimConfig.szIdle, true);
            m_AttackCoolding = 0f;
            m_AttackInterval = m_Cfg.fAttackInterval;
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            m_AttackCoolding += TimeUtils.DeltaTime;

            if (m_AttackCoolding >= m_AttackInterval)
            {
                m_AttackCoolding = 0f;

                if (m_StateMachine != null)
                {
                    
                    //玩家有一定概率不攻击 而是跳跃
                    // if (!((IMonster)m_Owner.GetCreatureEntity()).IsBoos() && Random.value < 0.1f)
                    // {
                    //     m_StateMachine.ChangeState<ActorJumpState>();
                    // }
                    // else
                    {
                        m_StateMachine.ChangeState<MonsterAttackState>();
                    }
                }
            }
        }
    }
}
