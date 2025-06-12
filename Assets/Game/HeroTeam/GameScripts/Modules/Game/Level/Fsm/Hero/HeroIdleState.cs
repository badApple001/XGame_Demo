using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;
using XClient.Entity;

namespace GameScripts.HeroTeam
{
    public class HeroIdleState : SpineCreatureStateBase
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

            Debug.Log("EnterIdle");

            m_Anim.state.SetAnimation(0, m_ActorAnimConfig.szIdle, true);
            m_AttackCoolding = 0f;
            m_AttackInterval = m_Owner.GetATKInterval();


            //有一定的概率进入AI行为状态
            if (Random.value < 0.2f)
            {
                m_StateMachine.ChangeState<HeroRandomBehaviourState>();
            }
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
                    m_StateMachine.ChangeState<HeroAttackState>();
                }
            }
        }
    }
}
