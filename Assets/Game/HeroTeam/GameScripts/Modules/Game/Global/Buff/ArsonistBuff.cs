using Spine.Unity;
using UnityEngine;

namespace GameScripts.HeroTeam
{

    /// <summary>
    /// 纵火者Buff
    /// 
    /// 每秒钟受到固定伤害
    /// 持续N秒
    /// 
    /// </summary>
    public class ArsonistBuff : BuffBase
    {

        private const string szEmojiPath = "Game/HeroTeam/GameResources/Prefabs/Game/Emoji/EmojiCry.prefab";


        public override void OnInit(ISpineCreature owner, cfg_HeroTeamBuff cfg)
        {
            base.OnInit(owner, cfg);

            // var mr = owner.GetSkeleton().GetComponent<MeshRenderer>();
            // var mat = mr.material;
            // mat.SetColor("_FillColor", Color.red);
            // mat.SetFloat("_FillPhase", 1f);
            // mr.material = mat;

            if (ColorUtility.TryParseHtmlString("#C34FF8", out var color))
            {
                owner.GetSkeleton().skeleton.SetColor(color);
            }
        }

        public override void OnClear()
        {
            if (m_Owner != null && m_Owner.GetState() < ActorState.Dying)
                m_Owner.GetSkeleton().skeleton.SetColor(Color.white);
            base.OnClear();
        }

        public override void OnStep(float now)
        {
            base.OnStep(now);
            if (null != m_Owner)
            {
                m_Owner.SetHPDelta(m_Cfg.iAddHp);
            }

            if (m_Owner is IHero hero)
            {
                hero.ShowEmoji(szEmojiPath);
            }
        }


    }

}