using DG.Tweening;
using UnityEngine;
using XClient.Common;
using XGame.EventEngine;

namespace GameScripts.HeroTeam
{
    public class Hero : SpineCreature, IHero, IEventExecuteSink
    {

        protected override void InitFsm()
        {
            base.InitFsm();
            m_fsmActor.AddNode<HeroIdleState>();
            m_fsmActor.AddNode<HeroAttackState>();
            m_fsmActor.AddNode<HeroSkillState>();
            m_fsmActor.AddNode<HeroHitState>();
            m_fsmActor.AddNode<HeroDeathState>();
            m_fsmActor.AddNode<HeroWinState>();
            m_fsmActor.AddNode<HeroJumpState>();
        }

        protected override void OnAfterInit(object context)
        {
            base.OnAfterInit(context);
            GameGlobal.EventEgnine.Subscibe(this, DHeroTeamEvent.EVENT_INTO_FIGHT_STATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "SpineCreature:Start");

        }

        protected override void OnReset()
        {
            base.OnReset();
            GameGlobal.EventEgnine.UnSubscibe(this, DHeroTeamEvent.EVENT_INTO_FIGHT_STATE, DEventSourceType.SOURCE_TYPE_ENTITY, 0);
        }

        protected override void OnHpChanged(int delta)
        {

            if (delta < 0)
            {
                //进入死亡动画
                if (GetHP() <= 0)
                {
                    m_fsmActor.ChangeState<HeroDeathState>();
                }
                else
                {
                    m_fsmActor.ChangeState<HeroHitState>();
                }
            }
        }

        public void EludeBossSkill(Vector3 bossPos, Vector3 bossDir, float radius, float angleDeg)
        {
            if (null != GetFaceTr())
            {
                GetFaceTr().gameObject.SetActive(true);
                AddTimer(2.5f, () => GetFaceTr().gameObject.SetActive(false));
            }

            // DodgeAndReturn(transform, bossPos, bossDir);
        }


        Vector2 GetSideDodgeDirection(Vector2 bossDir, Vector2 toNpc)
        {
            // 取 bossDir 的垂直方向（法向量）
            Vector2 side = Vector2.Perpendicular(bossDir).normalized;

            // 决定往左还是往右闪，依据 npc 在哪一侧
            float sign = Mathf.Sign(Vector2.Dot(side, toNpc));
            return side * sign;
        }

        // public void DodgeAndReturn(Transform tr, Vector2 bossPos, Vector2 bossDir, float dodgeDistance = 7f, float waitTime = 1.5f, float duration = 1f)
        // {
        //     Vector2 toNpc = (Vector2)tr.position - bossPos;
        //     Vector2 dodgeDir = GetSideDodgeDirection(bossDir, toNpc);

        //     Vector3 startPos = tr.position;
        //     Vector3 dodgeTarget = startPos + (Vector3)(dodgeDir * dodgeDistance);

        //     var anim = tr.GetComponent<Actor>().GetSkeleton();
        //     var animConfig = tr.GetComponent<Actor>().GetAnimConfig();
        //     anim.state.SetAnimation(1, animConfig.szHit, true);
        //     tr.DOKill();
        //     tr.DOMove(dodgeTarget, duration)
        //         .SetEase(Ease.OutQuad)
        //         .OnComplete(() =>
        //         {
        //             // anim.state.SetAnimation(1, animConfig.szIdle, true);
        //             // DOVirtual.DelayedCall(waitTime, () =>
        //             // {
        //             //     anim.state.SetAnimation(1, animConfig.szMove, true);
        //             //     tr.DOMove(startPos, duration).SetEase(Ease.InQuad).OnComplete(() =>
        //             //     {
        //             //         anim.state.ClearTrack(1);
        //             //         anim.state.SetAnimation(0, animConfig.szIdle, true);
        //             //     });
        //             // });
        //             AddTimer(waitTime, () =>
        //             {
        //                 anim.state.SetAnimation(1, animConfig.szMove, true);
        //                 tr.DOMove(startPos, duration).SetEase(Ease.InQuad).OnComplete(() =>
        //                 {
        //                     anim.state.ClearTrack(1);
        //                     anim.state.SetAnimation(0, animConfig.szIdle, true);
        //                 });
        //             });
        //         });
        // }

        public void ReceiveBossSelect(Vector3 bossPos)
        {
            Vector3 startPos = transform.position;
            float repulseDistance = 10f;
            Vector3 repulseDir = (transform.position - bossPos).normalized;
            Vector3 repulseTarget = startPos + (Vector3)(repulseDir * repulseDistance);
            transform.DOKill();
            transform.DOMove(repulseTarget, 0.3f)
               .SetEase(Ease.OutQuad)
               .OnComplete(() =>
               {
                   //    DOVirtual.DelayedCall(0.5f, () =>
                   //    {
                   //        transform.DOMove(startPos, 0.6f).SetEase(Ease.OutCirc);
                   //    });
                   AddTimer(0.5f, () =>
                                     {
                                         transform.DOMove(startPos, 0.6f).SetEase(Ease.OutCirc);
                                     });
               });
            transform.DOScale(1.4f, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
               {
                   AddTimer(0.5f, () =>
                  {
                      transform.DOScale(1, 0.6f).SetEase(Ease.OutCirc);
                  });
               });

        }


        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            if (wEventID == DHeroTeamEvent.EVENT_INTO_FIGHT_STATE)
            {
                Debug.Log("战斗, 爽!" + this.name);
                m_fsmActor.Run<HeroIdleState>();
            }
        }
    }
}