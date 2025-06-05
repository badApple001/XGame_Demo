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
            if (m_Owner.GetConfig().HeroClass > HeroClassDef.WARRIOR)
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
            bullet.SetSender(m_Owner.id);
            bullet.SetTarget(m_Owner);
        }

        //领取奖品
        private void RecevieBonus()
        {
            Debug.Log("领取奖励");

            //胜利动画
            m_Anim.state.Complete += entry =>
                            {
                                m_Anim.AnimationState.SetAnimation(0, m_ActorAnimConfig.szIdle, true);
                            };
            m_Anim.AnimationState.SetAnimation(0, m_ActorAnimConfig.szWin, false);


            //胜利表情
            string faceResPath = "Game/HeroTeam/GameResources/Prefabs/Game/Emoji/EmojiStarstruck.prefab";
            var face = GameEffectManager.Instance.ShowEffect(faceResPath, m_Owner.GetTr().position, Quaternion.identity, 2f);
            face.SetParent(m_Owner.GetTr(), false);
            face.localPosition =m_Owner.GetFaceTr().localPosition;

            //TODO: 后续配置表
            string fxResPath = "Game/HeroTeam/GameResources/Prefabs/Game/Fx/PowerOrbYellow.prefab";

            var fx = GameEffectManager.Instance.ShowEffect(fxResPath, m_Owner.GetTr().position, Quaternion.Euler(243f, 0, 0), 4);
            fx.SetParent(m_Owner.GetTr());
            fx.localPosition = Vector3.up * 5.35f;
            fx.localScale = Vector3.one * 5;
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
                RecevieBonus();
            });
        }

    }

}