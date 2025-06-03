using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame;
using XGame.Entity;

public class CombustibilityAction : IEffectAction
{

    public ulong m_owerID;

    public int m_nSubHpPer = 0;
    public float m_fInterval = 1;
    public int m_nCount = 3;
    public int m_nCurrentTimes = 0;
    private float m_nNextTime = 0;

    public bool Create()
    {
        return true;
    }

    public void Init(object context = null)
    {
        CreateEffectContext createEffectContext = (CreateEffectContext)context;
        EffectCmdContext effectCmdContext = createEffectContext.effectCmdContext;
        m_owerID = createEffectContext.srcID;
        if (null == effectCmdContext.param || effectCmdContext.param.Count < 1)
        {
            Debug.LogError("AttackSecKillAction 参数找不到" + effectCmdContext.param.Count);
            return;
        }
        m_nSubHpPer = effectCmdContext.param[0];
        m_fInterval = effectCmdContext.param[1];
        m_nCount = effectCmdContext.param[2];
        m_nCurrentTimes = 0;
        m_nNextTime = Time.time + m_fInterval;
    }

    public bool IsFinish()
    {
        return m_nCurrentTimes >= m_nCount;
    }

    public void ForceDone() => m_nCurrentTimes = m_nCount;

    public void OnUpdate()
    {
        if (Time.time >= m_nNextTime)
        {
            m_nNextTime = Time.time + m_fInterval;
            m_nCurrentTimes++;
            ProcessDamage();
        }
    }


    private void ProcessDamage()
    {
        ICreatureEntity entity = GameGlobal.EntityWorld.Local.GetEntity(m_owerID) as ICreatureEntity;
        if (null != entity)
        {
            if (entity.IsDie())
            {
                ForceDone();
                return;
            }

            entity.SetHPDelta(-m_nSubHpPer);
        }
    }

    public void Release()
    {

    }

    public void Reset()
    {
        m_nCurrentTimes = 0;
        m_nNextTime = Time.time + m_fInterval;
    }

    public void Start()
    {

    }

    public void Stop()
    {

    }
}
