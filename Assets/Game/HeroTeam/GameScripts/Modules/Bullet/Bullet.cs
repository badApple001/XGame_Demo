using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Entity;
using XGame;
using XGame.Asset;

namespace GameScripts.HeroTeam
{
    /// <summary>
    /// 子弹基类
    /// </summary>
    public class Bullet : IBullet
    {

        //这里的命名规则 我转为遵守unity内部的
        //主要原因还是迁就开发者的习惯
        //其它类依旧采用 m_nXXX 的规则
        protected Transform transform;
        protected GameObject gameObject;
        protected GameObject hitPrefab;
        protected Transform target;
        protected IMonster targetEntity;
        protected float speed;
        protected int harm;
        protected ulong sender;
        protected float radius;
        protected int poolId;


        public virtual void Init(GameObject objRef)
        {
            gameObject = objRef;
            transform = gameObject.transform;
        }

        public virtual void Active(Vector3 newPos)
        {
            if (transform != null)
            {
                // var ps = transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
                // ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                transform.position = newPos;
                // ps.Clear(true);
                // ps.Play(true);
            }
        }

        //默认: 圆形碰撞检测
        //可继承后扩展
        public virtual bool CheckCollision() => Vector3.Distance(target.position, transform.position) <= 1e-1;

        public virtual void ClearState()
        {
            target = null;
        }
        public virtual void Fly()
        {
            if (target != null)
            {
                Vector3 dir = speed * Time.deltaTime * (target.position - transform.position).normalized;
                transform.position += dir;
            }
        }

        public virtual void OnCollision()
        {
            BulletManager.Instance.ShowEffect(hitPrefab, target.position);
            targetEntity.SetHPDelta(-harm);
        }

        public Transform GetTr() => transform;

        public void SetTarget(IMonster target)
        {
            targetEntity = target;
            this.target = target.GetLockTr();
        }

        public void SetConfig(cfg_HeroTeamBullet cfg)
        {
            radius = cfg.fRadius;
            speed = cfg.fSpeed;
            poolId = cfg.iID;
            transform.GetChild(0).localScale = cfg.fScale * Vector3.one;
            var resLoader = XGameComs.Get<IGAssetLoader>();
            uint handle = 0;
            hitPrefab = (GameObject)resLoader.LoadResSync<GameObject>(cfg.szHitEffect, out handle);
        }

        public int GetPoolId() => poolId;

        public void SetHarm(int harm)
        {
            this.harm = harm;
        }

        public void SetSender(ulong entityId)
        {
            this.sender = entityId;
        }


    }
}
