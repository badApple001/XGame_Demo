using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GameScripts.HeroTeam;
using UnityEngine;

public class HeroJumpState : SpineCreatureStateBase
{

    public override void OnEnter()
    {
        base.OnEnter();

        m_Anim.state.SetAnimation(0, m_ActorAnimConfig.szJump1, false);
        // GameManager.instance.AddTimer(0.667f, () =>
        // {
        //     m_Anim.state.SetAnimation(0, m_ActorAnimConfig.szJump2, false);
        // });
        m_Owner.GetTr().DOScale(1.2f, 0.5f).OnComplete(() => m_Owner.GetTr().DOScale(1f, 0.5f));
        m_Owner.GetTr().DOJump(m_Owner.GetTr().position, 7f, 1, 1f).OnComplete(() =>
        {
            m_StateMachine.ChangeState<HeroIdleState>();
        }).SetAutoKill(true);
    }



}
