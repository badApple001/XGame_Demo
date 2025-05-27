using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniFramework.Machine;
using UnityEngine;
using XClient.Entity;

namespace GameScripts.HeroTeam
{
    public class ActorAttackState : ActorStateBase
    {
        protected int m_AttackCount = 0;
        protected AttackTypeDef m_AttackType = AttackTypeDef.Fencer;
        public override void OnCreate(StateMachine machine)
        {
            base.OnCreate(machine);
        }

        private cfg_HeroTeamSkills HasSkillCooldown()
        {
            var skills = ((IActor)m_StateMachine.Owner).GetSkills();
            int elapseTime = Mathf.RoundToInt(m_AttackCount * ((IActor)m_StateMachine.Owner).GetMonsterCfg().fAttackInterval);
            var cooldSkills = skills.FindAll(s => s.iSkillCD <= elapseTime);
            if (cooldSkills.Count == 0)
            {
                return null;
            }
            var skill = cooldSkills.PickRandom();
            return skill;
        }

        private void NormalAttack()
        {
            var monsters = MonsterSystem.Instance.GetMonstersNotEqulCamp(((IActor)m_StateMachine.Owner).GetCreatureEntity().GetCamp());
            if (monsters.Count > 0)
            {
                IMonster maxHatredMonster = monsters.Aggregate((max, current) => current.GetHatred() > max.GetHatred() ? current : max);
                if (null != maxHatredMonster)
                {
                    m_Anim.state.SetAnimation(0, m_ActorAnimConfig.szAttack, false);

                    if (m_AttackType == AttackTypeDef.Fencer)
                    {
                        GameManager.instance.AddTimer(0.5f, () =>
                        {
                            var damage = ((IActor)m_StateMachine.Owner).GetCreatureEntity().GetIntAttr(CreatureAttributeDef.ATTACK);
                            maxHatredMonster.SetHPDelta(-damage);
                        });

                        GameManager.instance.AddTimer(1f, () =>
                        {
                            m_AttackCount++;
                            m_StateMachine.ChangeState<ActorIdleState>();
                        });
                    }
                    else
                    {
                        //创建一枚子弹/发球
                    
                    }
                }
                else
                {
                    Debug.LogError("No valid target found for attack.");
                }
            }
        }




        public override void OnEnter()
        {
            base.OnEnter();

            int nType = ((IActor)m_StateMachine.Owner).GetMonsterCfg().AttackType;
            m_AttackType = (AttackTypeDef)nType;

            var skill = HasSkillCooldown();
            if (skill != null)
            {
                m_AttackCount = 0;
                m_StateMachine.SetBlackboardValue(Actor.Machine_blackBoard_CooldownSkill, skill);
                m_StateMachine.ChangeState<ActorSkillState>();
            }
            else
            {
                NormalAttack();
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
