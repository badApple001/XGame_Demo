using System;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UniFramework.Machine;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame.Entity;
using XGame.EventEngine;

namespace GameScripts.HeroTeam
{
    /// <summary>
    /// 
    /// 轻量化战斗单位
    /// 
    /// 由配置表+Entity+Fsm管理
    /// 
    /// 配置表提供 每个角色不同的技能，不同的CD，生命，攻击，等属性
    /// Enity管理 每个角色的生物周期属性，负责创建和销毁
    /// Fsm辅助处理 角色不同状态下的行为,包括动画
    /// </summary>
    public class Actor : VisibleEntity, IActor, IEventExecuteSink
    {

        /// <summary>
        /// 角色的有限状态机
        /// </summary>
        private StateMachine m_fsmActor;

        /// <summary>
        /// 拥有的所有技能
        /// </summary>
        private List<cfg_HeroTeamSkills> m_Skills = null;

        /// <summary>
        /// Spine组件
        /// </summary>
        private SkeletonAnimation m_SkeletonAnimation;


        /// <summary>
        /// 被锁定的目标节点
        /// </summary>
        private Transform m_trLockTarget;

        /// <summary>
        /// 挂表情的节点
        /// </summary>
        private Transform m_trFace;


        /// <summary>
        /// 死亡
        /// </summary>
        private bool m_IsDead = false;


        /// <summary>
        /// Monster共享一个 生成下标
        /// 此下标用来独立每个角色的名称: {heroClass}{m_byteShareSpawnIndex++}
        /// 后续策划如果有想法就增加配表
        /// </summary>
        private static byte m_byteShareSpawnIndex = 0;


        /// <summary>
        /// 总共造成的伤害，如果是奶职业，就是治疗总量
        /// </summary>
        private int m_totalHarm = 0;

        /// <summary>
        /// 角色职业
        /// </summary>
        private int m_HeroCls = 0;


        /// <summary>
        /// 资源路径
        /// </summary>
        private string m_resPath;

        /// <summary>
        /// int 属性字典
        /// </summary>
        private Dictionary<int, int> m_dicProp = new Dictionary<int, int>();

        /// <summary>
        /// 父节点
        /// </summary>
        protected Transform m_trParent = null;

        /// <summary>
        /// 定时器协程组
        /// </summary>
        private List<Coroutine> m_arrCoroutineGroup = new List<Coroutine>();

        protected override void OnAfterInit(object context)
        {
            base.OnAfterInit(context);

            var cfg = GameGlobal.GameScheme.Monster_0((uint)configId);
            if (cfg == null)
            {
                Debug.LogError("读取失败的角色配置 configId=" + configId);
            }
            else
            {
                config = cfg;
                var ctx = context as NetEntityShareInitContext;
                CreateActorContext createActorContext = (CreateActorContext)(ctx.localInitContext);
                GetPart<ActorDataPart>().m_camp.Value = createActorContext.nCamp;
                m_trParent = createActorContext.parent;

                m_resPath = cfg.szResPath;
                if (string.IsNullOrEmpty(m_resPath))
                {
                    Debug.LogError("资源路径没有配置 configId=" + configId);
                }
            }

            //速度
            SetSpeed(cfg.fMoveSpeed);

            //攻击 / 治疗
            SetPower(cfg.iAttack);

            //生命值
            SetHPDelta(cfg.baseHP - (int)GetHP());
            SetMaxHP(GetHP());

            //职业
            m_HeroCls = cfg.HeroClass;

            //名称生成
            name = cfg.szName + (char)('A' + m_byteShareSpawnIndex++);
        }

        public override string GetResPath()
        {
            return m_resPath;
        }

