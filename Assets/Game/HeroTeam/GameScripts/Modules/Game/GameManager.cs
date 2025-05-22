using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using XClient.Common;
using GameScripts.Monster;
using XGame.EventEngine;

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
            Debug.Log( m_vec3HeroSpawnPoints.Count );
            for ( int i = 0; i < m_vec3HeroSpawnPoints.Count; i++ )
            {
                RefreshMonsterMgr.Instance.RefreshHero( 1004, m_vec3HeroSpawnPoints[ i ], BATTLE_CAMP_DEF.BATTLE_CAMP_HERO );
            }

        }

        private void CreateBoss( )
        {

            cfg_HeroTeamBuff buff = GameGlobal.GameScheme.HeroTeamBuff_0( 1001 );

            RefreshMonsterMgr.Instance.RefreshBoss( 3, 1, 1, 1 );
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


        //private void Update( )
        //{
        //    if ( Input.GetKeyDown( KeyCode.Space ) )
        //    {
        //        var pContext = CameraShakeEventContext.Ins;
        //        pContext.intensity = 1f;
        //        pContext.duration = 0.5f;
        //        pContext.vibrato = 30;
        //        pContext.randomness = 100f;
        //        pContext.fadeOut = true;
        //        GameGlobal.EventEgnine.FireExecute( DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext );
        //    }
        //}
    }

}