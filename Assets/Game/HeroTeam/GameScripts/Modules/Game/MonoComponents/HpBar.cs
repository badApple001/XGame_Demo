using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;


namespace GameScripts.HeroTeam
{

    public class HpBar : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer m_SpriteRenderer;
        private MaterialPropertyBlock m_MPB;

        private static readonly int HealthID = Shader.PropertyToID("_Health");
        private static readonly int TrailAlphaID = Shader.PropertyToID("_TrailAlpha");
        private static readonly int LastHealthID = Shader.PropertyToID("_LastHealth");

        private float m_fCurrentHealth = 1.1f;
        private float m_fLastHealth = 1f;
        private float m_fTrailAlpha = 0f;
        private ISpineCreature m_iEntity;
        private Action m_applyDamage;

        private void Awake()
        {
            m_MPB = new MaterialPropertyBlock();
        }

        private void Update()
        {

            if (m_iEntity != null)
            {
                float newHealth = m_iEntity.GetHP() * 1.0f / m_iEntity.GetMaxHP();
                if (newHealth != m_fCurrentHealth)
                {
                    if (null != m_applyDamage && newHealth < m_fCurrentHealth)
                    {
                        m_applyDamage();
                    }
                    RefreshMaterialProperty(newHealth);
                }
            }

            if (m_fTrailAlpha > 0f)
            {
                m_fTrailAlpha -= TimeUtils.DeltaTime * 1.5f;
                m_fTrailAlpha = Mathf.Max(m_fTrailAlpha, 0f);
                UpdateMaterial();
            }
        }

        public void SetEntity(ISpineCreature actor)
        {
            m_iEntity = actor;
        }

        // ���²���
        private void UpdateMaterial()
        {
            m_SpriteRenderer.GetPropertyBlock(m_MPB);
            m_MPB.SetFloat(HealthID, m_fCurrentHealth);
            m_MPB.SetFloat(LastHealthID, m_fLastHealth);
            m_MPB.SetFloat(TrailAlphaID, m_fTrailAlpha);
            m_SpriteRenderer.SetPropertyBlock(m_MPB);
        }

        // ���˵��ã����õ�ǰѪ����������βЧ��
        private void RefreshMaterialProperty(float newHealth)
        {
            newHealth = Mathf.Clamp01(newHealth);

            if (!Mathf.Approximately(newHealth, m_fCurrentHealth))
            {
                if (newHealth > m_fCurrentHealth)
                {
                    m_fLastHealth = newHealth;
                    m_fCurrentHealth = newHealth;
                }
                else
                {
                    m_fLastHealth = m_fCurrentHealth;
                    m_fCurrentHealth = newHealth;
                }

                m_fTrailAlpha = 1f;
                UpdateMaterial();
            }
        }

        public void SetApplyDamageCallback(Action callback)
        {
            m_applyDamage = callback;
        }

    }


}