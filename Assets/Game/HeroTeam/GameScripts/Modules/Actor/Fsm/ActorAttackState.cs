using System;
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


            IMonster target = null;
            int damage = 0;
            //奶妈职业
            if (m_AttackType == AttackTypeDef.Sage)
            {
                var friends = MonsterSystem.Instance.GetMonstersByCamp(m_Owner.GetCreatureEntity().GetCamp());
                float minHpRate = 1.01f;
                foreach (var friend in friends)
                {
                    float rate = friend.GetHP() * 1.0f / friend.GetMaxHP();
                    if (rate < minHpRate)
                    {
                        minHpRate = rate;
                        target = friend;
                    }
                }
                //治疗是加血 等于 减减血~~~
                damage = -m_Owner.GetCreatureEntity().GetIntAttr(CreatureAttributeDef.ATTACK);
            }
            else
            {
                var monsters = MonsterSystem.Instance.GetMonstersNotEqulCamp(m_Owner.GetCreatureEntity().GetCamp());
                if (monsters.Count > 0)
                {

                    damage = m_Owner.GetCreatureEntity().GetIntAttr(CreatureAttributeDef.ATTACK);
                    // target = monsters.Aggregate((max, current) => current.GetHatred() > max.GetHatred() ? current : max);
                    int maxHatred = -1;
                    monsters.ForEach(current =>
                    {
                        if (maxHatred < current.GetHatred())
                        {
                            maxHatred = current.GetHatred();
                            target = current;
                        }
                    });
                }
            }


            if (null != target)
            {
                float dirX = target.GetLockTr().position.x - m_Anim.transform.position.x;
                m_Anim.skeleton.ScaleX = dirX > 0 ? 1f : -1f;
                m_Anim.state.SetAnimation(0, m_ActorAnimConfig.szAttack, false);

                //仇恨值
                IMonster m = m_Owner.GetCreatureEntity() as IMonster;
                int newHatred = m.GetHatred() + Mathf.FloorToInt(m_Owner.GetMonsterCfg().iAttackHatred / 100f * Mathf.Abs(damage));
                m.SetHatred(newHatred);

                if (m_AttackType == AttackTypeDef.Fencer)
                {
                    //后续需要绑定动画帧事件
                    GameManager.instance.AddTimer(0.5f, () =>
                    {
                        string szAttackEffectPath = m_Owner.GetMonsterCfg().szAttackEffectPath;
                        if (!string.IsNullOrEmpty(szAttackEffectPath))
                        {
                            GameEffectManager.Instance.ShowEffect(szAttackEffectPath, target.GetLockTr().position, Quaternion.identity);
                        }
                        target.SetHPDelta(-damage);
                    });
                }
                else
                {
                    //后续需要绑定动画帧事件
                    GameManager.instance.AddTimer(0.5f, () =>
                                            {
                                                Shot(damage, target);
                                            });
                }
            }
            else
            {
                Debug.LogError("No valid target found for attack.");
            }

            GameManager.instance.AddTimer(1f, () =>
                  {
                      m_AttackCount++;
                      m_StateMachine.ChangeState<ActorIdleState>();
                  });
        }

        private void Shot(int damage, IMonster target)
        {

            //发射的骨骼点
            if (m_ShotBone == null && !string.IsNullOrEmpty(m_Owner.GetMonsterCfg().szShotBone))
            {
                m_ShotBone = m_Anim.skeleton.FindBone(m_Owner.GetMonsterCfg().szShotBone);
            }

            Vector3 shotPos;
            if (m_ShotBone != null)
            {
                shotPos = m_Anim.transform.TransformPoint(new Vector3(m_ShotBone.WorldX, m_ShotBone.WorldY, 0));
            }
            else
            {
                shotPos = m_Anim.transform.position + Vector3.up * 0.5f;
            }

            //创建一枚子弹/发球
            var bullet = BulletManager.Instance.Get<Bullet>(m_cfgBullet, shotPos);
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
