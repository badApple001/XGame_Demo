using DG.Tweening;
using GameScripts.HeroTeam;
using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;
using XClient.Common;
using XClient.Entity;

namespace GameScripts.HeroTeam
{
    public class ActorSkillState : ActorStateBase
    {
        public override void OnCreate(StateMachine machine)
        {
            base.OnCreate(machine);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (m_StateMachine.GetBlackboardValue(Actor.Machine_blackBoard_CooldownSkill) is cfg_HeroTeamSkills skill)
            {
                GameManager.instance.OpenCoroutine(ActiveSkill(skill));
            }
            else
            {
                Debug.LogError("No valid skill found for BossSkillState.");
            }
        }

        private IEnumerator ActiveSkill(cfg_HeroTeamSkills skill)
        {

            if (!string.IsNullOrEmpty(skill.szChat))
            {
                var toast = ToastManager.Instance.Get();
                toast.transform.SetParent(((Actor)m_StateMachine.Owner).transform.Find("ChatPoint"));
                toast.transform.localPosition = Vector3.zero;
                toast.Show(skill.szChat, 2f);
            }

            //技能枚举
            switch (skill.iID)
            {
                case 200001:
                    {
                        yield return Load_Skill_200001(skill);
                    }
                    break;
                case 500001:
                    {
                        yield return Load_Skill_500001(skill);
                    }
                    break;
            }
        }

        private IEnumerator Load_Skill_200001(cfg_HeroTeamSkills skill)
        {

            float randomAngle = Random.Range(0, 360f);
            if (skill.hasSkillTip == 1)
            {

                switch (skill.iSkillTipPrefabID)
                {
                    case 1:

                        var rangeTipSr = ((Boss)m_StateMachine.Owner).GetRangeSkillTipSr();
                        rangeTipSr.gameObject.SetActive(true);

                        rangeTipSr.transform.localEulerAngles = new Vector3(0, 0, randomAngle);
                        rangeTipSr.DOColor(new Color(1, 1, 1, 0.5f), 0.5f).SetLoops(4, LoopType.Yoyo).OnComplete(() =>
                        {
                            rangeTipSr.gameObject.SetActive(false);
                        });

                        yield return new WaitForSeconds(2f);
                        break;
                }
            }

            ((IActor)m_StateMachine.Owner).GetSkeleton().state.SetAnimation(0, skill.szAnimName, false);


            GameManager.instance.AddTimer(2.0f, () =>
            {

                var pContext = CameraShakeEventContext.Ins;
                pContext.intensity = 1f;
                pContext.duration = 0.5f;
                pContext.vibrato = 30;
                pContext.randomness = 100f;
                pContext.fadeOut = true;
                GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext);
            });

            GameManager.instance.AddTimer(2.3f, () =>
            {
                m_StateMachine.ChangeState<ActorIdleState>();
            });
        }
        private IEnumerator Load_Skill_500001(cfg_HeroTeamSkills skill)
        {

            yield return null;

            ((Boss)m_StateMachine.Owner).GetSkeleton().state.SetAnimation(0, skill.szAnimName, false);
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