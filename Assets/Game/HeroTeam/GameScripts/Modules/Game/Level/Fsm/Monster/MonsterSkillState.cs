using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UniFramework.Machine;
using UnityEngine;
using XClient.Common;

namespace GameScripts.HeroTeam
{
    public class MonsterSkillState : SpineCreatureStateBase
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
                m_StateMachine.ChangeState<MonsterIdleState>();
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

            Debug.Log($"---------------- [Monster Skill]: {skill.iID}");

            //技能枚举
            switch (skill.iID)
            {
                case 200001:
                    {
                        yield return GameManager.Instance.OpenCoroutine(Load_Skill_200001(skill));
                    }
                    break;
                case 500001:
                    {
                        yield return GameManager.Instance.OpenCoroutine(Load_Skill_500001(skill));
                    }
                    break;
                default:
                    {
                        Debug.LogError($"################ Undefine Skill: {skill.iID}");
                        yield return new WaitForSeconds(1f);
                        m_StateMachine.ChangeState<MonsterIdleState>();
                        break;
                    }
            }
        }


        /// <summary>
        /// 大招AOE
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        private IEnumerator Load_Skill_200001(cfg_HeroTeamSkills skill)
        {

            //新增：有一定概率朝团长丢技能

            float randomAngle = Random.Range(0, 360f);
            if (LeaderManager.Instance.LockLeader())
            {
                var leader = LeaderManager.Instance.GetLeader();
                var dir1 = leader.GetTr().position - m_Owner.GetTr().position;
                dir1.Normalize();
                randomAngle = Mathf.Atan2(dir1.y, dir1.x) * Mathf.Rad2Deg;
#if UNITY_EDITOR
                Debug.Log("$$$$$$$$$$$ Skill 200001: Lock Leader");
#endif
            }

            float skill_range_angle = 60;
            var rangeTipSr = ((IMonster)m_Owner).GetSkillTipRenderer();
            var skillRoot = ((IMonster)m_Owner).GetSkillRoot();
            var dir = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
            dir.Normalize();

            //技能指示器类型
            if (skill.hasSkillTip == 1)
            {

                switch (skill.iSkillTipPrefabID)
                {
                    case 1:

                        //通知扇形区域内的角色躲避
                        List<IHero> heros = GetPlayersInSector(skillRoot.position, dir, 100, skill_range_angle + 5);
                        AddTimer(0.5f, () =>
                        {


                            bool allActorEludeSkill = true;

                            //新需求， 团长自己走去范围
                            var leader = GameManager.Instance.m_Leader;
                            heros.ForEach(h =>
                            {
                                if (h != leader)
                                {
                                    allActorEludeSkill &= h.EludeBossSkill(skillRoot.position, dir, 100, skill_range_angle + 5);
                                }
                            });

                            //存在队员没有躲避技能的情况下 提示团长
                            if (!allActorEludeSkill)
                            {
                                GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_HARM_RED_SCREEN, DEventSourceType.SOURCE_TYPE_ENTITY, 0, null);
                            }
                        });

                        //技能指示器
                        skillRoot.gameObject.SetActive(true);
                        skillRoot.localEulerAngles = new Vector3(0, 0, randomAngle);
                        rangeTipSr.DOColor(new Color(1, 1, 1, 0.5f), 0.5f).SetLoops(4, LoopType.Yoyo).OnComplete(() =>
                        {
                            skillRoot.gameObject.SetActive(false);
                        });
                        yield return new WaitForSeconds(2f);
                        break;
                }
            }

            m_Anim.state.SetAnimation(0, skill.szAnimName, false);
            var trFx = GameEffectManager.Instance.ShowEffect(skill.szVfxResPath, skillRoot.position, Quaternion.Euler(0, 0, randomAngle));
            if (trFx != null)
            {
                trFx.DOMove(skillRoot.position + dir * 100f, 1f).SetEase(Ease.InOutQuad);
            }
            yield return new WaitForSeconds(0.4f);

            //对还没有离开boss区域的玩家进行扣血
            List<IHero> selects = GetPlayersInSector(skillRoot.position, dir, 100, skill_range_angle);
            selects.ForEach(h => h.SetHPDelta(-skill.iValue));
            // Debug.Log("============= Boss AOE SKILL =============");
            // if (selects.Count > 0)
            // {
            //     selects.ForEach(h =>
            //     {
            //         Debug.Log($"Target: {h.id}");
            //         h.SetHPDelta(-skill.iValue);
            //     });
            // }
            // Debug.Log("============= Boss AOE END =============");


            yield return new WaitForSeconds(0.6f);
            var pContext = CameraShakeEventContext.Ins;
            pContext.intensity = 1f;
            pContext.duration = 0.5f;
            pContext.vibrato = 30;
            pContext.randomness = 100f;
            pContext.fadeOut = true;
            GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_CAMERA_SHAKE, DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext);

            yield return new WaitForSeconds(1.4f);
            m_StateMachine.ChangeState<MonsterIdleState>();
        }


        /// <summary>
        /// 随机点名
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        private IEnumerator Load_Skill_500001(cfg_HeroTeamSkills skill)
        {
            m_Anim.state.SetAnimation(0, skill.szAnimName, false);
            yield return null;

            var foesCamp = m_Owner.GetCamp() == CampDef.HERO ? CampDef.MONSTER : CampDef.HERO;
            var foes = LevelManager.Instance.GetActorsByCamp(foesCamp);
            List<IHero> selects = new List<IHero>();
            int tankCount = 0;
            for (int i = 0; i < 3; i++)
            {
                var p = foes.PickRandom();
                var cfg = p.GetCreatureCig();
                if (cfg.HeroClass == HeroClassDef.TANK && tankCount == 2)
                {
                    i--;
                    yield return null;
                }
                selects.Add(p as IHero);
                if (cfg.HeroClass == HeroClassDef.TANK) tankCount++;
            }

            //除了 1类型是治疗， 其它类型如伤害，击飞，控制的，此值都表示额外伤害
            int damage = skill.iType == 1 ? skill.iValue : -skill.iValue;
            damage = CombatUtils.ApplyRandomVariance(damage);

            var bullet_cfg = GameGlobal.GameScheme.HeroTeamBullet_0(105);
            Vector3 pos = m_Owner.GetLockTr().position;
            selects.ForEach(m =>
            {
                var bullet = BulletManager.Instance.Get<EventBullet>(bullet_cfg, pos);
                bullet.SetHarm(-damage);
                bullet.SetSender(m_Owner.id);
                bullet.SetTarget(m);
                bullet.SetCollisionCallback(() =>
                {
                    m.ReceiveBossSelect(pos);
                    m.SetHatred(0);//点名的角色 仇恨值清零
                });
            });

            string szVfx = "Game/HeroTeam/GameResources/Epic Toon FX/Prefabs/Combat/Nova/Standard/NovaFire.prefab";
            GameEffectManager.Instance.ShowEffect(szVfx, m_Owner.GetLockTr().position, Quaternion.identity, 1f);

            yield return new WaitForSeconds(1.4f);
            m_StateMachine.ChangeState<MonsterIdleState>();
        }


        private List<IHero> GetPlayersInSector(Vector3 bossPos, Vector3 bossDir, float radius, float angle)
        {
            var foesCamp = m_Owner.GetCamp() == CampDef.HERO ? CampDef.MONSTER : CampDef.HERO;
            var foes = LevelManager.Instance.GetActorsByCamp(foesCamp);
            float halfAngle = angle * 0.5f;
            List<IHero> result = new List<IHero>();
            foreach (var foe in foes)
            {
                var toPlayer = foe.GetPos() - bossPos;
                if (toPlayer.magnitude > radius) continue;
                if (Vector3.Angle(toPlayer.normalized, bossDir) <= halfAngle)
                {
                    result.Add(foe as IHero);
#if UNITY_EDITOR
                    Debug.DrawLine(bossPos, foe.GetTr().position, Color.red, 2f);
#endif
                }

#if UNITY_EDITOR
                else
                {
                    Debug.DrawLine(bossPos, foe.GetTr().position, Color.green, 2f);
                }
#endif
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