namespace GameScripts.HeroTeam
{
    using System.Collections.Generic;
    using Spine.Unity;
    using UniFramework.Machine;
    using UnityEngine;
    using XClient.Common;
    using XClient.Entity;
    using XGame.Entity;

    public class SpineCreature : Actor, ISpineCreature
    {

        /// <summary>
        /// 角色的有限状态机
        /// </summary>
        protected StateMachine m_fsmActor;

        /// <summary>
        /// 拥有的所有技能
        /// </summary>
        protected List<cfg_HeroTeamSkills> m_Skills = null;


        /// <summary>
        /// Spine组件
        /// </summary>
        protected SkeletonAnimation m_SkeletonAnimation;


        /// <summary>
        /// 挂表情的节点
        /// </summary>
        protected Transform m_trFace;

        /// <summary>
        /// 蒙皮父节点
        /// </summary>
        protected Transform m_trVisual;


        /// <summary>
        /// 角色职业
        /// </summary>
        protected int m_HeroCls = 0;


        /// <summary>
        /// 模型缩放倍率
        /// </summary>
        protected float m_fModeScaleMul = 1f;


        public int GetHeroCls() => m_HeroCls;


        /// <summary>
        /// 总共造成的伤害，如果是奶职业，就是治疗总量
        /// </summary>
        protected int m_nTotalHarm = 0;


        /// <summary>
        /// 此下标用来独立每个角色的名称: {heroClass}{m_byteShareSpawnIndex++}
        /// 后续策划如果有想法就增加配表
        /// </summary>
        protected static byte m_byteShareSpawnIndex = 0;

        protected override void OnInit(object context)
        {
            base.OnInit(context);

            //名字后面要策划的表来配 这里只是临时用来区分每个玩家
            name += string.Format("{0:D2}", m_byteShareSpawnIndex++);
        }

        protected override void OnAfterInit(object context)
        {
            base.OnAfterInit(context);


            var ctx = context as NetEntityShareInitContext;
            CreateActorContext createActorContext = (CreateActorContext)(ctx.localInitContext);
            m_fModeScaleMul = createActorContext.modeScaleMul;
            GetPart<CreatureDataPart>().m_camp.Value = createActorContext.nCamp;
            SetPos(createActorContext.worldPos);
            SetRotation(Quaternion.Euler(createActorContext.eulerAngles));
            var cfg = (cfg_HeroTeamCreature)GetCreatureCig();
            //速度
            SetSpeed(cfg.fMoveSpeed);
            //攻击|治疗
            SetPower(cfg.iAttack);
            //生命值
            GetPart<CreatureDataPart>().m_maxHp.Value = cfg.baseHP;
            GetPart<CreatureDataPart>().m_hp.Value = cfg.baseHP;
            //职业
            m_HeroCls = cfg.HeroClass;
        }

        protected override void OnInstantiated()
        {
            base.OnInstantiated();

            if (null != m_trParent)
                transform.BetterSetParent(m_trParent);

            //注意: 这只是Spine的生命体是这么设置的
            SetPos(GetPart<CreatureDataPart>().m_pos.Value);
            //如果是3D的，可以根据实际情况去算Rotation
            SetRotation(Quaternion.FromToRotation(Vector3.right, GetPart<CreatureDataPart>().m_forward.Value));

            m_SkeletonAnimation = transform.GetComponentInChildren<SkeletonAnimation>();
            m_trVisual = transform.GetChild(0);

            var cfg = (cfg_Actor)base.GetActorCig();
            m_trVisual.localScale = m_fModeScaleMul * cfg.fSizeScale * Vector3.one;

            var bar = transform.GetComponentInChildren<HpBar>();
            if (null != bar)
            {
                bar.SetEntity(this);
            }

            m_trFace = transform.Find("Face");

            //初始化状态机
            InitFsm();

            //模型加载完成回调
            m_modelLoadedCallback?.Invoke();
            m_modelLoadedCallback = null;
        }


