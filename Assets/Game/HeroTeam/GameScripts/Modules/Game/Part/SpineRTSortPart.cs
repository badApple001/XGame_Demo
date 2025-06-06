using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using XGame.Entity;
using XGame.Entity.Part;

namespace GameScripts.HeroTeam
{
    public class SpineRTSortPart : BasePart
    {

        private ISpineCreature m_actor;

        private SkeletonAnimation m_skel;

        private Renderer m_renderer;

        private int m_nSortingLayer = int.MinValue;

        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            base.OnReceiveEntityMessage(id, data);
            if (id == EntityMessageID.ResLoaded)
            {
                m_actor = master as ISpineCreature;
                m_skel = m_actor.GetSkeleton();
                m_renderer = m_skel?.GetComponent<Renderer>();
            }
        }

        protected override void OnReset()
        {
            base.OnReset();

        }
        public override void OnUpdate()
        {
            base.OnUpdate();

            if (null != m_renderer)
            {
                Vector3 pos = m_actor.GetPos();
                int currentLayer = (int)(((-pos.y + 25) * 2 + (pos.x + 25)) * 100);
                if (currentLayer != m_nSortingLayer)
                {
                    m_nSortingLayer = currentLayer;
                    m_renderer.sortingOrder = m_nSortingLayer;
                }
            }

        }
    }

}