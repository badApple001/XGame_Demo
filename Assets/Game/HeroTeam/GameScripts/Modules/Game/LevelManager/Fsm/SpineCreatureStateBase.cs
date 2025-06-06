using System;
using System.Collections.Generic;
using Spine.Unity;
using UniFramework.Machine;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    public class SpineCreatureStateBase : IStateNode
    {
        protected SkeletonAnimation m_Anim;
        protected cfg_ActorAnimConfig m_ActorAnimConfig;
        protected StateMachine m_StateMachine;
        protected ISpineCreature m_Owner;
        protected List<Coroutine> m_arrTimerGroup = new List<Coroutine>();
        protected cfg_HeroTeamCreature m_Cfg;
        public virtual void OnCreate(StateMachine machine)
        {
            m_Anim = ((ISpineCreature)machine.Owner).GetSkeleton();
            m_StateMachine = machine;
        }

        public virtual void OnEnter()
        {
            m_Owner = (ISpineCreature)m_StateMachine.Owner;
            m_ActorAnimConfig = m_Owner.GetAnimConfig();
            m_Cfg = (cfg_HeroTeamCreature)m_Owner.GetCreatureCig();
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