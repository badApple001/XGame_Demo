using DG.Tweening;
using GameScripts.HeroTeam.UI.Win;
using GameScripts.Monster;
using RootMotion;
using Spine.Unity;
using System;
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
using XGame.UI.Framework;

namespace GameScripts.HeroTeam
{
    public class GameManager : Singleton<GameManager>, IEventExecuteSink
    {
        [SerializeField] private Transform m_trHeroSpawnRoot;
        [SerializeField] private Transform m_trBossSpawnRoot;

        [Header("地图路径节点")]
        [SerializeField] private Transform m_trAcherRoadRoot;
        [SerializeField] private Transform m_trTankRoadRoot;
        [SerializeField] private Transform m_tWarriorRoadRoot;
        private List<Vector3> m_vec3HeroSpawnPoints = new List<Vector3>();


        [Header("子弹池回收组")]
        [SerializeField] Transform m_trBulletActiveRoot;
        [SerializeField] Transform m_trBulletHiddenRoot;

        [Header("特效池回收组")]
        [SerializeField] Transform m_trEffectActiveRoot;
        [SerializeField] Transform m_trEffectHiddenRoot;

        GameObject m_refNpc;

        // Start is called before the first frame update
        void Start()
        {
            BulletManager.Instance.Setup(m_trBulletActiveRoot, m_trBulletHiddenRoot);
            GameEffectManager.Instance.Setup(m_trEffectActiveRoot, m_trEffectHiddenRoot);
            FillSpawnPoints();
            CreateHeros();
            CreateNpc();
            //CreateBoss( );
        }


        /// <summary>
        /// 一个协程延迟调用接口
        /// </summary>
        /// <param name="delay"> 等待多少秒 </param>
        /// <param name="callback"> 回调 </param>
        public Coroutine AddTimer(float delay, Action callback)
        {
            return StartCoroutine(OpenCoroutineTimer(delay, callback));
        }

        private IEnumerator OpenCoroutineTimer(float delay, Action callback)
        {
            yield return new WaitForSeconds(delay);
            try
            {
                callback?.Invoke();
            }
            catch (Exception e)
            {

                string callbackInfo = callback?.ToString();
                string targetInfo = "nullptr";
                if (callback?.Target != null)
                {
                    targetInfo = callback.Target.ToString();
                }
                Debug.LogError($"协程延迟回调移除崩溃:<*{targetInfo}->{callbackInfo}> {e.Message}");
            }
        }

        /// <summary>
        /// 清除定时器
        /// </summary>
        /// <param name="timerHandlers"></param>
        public void ClearTimers(List<Coroutine> timerHandlers)
        {
            timerHandlers.ForEach(handler => StopCoroutine(handler));
        }


        /// <summary>
        /// 扩展一个协程接口  方便后续统一管理
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public Coroutine OpenCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }



        private void FillSpawnPoints()
        {
            Debug.Assert(m_trHeroSpawnRoot != null && m_trHeroSpawnRoot.childCount > 0, "��ɫ�����㲻��Ϊ��");
            for (int i = 0; i < m_trHeroSpawnRoot.childCount; i++)
            {
                Transform spawnPoint = m_trHeroSpawnRoot.GetChild(i);
                m_vec3HeroSpawnPoints.Add(spawnPoint.position);
            }
        }


