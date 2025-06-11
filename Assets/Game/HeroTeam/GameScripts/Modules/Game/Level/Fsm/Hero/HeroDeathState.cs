using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniFramework.Machine;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    public class HeroDeathState : SpineCreatureStateBase
    {

        public override void OnCreate(StateMachine machine)
        {
            base.OnCreate(machine);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            m_Owner.GetTr().DOKill();
            m_Owner.ClearTimes();
            m_Owner.SetState(ActorState.Dying);
            m_Anim.state.SetAnimation(0, m_ActorAnimConfig.szDeath, false);
            //后续换到帧事件中
            AddTimer(1f, () =>
            {

                m_Owner.SetState(ActorState.Release);
            });
        }

        public override void OnExit()
        {
            base.OnExit();

        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}