using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using XClient.Common;
using GameScripts.Monster;

namespace GameScripts.HeroTeam
{
    public class GameManager : MonoBehaviour
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

            for ( int i = 0; i < m_vec3HeroSpawnPoints.Count; i++ )
            {
                RefreshMonsterMgr.Instance.RefreshHero( 1004, m_vec3HeroSpawnPoints[ i ], 1 );
            }
        }

        private void CreateBoss( )
        {
            RefreshMonsterMgr.Instance.RefreshBoss( 3, 1, 1, 1 );
        }





    }

}