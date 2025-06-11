using UnityEngine;

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
        protected string szHitPrefab;
        protected Transform target;
        protected ISpineCreature targetEntity;
        protected float speed;
        protected int harm;
        protected ulong sender;
        protected float radius;
        protected float maxLifeTime;//最大飞行时间
        protected Vector3 pos;
        protected bool isExpired;
        protected int poolId;

        public virtual void Init(GameObject objRef)
        {
            transform = objRef.transform;
        }

        public virtual void Active(Vector3 newPos)
        {
            pos = newPos;
            if (transform != null)
            {
                // var ps = transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
                // ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                transform.position = pos;
                // ps.Clear(true);
                // ps.Play(true);
            }
            //重置飞行时间
            //子类继承可以自行修改
            maxLifeTime = 3f;
            isExpired = false;
        }

        //默认: 圆形碰撞检测
        //可继承后扩展
        protected virtual bool CheckCollision() => Vector3.Distance(target.position, transform.position) <= 1e-1;

        public virtual bool IsExpired() => isExpired;

        public virtual void ClearState()
        {
            target = null;
            isExpired = true;
        }


        /// <summary>
        /// 可以通过继承后扩展其它功能，默认就是追踪子弹
        /// </summary>
        public virtual void Fly()
        {
            if (!isExpired && target != null && transform != null)
            {
                maxLifeTime -= TimeUtils.DeltaTime;
                if (maxLifeTime <= 0f)
                {
                    isExpired = true;
                    OnExpired();
                    return;
                }
                Vector3 dir = speed * TimeUtils.DeltaTime * (target.position - transform.position).normalized;
                pos += dir;
                transform.position = pos;

                if (CheckCollision())
                {
                    OnCollision();
                }
            }
        }

        protected virtual void OnExpired()
        {

        }

        protected virtual void OnCollision()
        {
            isExpired = true;

            if (targetEntity.IsDie())
            {
                // Debug.Log("目标已经死亡~");
                return;
            }

            BulletManager.Instance.ShowEffect(szHitPrefab, target.position);
            targetEntity.SetHPDelta(-harm);
        }

        public Transform GetTr() => transform;

        public void SetTarget(ISpineCreature target)
        {
            targetEntity = target;
            this.target = target.GetLockTr();
        }

        public void SetConfig(cfg_HeroTeamBullet cfg)
        {
            radius = cfg.fRadius;
            speed = cfg.fSpeed;
            szHitPrefab = cfg.szHitEffect;
            poolId = cfg.iID;
            transform.GetChild(0).localScale = cfg.fScale * Vector3.one;
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
