using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameScripts.HeroTeam
{

    public class EventBullet : Bullet
    {
        private Action m_OnCollisionCallback;  //碰撞后的回调
        private Action m_OnFlyTimeoutCallback; //飞行超过了时间限制回调


        protected override void OnCollision()
        {
            base.OnCollision();
            m_OnCollisionCallback?.Invoke();
        }


        protected override void OnExpired()
        {
            m_OnFlyTimeoutCallback?.Invoke();
        }

        public void SetCollisionCallback(Action callback)
        {
            m_OnCollisionCallback = callback;
        }

        public void SetExpiredCallback(Action callback)
        {
            m_OnFlyTimeoutCallback = callback;
        }

        public override void ClearState()
        {
            base.ClearState();
            m_OnCollisionCallback = null;
        }

    }

}