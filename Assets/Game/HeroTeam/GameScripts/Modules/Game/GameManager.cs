using DG.Tweening;
using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XGame.EventEngine;

namespace GameScripts.HeroTeam
{
    public class GameManager : MonoSingleton<GameManager>, IEventExecuteSink
    {
        [Header("出生节点")]
        [SerializeField] private Transform m_trHeroSpawnRoot;
        [SerializeField] private Transform m_trBossSpawnRoot;

        [Header("地图路径节点")]
        [SerializeField] private Transform m_trAcherRoadRoot;
        [SerializeField] private Transform m_trTankRoadRoot;
        [SerializeField] private Transform m_tWarriorRoadRoot;

        [Header("子弹池回收组")]
        [SerializeField] Transform m_trBulletActiveRoot;
        [SerializeField] Transform m_trBulletHiddenRoot;

        [Header("特效池回收组")]
        [SerializeField] Transform m_trEffectActiveRoot;
        [SerializeField] Transform m_trEffectHiddenRoot;

        [Header("特效")]
        [SerializeField] Material m_FlameBurningEffectMat;


        /// <summary>
        /// 当前关卡
        /// </summary>
        [HideInInspector]
        public int LevelID = 1;

        /// <summary>
        /// Npc
        /// </summary>
        private ISpineCreature m_Npc;

        /// <summary>
        /// 团长
        /// </summary>
        private IHero m_Leader;

        //当前场景就一个boss，访问量较高，为了避免每次都要遍历，直接缓存一个
        private IMonster m_Boss = null;

        //缓存一份Boss的死亡位置        
        public Vector3 BossDeathPosition { private set; get; }

        //需求：增加一个键盘摇杆操作映射
        private bool m_bEnableUserInput = false;

        void Start()
        {
            //子弹
            BulletManager.Instance.Setup(m_trBulletActiveRoot, m_trBulletHiddenRoot);

            //特效
            GameEffectManager.Instance.Setup(m_trEffectActiveRoot, m_trEffectHiddenRoot);

            //关卡
            LevelManager.Instance.Setup(null);
            LevelManager.Instance.ActorDieHandler += OnActorDieHandle;
            LevelManager.Instance.RegisterActorCampUpdateProcessPipe(CampDef.HERO, RankPipe.Instance);

            //气泡弹窗
            BubbleMessageManager.Instance.Setup(GameRoots.Instance.BattleBubbleMessageRoot);

            InitGame();
        }

        private void ResetGame()
        {
            Debug.Log("########## 重置游戏");
            LevelManager.Instance.RecycleAll();
            GameEffectManager.Instance.Release();
            BuffManager.Instance.Release();
            GC.Collect();
            InitGame();
        }

