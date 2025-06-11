namespace GameScripts.HeroTeam
{
    public class FirepowerBuff : BuffBase
    {
        private float m_fOldAttackSpeed = 1f;
        public override void OnInit(ISpineCreature owner, cfg_HeroTeamBuff cfg)
        {
            base.OnInit(owner, cfg);

            m_fOldAttackSpeed = m_Owner.GetFloatAttr(ActorPropKey.ACTOR_PROP_ATTACK_SPEED);
            m_Owner.SetFloatAttr(ActorPropKey.ACTOR_PROP_ATTACK_SPEED, m_fOldAttackSpeed * cfg.fATKSpeedMul);
        }
        public override void OnClear()
        {
            base.OnClear();

            if (m_Owner != null && m_Owner.GetState() < ActorState.Dying)
            {
                m_Owner.SetFloatAttr(ActorPropKey.ACTOR_PROP_ATTACK_SPEED, m_fOldAttackSpeed);
            }
        }
    }
}