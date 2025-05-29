using System;
using System.Collections.Generic;
using Spine.Unity;
using UniFramework.Machine;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    public class ActorStateBase : IStateNode
    {
        protected SkeletonAnimation m_Anim;
        protected cfg_ActorAnimConfig m_ActorAnimConfig;
        protected StateMachine m_StateMachine;
        protected IActor m_Owner;
        protected List<Coroutine> m_arrTimerGroup = new List<Coroutine>();
        public virtual void OnCreate(StateMachine machine)
        {
            m_Anim = ((IActor)machine.Owner).GetSkeleton();
            m_StateMachine = machine;
        }

        public virtual void OnEnter()
        {
            m_Owner = ((IActor)m_StateMachine.Owner);
            m_ActorAnimConfig = ((IActor)m_StateMachine.Owner).GetAnimConfig();
        }

        protected void AddTimer(float delay, Action callback)
        {
            var handler = GameManager.instance.AddTimer(delay, callback);
            m_arrTimerGroup.Add(handler);
        }

        public virtual void OnExit()
        {
            if (m_arrTimerGroup.Count > 0)
            {
                GameManager.instance.ClearTimers(m_arrTimerGroup);
                m_arrTimerGroup.Clear();
            }

        }

        public virtual void OnUpdate()
        {
        }
    }

}