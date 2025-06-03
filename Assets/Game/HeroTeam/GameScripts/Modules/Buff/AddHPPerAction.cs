/*******************************************************************
** �ļ���:	AddHPPerAction.cs
** ��  Ȩ:	(C) ��������
** ������:	���¼�
** ��  ��:	2024.7.16
** ��  ��:	1.0
** ��  ��:	
** Ӧ  ��:  ���ٷֱ�����Ѫ��

**************************** �޸ļ�¼ ******************************
** �޸���: 
** ��  ��: 
** ��  ��: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;
using XGame.Entity;
using XGame;

namespace GameScripts.HeroTeam
{
    public class AddHPPerAction : IEffectAction
    {
        //ӵ����ID
        public ulong m_owerID;

        //��Ѫ�ĸ���
        public int m_addRate = 0;

        //��������
        public int m_monsterType = 0;

        //����Ѫ���İٷֱ�
        public int m_hpCoff = 0;

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
                Debug.LogError("AttackSecKillAction �������ԣ�����Ϊ2����ʵ��Ϊ" + effectCmdContext.param.Count);
                return;
            }
            m_addRate = effectCmdContext.param[0];
            m_monsterType = effectCmdContext.param[1];
            m_hpCoff = effectCmdContext.param[2];
        }

        public bool IsFinish()
        {
            return true;
        }

        public void OnUpdate()
        {
           
        }

        public void Release()
        {
            Reset();
        }

        public void Reset()
        {
           
        }

        public void Start()
        {
            // IEntityManager manager = XGameComs.Get<IEntityManager>();
            // ICreatureEntity entity = manager.GetEntity(m_owerID) as ICreatureEntity;
            // if (null != entity)
            // {
            //     cfg_Monster cfg_Monster = entity.config as cfg_Monster;
            //     if (m_monsterType==0||cfg_Monster != null && cfg_Monster.iMonsterType == m_monsterType)
            //     {
            //         int rate = Random.Range(1, 1001);
            //         if(rate<= m_addRate)
            //         {
            //             int hp = entity.GetHP();
            //             int incHP = (int)(hp * (float)m_hpCoff / 1000.0f); 
            //             entity.SetHPDelta(incHP);

            //             //if (true)
            //             //{
            //             //    int finalHP = entity.GetHP();
            //             //    Debug.LogWarning("��" + cfg_Monster.szName + "����Ѫ��:" + incHP + "ԭʼѪ�� =" + hp + ",����Ѫ�� finalHP=" + finalHP);
            //             //}
            //         }
            //     }
            // }
        }

        public void Stop()
        {
           
        }
    }

}
