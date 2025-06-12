using System;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XClient.Entity;
using XGame.Entity;

namespace GameScripts.HeroTeam
{

    public enum ActorState
    {
        Spawn,
        Normal,
        Dying,
        Release
    }


    public abstract class Actor : VisibleEntity, IActor
    {

        /// <summary>
        /// 被锁定的目标节点
        /// </summary>
        protected Transform m_trLockTarget;


        /// <summary>
        /// 状态
        /// </summary>
        protected ActorState m_state = ActorState.Spawn;


        /// <summary>
        /// 资源路径
        /// </summary>
        protected string m_resPath;

        /// <summary>
        /// int 属性字典
        /// </summary>
        protected Dictionary<int, int> m_dicIntProp = new Dictionary<int, int>();

        /// <summary>
        /// float 字典
        /// </summary>
        protected Dictionary<int, float> m_dicFloatProp = new Dictionary<int, float>();

        /// <summary>
        /// 父节点
        /// </summary>
        protected Transform m_trParent = null;

        /// <summary>
        /// 定时器协程组
        /// </summary>
        protected List<Coroutine> m_arrCoroutineGroup = new List<Coroutine>();

        /// <summary>
        /// 搭载的MonoType
        /// </summary>
        protected List<Type> m_MonoType = new List<Type>();


        /// <summary>
        /// 模型资源加载完成后回调
        /// </summary>
        protected Action m_modelLoadedCallback = null;

        /// <summary>
        /// 位置
        /// </summary>
        protected Vector3 m_wolrdPos;

        /// <summary>
        /// 角度
        /// </summary>
        protected Quaternion m_Rotation;

        protected override void OnInit(object context)
        {
            base.OnInit(context);
            m_state = ActorState.Spawn;
            var cfg = GetActorCig();
            if (cfg == null)
            {
                Debug.LogError("读取失败的角色配置 configId=" + configId);
            }
            else
            {
                //资源
                m_resPath = cfg.szResPath;
                //名称生成
                name = cfg.szName;
            }
            if (string.IsNullOrEmpty(m_resPath))
            {
                Debug.LogError("资源路径没有配置 configId=" + configId);
            }
        }

        protected override void OnAfterInit(object context)
        {
            base.OnAfterInit(context);

            var ctx = context as NetEntityShareInitContext;
            CreateActorContext createActorContext = (CreateActorContext)(ctx.localInitContext);
            if (createActorContext.MonoTypes.Count > 0)
            {
                m_MonoType.AddRange(createActorContext.MonoTypes);
            }
            m_trParent = createActorContext.parent;
            SetPos(createActorContext.worldPos);
            SetRotation(Quaternion.Euler(createActorContext.eulerAngles));
        }

        public override string GetResPath()
        {
            return m_resPath;
        }

        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            base.OnReceiveEntityMessage(id, data);

            if (id == EntityMessageID.ResLoaded)
            {
                if (m_MonoType.Count > 0)
                {
                    m_MonoType.ForEach(type =>
                    {
                        if (transform.gameObject.GetComponent(type) == null)
                        {
                            transform.gameObject.AddComponent(type);
                        }
                    });
                }
#if UNITY_EDITOR
                if (transform.TryGetComponent<ActorPartInspector>(out var componet))
                {
                    componet.BindEntity(this);
                }
#endif
                OnInstantiated();
            }
        }

        protected virtual void OnInstantiated()
        {
            //由子类类处理, 等会我会在这个类声明成抽象类,必须要继承实现这个类才行
            // transform.position = m_wolrdPos;
            // transform.rotation = m_Rotation;

            m_trLockTarget = transform.Find("LockTarget");
            if (m_trLockTarget == null)
                m_trLockTarget = transform;
        }


        public override void OnUpdate()
        {
            base.OnUpdate();
            OnUpdatePart();
        }

        public Transform GetTr() => transform;

        public cfg_Actor GetActorCig() => GameGlobal.GameScheme.Actor_0(configId);

        public virtual Vector3 GetPos() => m_wolrdPos;

        public virtual void SetPos(Vector3 pos)
        {
            if (!pos.Equals(m_wolrdPos))
            {
                m_wolrdPos = pos;
                transform?.SetPosition(pos);
            }
        }

        public virtual void SetRotation(Quaternion rotation)
        {
            if (!rotation.Equals(m_Rotation))
            {
                m_Rotation = rotation;
                if (transform != null)
                    transform.rotation = rotation;
            }
        }

        public virtual Quaternion GetRotation()
        {
            return m_Rotation;
        }

        public void SetPos(float[] float3Pos)
        {
            Vector3 pos = new Vector3().FromArray(float3Pos);
            SetPos(pos);
        }

        public Transform GetLockTr()
        {
            return m_trLockTarget;
        }

        public ActorState GetState()
        {
            return m_state;
        }

        public virtual void SetState(ActorState newState)
        {
            m_state = newState;
        }

        public int GetIntAttr(int propID)
        {
            int val = 0;
            m_dicIntProp.TryGetValue(propID, out val);
            return val;
        }

        public void SetIntAttr(int propID, int val)
        {
            m_dicIntProp[propID] = val;
        }

        public float GetFloatAttr(int propID)
        {
            float val = 0;
            m_dicFloatProp.TryGetValue(propID, out val);
            return val;
        }

        public void SetFloatAttr(int propID, float val)
        {
            m_dicFloatProp[propID] = val;
        }

        public void SetResLoadedCallback(Action callback)
        {
            m_modelLoadedCallback = callback;
        }

        protected override void OnReset()
        {
            base.OnReset();
            m_resPath = null;
            m_dicIntProp.Clear();
            m_MonoType.Clear();
            ClearTimes();
        }

        public void ClearTimes()
        {
            if (m_arrCoroutineGroup.Count > 0)
            {
                GameManager.Instance.ClearTimers(m_arrCoroutineGroup);
                m_arrCoroutineGroup.Clear();
            }
        }

        protected void AddTimer(float delay, Action callback)
        {
            if (callback == null) throw new Exception("ActorModel->AddTimer Callback is null");
            m_arrCoroutineGroup.Add(GameManager.Instance.AddTimer(delay, callback));
        }
    }

}