        private void InitGame()
        {
            CreateHeros();
            CreateNpc();
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


        /// <summary>
        /// 扩展一个协程接口  方便后续统一管理
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public Coroutine OpenCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
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
                Debug.LogError($"协程延迟回调触发崩溃:<*{targetInfo}->{callbackInfo}> {e.Message}");
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
        /// 获取当前关卡配置
        /// </summary>
        /// <returns></returns>
        public cfg_HeroTeamLevels GetCurrentLevelConfig()
        {
            return GameGlobal.GameScheme.HeroTeamLevels_0(LevelID);
        }

        private void CreateHeros()
        {
            var levelCfg = GetCurrentLevelConfig();
            int leaderIndex = levelCfg.iLeaderIndex;
            for (int i = 0; i < levelCfg.aryHerosBornPos.Length; i++)
            {
                Vector3 pos = m_trHeroSpawnRoot.GetChild(i).position;
                var pContext = CreateActorContext.Instance;
                pContext.nActorCfgID = levelCfg.aryHerosBornPos[i];
                pContext.worldPos = pos;
                pContext.nCamp = CampDef.HERO;
                ISpineCreature actor = LevelManager.Instance.CreateHero(pContext);
                if (i == leaderIndex)
                {
                    m_Leader = (IHero)actor;
                    actor.SetResLoadedCallback(OnLeaderInited);
                }
            }
        }

        /// <summary>
        /// 对团长的特殊处理 不要糅合进Actor里， 直接拆出来做
        /// 
        /// TODO: 血条后面改成Part
        /// 
        /// </summary>
        private void OnLeaderInited()
        {
            var bar = m_Leader.GetTr().GetComponentInChildren<HpBar>();
            if (null != bar)
            {
                //团长特殊处理
                m_Leader.GetVisual().localScale = m_Leader.GetActorCig().fSizeScale * GetCurrentLevelConfig().fLeaderModeScale * Vector3.one;
                Vector3 lp = bar.transform.localPosition;
                lp.y *= GetCurrentLevelConfig().fLeaderModeScale;
                bar.transform.localPosition = lp;

                //以buff的形式给主角一个特效标识
                BuffManager.Instance.CreateBuff(m_Leader, 102);

                //受到伤害红屏
                // bar.SetApplyDamageCallback(() =>
                // {
                //     GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_HARM_RED_SCREEN, DEventSourceType.SOURCE_TYPE_ENTITY, 0, null);
                // });
            }
        }

        private void CreateNpc()
        {
            var pContext = CreateActorContext.Instance;
            pContext.nActorCfgID = GetCurrentLevelConfig().iNpcID;
            pContext.worldPos = new Vector3().FromArray(GetCurrentLevelConfig().aryNpcBornPos);
            m_Npc = LevelManager.Instance.CreateHero(pContext);
        }

        private void OnClickStartGame()
        {
            AddTimer(GetCurrentLevelConfig().iBossBornDelaySeconds, DelayCreateBoss);
            StartCoroutine(NpcBossChat());
            HerosMove2BattleScene();
        }

        private void HerosMove2BattleScene()
        {
            var heros = LevelManager.Instance.GetActorsByCamp(CampDef.HERO);
            //远程英雄
            var remoteHeros = heros.FindAll(hero => hero.GetHeroCls() > HeroClassDef.WARRIOR);
            //肉坦
            var tankHeros = heros.FindAll(hero => hero.GetHeroCls() == HeroClassDef.TANK);

            //战士和刺客
            var warriorHeros = heros.FindAll(hero => hero.GetHeroCls() == HeroClassDef.WARRIOR);


            void Move2Path(Transform rootNode, List<ISpineCreature> heros, bool autoNear = false, bool ergodic = true)
            {
                for (int i = 0; i < heros.Count; i++)
                {
                    List<Vector3> path = new List<Vector3>();
                    if (autoNear)
                    {
                        if (i >= heros.Count / 2)
                        {

                            for (int j = rootNode.childCount - 1; j >= i; j--)
                            {
                                var p = rootNode.GetChild(j).position;
                                p.z = 0f;
                                path.Add(p);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < rootNode.childCount / 2 - i; j++)
                            {
                                var p = rootNode.GetChild(j).position;
                                p.z = 0f;
                                path.Add(p);
                            }
                        }
                    }
                    else
                    {
                        if (!ergodic)
                        {
                            var p = rootNode.GetChild(i).position;
                            p.z = 0f;
                            path.Add(p);
                        }
                        else
                        {
                            for (int j = 0; j <= rootNode.childCount - 1 - i; j++)
                            {
                                var p = rootNode.GetChild(j).position;
                                p.z = 0f;
                                path.Add(p);
                            }
                        }
                    }
                    var moverPart = heros[i].GetPart<SpineCreatureTargetMoverPart>();
                    if (null != moverPart)
                    {
                        moverPart.SetPath(path).Start();
                    }
                }
            }

            //Acher
            Move2Path(m_trAcherRoadRoot, remoteHeros, true);

            //Tank
            Move2Path(m_trTankRoadRoot, tankHeros, false, false);

            //Warrior
            Move2Path(m_tWarriorRoadRoot, warriorHeros);
        }

        private void DelayCreateBoss()
        {
            var pContext = CreateActorContext.Instance;
            pContext.nActorCfgID = GetCurrentLevelConfig().iBossID;
            pContext.nCamp = CampDef.MONSTER;
            pContext.worldPos = new Vector3().FromArray(GetCurrentLevelConfig().aryBossBornPos);
            m_Boss = (IMonster)LevelManager.Instance.CreateMonster(pContext);
            m_Boss.SetBoos();
            m_Boss.SetForward(Vector3.right);
        }

        public ISpineCreature GetBossEntity() => m_Boss;

        private void OnEnable()
        {
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0, "GameManager:OnEnable");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_WIN, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "GameManager:OnEnable");


            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_LEADER_SKILL_ATTACK, DEventSourceType.SOURCE_TYPE_UI, 0, "GameManager:OnEnable");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_LEADER_SKILL_AVOIDANCE, DEventSourceType.SOURCE_TYPE_UI, 0, "GameManager:OnEnable");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_LEADER_SKILL_TREAT, DEventSourceType.SOURCE_TYPE_UI, 0, "GameManager:OnEnable");

            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_JOYSTICK_ACTIVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "GameManager:OnEnable");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_RESET_GAME, DEventSourceType.SOURCE_TYPE_UI, 0, "GameManager:OnEnable");
            // ListenJoystickEvent();
        }

        private void OnDisable()
        {
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_START_GAME, DEventSourceType.SOURCE_TYPE_UI, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_WIN, DEventSourceType.SOURCE_TYPE_ENTITY, 0);


            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_JOYSTICK_STARTED, DEventSourceType.SOURCE_TYPE_UI, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_JOYSTICK_CHANGED, DEventSourceType.SOURCE_TYPE_UI, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_JOYSTICK_ENDED, DEventSourceType.SOURCE_TYPE_UI, 0);

            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_LEADER_SKILL_ATTACK, DEventSourceType.SOURCE_TYPE_UI, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_LEADER_SKILL_AVOIDANCE, DEventSourceType.SOURCE_TYPE_UI, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_LEADER_SKILL_TREAT, DEventSourceType.SOURCE_TYPE_UI, 0);

            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_JOYSTICK_ACTIVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_RESET_GAME, DEventSourceType.SOURCE_TYPE_UI, 0);
        }

        private void ListenJoystickEvent()
        {
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_JOYSTICK_STARTED, DEventSourceType.SOURCE_TYPE_UI, 0, "GameManager:OnEnable");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_JOYSTICK_CHANGED, DEventSourceType.SOURCE_TYPE_UI, 0, "GameManager:OnEnable");
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_JOYSTICK_ENDED, DEventSourceType.SOURCE_TYPE_UI, 0, "GameManager:OnEnable");
        }


        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            if (wEventID == DHeroTeamEvent.EVENT_START_GAME)
            {
                OnClickStartGame();
            }
            else if (wEventID == DHeroTeamEvent.EVENT_JOYSTICK_ACTIVE)
            {
                m_bEnableUserInput = true;
            }
            else if (wEventID == DHeroTeamEvent.EVENT_WIN)
            {

                AddTimer(1f, () =>
                {
                    Debug.Log("清除GameManger所有协程");
                    StopAllCoroutines();
                    OnGameOverAndPlayerWin();
                });
            }
            else if (wEventID == DHeroTeamEvent.EVENT_JOYSTICK_STARTED)
            {
                OnJoystickStarted();
            }
            else if (wEventID == DHeroTeamEvent.EVENT_JOYSTICK_CHANGED)
            {
                OnJoystickChanged();
            }
            else if (wEventID == DHeroTeamEvent.EVENT_JOYSTICK_ENDED)
            {
                OnJoystickEnded();
            }
            else if (wEventID == DHeroTeamEvent.EVENT_LEADER_SKILL_ATTACK)
            {
                OnLeaderSkillAttack();
            }
            else if (wEventID == DHeroTeamEvent.EVENT_LEADER_SKILL_AVOIDANCE)
            {
                OnLeaderSkillAvoidance();
            }
            else if (wEventID == DHeroTeamEvent.EVENT_LEADER_SKILL_TREAT)
            {
                OnLeaderSkillTreat();
            }
            else if (wEventID == DHeroTeamEvent.EVENT_RESET_GAME)
            {
                ResetGame();
            }
        }

        //团长指令: 攻击
        //新更改: 集火 - 给队员增加 无限火力buff
        private void OnLeaderSkillAttack()
        {
            // 团长攻击技能逻辑
            if (m_Leader != null && m_Leader.GetState() < ActorState.Dying)
            {
                m_Leader.GetSkeleton().state.SetAnimation(0, "skill2", false);
                m_Leader.ShowEmoji("Game/HeroTeam/GameResources/Prefabs/Game/Emoji/EmojiMad.prefab");

                //无限火力buff
                var heros = LevelManager.Instance.GetActorsByCamp(CampDef.HERO);
                heros.ForEach(h => BuffManager.Instance.CreateBuff(h, 103));
            }
        }

        //团长指令: 散开
        private void OnLeaderSkillAvoidance()
        {
            // 团长散开技能逻辑
            if (m_Leader != null && m_Leader.GetState() < ActorState.Dying)
            {
                m_Leader.GetSkeleton().state.SetAnimation(0, "skill2", false);
                m_Leader.ShowEmoji("Game/HeroTeam/GameResources/Prefabs/Game/Emoji/EmojiMad.prefab");

                var heros = LevelManager.Instance.GetActorsByCamp(CampDef.HERO);
                heros.ForEach(h =>
                {
                    if (h is IHero hero)
                    {
                        if (!hero.IsDodge())
                        {
                            hero.DodgeAndReturn();
                        }
                    }
                });
            }
        }

        //团长指令: 治疗
        //新更改: 驱散buff
        private void OnLeaderSkillTreat()
        {
            // 团长治疗技能逻辑
            if (m_Leader != null && m_Leader.GetState() < ActorState.Dying)
            {
                m_Leader.GetSkeleton().state.SetAnimation(0, "skill2", false);
                m_Leader.ShowEmoji("Game/HeroTeam/GameResources/Prefabs/Game/Emoji/EmojiMad.prefab");

                // //治疗玩家
                // var heros = LevelManager.Instance.GetActorsByCamp(CampDef.HERO);
                // //治疗
                // var sageHeros = heros.FindAll(hero => hero.GetHeroCls() > HeroClassDef.SAGE);
                // sageHeros.ForEach(sage => sage.GetStateMachine().ChangeState<HeroAttackState>());

                string szLevUpVfxPath = "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Level Up/Cylinder/LevelupCylinderGreen.prefab";
                GameEffectManager.Instance.ShowEffect(szLevUpVfxPath, m_Leader.GetTr().position + Vector3.back * 1.4f, Quaternion.Euler(40, 0, 0), 2);

                //驱散减益buff
                string clearBuffVfxPath = "Game/HeroTeam/GameResources/Prefabs/Game/Fx/MagicBuffBlue.prefab";
                var buffs = BuffManager.Instance.GetActiveBuffByType(BuffTypeDef.DEBUFF);
                buffs.ForEach(buff =>
                {
                    var pos = buff.GetOwner().GetTr().position;
                    GameEffectManager.Instance.ShowEffect(clearBuffVfxPath, pos, Quaternion.Euler(-90, 0, 0), 2);
                    BuffManager.Instance.ReleaseBuff(buff);
                });
            }
        }

        private IEnumerator NpcBossChat()
        {
            yield return new WaitForSeconds(2f);
            var chatPoint = m_Npc.transform.Find("ChatPoint");
            List<string> chats = new List<string>()
            {
                "管理者埃克索图斯：“拉格纳罗斯，火焰之王，他比这个世界本身还要古老，在他面前屈服吧，在你们的末日面前屈服吧！”",
                "拉格纳罗斯：“你为什么要唤醒我，埃克索图斯，为什么要打扰我？“",
                "管理者埃克索图斯：“是因为这些入侵者，我的主人，他们闯入了您的圣殿，想要窃取你的秘密。”",
                "拉格纳罗斯：“蠢货，你让这些不值一提的虫子进入了这个神圣的地方，现在还将他们引到了我这里来，你太让我失望了，埃克索图斯，你太让我失望了！”",
                "管理者埃克索图斯：“我的火焰，请不要夺走我的火焰。”（管理者埃克索图斯死亡）",
                "拉格纳罗斯：“现在轮到你们了，你们愚蠢的追寻拉格纳罗斯的力量，现在你们即将亲眼见到它。”"
            };

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
                yield return new WaitForSeconds(1f);
            }
            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_INTO_FIGHT_STATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, null);


            var sa = m_Npc.GetSkeleton();
            sa.AnimationState.SetAnimation(0, "dead", false);

            //燃烧效果
            var mr = sa.GetComponent<MeshRenderer>();
            if (null != mr)
            {
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                DOTween.To(() => 0f, f =>
                {
                    mr.GetPropertyBlock(mpb);
                    mpb.SetFloat("_DissolveAmount", f);
                    mr.SetPropertyBlock(mpb);
                }, 1f, 1f);
            }

            //火焰特效
            string npcFireFxResPath = "Game/HeroTeam/GameResources/Prefabs/Game/Fx/SpikyFireBigAdditiveRed.prefab";
            GameEffectManager.Instance.ShowEffect(npcFireFxResPath, m_Npc.transform.position + Vector3.up * 2.31f);

            yield return new WaitForSeconds(1f);
            LevelManager.Instance.DestroyActor(m_Npc);

            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_JOYSTICK_ACTIVE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, null);
            ListenJoystickEvent();
        }

        private bool m_OnJoystickTouched = false;
        private void OnJoystickStarted()
        {
            m_OnJoystickTouched = true;

            //刷新动画

        }
        private void OnJoystickChanged()
        {

        }

        private void OnJoystickEnded()
        {
            m_OnJoystickTouched = false;

        }

        private void ApplyUserInput()
        {
            var delta = JoystickEventContext.Ins.delta;
            // Debug.Log(delta);
            var viewBounds = CameraController.Instance.GetCamViewBounds();
            var nextPos = m_Leader.GetPos() + delta;
            if (!viewBounds.Contains(nextPos))
            {
                nextPos.x = Mathf.Clamp(nextPos.x, viewBounds.xMin, viewBounds.xMax);
                nextPos.y = Mathf.Clamp(nextPos.y, viewBounds.yMin, viewBounds.yMax);
            }
            m_Leader.GetPart<SpineCreatureTargetMoverPart>().SetDestination(nextPos).Start();
        }

        private void OnActorDieHandle(ISpineCreature actor)
        {

            if (actor is IMonster monster)
            {

                if (monster.IsBoos())
                {
                    //记录一下boss的死亡位置
                    BossDeathPosition = actor.GetTr().position;
                    GameGlobal.EventEgnine.FireExecute(GameScripts.HeroTeam.DHeroTeamEvent.EVENT_WIN, GameScripts.HeroTeam.DEventSourceType.SOURCE_TYPE_ENTITY, 0, null);

                    //Boss掉宝
                    //TODO: 后续由关卡表配置
                    string propResPath = "Game/HeroTeam/GameResources/Prefabs/Game/Fx/ExclTiltedGlossy.prefab";
                    GameEffectManager.Instance.ShowEffect(propResPath, BossDeathPosition + Vector3.up * 3f, Quaternion.identity, 10f);

                    //Boss死亡特效
                    string explosResPath = "Game/HeroTeam/GameResources/Prefabs/Game/Fx/ExplosionFireballSharpFire.prefab";
                    GameEffectManager.Instance.ShowEffect(explosResPath, BossDeathPosition + Vector3.up * 7.57f);
                }
            }
            else
            {
                //玩家死亡.
                Debug.Log($"<color=#ff0000>######## 玩家{actor.name}已死亡 </color>");
            }


        }


        private void OnGameOverAndPlayerWin()
        {
            Debug.Log($"<color=#ff0000>######## Boss已死亡 </color>");

            var heros = LevelManager.Instance.GetActorsByCamp(CampDef.HERO);
            foreach (ISpineCreature hero in heros)
            {
                if (null != hero)
                {
                    if (!hero.IsDie())
                    {
                        hero.GetStateMachine().ChangeState<HeroWinState>();
                    }
                }
            }
        }

        private void Update()
        {
            TimeUtils.Update();

            if (m_OnJoystickTouched)
            {

                //向服务器同步玩家操作


                //本地预测
                ApplyUserInput();
            }

            if (m_bEnableUserInput) //激活键盘映射
            {
                float x = Input.GetAxis("Horizontal");
                float y = Input.GetAxis("Vertical");
                if (x != 0 || y != 0)
                {
                    var localDelta = new Vector2(x, y);
                    JoystickEventContext.Ins.delta = localDelta;
                    ApplyUserInput();
                }
            }
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     // var pContext = CameraShakeEventContext.Ins;
            //     // pContext.intensity = 1f;
            //     // pContext.duration = 0.5f;
            //     // pContext.vibrato = 30;
            //     // pContext.randomness = 100f;
            //     // pContext.fadeOut = true;
            //     // GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext);

            //     GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_HARM_RED_SCREEN, DEventSourceType.SOURCE_TYPE_ENTITY, 0, null);

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