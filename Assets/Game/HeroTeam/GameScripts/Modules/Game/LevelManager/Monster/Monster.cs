using UnityEngine;
using XClient.Common;
using XGame.EventEngine;



namespace GameScripts.HeroTeam
{
    public class Monster : SpineCreature, IMonster, IEventExecuteSink
    {
        [SerializeField] private SpriteRenderer m_srRangeSkillTip;
        public SpriteRenderer GetRangeSkillTipSr() => m_srRangeSkillTip;
        public Transform SkillOrgin;

        private bool m_IsBoss = false;

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
                m_fsmActor.Run<HeroIdleState>();
            }
        }

        protected override void OnHpChanged(int delta)
        {

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