        public override void OnReceiveEntityMessage(uint id, object data = null)
        {

            if (id == EntityMessageID.ResLoaded)
            {

                //这块我个人感觉，更应该写在Part里 而不是单独占用一个mono
                // var spineAni = transform.GetComponent<SpineAni>();
                // Debug.Assert(spineAni != null, "spineAni Component not found.");
                m_SkeletonAnimation = transform.GetComponentInChildren<SkeletonAnimation>();

                if (null != m_trParent)
                    transform.BetterSetParent(m_trParent);

                transform.position = GetPart<ActorDataPart>().m_pos.Value;
                transform.rotation = Quaternion.FromToRotation(Vector3.right, GetPart<ActorDataPart>().m_forward.Value);


                m_fsmActor = new StateMachine(this);
                m_fsmActor.AddNode<ActorIdleState>();
                m_fsmActor.AddNode<ActorAttackState>();
                m_fsmActor.AddNode<ActorSkillState>();
                m_fsmActor.AddNode<ActorHitState>();
                m_fsmActor.AddNode<ActorDeathState>();
                m_fsmActor.AddNode<ActorWinState>();
                m_fsmActor.AddNode<ActorJumpState>();


                var bar = transform.GetComponentInChildren<HpBar>();
                if (null != bar)
                {
                    bar.SetEntity(this);
                }

                m_trLockTarget = transform.Find("LockTarget");
                if (m_trLockTarget == null)
                    m_trLockTarget = transform;

                m_trFace = transform.Find("Face");



                GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_INTO_FIGHT_STATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "Actor:Start");
            }
        }


        public Transform GetTr()
        {
            return transform;
        }


        public cfg_ActorAnimConfig GetAnimConfig() => GameGlobal.GameScheme.ActorAnimConfig_0((int)GetMonsterCfg().nID);


        public long GetHP() => GetPart<ActorDataPart>().m_hp.Value;

        public long GetMaxHP() => GetPart<ActorDataPart>().m_maxHp.Value;

        public cfg_Monster GetMonsterCfg() => (cfg_Monster)config;

        public Vector3 GetPos() => GetPart<ActorDataPart>().m_pos.Value;

        public void SetPos(ref Vector3 pos)
        {
            GetPart<ActorDataPart>().m_pos.Value = pos;
            transform?.SetPosition(pos);
        }

        public void SetPos(float[] float3Pos)
        {
            Vector3 pos = new Vector3().FromArray(float3Pos);
            SetPos(ref pos);
        }

        public SkeletonAnimation GetSkeleton() => m_SkeletonAnimation;

        public List<cfg_HeroTeamSkills> GetSkills()
        {
            if (m_Skills != null)
            {
                return m_Skills;
            }

            List<cfg_HeroTeamSkills> skills = new List<cfg_HeroTeamSkills>();
            var config = GetMonsterCfg();
            if (config != null)
            {
                int[] skillIDs = config.Skills;
                for (int i = 0; i < skillIDs.Length; i++)
                {
                    cfg_HeroTeamSkills skill = GameGlobal.GameScheme.HeroTeamSkills_0(skillIDs[i]);
                    if (skill != null)
                    {
                        skills.Add(skill);
                    }
                }
            }
            m_Skills = skills;
            return skills;
        }


        public int GetHatred()
        {
            return GetPart<ActorDataPart>().m_Hatred.Value;
        }

        public void SetHatred(int value)
        {
            GetPart<ActorDataPart>().m_Hatred.Value = value;
        }


        public Transform GetLockTr()
        {
            return m_trLockTarget;
        }

        public Transform GetFaceTr()
        {
            return m_trFace;
        }


        public void EludeBossSkill(Vector3 bossPos, Vector3 bossDir, float radius, float angleDeg)
        {
            if (null != GetFaceTr())
            {
                GetFaceTr().gameObject.SetActive(true);
                AddTimer(2.5f, () => GetFaceTr().gameObject.SetActive(false));
            }

            DodgeAndReturn(transform, bossPos, bossDir);

        }


