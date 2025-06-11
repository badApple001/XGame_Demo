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
            m_fsmActor.AddNode<HeroRandomBehaviourState>();
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
            if (GetState() > ActorState.Normal) return;

            FlowTextManager.Instance.ShowFlowText(delta < 0 ? FlowTextType.Normal : FlowTextType.Treat, delta.ToString(), GetTr().position);
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

        public bool EludeBossSkill(Vector3 bossPos, Vector3 bossDir, float radius, float angleDeg)
        {

            return DodgeAndReturn(transform, bossPos, bossDir);
        }


        Vector2 GetSideDodgeDirection(Vector2 bossDir, Vector2 toNpc)
        {
            // 取 bossDir 的垂直方向（法向量）
            Vector2 side = Vector2.Perpendicular(bossDir).normalized;

            // 决定往左还是往右闪，依据 npc 在哪一侧
            float sign = Mathf.Sign(Vector2.Dot(side, toNpc));
            return side * sign;
        }


        private Vector3 startPos;
        private Vector3 dodgeTarget;
        private bool dodge = true;
        private float dodgeDistance = 7f;
        private float waitTime = 1.5f;
        private float duration = 1f;

        public bool DodgeAndReturn(Transform tr, Vector2 bossPos, Vector2 bossDir)
        {
            Vector2 toPlayer = (Vector2)tr.position - bossPos;
            Vector2 dodgeDir = GetSideDodgeDirection(bossDir, toPlayer);

            startPos = tr.position;
            dodgeTarget = startPos + (Vector3)(dodgeDir * dodgeDistance);

            dodge = false;
            if (Random.value > 0.4f)
            {

                DodgeAndReturn();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsDodge()
        {
            return dodge;
        }

        public void ShowEmoji(string emojiId, float showSeconds = 2)
        {
            if (null != GetFaceTr())
            {
                var emoji = GameEffectManager.Instance.ShowEffect(emojiId, GetFaceTr().position, Quaternion.Euler(-90f, 0, 0), showSeconds);
                emoji.SetParent(GetFaceTr());
            }
        }

        public void DodgeAndReturn()
        {
            if (dodge) return;
            dodge = true;

            var anim = m_SkeletonAnimation;
            var animConfig = GetAnimConfig();
            var tr = transform;
            // anim.state.SetAnimation(1, animConfig.szHit, true);

            //惊吓的表情
            ShowEmoji("Game/HeroTeam/GameResources/Prefabs/Game/Emoji/EmojiScared.prefab", 2f);

            tr.DOKill();
            tr.DOMove(dodgeTarget, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    SetPos(dodgeTarget);
                    AddTimer(waitTime, () =>
                    {
                        anim.state.SetAnimation(1, animConfig.szMove, true);
                        tr.DOMove(startPos, duration).SetEase(Ease.InQuad).OnComplete(() =>
                        {
                            SetPos(startPos);
                            anim.state.ClearTrack(1);
                            anim.state.SetAnimation(0, animConfig.szIdle, true);
                        });
                    });
                });
        }

        public void ReceiveBossSelect(Vector3 bossPos)
        {
            Vector3 startPos = transform.position;
            float repulseDistance = 10f;
            Vector3 repulseDir = (transform.position - bossPos).normalized;
            Vector3 repulseTarget = startPos + (Vector3)(repulseDir * repulseDistance);
            transform.DOKill();

            //拖尾
            var szfogTailPath = "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Environment/Water/Bubbles/BubbleSimpleTrail_Fog.prefab";
            var fogTail = GameEffectManager.Instance.ShowEffect(szfogTailPath, 0.8f);
            fogTail.SetParent(transform, false);
            fogTail.localPosition = Vector3.zero;
            fogTail.localScale = Vector3.one * 5f;

            transform.DOMove(repulseTarget, 0.3f)
               .SetEase(Ease.OutQuad)
               .OnComplete(() =>
               {
                   SetPos(repulseTarget);
                   AddTimer(0.5f, () =>
                                     {
                                         transform.DOMove(startPos, 0.6f).SetEase(Ease.OutCirc).OnComplete(() =>
                                         {
                                             SetPos(startPos);
                                         });
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
                AddTimer(Random.Range(0, 4f), () => m_fsmActor.Run<HeroIdleState>());
            }
        }
    }
}