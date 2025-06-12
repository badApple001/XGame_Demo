using System.Collections;
using UniFramework.Machine;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    public class HeroSkillState : SpineCreatureStateBase
    {
        public override void OnCreate(StateMachine machine)
        {
            base.OnCreate(machine);
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (m_StateMachine.GetBlackboardValue(BlackboardDef.ACTOR_SKILL_CD_COMPLETED) is cfg_HeroTeamSkills skill)
            {
                GameManager.Instance.OpenCoroutine(ActiveSkill(skill));
            }
            else
            {
                Debug.LogError("No valid skill found for BossSkillState.");
                m_StateMachine.ChangeState<HeroIdleState>();
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

            Debug.Log($"---------------- [Hero Skill]: {skill.iID}");

            //技能枚举
            switch (skill.iID)
            {
                case 400001:
                    {
                        yield return GameManager.Instance.OpenCoroutine(Load_Skill_400001(skill));
                    }
                    break;
                case 600001:
                    {
                        yield return GameManager.Instance.OpenCoroutine(Load_Skill_600001(skill));
                    }
                    break;
                default:
                    {
                        Debug.LogError($"################ Undefine Skill: {skill.iID}");
                        yield return new WaitForSeconds(1f);
                        m_StateMachine.ChangeState<HeroIdleState>();
                        break;
                    }
            }
        }


        /// <summary>
        /// 嘲讽
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        private IEnumerator Load_Skill_400001(cfg_HeroTeamSkills skill)
        {
            m_Anim.state.SetAnimation(0, skill.szAnimName, false);


            //固伤+百分比暴击
            var damage = skill.iValue + skill.iPercentValue / 100.0f * m_Cfg.iAttack;
            //技能附带仇恨 + 攻击百分比仇恨值叠加
            var hate = skill.iHate + m_Cfg.iAttackHatred / 100f * damage;

            var target = GameManager.Instance.GetBossEntity();
            if (target.GetState() < ActorState.Dying)
            {
                //嘲讽技能， 没有伤害
                // target.SetHPDelta(-Mathf.FloorToInt(damage));
                target.SetHatred(target.GetHatred() + Mathf.FloorToInt(hate));

                yield return new WaitForSeconds(1.4f);
                m_StateMachine.ChangeState<HeroIdleState>();
            }
        }

        /// <summary>
        /// 技能暴击
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        private IEnumerator Load_Skill_600001(cfg_HeroTeamSkills skill)
        {

            m_Anim.state.SetAnimation(0, skill.szAnimName, false);

            //固伤+百分比暴击
            var damage = skill.iValue + skill.iPercentValue / 100f * m_Cfg.iAttack;
            //技能附带仇恨 + 攻击百分比仇恨值叠加
            var hate = skill.iHate + m_Cfg.iAttackHatred / 100f * damage;
            //获取伤害浮动后的值
            int iDamage = CombatUtils.ApplyRandomVariance(-Mathf.FloorToInt(damage));

            var target = GameManager.Instance.GetBossEntity();
            if (target.GetState() < ActorState.Dying)
            {

                target.SetHPDelta(iDamage);
                target.SetHatred(target.GetHatred() + Mathf.FloorToInt(hate));

                //暴击飘字
                FlowTextManager.Instance.ShowFlowText(FlowTextType.CriticalHit, Mathf.FloorToInt(damage).ToString(), target.GetTr().position);

                yield return new WaitForSeconds(1.4f);
                m_StateMachine.ChangeState<HeroIdleState>();
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