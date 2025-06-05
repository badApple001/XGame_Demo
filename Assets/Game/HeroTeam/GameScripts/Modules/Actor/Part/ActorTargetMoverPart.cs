using System;
using System.Collections.Generic;
using UnityEngine;
using XGame.Entity;
using XGame.Entity.Part;

namespace GameScripts.HeroTeam
{
    public class ActorTargetMoverPart : BasePart
    {
        // public enum EMoveType
        // {
        //     ToDestination,
        //     StepPath
        // }

        private IActor m_actor;
        // private EMoveType m_moveType;
        private bool m_stop = true;
        private List<Vector3> m_path = new List<Vector3>();
        private int m_stepIndex = 0;
        // private Vector3 m_destination;
        private Action m_arriveCallback;
        protected override void OnInit(object context)
        {
            base.OnInit(context);
        }

        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            base.OnReceiveEntityMessage(id, data);
            if (id == EntityMessageID.ResLoaded)
            {

                m_actor = master as IActor;
            }
        }

        protected override void OnReset()
        {
            base.OnReset();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (null != m_actor && !m_stop)
            {
                // Vector3 directional = Vector3.zero;
                // if (m_moveType == EMoveType.StepPath)
                // {
                Vector3 directional = m_path[m_stepIndex] - m_actor.GetPos();
                if (directional.magnitude < 1e-2f)
                {
                    if (++m_stepIndex == m_path.Count)
                    {
                        OnArrive();
                    }
                }
                // }
                // else
                // {
                //     directional = m_destination - m_actor.GetPos();
                // }

                Vector3 N = directional.normalized;
                Vector3 velocity = m_actor.GetSpeed() * TimeUtils.DeltaTime * N;

                //避免走过去了
                if (velocity.magnitude > directional.magnitude)
                {
                    velocity = directional;
                }
                m_actor.SetPos(m_actor.GetPos() + velocity);
                // }
            }
        }

        private void OnArrive()
        {
            m_arriveCallback?.Invoke();
            m_arriveCallback = null;
            m_path.Clear();
            Stop();
        }

        public void Start()
        {
            Debug.Assert(m_path.Count > 0, "不允许路径节点为空，你必须在调用Start之前先调用SetDestination或者SetPath");
            m_stepIndex = 0;
            m_stop = false;
        }
        public void Stop()
        {
            m_stop = true;
        }
        public ActorTargetMoverPart SetPath(List<Vector3> path)
        {
            // m_moveType = EMoveType.StepPath;
            m_path = path;
            return this;
        }
        public ActorTargetMoverPart SetDestination(Vector3 destination)
        {
            // m_moveType = EMoveType.ToDestination;
            // m_destination = destination;
            m_path.Clear();
            m_path.Add(destination);
            return this;
        }

        public ActorTargetMoverPart SetArriveCallback(Action callback)
        {
            m_arriveCallback = callback;
            return this;
        }
    }

}