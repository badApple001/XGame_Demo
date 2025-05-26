/*******************************************************************
** 文件名:	AIRouteMoveAction.cs
** 版  权:	(C) 冰川网络
** 创建人:	许德纪
** 日  期:	2024.8.28
** 版  本:	1.0
** 描  述:	
** 应  用:  控制实体按路由点移动

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
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
        //行为的优先级
        private int m_nPriority = 0;

        //这个AI的拥有者
        private ICreatureEntity m_Master;

        //初始化位置
        private Vector3 m_startPos;

        //目标位置
        private Vector3 m_targetPos;

        //移动的距离
        private float m_fDistance;

        //是否运行
        private bool m_bRun = false;

        //前向移动组件
        private ForwardMovement m_forwardMovement;

        //spine组件控制动作
        private SpineAni m_spineAni;

        //当前第几段路
        private int m_moveSeg = 0;

        //路由表格
        private List<Vector3> m_listRoutePoints = null;


        public bool Create( )
        {
            return true;
        }

        public int GetPriority( )
        {
            return m_nPriority;
        }

        public void Init( object context = null )
        {
            cfg_AI cfg = context as cfg_AI;
            m_nPriority = cfg.iPriority;
            m_fDistance = cfg.param[ 0 ];


            m_moveSeg = 0;


        }

        public void OnExecute( ushort wEventID, byte bSrcType, uint dwSrcID, object pContext )
        {

            if ( wEventID == DHeroTeamEvent.EVENT_START_BATTLE )
            {
                //可以执行移动了
                m_bRun = true;
                Debug.Log( "AI: 开始战斗" );

                //初始化移动段
                m_moveSeg = 0;
                __InitNextSeg( );
            }

            //if (wEventID == DGlobalEventEx.EVENT_STOP_MOVE)
            //{
            //    SEVENT_STOP_MOVE context = (SEVENT_STOP_MOVE)pContext;
            //    if (m_Master != null && m_Master.id == context.srcID)
            //    {
            //        //停止移动
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
            //        //停止移动
            //        if (m_forwardMovement != null && m_forwardMovement.IsMoving())
            //        {
            //            m_forwardMovement.StopMove();
            //            m_spineAni?.DoAction("idle", true);
            //        }
            //    }
            //}



        }

        public bool OnExeUpdate( )
        {

            if ( m_bRun == false )
                return false;


            if ( m_Master.IsDie( ) )
            {
                m_forwardMovement.StopMove( );
                m_spineAni?.DoAction( "death", false, true );
                return false;
            }



            //假如已经到达目标点了
            //PrefabPart visiblePart = m_Master.GetPart<PrefabPart>( );
            Vector3 curPos = m_Master.GetPos( );// visiblePart.transform.position;// m_Master.GetPos();
            if ( Vector3.Distance( curPos, m_targetPos ) <= 0.01f )
            {
                //先停止移动，后续从新开始移动
                m_forwardMovement.StopMove( );
                m_Master.SetPos( ref m_targetPos );


                //到目的地了,移动到下一段，没有下一段了，就移动结束
                m_moveSeg++;
                if ( __InitNextSeg( ) == false )
                {
                    m_spineAni?.DoAction( "idle", true );
                    m_bRun = false;
                    return false;
                }
            }
            else
            {
                //向目标点移动


            }

            //没有在移动或者目标点大于0.1的，重新移动
            if ( m_forwardMovement.IsMoving( ) == false )
            {

                //发送投票消息，询问是否能移动
                //SEVENT_CAN_MOVE.Instance.srcID = m_Master.id;
                bool ret = true;// GameGlobalEx.EventEgnine.FireVote( DGlobalEventEx.EVENT_CAN_MOVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, SEVENT_CAN_MOVE.Instance );
                if ( ret )
                {
                    m_forwardMovement.StartMove( m_Master.GetSpeed( ), curPos, m_targetPos );
                    m_spineAni?.DoAction( "run", true );
                }
            }


            return false;
        }

        public void OnReceiveEntityMessage( uint id, object data = null )
        {
            if ( id == EntityMessageID.ResLoaded )
            {

                //通过阵营判断是本地端还是远端
                //ulong camp = m_Master.GetCamp( );
                //if ( CampDef.IsLocalCamp( camp ) == false )
                //{
                //    return;
                //}

                //m_listRoutePoints = RefreshMonsterMgr.Instance.GetSelfRoutePoints( );
                if ( m_Master is IMonster m )
                {
                    m_listRoutePoints = m.GetRoad( );
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

                PrefabPart visiblePart = m_Master.GetPart<PrefabPart>( );
                if ( null != visiblePart && visiblePart.transform )
                {
                    m_spineAni = visiblePart.transform.GetComponent<SpineAni>( );
                    m_forwardMovement = visiblePart.transform.GetComponent<ForwardMovement>( );
                    if ( m_forwardMovement )
                    {
                        //AI的移动,每个客户端自己管，不需要同步
                        m_forwardMovement.EnableSync( false );

                        //开始移动
                        //m_forwardMovement.StartMove( m_Master.GetSpeed( ), m_startPos, m_targetPos );
                        //m_spineAni?.DoAction( "move", true );
                    }
                }

            }
        }

        public void Release( )
        {
            Reset( );
        }

        public void Reset( )
        {
            Stop( );
            if ( m_forwardMovement )
            {
                m_forwardMovement.StopMove( );
                m_spineAni?.DoAction( "idle", true );
            }

            m_Master = null;
            m_forwardMovement = null;
            m_spineAni = null;
            m_listRoutePoints = null;
            m_bRun = false;
        }

        public void SetMaster( ICreatureEntity master )
        {
            m_Master = master;

        }

        public void Start( )
        {
            //订阅销毁消息
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>( );
            eventEngine.Subscibe( this, DHeroTeamEvent.EVENT_START_BATTLE, DEventSourceType.SOURCE_TYPE_UI, 0, "AIRouteMoveAction:Start" );// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity)
            //eventEngine.Subscibe( this, DGlobalEventEx.EVENT_MOVE_SPEED_UPDATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "AIMoveAction:Start" );// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity)

        }

        public void Stop( )
        {
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>( );
            eventEngine.UnSubscibe( this, DHeroTeamEvent.EVENT_START_BATTLE, DEventSourceType.SOURCE_TYPE_UI, 0 );// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity)

            //    eventEngine.UnSubscibe(this, DGlobalEventEx.EVENT_STOP_MOVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity)
            //    eventEngine.UnSubscibe(this, DGlobalEventEx.EVENT_MOVE_SPEED_UPDATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
        }

        private bool __InitNextSeg( )
        {
            m_startPos = m_Master.GetPos( );
            m_targetPos = m_startPos;
            if ( m_moveSeg < m_listRoutePoints.Count )
            {
                m_targetPos = m_listRoutePoints[ m_moveSeg ];
                return true;
            }

            return false;
        }
    }


}