        Vector2 GetSideDodgeDirection(Vector2 bossDir, Vector2 toNpc)
        {
            // 取 bossDir 的垂直方向（法向量）
            Vector2 side = Vector2.Perpendicular(bossDir).normalized;

            // 决定往左还是往右闪，依据 npc 在哪一侧
            float sign = Mathf.Sign(Vector2.Dot(side, toNpc));
            return side * sign;
        }

        public void DodgeAndReturn(Transform tr, Vector2 bossPos, Vector2 bossDir, float dodgeDistance = 7f, float waitTime = 1.5f, float duration = 1f)
        {
            Vector2 toNpc = (Vector2)tr.position - bossPos;
            Vector2 dodgeDir = GetSideDodgeDirection(bossDir, toNpc);

            Vector3 startPos = tr.position;
            Vector3 dodgeTarget = startPos + (Vector3)(dodgeDir * dodgeDistance);

            var anim = tr.GetComponent<Actor>().GetSkeleton();
            var animConfig = tr.GetComponent<Actor>().GetAnimConfig();
            anim.state.SetAnimation(1, animConfig.szHit, true);
            tr.DOKill();
            tr.DOMove(dodgeTarget, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // anim.state.SetAnimation(1, animConfig.szIdle, true);
                    // DOVirtual.DelayedCall(waitTime, () =>
                    // {
                    //     anim.state.SetAnimation(1, animConfig.szMove, true);
                    //     tr.DOMove(startPos, duration).SetEase(Ease.InQuad).OnComplete(() =>
                    //     {
                    //         anim.state.ClearTrack(1);
                    //         anim.state.SetAnimation(0, animConfig.szIdle, true);
                    //     });
                    // });
                    AddTimer(waitTime, () =>
                    {
                        anim.state.SetAnimation(1, animConfig.szMove, true);
                        tr.DOMove(startPos, duration).SetEase(Ease.InQuad).OnComplete(() =>
                        {
                            anim.state.ClearTrack(1);
                            anim.state.SetAnimation(0, animConfig.szIdle, true);
                        });
                    });
                });
        }

        public void ReceiveBossSelect(Vector3 bossPos)
        {
            Vector3 startPos = transform.position;
            float repulseDistance = 10f;
            Vector3 repulseDir = (transform.position - bossPos).normalized;
            Vector3 repulseTarget = startPos + (Vector3)(repulseDir * repulseDistance);
            transform.DOKill();
            transform.DOMove(repulseTarget, 0.3f)
               .SetEase(Ease.OutQuad)
               .OnComplete(() =>
               {
                   //    DOVirtual.DelayedCall(0.5f, () =>
                   //    {
                   //        transform.DOMove(startPos, 0.6f).SetEase(Ease.OutCirc);
                   //    });
                   AddTimer(0.5f, () =>
                                     {
                                         transform.DOMove(startPos, 0.6f).SetEase(Ease.OutCirc);
                                     });
               });
            transform.DOScale(1.4f, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
               {
                   AddTimer(0.5f, () =>
                  {
                      transform.DOScale(1, 0.6f).SetEase(Ease.OutCirc);
                  });
               });

        }

        public int GetHeroCls() => m_HeroCls;


        public void RecordHarm(int addHarm)
        {
            m_totalHarm += addHarm;
        }

        public int GetTotalHarm() => m_totalHarm;


        public float GetSpeed()
        {
            return GetPart<ActorDataPart>().m_speed.Value;
        }

        public void SetSpeed(float speed)
        {
            GetPart<ActorDataPart>().m_speed.Value = speed;
        }

        public int GetPower()
        {
            return GetPart<ActorDataPart>().m_power.Value;
        }

        public void SetPower(int v)
        {
            GetPart<ActorDataPart>().m_power.Value = v;
        }

        public bool IsDie()
        {
            return m_IsDead;
        }

        public void GoDead()
        {
            m_IsDead = true;
        }

        public void SetHPDelta(int hp)
        {
            var dataPart = GetPart<ActorDataPart>();
            if (dataPart != null)
            {
                dataPart.m_hp.RemoteValueDelta += hp;
                dataPart.m_hp.Value += hp;

                if (hp < 0 && !IsBoos() && !IsDie())
                {
                    // GetSkeletonAnimation().AnimationState.SetAnimation(0, "hit2", false);
                    var actor = GetComponent<Actor>();
                    var skel = actor.GetSkeleton();
                    if (null != actor && skel != null)
                    {

                        var cfg = actor.GetAnimConfig();
                        if (cfg == null)
                        {
                            Debug.Log($"找不到动画: {((cfg_Monster)config).nID}");
                            return;
                        }
                        skel.state.SetAnimation(1, cfg.szHit, false);
                        AddTimer(0.6f, () =>
                        {
                            skel.state.ClearTrack(1);
                            skel.state.SetAnimation(0, actor.GetAnimConfig().szIdle, true);
                        });
                    }
                }

                //广播boss的生命值
                if (IsBoos())
                {
                    var pContext = BossHpEventContext.Ins;
                    pContext.Health = GetHP() * 1.0f / GetMaxHP();
                    GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_BOSS_HP_CHANGED, GameScripts.HeroTeam.DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext);
                }
            }
        }

        public void SetMaxHP(long maxHp)
        {
            GetPart<ActorDataPart>().m_maxHp.Value = maxHp;
        }

        public void SetRotation(Quaternion rotate)
        {
            //Vector3.right   2D角色的默认朝向， 如果是3D 手动改成 Vector3.forward
            GetPart<ActorDataPart>().m_forward.Value = rotate * Vector3.right;

            if (transform != null)
                transform.rotation = rotate;
        }
        public Quaternion GetRotation()
        {
            return Quaternion.FromToRotation(Vector3.right, GetPart<ActorDataPart>().m_forward.Value);
        }


        public int GetIntAttr(int propID)
        {
            int val = 0;
            m_dicProp.TryGetValue(propID, out val);
            return val;
        }

        public void SetIntAttr(int propID, int val)
        {
            m_dicProp[propID] = val;
        }


        public void SetBoos()
        {
            GetPart<ActorDataPart>().m_IsBoss.Value = true;
        }

        public bool IsBoos()
        {
            return GetPart<ActorDataPart>().m_IsBoss.Value;
        }

        public int GetCamp()
        {
            return GetPart<ActorDataPart>().m_camp.Value;
        }


        #region  临时  后面删除

        List<Vector3> road;
        public void SetRoad(List<Vector3> road)
        {
            this.road = road;
        }
        public List<Vector3> GetRoad()
        {
            return road;
        }
        #endregion




        protected override void OnUpdate(object updateContext)
        {
            base.OnUpdate(updateContext);
            m_fsmActor.Update();
        }

        protected override void OnReset()
        {
            base.OnReset();
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_INTO_FIGHT_STATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
            m_fsmActor = null;
            m_SkeletonAnimation = null;
            m_Skills = null;
            m_totalHarm = 0;
            m_dicProp.Clear();
            m_HeroCls = 0;
            m_IsDead = false;
            m_resPath = null;
            if (m_arrCoroutineGroup.Count > 0)
            {
                GameManager.instance.ClearTimers(m_arrCoroutineGroup);
                m_arrCoroutineGroup.Clear();
            }
        }

        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            if (wEventID == DHeroTeamEvent.EVENT_INTO_FIGHT_STATE)
            {
                Debug.Log("战斗, 爽!");
                m_fsmActor.Run<ActorIdleState>();
            }
        }

        protected void AddTimer(float delay, Action callback)
        {
            if (callback == null) throw new Exception("ActorModel->AddTimer Callback is null");
            m_arrCoroutineGroup.Add(GameManager.instance.AddTimer(delay, callback));
        }





    }

}