        protected virtual void InitFsm()
        {
            m_fsmActor = new StateMachine(this);
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            m_fsmActor?.Update();
        }

        public StateMachine GetStateMachine() => m_fsmActor;
        public SkeletonAnimation GetSkeleton() => m_SkeletonAnimation;

        public cfg_HeroTeamCreature GetCreatureCig() => GameGlobal.GameScheme.HeroTeamCreature_0(configId);

        public cfg_ActorAnimConfig GetAnimConfig() => GameGlobal.GameScheme.ActorAnimConfig_0(configId);

        public List<cfg_HeroTeamSkills> GetSkills()
        {
            if (m_Skills != null)
            {
                return m_Skills;
            }
            List<cfg_HeroTeamSkills> skills = new List<cfg_HeroTeamSkills>();
            var config = (cfg_HeroTeamCreature)GetCreatureCig();
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
            return GetPart<CreatureDataPart>().m_Hatred.Value;
        }

        public void SetHatred(int value)
        {
            GetPart<CreatureDataPart>().m_Hatred.Value = value;
        }


        public Transform GetFaceTr()
        {
            return m_trFace;
        }


        public void RecordHarm(int addHarm)
        {
            m_nTotalHarm += addHarm;
        }

        public int GetTotalHarm() => m_nTotalHarm;

        public bool IsDie() => m_state == ActorState.Release;

        public void SetHPDelta(int hp)
        {
            var dataPart = GetPart<CreatureDataPart>();
            if (dataPart != null)
            {
                dataPart.m_hp.RemoteValueDelta += hp;
                dataPart.m_hp.Value += hp;
            }
            OnHpChanged(hp);
        }

        protected virtual void OnHpChanged(int delta)
        {

        }

        public void SetMaxHP(long maxHp)
        {
            GetPart<CreatureDataPart>().m_maxHp.Value = maxHp;
        }

        protected override void OnReset()
        {
            base.OnReset();
            m_fsmActor = null;
            m_SkeletonAnimation = null;
            m_Skills = null;
            m_nTotalHarm = 0;
            m_HeroCls = 0;
            m_fModeScaleMul = 1f;
            m_trFace = null;
            m_trVisual = null;
        }


        public int GetCamp()
        {
            return GetPart<CreatureDataPart>().m_camp.Value;
        }

        public float GetSpeed()
        {
            return GetPart<CreatureDataPart>().m_speed.Value;
        }

        public void SetSpeed(float speed)
        {
            GetPart<CreatureDataPart>().m_speed.Value = speed;
        }

        public override void SetPos(Vector3 pos)
        {
            GetPart<CreatureDataPart>().m_pos.Value = pos;
            transform?.SetPosition(pos);
        }

        public override Vector3 GetPos() => GetPart<CreatureDataPart>().m_pos.Value;


        public override void SetRotation(Quaternion rotate)
        {
            GetPart<CreatureDataPart>().m_forward.Value = rotate * Vector3.right;
            if (transform != null)
                transform.rotation = rotate;
        }

        public override Quaternion GetRotation()
        {
            return Quaternion.FromToRotation(Vector3.right, GetPart<CreatureDataPart>().m_forward.Value);
        }

        public Vector3 GetForward()
        {
            return GetPart<CreatureDataPart>().m_forward.Value;
        }

        public void SetForward(Vector3 forward)
        {

            //由 Part里去设置Spine的ScaleX
            GetPart<CreatureDataPart>().m_forward.Value = forward;
        }

        public int GetPower()
        {
            return GetPart<CreatureDataPart>().m_power.Value;
        }

        public void SetPower(int v)
        {
            GetPart<CreatureDataPart>().m_power.Value = v;
        }




        public long GetHP() => GetPart<CreatureDataPart>().m_hp.Value;

        public long GetMaxHP() => GetPart<CreatureDataPart>().m_maxHp.Value;

        public Transform GetVisual() => m_trVisual;

    }
}