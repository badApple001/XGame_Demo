using System;
using System.Collections.Generic;
using DG.Tweening;
using UniFramework.Machine;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    public class HeroRandomBehaviourState : SpineCreatureStateBase
    {

        private List<Action> m_arrActions = new List<Action>();

        private static readonly string[] emojis = new string[] {
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiCool.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiCute.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiDerpGasp.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiDrool.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiLaughCry.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiStarstruck.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Joy/EmojiXD.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Disgust/EmojiSick.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Disgust/EmojiSick.prefab",
            "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Interactive/Emojis/Anger/EmojiAngry.prefab",
        };

        public override void OnCreate(StateMachine machine)
        {
            base.OnCreate(machine);
            m_arrActions.Add(Task_Jump);
            m_arrActions.Add(Task_Emoji);
            m_arrActions.Add(Task_ChatMessage);

        }
        public override void OnEnter()
        {
            base.OnEnter();
            int tskIndex = UnityEngine.Random.Range(0, m_arrActions.Count);
            try
            {
                m_arrActions[tskIndex]();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Back2IdleState();
            }
        }
        private void Back2IdleState() => m_StateMachine.ChangeState<HeroIdleState>();
        private void Task_Jump()
        {
            m_Anim.state.SetAnimation(0, m_ActorAnimConfig.szJump1, false);
            m_Owner.GetTr().DOScale(1.2f, 0.2f).OnComplete(() => m_Owner.GetTr().DOScale(1f, 0.3f));
            m_Owner.GetTr().DOJump(m_Owner.GetTr().position, 7f, 1, 0.5f).OnComplete(() =>
            {
                Back2IdleState();
            }).SetAutoKill(true);
        }
        private void Task_Emoji()
        {
            AddTimer(1f, Back2IdleState);
            if (m_Owner is IHero hero) hero.ShowEmoji(emojis[UnityEngine.Random.Range(0, emojis.Length)]);
        }
        private void Task_ChatMessage()
        {

        }
    }
}