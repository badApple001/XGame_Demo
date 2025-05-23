using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using XGame;
using XGame.EventEngine;
using XGame.Entity;
using GameScripts.Monster;
using XClient.Common;
using XClient.Entity;

namespace GameScripts.HeroTeam
{
    public class GameManager : MonoBehaviour, IEventExecuteSink
    {
        [SerializeField] private Transform m_trHeroSpawnRoot;
        [SerializeField] private Transform m_trBossSpawnRoot;
        private List<Vector3> m_vec3HeroSpawnPoints = new List<Vector3>( );

        // Start is called before the first frame update
        void Start( )
        {
            //GameGlobal.GameScheme.


            ReadSpawnPoints( );
            CreateHeros( );
            CreateBoss( );
        }

        private void ReadSpawnPoints( )
        {
            Debug.Assert( m_trHeroSpawnRoot != null && m_trHeroSpawnRoot.childCount > 0, "角色出生点不能为空" );

            for ( int i = 0; i < m_trHeroSpawnRoot.childCount; i++ )
            {
                Transform spawnPoint = m_trHeroSpawnRoot.GetChild( i );
                m_vec3HeroSpawnPoints.Add( spawnPoint.position );
            }
        }


        private void CreateHeros( )
        {

            //临时， 后续移到关卡表
            Dictionary<int, int> dictTmpHeros = new Dictionary<int, int>( )
            {
                { 1004,3 },
                { 1005,2 },
                { 1006,2 },
                { 1007,3 },
                { 1008,4 },
                { 1009,3 },
                { 1010,5 },
                { 1011,3 },
            };


            foreach ( var cfg in dictTmpHeros )
            {
                for ( int i = 0; i < cfg.Value; i++ )
                {
                    Vector3 pos = m_vec3HeroSpawnPoints[ m_vec3HeroSpawnPoints.Count - 1 ];
                    m_vec3HeroSpawnPoints.RemoveAt( m_vec3HeroSpawnPoints.Count - 1 );

                    RefreshMonsterMgr.Instance.RefreshHero( cfg.Key, pos, BATTLE_CAMP_DEF.BATTLE_CAMP_HERO );
                }
            }

        }

        private void CreateBoss( )
        {

            ulong bossEntityId = RefreshMonsterMgr.Instance.RefreshBoss( 3, 1, 1, 1 );

            IEntity bossEntity = GameGlobal.EntityWorld.Local.GetEntity( bossEntityId );

            if( bossEntity is IMonster monster )
            {

                //所有的远程角色平均分布在外圈




            }
        }

        private void OnEnable( )
        {
            GameGlobal.EventEgnine.Subscibe( this, DHeroTeamEvent.EVENT_START_BATTLE, DEventSourceType.SOURCE_TYPE_UI, 0, "GameManager:OnEnable" );
        }
        private void OnDisable( )
        {
            GameGlobal.EventEgnine.UnSubscibe( this, DHeroTeamEvent.EVENT_START_BATTLE, DEventSourceType.SOURCE_TYPE_UI, 0 );
        }


        public void OnExecute( ushort wEventID, byte bSrcType, uint dwSrcID, object pContext )
        {
            Debug.Log( "开始战斗" );



        }


        private void Update( )
        {
            if ( Input.GetKeyDown( KeyCode.Space ) )
            {
                var pContext = CameraShakeEventContext.Ins;
                pContext.intensity = 1f;
                pContext.duration = 0.5f;
                pContext.vibrato = 30;
                pContext.randomness = 100f;
                pContext.fadeOut = true;
                GameGlobal.EventEgnine.FireExecute( DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext );
            }

            if ( Input.GetKeyDown( KeyCode.Q ) )
            {
                var pContext = BossHpEventContext.Ins;
                pContext.Health -= 0.2f;
                GameGlobal.EventEgnine.FireExecute( DHeroTeamEvent.EVENT_BOSS_HP_CHANGED, DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext );
            }
        }
    }

}