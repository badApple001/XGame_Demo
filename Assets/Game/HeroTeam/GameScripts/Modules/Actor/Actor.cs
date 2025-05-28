using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using XClient.Entity;
using Spine.Unity;
using XClient.Common;
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
    public partial class Actor : MonoBehaviour, IActor, IEventExecuteSink
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
        /// 基础配置
        /// </summary>
        private cfg_Monster m_MonsterConfig;

        /// <summary>
        /// Entity组件
        /// </summary>
        private ICreatureEntity m_CreatureEntity;

        /// <summary>
        /// Spine组件
        /// </summary>
        private SkeletonAnimation m_SkeletonAnimation;

        /// <summary>
        /// 缓存一份Transform引用
        /// </summary>
        private Transform m_Tr;

        protected virtual void Start()
        {
            m_Tr = transform;

            var spineAni = GetComponent<SpineAni>();
            Debug.Assert(spineAni != null, "spineAni Component not found.");
            m_SkeletonAnimation = spineAni.skeletonAnimation;

            m_fsmActor = new StateMachine(this);
            m_fsmActor.AddNode<ActorIdleState>();
            m_fsmActor.AddNode<ActorAttackState>();
            m_fsmActor.AddNode<ActorSkillState>();
            m_fsmActor.AddNode<ActorHitState>();
            m_fsmActor.AddNode<ActorDeathState>();
            m_fsmActor.AddNode<ActorWinState>();
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_INTO_FIGHT_CHANGED, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "Actor:Start");
        }

        public Transform GetTr() => m_Tr;


        public cfg_ActorAnimConfig GetAnimConfig() => GameGlobal.GameScheme.ActorAnimConfig_0((int)m_MonsterConfig.nID);

        public void SetCreatureEntity(ICreatureEntity entity) => m_CreatureEntity = entity;

        public ICreatureEntity GetCreatureEntity() => m_CreatureEntity;

        public SkeletonAnimation GetSkeleton() => m_SkeletonAnimation;

        public void SetMonsterCfg(cfg_Monster cfg) => m_MonsterConfig = cfg;

        public cfg_Monster GetMonsterCfg() => m_MonsterConfig;

        public List<cfg_HeroTeamSkills> GetSkills()
        {
            if (m_Skills != null)
            {
                return m_Skills;
            }

            List<cfg_HeroTeamSkills> skills = new List<cfg_HeroTeamSkills>();
            if (m_MonsterConfig != null)
            {
                int[] skillIDs = m_MonsterConfig.Skills;
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

        public void Switch2State<T>() where T : IStateNode => m_fsmActor.ChangeState<T>();

        protected virtual void Update() => m_fsmActor.Update();

        protected virtual void OnDestroy()
        {
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_INTO_FIGHT_CHANGED, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
            m_fsmActor = null;
            m_SkeletonAnimation = null;
            m_CreatureEntity = null;
            m_MonsterConfig = null;
            m_Skills = null;
        }

        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            if (wEventID == DHeroTeamEvent.EVENT_INTO_FIGHT_CHANGED)
            {
                Debug.Log("战斗, 爽!");
                m_fsmActor.Run<ActorIdleState>();
            }
        }
    }

}