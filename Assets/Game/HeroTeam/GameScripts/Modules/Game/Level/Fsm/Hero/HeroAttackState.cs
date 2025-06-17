using System;
using UniFramework.Machine;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XClient.Rand;

namespace GameScripts.HeroTeam
{
    public class HeroAttackState : SpineCreatureStateBase
    {
        protected int m_AttackCount = 0;
        protected AttackTypeDef m_AttackType = AttackTypeDef.Fencer;

        protected Spine.Bone m_ShotBone = null;

        protected cfg_HeroTeamBullet m_CfgBullet;

        public override void OnCreate(StateMachine machine)
        {
            base.OnCreate(machine);
        }

        private cfg_HeroTeamSkills HasSkillCooldown()
        {
            var skill = m_Owner.RandomSelectSkill();
            return skill;
        }


        private static int SelectMaxHateSortComparer(ISpineCreature a, ISpineCreature b)
        {
            return a.GetHatred().CompareTo(b.GetHatred());
        }
        private void NormalAttack()
        {


            ISpineCreature target = null;
            int damage = 0;
            //奶妈职业
            if (m_AttackType == AttackTypeDef.Sage)
            {
                var friends = LevelManager.Instance.GetActorsByCamp(m_Owner.GetCamp());
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
                damage = -m_Owner.GetPower();
            }
            else
            {
                // var foesCamp = m_Owner.GetCamp() == CampDef.HERO ? CampDef.MONSTER : CampDef.HERO;
                // var foes = LevelManager.Instance.GetActorsByCamp(foesCamp);
                // if (foes.Count > 0)
                // {

                //     damage = m_Owner.GetPower();
                //     // target = monsters.Aggregate((max, current) => current.GetHatred() > max.GetHatred() ? current : max);

                //     //排序仇恨值
                //     foes.Sort(SelectMaxHateSortComparer);
                //     var maxHate = foes[foes.Count - 1].GetHatred();

                //     int i = foes.Count - 2;
                //     for (; i >= 0; i--)
                //     {
                //         if (foes[i].GetHatred() != maxHate)
                //         {
                //             break;
                //         }
                //     }
                //     if (i >= 0 && i != foes.Count - 1)
                //     {
                //         //如果有多个仇恨值相同的怪物，随机选一个
                //         var randomIndex = UnityEngine.Random.Range(i + 1, foes.Count);
                //         target = foes[randomIndex];
                //         Debug.Log($"<color=0x00ff00>{foes.Count - 1 - i}个Hero仇恨值相同, 执行随机选择</color>");
                //     }
                //     else
                //     {
                //         //如果只有一个仇恨值最大的怪物
                //         target = foes[foes.Count - 1];
                //     }

                //     //找到仇恨值最大的
                //     // target = monsters[0];
                //     // int maxHatred = -1;
                //     // monsters.ForEach(current =>
                //     // {
                //     //     if (maxHatred < current.GetHatred())
                //     //     {
                //     //         maxHatred = current.GetHatred();
                //     //         target = current;
                //     //     }
                //     // });
                // }
                damage = m_Owner.GetPower();
                target = GameManager.Instance.GetBossEntity();
            }


            if (null != target)
            {
                // damage *= XClient.Rand.RandomUtility.Range(-0.9f,)
                // damage = Mathf.FloorToInt(damage * Random.Range(0.95f, 1.05f));
                damage = CombatUtils.ApplyRandomVariance(damage);
                float dirX = target.GetLockTr().position.x - m_Anim.transform.position.x;
                m_Anim.skeleton.ScaleX = dirX > 0 ? 1f : -1f;
                m_Anim.state.SetAnimation(0, m_ActorAnimConfig.szAttack, false);

                //仇恨值
                ISpineCreature m = m_Owner;
                int newHatred = m.GetHatred() + Mathf.FloorToInt(m_Cfg.iAttackHatred / 100f * Mathf.Abs(damage));
                m.SetHatred(newHatred);
                m.RecordHarm(Mathf.Abs(damage));

                if (m_AttackType == AttackTypeDef.Fencer)
                {
                    //后续需要绑定动画帧事件
                    AddTimer(0.5f, () =>
                    {
                        string szAttackEffectPath = m_Cfg.szAttackEffectPath;
                        if (!string.IsNullOrEmpty(szAttackEffectPath))
                        {
                            GameEffectManager.Instance.ShowEffect(szAttackEffectPath, target.GetLockTr().position, Quaternion.identity);
                        }

                        if (m_Cfg.iAttkBuffID != 0)
                        {
                            BuffManager.Instance.CreateBuff(target, m_Cfg.iAttkBuffID);
                            Debug.Log($"攻击buff: {m_Cfg.iAttkBuffID}");
                        }

                        target.SetHPDelta(-damage);
                    });
                }
                else
                {
                    //后续需要绑定动画帧事件
                    AddTimer(0.5f, () =>
                                            {
                                                Shoot(damage, target);
                                            });
                }
            }
            else
            {
                // Debug.LogWarning("No valid target found for attack.");
            }

            AddTimer(1f, () =>
                  {
                      m_AttackCount++;
                      m_StateMachine.ChangeState<HeroIdleState>();
                  });
        }

        private void Shoot(int damage, ISpineCreature target)
        {


            //发射的骨骼点
            if (m_ShotBone == null && !string.IsNullOrEmpty(m_Cfg.szShotBone))
            {
                m_ShotBone = m_Anim.skeleton.FindBone(m_Cfg.szShotBone);
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
            //这里本来是想用反射在表里配置的， 但考虑到子弹是高频类，所以还是加一些定义来处理吧
            var bullet = BulletManager.Instance.GetBullet(m_CfgBullet, shotPos);
            bullet.SetHarm(damage);
            bullet.SetSender(m_Owner.id);
            bullet.SetTarget(target);
            if (bullet is LightBeamBullet lb) lb.SetShooter(m_Owner.GetLockTr());
        }

        // public Type GetBulletClsByCfgId(int cfgId)
        // {
        //     return cfgId switch
        //     {
        //         1 => typeof(Bullet),
        //         2 => typeof(LightBeamBullet),
        //         3 => typeof(EventBullet),
        //         _ => typeof(Bullet),
        //     };
        // }

        // public Bullet GetBullet(cfg_HeroTeamBullet cfg, Vector3 pos)
        // {
        //     int iType = cfg.iType;
        //     return iType switch
        //     {
        //         1 => BulletManager.Instance.Get<Bullet>(cfg, pos),
        //         2 => BulletManager.Instance.Get<LightBeamBullet>(cfg, pos),
        //         3 => BulletManager.Instance.Get<EventBullet>(cfg, pos),
        //         _ => BulletManager.Instance.Get<Bullet>(cfg, pos),
        //     };
        // }

        public override void OnEnter()
        {
            base.OnEnter();

            int nType = m_Cfg.AttackType;
            m_AttackType = (AttackTypeDef)nType;
            m_CfgBullet = GameGlobal.GameScheme.HeroTeamBullet_0(m_Cfg.iBullet);

            var skill = HasSkillCooldown();
            if (skill != null)
            {
                m_AttackCount = 0;
                m_StateMachine.SetBlackboardValue(BlackboardDef.ACTOR_SKILL_CD_COMPLETED, skill);
                m_StateMachine.ChangeState<HeroSkillState>();
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
