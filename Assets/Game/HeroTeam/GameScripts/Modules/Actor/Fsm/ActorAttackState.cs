using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniFramework.Machine;
using UnityEngine;
using XClient.Common;
using XClient.Entity;

namespace GameScripts.HeroTeam
{
    public class ActorAttackState : ActorStateBase
    {
        protected int m_AttackCount = 0;
        protected AttackTypeDef m_AttackType = AttackTypeDef.Fencer;

        protected Spine.Bone m_ShotBone = null;

        protected cfg_HeroTeamBullet m_cfgBullet;

        public override void OnCreate(StateMachine machine)
        {
            base.OnCreate(machine);
        }

        private cfg_HeroTeamSkills HasSkillCooldown()
        {
            var skills = m_Owner.GetSkills();
            int elapseTime = Mathf.RoundToInt(m_AttackCount * m_Owner.GetMonsterCfg().fAttackInterval);
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
            var monsters = MonsterSystem.Instance.GetMonstersNotEqulCamp(m_Owner.GetCreatureEntity().GetCamp());
            if (monsters.Count > 0)
            {
                IMonster maxHatredMonster = monsters.Aggregate((max, current) => current.GetHatred() > max.GetHatred() ? current : max);
                var damage = m_Owner.GetCreatureEntity().GetIntAttr(CreatureAttributeDef.ATTACK);

                if (null != maxHatredMonster)
                {
                    m_Anim.state.SetAnimation(0, m_ActorAnimConfig.szAttack, false);

                    if (m_AttackType == AttackTypeDef.Fencer)
                    {
                        //后续需要绑定动画帧事件
                        GameManager.instance.AddTimer(0.5f, () =>
                        {
                            maxHatredMonster.SetHPDelta(-damage);
                        });
                    }
                    else
                    {
                        //后续需要绑定动画帧事件
                        GameManager.instance.AddTimer(0.5f, () =>
                                                {
                                                    Shot(damage, maxHatredMonster);
                                                });
                    }

                    GameManager.instance.AddTimer(1f, () =>
                       {
                           m_AttackCount++;
                           m_StateMachine.ChangeState<ActorIdleState>();
                       });
                }
                else
                {
                    Debug.LogError("No valid target found for attack.");
                }
            }
        }

        private void Shot(int damage, IMonster target)
        {

            //发射的骨骼点
            // if (m_ShotBone == null)
            // {
            //     m_ShotBone = m_Owner.GetSkeleton().skeleton.FindBone(m_Owner.GetMonsterCfg().szShotBone);
            // }
            // Vector3 shotPos = m_ShotBone == null ? m_Owner.GetSkeleton().transform.position : new Vector3(m_ShotBone.WorldX, m_ShotBone.WorldY, 0);
            //网上的资源骨骼Bone都不是很准确，还是先写死吧
            //后续招了spine再约定好骨骼名称
            Vector3 shotPos = m_Owner.GetSkeleton().transform.position + Vector3.up * 0.5f;


            //创建一枚子弹/发球
            var bullet = BulletManager.Instance.Get<Bullet>(m_cfgBullet);
            bullet.GetTr().position = shotPos;
            bullet.SetHarm(damage);
            bullet.SetSender(m_Owner.GetCreatureEntity().id);
            bullet.SetTarget(target);
        }



        public override void OnEnter()
        {
            base.OnEnter();

            int nType = m_Owner.GetMonsterCfg().AttackType;
            m_AttackType = (AttackTypeDef)nType;
            m_cfgBullet = GameGlobal.GameScheme.HeroTeamBullet_0(m_Owner.GetMonsterCfg().iBullet);

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
