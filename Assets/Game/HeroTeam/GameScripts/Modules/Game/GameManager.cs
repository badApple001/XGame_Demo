using DG.Tweening;
using GameScripts.Monster;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame;
using XGame.Asset;
using XGame.Entity;
using XGame.Entity.Part;
using XGame.EventEngine;

namespace GameScripts.HeroTeam
{
    public class GameManager : MonoBehaviour, IEventExecuteSink
    {
        [SerializeField] private Transform m_trHeroSpawnRoot;
        [SerializeField] private Transform m_trBossSpawnRoot;

        [Header( "三种攻击类型的道路根节点" )]
        [SerializeField] private Transform m_trAcherRoadRoot;
        [SerializeField] private Transform m_trTankRoadRoot;
        [SerializeField] private Transform m_tWarriorRoadRoot;
        private List<Vector3> m_vec3HeroSpawnPoints = new List<Vector3>( );


        // Start is called before the first frame update
        void Start( )
        {
            FillSpawnPoints( );
            CreateHeros( );
            //CreateBoss( );
        }

        private void FillSpawnPoints( )
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
            //需要创建的角色数量
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

            //近战角色
            List<int> arrTmpWarrior = new List<int>( )
            {
                1005,1005,
                1006,1006
            };

            //远程角色
            List<int> arrTmpAcher = new List<int>( )
            {
                1007,1007,1007,
                1008,1008,1008,1008,
                1009,1009,1009
            };

            //坦克角色
            List<int> arrTmpTank = new List<int>( )
            {
                1004,1004,1004,
            };

            //治疗角色
            List<int> arrTmpHealer = new List<int>( )
            {
                1010,1010,1010,1010,1010,
                1011,1011,1011
            };

            void InsertListEvenly( List<int> target, int[] source )
            {
                int targetCount = target.Count;
                int sourceCount = source.Length;

                // 计算插入间隔
                int interval = targetCount / ( sourceCount + 1 );

                // 从后往前插入，以避免插入操作影响后续插入位置
                int sourceIndex = 0;
                for ( int i = 1; i <= sourceCount; i++ )
                {
                    // 计算插入位置
                    int insertIndex = interval * i;

                    // 确保插入位置不超过当前List的大小
                    if ( insertIndex >= target.Count )
                        break;

                    // 在指定位置插入源数组的元素
                    target.Insert( insertIndex, source[ sourceIndex ] );
                    sourceIndex++;
                }
            }
            InsertListEvenly( arrTmpAcher, arrTmpHealer.ToArray( ) );


            void SpawnByRoadRoot( Transform rootNode, List<int> heroIds, bool autoNear = false, bool ergodic = true )
            {
                Debug.Assert( heroIds.Count != rootNode.childCount, "寻路节点不够" );
                for ( int i = 0; i < heroIds.Count; i++ )
                {
                    Vector3 pos = m_vec3HeroSpawnPoints[ m_vec3HeroSpawnPoints.Count - 1 ];
                    m_vec3HeroSpawnPoints.RemoveAt( m_vec3HeroSpawnPoints.Count - 1 );

                    List<Vector3> road = new List<Vector3>( );

                    if ( autoNear )
                    {
                        if ( i >= heroIds.Count / 2 )
                        {

                            for ( int j = rootNode.childCount - 1; j >= i; j-- )
                            {
                                var p = rootNode.GetChild( j ).position;
                                p.z = 0f;
                                road.Add( p );
                            }
                        }
                        else
                        {
                            for ( int j = 0; j < rootNode.childCount / 2 - i; j++ )
                            {
                                var p = rootNode.GetChild( j ).position;
                                p.z = 0f;
                                road.Add( p );
                            }
                        }
                    }
                    else
                    {
                        if ( !ergodic )
                        {
                            var p = rootNode.GetChild( i ).position;
                            p.z = 0f;
                            road.Add( p );
                        }
                        else
                        {
                            for ( int j = 0; j <= rootNode.childCount - 1 - i; j++ )
                            {
                                var p = rootNode.GetChild( j ).position;
                                p.z = 0f;
                                road.Add( p );
                            }
                        }
                    }
                    RefreshMonsterMgr.Instance.RefreshHero( heroIds[ i ], pos, BATTLE_CAMP_DEF.BATTLE_CAMP_HERO, road );
                }
            }

            //arrTmpAcher
            SpawnByRoadRoot( m_trAcherRoadRoot, arrTmpAcher, true );


