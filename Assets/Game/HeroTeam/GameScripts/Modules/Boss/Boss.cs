using UnityEngine;


namespace GameScripts.HeroTeam
{

    public class Boss : Actor
    {
        [SerializeField] private SpriteRenderer m_srRangeSkillTip;
        public SpriteRenderer GetRangeSkillTipSr() => m_srRangeSkillTip;
    }

}