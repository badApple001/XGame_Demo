/*******************************************************************
** �ļ���:	AIRouteMoveAction.cs
** ��  Ȩ:	(C) ��������
** ������:	���¼�
** ��  ��:	2024.8.28
** ��  ��:	1.0
** ��  ��:	
** Ӧ  ��:  ����ʵ�尴·�ɵ��ƶ�

**************************** �޸ļ�¼ ******************************
** �޸���: 
** ��  ��: 
** ��  ��: 
********************************************************************/

using GameScripts;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame.Entity.Part;
using XGame.Entity;
using XGame;
using XGame.EventEngine;
using GameScripts.Monster;

namespace GameScripts.HeroTeam
{

    public class AIRouteMoveAction : IAIAction, IEventExecuteSink
    {
        //��Ϊ�����ȼ�
        private int m_nPriority = 0;

        //���AI��ӵ����
        private ICreatureEntity m_Master;

        //��ʼ��λ��
        private Vector3 m_startPos;

        //Ŀ��λ��
        private Vector3 m_targetPos;

        //�ƶ��ľ���
        private float m_fDistance;

        //�Ƿ�����
        private bool m_bRun = false;

        //ǰ���ƶ����
        private ForwardMovement m_forwardMovement;

        //spine������ƶ���
        private SpineAni m_spineAni;

        //��ǰ�ڼ���·
        private int m_moveSeg = 0;

        //·�ɱ���
        private List<Vector3> m_listRoutePoints = null;


        public bool Create()
        {
            return true;
        }

        public int GetPriority()
        {
            return m_nPriority;
        }

        public void Init(object context = null)
        {
            cfg_AI cfg = context as cfg_AI;
            m_nPriority = cfg.iPriority;
            m_fDistance = cfg.param[0];


            m_moveSeg = 0;


        }

        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {

            if (wEventID == DHeroTeamEvent.EVENT_START_GAME)
            {
                //����ִ���ƶ���
                m_bRun = true;
                Debug.Log("AI: ��ʼս��");

                //��ʼ���ƶ���
                m_moveSeg = 0;
                __InitNextSeg();
            }

            //if (wEventID == DGlobalEventEx.EVENT_STOP_MOVE)
            //{
            //    SEVENT_STOP_MOVE context = (SEVENT_STOP_MOVE)pContext;
            //    if (m_Master != null && m_Master.id == context.srcID)
            //    {
            //        //ֹͣ�ƶ�
            //        if (m_forwardMovement != null && m_forwardMovement.IsMoving())
            //        {
            //            m_forwardMovement.StopMove();
            //            m_spineAni?.DoAction("idle", true);
            //        }
            //    }
            //}
            //else
            //{
            //    SEVENTP_MOVE_SPEED_UPDATE context = (SEVENTP_MOVE_SPEED_UPDATE)pContext;
            //    if (m_Master != null && m_Master.id == context.srcID)
            //    {
            //        //ֹͣ�ƶ�
            //        if (m_forwardMovement != null && m_forwardMovement.IsMoving())
            //        {
            //            m_forwardMovement.StopMove();
            //            m_spineAni?.DoAction("idle", true);
            //        }
            //    }
            //}



        }