        private void CreateHeros()
        {

            //��ʱ�� �����Ƶ��ؿ���
            //��Ҫ�����Ľ�ɫ����
            // Dictionary<int, int> dictTmpHeros = new Dictionary<int, int>()
            // {
            //     { 1004,3 },
            //     { 1005,2 },
            //     { 1006,2 },
            //     { 1007,3 },
            //     { 1008,4 },
            //     { 1009,3 },
            //     { 1010,5 },
            //     { 1011,3 },
            // };

            //��ս��ɫ
            List<int> arrTmpWarrior = new List<int>()
            {
                1005,1005,
                1006,1006
            };

            //Զ�̽�ɫ
            List<int> arrTmpAcher = new List<int>()
            {
                1007,1007,1007,
                1008,1008,1008,1008,
                1009,1009,1009
            };

            //̹�˽�ɫ
            List<int> arrTmpTank = new List<int>()
            {
                1004,1004,1004,
            };

            //���ƽ�ɫ
            List<int> arrTmpHealer = new List<int>()
            {
                1010,1010,1010,1010,1010,
                1011,1011,1011
            };

            List<T> InsertListEvenly<T>(List<T> a, List<T> b)
            {
                var result = new List<T>();
                int a_len = a.Count;
                int b_len = b.Count;

                if (a_len == 0) return new List<T>(b);
                if (b_len == 0) return new List<T>(a);

                // 每多少个 b 插入一个 a
                double interval = (double)b_len / a_len;
                int a_index = 0;
                for (int i = 0; i < b_len; i++)
                {
                    result.Add(b[i]);
                    double target = (a_index + 1) * interval - 0.5; // 插入点尽量均匀
                    if (i + 1 >= target && a_index < a_len)
                    {
                        result.Add(a[a_index]);
                        a_index++;
                    }
                }

                // 如果还有剩余 a，全部塞进尾部
                for (; a_index < a_len; a_index++)
                {
                    result.Add(a[a_index]);
                }

                return result;
            }
            arrTmpAcher = InsertListEvenly(arrTmpAcher, arrTmpHealer);

            void SpawnByRoadRoot(Transform rootNode, List<int> heroIds, bool autoNear = false, bool ergodic = true)
            {
                Debug.Assert(heroIds.Count == rootNode.childCount, "Ѱ·�ڵ㲻��");
                for (int i = 0; i < heroIds.Count; i++)
                {
                    Vector3 pos = m_vec3HeroSpawnPoints[m_vec3HeroSpawnPoints.Count - 1];
                    m_vec3HeroSpawnPoints.RemoveAt(m_vec3HeroSpawnPoints.Count - 1);

                    List<Vector3> road = new List<Vector3>();

                    if (autoNear)
                    {
                        if (i >= heroIds.Count / 2)
                        {

                            for (int j = rootNode.childCount - 1; j >= i; j--)
                            {
                                var p = rootNode.GetChild(j).position;
                                p.z = 0f;
                                road.Add(p);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < rootNode.childCount / 2 - i; j++)
                            {
                                var p = rootNode.GetChild(j).position;
                                p.z = 0f;
                                road.Add(p);
                            }
                        }
                    }
                    else
                    {
                        if (!ergodic)
                        {
                            var p = rootNode.GetChild(i).position;
                            p.z = 0f;
                            road.Add(p);
                        }
                        else
                        {
                            for (int j = 0; j <= rootNode.childCount - 1 - i; j++)
                            {
                                var p = rootNode.GetChild(j).position;
                                p.z = 0f;
                                road.Add(p);
                            }
                        }
                    }
                    RefreshMonsterMgr.Instance.RefreshHero(heroIds[i], pos, BATTLE_CAMP_DEF.BATTLE_CAMP_HERO, road);
                }
            }

            //arrTmpAcher
            SpawnByRoadRoot(m_trAcherRoadRoot, arrTmpAcher, true);


            //arrTmpWarrior
            SpawnByRoadRoot(m_tWarriorRoadRoot, arrTmpWarrior);


            //arrTmpTank
            SpawnByRoadRoot(m_trTankRoadRoot, arrTmpTank, false, false);



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


        private void CreateNpc()
        {
            uint handle = 0;
            var loader = XGameComs.Get<IGAssetLoader>();
            var objPrefab = (GameObject)loader.LoadResSync<GameObject>("Game/HeroTeam/GameResources/Prefabs/Game/Npc.prefab", out handle);
            loader.UnloadRes(handle);
            m_refNpc = GameObject.Instantiate(objPrefab, new Vector3(-4.4f, 10.1f, 0), Quaternion.identity, null);
        }


        private void CreateBoss()
        {

            //IEntity bossEntity = GameGlobal.EntityWorld.Local.GetEntity( bossEntityId );

            //if ( bossEntity is IMonster monster )
            //{
            //    //���е�Զ�̽�ɫƽ���ֲ�����Ȧ

            //    monster.
            //}
            Invoke("DelayCreateBoss", 3f);
        }
        private void DelayCreateBoss()
        {
            var bossEntityId = RefreshMonsterMgr.Instance.RefreshBoss(3, 1, 1, 1);
            IEntity bossEntity = GameGlobal.EntityWorld.Local.GetEntity(bossEntityId);
            if (bossEntity is IMonster monster)
            {
                m_BossEntity = monster;
                m_BossEntity.SetBoos();
            }
            //ToastManager.Instance.Get( ).Show( "��������˹������ΪʲôҪ�����ң�������ͼ˹��ΪʲôҪ�����ң���", 1f );
        }

        //当前场景就一个boss，访问量较高，为了避免每次都要遍历，直接缓存一个
        private IMonster m_BossEntity = null;
        public IMonster GetBossEntity() => m_BossEntity;


        private void OnEnable()
        {
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0, "GameManager:OnEnable");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_WIN, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "GameManager:OnEnable");

        }
        private void OnDisable()
        {
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_WIN, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
        }


        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            //Debug.Log( "��ʼս��" );

            if (wEventID == DHeroTeamEvent.EVENT_START_GAME)
            {
                //GameGlobal.EventEgnine.FireExecute( DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, CameraShakeEventContext.Ins );
                //GameGlobal.EventEgnine.FireExecute( DHeroTeamEvent.EVENT_BOSS_HP_CHANGED, DEventSourceType.SOURCE_TYPE_ENTITY, 0, BossHpEventContext.Ins );

                //����boss
                CreateBoss();
                StartCoroutine(NpcBossChat());
            }
            else if (wEventID == DHeroTeamEvent.EVENT_WIN)
            {
                StopAllCoroutines();
            }
        }

        private IEnumerator NpcBossChat()
        {

            yield return new WaitForSeconds(2f);

            m_refNpc.transform.GetChild(0).localScale = Vector3.one * 4;
            var chatPoint = m_refNpc.transform.Find("ChatPoint");
            List<string> chats = new List<string>()
            {
                "管理者埃克索图斯：“拉格纳罗斯，火焰之王，他比这个世界本身还要古老，在他面前屈服吧，在你们的末日面前屈服吧！”",
                "拉格纳罗斯：“你为什么要唤醒我，埃克索图斯，为什么要打扰我？“",
                "管理者埃克索图斯：“是因为这些入侵者，我的主人，他们闯入了您的圣殿，想要窃取你的秘密。”",
                "拉格纳罗斯：“蠢货，你让这些不值一提的虫子进入了这个神圣的地方，现在还将他们引到了我这里来，你太让我失望了，埃克索图斯，你太让我失望了！”",
                "管理者埃克索图斯：“我的火焰，请不要夺走我的火焰。”（管理者埃克索图斯死亡）",
                "拉格纳罗斯：“现在轮到你们了，你们愚蠢的追寻拉格纳罗斯的力量，现在你们即将亲眼见到它。”"
            };

            // //跳过对话
            // if (true)
            // {
            //     chats.Clear();
            //     yield return new WaitForSeconds(1.5f);
            // }


            for (int i = 0; i < chats.Count; i++)
            {
                var toast = ToastManager.Instance.Get();
                toast.Show(chats[i], 1f);

                if (i % 2 == 0)
                {
                    toast.transform.SetParent(chatPoint);
                    toast.transform.localPosition = Vector3.zero;
                }
                else
                {
                    toast.transform.position = Vector3.up * 23f;
                }

                if (i == chats.Count - 1)
                {
                    //npc��������
                    var sa = m_refNpc.GetComponentInChildren<SkeletonAnimation>();
                    sa.AnimationState.SetAnimation(0, "dead", false);
                }


                yield return new WaitForSeconds(1f);
            }

            Destroy(m_refNpc);
            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_INTO_FIGHT_CHANGED, DEventSourceType.SOURCE_TYPE_ENTITY, 0, null);
        }

        private void Update()
        {
            // if ( Input.GetKeyDown( KeyCode.Space ) )
            // {
            //     var pContext = CameraShakeEventContext.Ins;
            //     pContext.intensity = 1f;
            //     pContext.duration = 0.5f;
            //     pContext.vibrato = 30;
            //     pContext.randomness = 100f;
            //     pContext.fadeOut = true;
            //     GameGlobal.EventEgnine.FireExecute( DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext );
            // }

            // if ( Input.GetKeyDown( KeyCode.Q ) )
            // {
            //     var pContext = BossHpEventContext.Ins;
            //     pContext.Health -= 0.2f;
            //     GameGlobal.EventEgnine.FireExecute( DHeroTeamEvent.EVENT_BOSS_HP_CHANGED, DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext );
            // }

        }
    }

}