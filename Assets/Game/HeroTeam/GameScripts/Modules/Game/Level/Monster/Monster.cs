using UnityEngine;
using XClient.Common;
using XGame.EventEngine;



namespace GameScripts.HeroTeam
{
    public class Monster : SpineCreature, IMonster, IEventExecuteSink
    {
        [SerializeField] private SpriteRenderer m_SkillTipRenderer;
        public SpriteRenderer GetSkillTipRenderer() => m_SkillTipRenderer;

        protected bool m_IsBoss = false;  //是否是boss
        protected bool m_IsBerserk = false;//是否已经狂暴了
        protected Transform m_SkillRoot; //技能施法的根节点
        public Transform GetSkillRoot() => m_SkillRoot;


        protected override void OnInstantiated()
        {
            base.OnInstantiated();
            m_SkillRoot = transform.Find("Root_Skill");
            m_SkillTipRenderer = m_SkillRoot.GetChild(0).GetComponent<SpriteRenderer>();
            m_SkeletonAnimation.state.SetAnimation(0, GetAnimConfig().szIdle, true);
        }

        protected override void InitFsm()
        {
            base.InitFsm();
            m_fsmActor.AddNode<MonsterIdleState>();
            m_fsmActor.AddNode<MonsterAttackState>();
            m_fsmActor.AddNode<MonsterSkillState>();
            m_fsmActor.AddNode<MonsterHitState>();
            m_fsmActor.AddNode<MonsterDeathState>();
        }

        protected override void OnAfterInit(object context)
        {
            base.OnAfterInit(context);
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_INTO_FIGHT_STATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "SpineCreature:Start");

        }

        protected override void OnReset()
        {
            base.OnReset();
            m_IsBoss = false;
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_INTO_FIGHT_STATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
        }
        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            if (wEventID == DHeroTeamEvent.EVENT_INTO_FIGHT_STATE)
            {
                Debug.Log("战斗, 爽!" + this.name);
                m_fsmActor.Run<MonsterIdleState>();
            }
        }

        protected override void OnHpChanged(int delta)
        {

            if (GetState() > ActorState.Normal) return;

            FlowTextManager.Instance.ShowFlowText(delta < 0 ? FlowTextType.Normal : FlowTextType.Treat, delta.ToString(), GetTr().position);

            if (delta < 0)
            {
                //进入死亡动画
                if (GetHP() <= 0)
                {
                    m_fsmActor.ChangeState<MonsterDeathState>();
                }
                else if (!IsBoos()) //boss是霸体状态 ( ps: 总不能一群人打它,每人打他一下 他都要进入hit一下吧, 从设计上就不合理了~ )
                {
                    m_fsmActor.ChangeState<MonsterHitState>();
                }

                //boss残血的时候狂暴
                if (!m_IsBerserk && GetState() < ActorState.Dying && IsBoos() && GetHP() <= GetMaxHP() * 0.1f)
                {
                    m_IsBerserk = true;
                    var pContext = UI.HeroTeamGame.BossRageEventContext.Instance;
                    pContext.showHintWindow = true;
                    pContext.bossCDScale = 1f / (1f + 1f);
                    GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_BOSS_BERSERK_HINT, DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext);
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

        public void SetBoos()
        {
            m_IsBoss = true;
        }

        public bool IsBoos()
        {
            return m_IsBoss;
        }

    }

}