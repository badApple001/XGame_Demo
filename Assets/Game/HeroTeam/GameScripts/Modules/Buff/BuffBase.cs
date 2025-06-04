using UnityEngine;
using XClient.Entity;

namespace GameScripts.HeroTeam
{

    public class BuffBase : IBuff
    {

        protected cfg_HeroTeamBuff m_Cfg;

        protected IActor m_Owner;

        protected float m_fNextTime = 0f;

        protected int m_nTimes = 0;

        public cfg_HeroTeamBuff GetCfg() => m_Cfg;


        public IActor GetOwner() => m_Owner;

        public virtual bool IsDone() => m_nTimes >= m_Cfg.iCount;

        public virtual float NextTime() => m_fNextTime;

        protected Transform m_trEffect;

        public virtual void OnClear()
        {

            if (null != m_trEffect)
            {
                GameEffectManager.Instance.ReleaseEffect(m_trEffect);
            }

            m_Owner = null;
            m_Cfg = null;
            m_trEffect = null;
            m_fNextTime = 0;
            m_nTimes = 0;
        }


        public virtual void OnInit(IActor owner, cfg_HeroTeamBuff cfg)
        {
            m_Owner = owner;
            m_Cfg = cfg;
            RePlay();

            CreateEffect();
        }

        protected void CreateEffect()
        {
            if (!string.IsNullOrEmpty(m_Cfg.szBuffEffect))
            {
                m_trEffect = GameEffectManager.Instance.ShowEffect(m_Cfg.szBuffEffect, Vector3.zero, Quaternion.identity, 999f);
                m_trEffect.SetParent(m_Owner.GetTr(), false);
                if (m_Cfg.float3RelativePos.Length > 0)
                {
                    m_trEffect.localPosition = new Vector3().FromArray(m_Cfg.float3RelativePos);
                }
            }
        }

        /// <summary>
        /// 重置时间
        /// </summary>
        public virtual void RePlay()
        {
            m_fNextTime = Time.time + m_Cfg.iInterval;
            m_nTimes = 0;
        }

        public virtual void OnStep(float now)
        {
            m_fNextTime = now + m_Cfg.iInterval;
            ++m_nTimes;
        }
    }
}