using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using XClient.Common;
using XClient.Entity;

namespace GameScripts.HeroTeam
{

    public class ActorWinState : ActorStateBase
    {


        public override void OnEnter()
        {
            base.OnEnter();

            RestoreAllBlood();

            //离的远的 跑到boss附近去
            if (m_Owner.GetMonsterCfg().HeroClass > HeroClassDef.WARRIOR)
            {
                Run2Boss();
            }
            else
            {
                RecevieBonus();
            }
        }

        //回满血
        private void RestoreAllBlood()
        {

            //模拟一发 贤者的子弹
            var cfg_sageBullet = GameGlobal.GameScheme.HeroTeamBullet_0(102);
            var pos = MonsterSystem.Instance.BossDeathPosition + Vector3.up * 2.4f;
            var bullet = BulletManager.Instance.Get<Bullet>(cfg_sageBullet, pos);
            bullet.SetHarm(-999999);//98# 加满！不差$
            bullet.SetSender(m_Owner.GetCreatureEntity().id);
            bullet.SetTarget(m_Owner.GetCreatureEntity() as IMonster);
        }

        //领取奖品
        private void RecevieBonus()
        {
            Debug.Log("领取奖励");

        }

        //跑到Boss边上
        private void Run2Boss()
        {
            m_Anim.AnimationState.SetAnimation(0, m_ActorAnimConfig.szMove, true);

            var bossPos = MonsterSystem.Instance.BossDeathPosition;
            var pos = m_Owner.GetTr().position;

            var target = (bossPos - pos) * 0.618f + pos;
            float moveTime = Mathf.Clamp01((bossPos - pos).magnitude / 14.965f) * 3f;

            m_Owner.GetTr().DOKill();
            m_Owner.GetTr().DOMove(target, moveTime).OnComplete(() =>
            {
                m_Anim.AnimationState.SetAnimation(0, m_ActorAnimConfig.szIdle, true);
                RecevieBonus();
            });
        }

    }

}