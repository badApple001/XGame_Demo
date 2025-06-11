using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    public class HeroHitState : SpineCreatureStateBase
    {
        public override void OnCreate(StateMachine machine)
        {
            base.OnCreate(machine);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            //后续改到帧动画里
            m_Anim.state.SetAnimation(1, m_ActorAnimConfig.szHit, false);
            AddTimer(0.6f, () =>
                {
                    m_Anim.state.ClearTrack(1);

                    m_StateMachine.ChangeState<HeroIdleState>();
                });


            if (Random.value < 0.8f && m_Owner.GetHP() < m_Owner.GetMaxHP() * 0.4f)
            {
                if (m_Owner is IHero hero)
                {
                    hero.ShowEmoji("Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Sadness/EmojiTearyEyes.prefab");
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();

        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}