            //arrTmpWarrior
            SpawnByRoadRoot( m_tWarriorRoadRoot, arrTmpWarrior );


            //arrTmpTank
            SpawnByRoadRoot( m_trTankRoadRoot, arrTmpTank, false, false );



            //foreach ( var cfg in dictTmpHeros )
            //{
            //    for ( int i = 0; i < cfg.Value; i++ )
            //    {
            //        Vector3 pos = m_vec3HeroSpawnPoints[ m_vec3HeroSpawnPoints.Count - 1 ];
            //        m_vec3HeroSpawnPoints.RemoveAt( m_vec3HeroSpawnPoints.Count - 1 );
            //        RefreshMonsterMgr.Instance.RefreshHero( cfg.Key, pos, BATTLE_CAMP_DEF.BATTLE_CAMP_HERO );
            //    }
            //}

        }

        private void CreateBoss( )
        {

            //IEntity bossEntity = GameGlobal.EntityWorld.Local.GetEntity( bossEntityId );

            //if ( bossEntity is IMonster monster )
            //{
            //    //所有的远程角色平均分布在外圈

            //    monster.
            //}
            Invoke( "DelayCreateBoss", 3f );
        }
        private void DelayCreateBoss( )
        {
            RefreshMonsterMgr.Instance.RefreshBoss( 3, 1, 1, 1 );
            //ToastManager.Instance.Get( ).Show( "拉格纳罗斯：“你为什么要唤醒我，埃克索图斯，为什么要打扰我？“", 1f );
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
            //Debug.Log( "开始战斗" );

            if ( wEventID == DHeroTeamEvent.EVENT_START_BATTLE )
            {
                //GameGlobal.EventEgnine.FireExecute( DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, CameraShakeEventContext.Ins );
                //GameGlobal.EventEgnine.FireExecute( DHeroTeamEvent.EVENT_BOSS_HP_CHANGED, DEventSourceType.SOURCE_TYPE_ENTITY, 0, BossHpEventContext.Ins );

                //产出boss
                CreateBoss( );

                StartCoroutine( NpcBossChat( ) );
            }

        }

        private IEnumerator NpcBossChat( )
        {
            //等角色跑完
            yield return new WaitForSeconds( 2f );

            uint handle = 0;
            var loader = XGameComs.Get<IGAssetLoader>( );
            var objPrefab = ( GameObject ) loader.LoadResSync<GameObject>( "Game/HeroTeam/GameResources/Prefabs/Game/Npc.prefab", out handle );
            loader.UnloadRes( handle );
            var npc = GameObject.Instantiate( objPrefab, new Vector3( -7f, -11.4f, 0 ), Quaternion.identity, null );
            npc.transform.GetChild( 0 ).localScale = Vector3.one * 4;
            var chatPoint = npc.transform.Find( "ChatPoint" );

            List<string> chats = new List<string>( )
            {
                "管理者埃克索图斯：“拉格纳罗斯，火焰之王，他比这个世界本身还要古老，在他面前屈服吧，在你们的末日面前屈服吧！”",
                "拉格纳罗斯：“你为什么要唤醒我，埃克索图斯，为什么要打扰我？“",
                "管理者埃克索图斯：“是因为这些入侵者，我的主人，他们闯入了您的圣殿，想要窃取你的秘密。”",
                "拉格纳罗斯：“蠢货，你让这些不值一提的虫子进入了这个神圣的地方，现在还将他们引到了我这里来，你太让我失望了，埃克索图斯，你太让我失望了！”",
                "管理者埃克索图斯：“我的火焰，请不要夺走我的火焰。”（管理者埃克索图斯死亡）",
                "拉格纳罗斯：“现在轮到你们了，你们愚蠢的追寻拉格纳罗斯的力量，现在你们即将亲眼见到它。”"
            };

            for ( int i = 0; i < chats.Count; i++ )
            {
                var toast = ToastManager.Instance.Get( );
                toast.Show( chats[ i ], 1f );

                if ( i % 2 == 0 )
                {
                    toast.transform.SetParent( chatPoint );
                    toast.transform.localPosition = Vector3.zero;
                }
                else
                {
                    toast.transform.position = Vector3.up * 23f;
                }

                if ( i == chats.Count - 1 )
                {
                    //npc死亡动画
                    var sa = npc.GetComponentInChildren<SkeletonAnimation>( );
                    sa.AnimationState.SetAnimation( 0, "dead", false );
                }


                yield return new WaitForSeconds( 1f );
            }

            Destroy( npc );
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