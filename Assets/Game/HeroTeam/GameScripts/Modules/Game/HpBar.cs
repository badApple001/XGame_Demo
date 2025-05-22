using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;

public class HpBar : MonoBehaviour
{
    private SpriteRenderer m_SpriteRenderer;
    private MaterialPropertyBlock m_MPB;

    private static readonly int HealthID = Shader.PropertyToID( "_Health" );
    private static readonly int TrailAlphaID = Shader.PropertyToID( "_TrailAlpha" );
    private static readonly int LastHealthID = Shader.PropertyToID( "_LastHealth" );

    private float m_fCurrentHealth = 1.1f;
    private float m_fLastHealth = 1f;
    private float m_fTrailAlpha = 0f;
    private ICreatureEntity m_iEntity;

    private void Awake( )
    {
        m_SpriteRenderer = GetComponent<SpriteRenderer>( );
        m_MPB = new MaterialPropertyBlock( );
    }

    private void Update( )
    {

        if ( m_iEntity != null )
        {
            float newHealth = m_iEntity.GetHP( ) / m_iEntity.GetMaxHP( );
            if( newHealth != m_fCurrentHealth )
            {
                TakeDamage( newHealth );
            }
        }


        // 拖尾透明度慢慢减少
        if ( m_fTrailAlpha > 0f )
        {
            m_fTrailAlpha -= Time.deltaTime * 1.5f; // 可调淡出速度
            m_fTrailAlpha = Mathf.Max( m_fTrailAlpha, 0f );
            UpdateMaterial( );
        }
    }

    public void SetEntity( ICreatureEntity creatureEntity )
    {
        m_iEntity = creatureEntity;
    }
 
    // 更新材质
    private void UpdateMaterial( )
    {
        m_SpriteRenderer.GetPropertyBlock( m_MPB );
        m_MPB.SetFloat( HealthID, m_fCurrentHealth );
        m_MPB.SetFloat( LastHealthID, m_fLastHealth );
        m_MPB.SetFloat( TrailAlphaID, m_fTrailAlpha );
        m_SpriteRenderer.SetPropertyBlock( m_MPB );
    }

    // 受伤调用，设置当前血量，启动拖尾效果
    private void TakeDamage( float newHealth )
    {
        newHealth = Mathf.Clamp01( newHealth );

        if ( !Mathf.Approximately( newHealth, m_fCurrentHealth ) )
        {
            m_fLastHealth = m_fCurrentHealth;
            m_fCurrentHealth = newHealth;

            m_fTrailAlpha = 1f;
            UpdateMaterial( );
        }
    }
}