        public bool OnExeUpdate()
        {

            if (m_bRun == false)
                return false;


            if (m_Master.IsDie())
            {
                m_forwardMovement.StopMove();
                m_spineAni?.DoAction("death", false, true);
                return false;
            }



            //�����Ѿ�����Ŀ�����
            //PrefabPart visiblePart = m_Master.GetPart<PrefabPart>( );
            Vector3 curPos = m_Master.GetPos();// visiblePart.transform.position;// m_Master.GetPos();
            if (Vector3.Distance(curPos, m_targetPos) <= 0.01f)
            {
                //��ֹͣ�ƶ����������¿�ʼ�ƶ�
                m_forwardMovement.StopMove();
                m_Master.SetPos(ref m_targetPos);


                //��Ŀ�ĵ���,�ƶ�����һ�Σ�û����һ���ˣ����ƶ�����
                m_moveSeg++;
                if (__InitNextSeg() == false)
                {
                    m_spineAni?.DoAction("idle", true);
                    m_bRun = false;



                    // //获取自身阵营
                    // var camp = m_Master.GetCamp();
                    // //获取地方阵营
                    // var foes = MonsterSystem.Instance.GetMonstersNotEqulCamp(camp);
                    // //找到最近的 朝向
                    // var bossEntity = GameManager.instance.GetBossEntity();
                    var pSkeleton = m_Master.GetPart<PrefabPart>().transform.GetComponent<SpineAni>().skeletonAnimation.skeleton;
                    // float x =  bossEntity.GetPos().x - m_Master.GetPos().x;
                    float x = m_Master.GetPos().x;
                    pSkeleton.ScaleX = x < 0 ? 1f : -1f;

                    return false;
                }
            }
            else
            {
                //��Ŀ����ƶ�


            }

            //û�����ƶ�����Ŀ������0.1�ģ������ƶ�
            if (m_forwardMovement.IsMoving() == false)
            {

                //����ͶƱ��Ϣ��ѯ���Ƿ����ƶ�
                //SEVENT_CAN_MOVE.Instance.srcID = m_Master.id;
                bool ret = true;// GameGlobalEx.EventEgnine.FireVote( DGlobalEventEx.EVENT_CAN_MOVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, SEVENT_CAN_MOVE.Instance );
                if (ret)
                {
                    m_forwardMovement.StartMove(m_Master.GetSpeed(), curPos, m_targetPos);
                    m_spineAni?.DoAction("run", true);
                }
            }


            return false;
        }

        public void OnReceiveEntityMessage(uint id, object data = null)
        {
            if (id == EntityMessageID.ResLoaded)
            {

                //ͨ����Ӫ�ж��Ǳ��ض˻���Զ��
                //ulong camp = m_Master.GetCamp( );
                //if ( CampDef.IsLocalCamp( camp ) == false )
                //{
                //    return;
                //}

                //m_listRoutePoints = RefreshMonsterMgr.Instance.GetSelfRoutePoints( );
                if (m_Master is IMonster m)
                {
                    m_listRoutePoints = m.GetRoad();
                }

                /*
                if (CampDef.IsLocalCamp(camp))
                {
                    m_listRoutePoints = RefreshMonsterMgr.Instance.GetSelfRoutePoints();    
                }else
                {
                    m_listRoutePoints = RefreshMonsterMgr.Instance.GetEnemyRoutePoints();
                }
                */



                //m_bRun = true;

                PrefabPart visiblePart = m_Master.GetPart<PrefabPart>();
                if (null != visiblePart && visiblePart.transform)
                {
                    m_spineAni = visiblePart.transform.GetComponent<SpineAni>();
                    m_forwardMovement = visiblePart.transform.GetComponent<ForwardMovement>();
                    if (m_forwardMovement)
                    {
                        //AI���ƶ�,ÿ���ͻ����Լ��ܣ�����Ҫͬ��
                        m_forwardMovement.EnableSync(false);

                        //��ʼ�ƶ�
                        //m_forwardMovement.StartMove( m_Master.GetSpeed( ), m_startPos, m_targetPos );
                        //m_spineAni?.DoAction( "move", true );
                    }
                }

            }
        }

        public void Release()
        {
            Reset();
        }

        public void Reset()
        {
            Stop();
            if (m_forwardMovement)
            {
                m_forwardMovement.StopMove();
                m_spineAni?.DoAction("idle", true);
            }

            m_Master = null;
            m_forwardMovement = null;
            m_spineAni = null;
            m_listRoutePoints = null;
            m_bRun = false;
        }

        public void SetMaster(ICreatureEntity master)
        {
            m_Master = master;

        }

        public void Start()
        {
            //����������Ϣ
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            eventEngine.Subscibe(this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0, "AIRouteMoveAction:Start");// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity)
            //eventEngine.Subscibe( this, DGlobalEventEx.EVENT_MOVE_SPEED_UPDATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "AIMoveAction:Start" );// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity)

        }

        public void Stop()
        {
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            eventEngine.UnSubscibe(this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0);// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity)

            //    eventEngine.UnSubscibe(this, DGlobalEventEx.EVENT_STOP_MOVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity)
            //    eventEngine.UnSubscibe(this, DGlobalEventEx.EVENT_MOVE_SPEED_UPDATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
        }

        private bool __InitNextSeg()
        {
            m_startPos = m_Master.GetPos();
            m_targetPos = m_startPos;
            if (m_moveSeg < m_listRoutePoints.Count)
            {
                m_targetPos = m_listRoutePoints[m_moveSeg];
                return true;
            }

            return false;
        }
    }


}



