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

                        var dir = Quaternion.Euler(0, 0, -randomAngle) * Vector2.left;
                        List<IMonster> monsters = GetPlayersInSector(m_Anim.transform.position, dir, 15, 45);
                        GameManager.instance.AddTimer(0.5f, () =>
                        {
                            monsters.ForEach(m => m.EludeBossSkill(m_Anim.transform.position, dir, 15, 45));
                        });

                        yield return new WaitForSeconds(2f);
                        break;
                }
            }

            ((IActor)m_StateMachine.Owner).GetSkeleton().state.SetAnimation(0, skill.szAnimName, false);


            GameManager.instance.AddTimer(1.0f, () =>
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


            GameManager.instance.AddTimer(2.3f, () =>
            {
                m_StateMachine.ChangeState<ActorIdleState>();
            });
        }


        private List<IMonster> GetPlayersInSector(Vector3 bossPos, Vector3 bossDir, float radius, float angle)
        {

            var monsters = MonsterSystem.Instance.GetMonstersNotEqulCamp(m_Owner.GetCreatureEntity().GetCamp());
            float halfAngleRad = angle * 0.5f * Mathf.Deg2Rad;
            List<IMonster> result = new List<IMonster>();
            foreach (var monster in monsters)
            {
                var toPlayer = monster.GetPos() - bossPos;
                if (toPlayer.magnitude > radius) continue;

                Vector2 toPlayerNormalized = toPlayer.normalized;
                float dot = Vector2.Dot(bossDir.normalized, toPlayerNormalized);
                float angleToPlayer = Mathf.Acos(dot); // 角度弧度制

                if (angleToPlayer <= halfAngleRad)
                {
                    result.Add(monster);
                }
            }

            return result;
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