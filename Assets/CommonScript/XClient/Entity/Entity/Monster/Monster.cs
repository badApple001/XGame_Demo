/*******************************************************************
** �ļ���:	Monster.cs
** ��  Ȩ:	(C) ��������
** ������:	���¼�
** ��  ��:	2024.6.25
** ��  ��:	1.0
** ��  ��:	
** Ӧ  ��:  ����ʵ����

**************************** �޸ļ�¼ ******************************
** �޸���: 
** ��  ��: 
** ��  ��: 
********************************************************************/

using System.Collections.Generic;
using DG.Tweening;
using GameScripts.HeroTeam;
using Spine.Unity;
using UnityEngine;
using XClient.Common;
using XGame.Entity;
using XGame.Entity.Part;

namespace XClient.Entity
{
    public class Monster : VisibleEntity, IMonster
    {
        public static readonly Vector3 s_vLeftScale = new Vector3(-1, 1, 1);

        public static readonly Vector3 s_vRightScale = Vector3.one;

        //��Դ·��
        private string m_resPath;

        //�ƶ��ٶ�
        private float m_speed;

        public int ceatureId;
        //�ѷ��б�
        private List<ulong> m_listFriendCamps;

        //�з��б�
        private List<ulong> m_listEnemyCamps;

        //�Ƿ񱾵ض���
        private bool m_bLocalObject = false;
        private bool m_bFaceLeft = false;
        private Quaternion m_rotate = Quaternion.identity;
        private Transform m_parent;

        //��������ͬ������
        private MonsterDataPart m_dataPart;

        //Ԥ���岿��
        private PrefabPart m_prefabPart;


        //Ԥ���岿��
        private SpineSkinPart m_skinPart;

        //�����޸���
        private IATTRModifier m_attrModifier;

        //int�����б�
        private Dictionary<int, int> m_dicProp = new Dictionary<int, int>();

        private List<Vector3> m_listRoads = new List<Vector3>();

        private bool m_bIsBoss = false;

        private int m_nHatred = 0;

        public override Vector3 position => GetPos();

        private Transform transform;


        private Transform m_trLockTarget;

        private GameObject m_refFace;
        public Transform GetFaceTr() => m_refFace?.transform;

        /// <summary>
        /// Monster共享一个 生成下标
        /// 此下标用来独立每个角色的名称: {heroClass}{m_byteShareSpawnIndex++}
        /// 后续策划如果有想法就增加配表
        /// </summary>
        private static byte m_byteShareSpawnIndex = 0;


        /// <summary>
        /// 总共造成的伤害，如果是奶职业，就是治疗总量
        /// </summary>
        private int m_totalHarm = 0;

        /// <summary>
        /// 角色职业
        /// </summary>
        private int m_HeroCls = 0;

        public ulong GetCamp()
        {
            return (ulong)m_dataPart.m_camp.Value;
        }

        public Vector3 GetForward()
        {
            if (m_prefabPart.transform == null)
            {
                return m_dataPart.m_forward.Value;
            }

            return m_prefabPart.transform.forward;
        }

        public Vector3 GetPos()
        {
            if (m_prefabPart.transform == null)
            {
                return m_dataPart.m_pos.Value;
            }

            return m_prefabPart.transform.position;
        }

        public Vector3 GetLocalPos()
        {
            if (m_prefabPart.transform == null)
            {
                return m_dataPart.m_localPos.Value;
            }

            return m_prefabPart.transform.localPosition;
        }

        public bool GetFaceLeft()
        {
            return m_bFaceLeft;
        }

        public void SetForward(ref Vector3 forward)
        {
            if (m_prefabPart.transform == null)
            {
                m_dataPart.m_forward.Value = forward;
                return;
            }

            m_prefabPart.transform.forward = forward;
        }

        public void SetParent(Transform parent)
        {
            if (m_prefabPart.transform == null)
            {
                m_parent = parent;
                return;
            }

            m_prefabPart.transform.BetterSetParent(parent);
        }

        public void SetFace(bool faceLeft)
        {
            if (m_prefabPart.transform == null)
            {
                m_bFaceLeft = faceLeft;
                return;
            }

            m_prefabPart.transform.localScale = m_bFaceLeft ? s_vLeftScale : s_vRightScale;
        }

        public void SetRotation(Quaternion rotate)
        {
            if (m_prefabPart.transform == null)
            {
                m_rotate = rotate;
                return;
            }


            m_prefabPart.transform.rotation = rotate;
        }

        public void SetPos(ref Vector3 pos)
        {
            if (m_prefabPart.transform == null)
            {
                m_dataPart.m_pos.Value = pos;
                m_dataPart.m_isLocalPos.Value = false;
                return;
            }

            m_prefabPart.transform.position = pos;
        }

        public void SetLocalPos(ref Vector3 localPos)
        {
            if (m_prefabPart.transform == null)
            {
                m_dataPart.m_localPos.Value = localPos;
                m_dataPart.m_isLocalPos.Value = true;
                return;
            }

            m_prefabPart.transform.localPosition = localPos;
        }


        public override string GetResPath()
        {
            return m_resPath;
        }

        protected override void OnInit(object context)
        {
            base.OnInit(context);

            cfg_Monster cfg = GameGlobal.GameScheme.Monster_0((uint)configId);

            if (cfg == null)
            {
                Debug.LogError("�����ڵ�CreatureView���� configId=" + configId);
            }
            else
            {
                config = cfg;
                var ctx = context as NetEntityShareInitContext;
                CreateMonsterContext createMonsterContext = (CreateMonsterContext)(ctx.localInitContext);
                if (!string.IsNullOrEmpty(createMonsterContext.resPath))
                {
                    m_resPath = createMonsterContext.resPath;
                }
                else
                {
                    m_resPath = cfg.szResPath;
                }
                ceatureId = createMonsterContext.creatureID;


                if (string.IsNullOrEmpty(m_resPath))
                {
                    Debug.LogError("�������Դ·��Ϊ�� configId=" + configId);
                }


                m_speed = 0;
            }

            //��ȡ�޸���
            m_attrModifier = MonsterSystem.Instance.GetAttrModified();

            //SetSpeed((int)cfg.fbaseSpeed);

            //职业
            m_HeroCls = cfg.HeroClass;

            //名称生成
            var cfg_monster = (cfg_Monster)config;
            name = cfg_monster.szName + (char)('A' + m_byteShareSpawnIndex++);
            Debug.Log(name);
        }

        protected override void OnAfterInit(object context)
        {
            base.OnAfterInit(context);

            //����ԭ��
            m_dataPart = GetPart<MonsterDataPart>();
            m_prefabPart = GetPart<PrefabPart>();
            //m_skinPart = GetPart<SpineSkinPart>();
            var ctx = context as NetEntityShareInitContext;
            m_bLocalObject = (ctx.isInitFromNet == false);
            if (m_bLocalObject)
            {
                CreateMonsterContext createMonsterContext = (CreateMonsterContext)(ctx.localInitContext);
                m_dataPart.m_camp.Value = (long)createMonsterContext.camp;
                m_listFriendCamps = createMonsterContext.listFriendCamps;
                m_listEnemyCamps = createMonsterContext.listEnemyCamps;

                m_parent = MonsterSystem.Instance.GetEntityRoot();
                m_bFaceLeft = createMonsterContext.faceLeft;
                m_rotate = Quaternion.identity;
                m_dataPart.m_hp.Value = createMonsterContext.baseHP;
                m_dataPart.m_maxHp.Value = createMonsterContext.maxHP;
                m_dataPart.m_isLocalPos.Value = createMonsterContext.isLocalPos;
                if (createMonsterContext.isLocalPos)
                {
                    m_dataPart.m_localPos.Value = createMonsterContext.pos;
                }
                else
                {
                    m_dataPart.m_pos.Value = createMonsterContext.pos;
                }

                m_dataPart.m_forward.Value = createMonsterContext.forward;
                m_dataPart.m_visibleType.Value = (int)createMonsterContext.visibleType;
            }
        }

        protected override void OnReset()
        {
            base.OnReset();
            m_attrModifier = null;
            m_parent = null;
            m_bFaceLeft = false;
            m_bIsBoss = false;
            m_totalHarm = 0;
        }

        public override void OnUpdate()
        {
            base.OnUpdatePart();
        }

        public bool IsDie()
        {
            //return false;
            return m_dataPart.m_hp.Value <= 0;
        }

        public int GetHP()
        {
            if (m_attrModifier != null)
            {
                return m_attrModifier.OnModifiedHP(this, (int)m_dataPart.m_hp.Value);
            }

            return (int)m_dataPart.m_hp.Value;
        }

        public int GetMaxHP()
        {
            return (int)m_dataPart.m_maxHp.Value;
        }

        public void SetMaxHP(int maxHp)
        {
            m_dataPart.m_maxHp.Value = maxHp;
        }

        public void SetHPDelta(int hp)
        {
            //if (m_resPath.IndexOf("Wall") >= 0)
            //{
            //    m_dataPart.m_hp.IsDebug = true;
            //    m_dataPart.m_hp.Name = "WallHp";
            //}


            m_dataPart.m_hp.RemoteValueDelta += hp;
            m_dataPart.m_hp.Value += hp;

            if (hp < 0 && !IsBoos() && !IsDie())
            {
                // GetSkeletonAnimation().AnimationState.SetAnimation(0, "hit2", false);
                var actor = GetComponent<Actor>();
                var skel = actor.GetSkeleton();
                if (null != actor && skel != null)
                {

                    var cfg = actor.GetAnimConfig();
                    if (cfg == null)
                    {
                        Debug.Log($"找不到动画: {((cfg_Monster)config).nID}");
                        return;
                    }
                    skel.state.SetAnimation(1, cfg.szHit, false);
                    GameManager.instance.AddTimer(0.6f, () =>
                    {
                        skel.state.ClearTrack(1);
                        skel.state.SetAnimation(0, actor.GetAnimConfig().szIdle, true);
                    });
                }
            }

            //广播boss的生命值
            if (IsBoos())
            {
                var pContext = BossHpEventContext.Ins;
                pContext.Health = GetHP() * 1.0f / GetMaxHP();
                GameGlobal.EventEgnine.FireExecute(DHeroTeamEvent.EVENT_BOSS_HP_CHANGED, GameScripts.HeroTeam.DEventSourceType.SOURCE_TYPE_ENTITY, 0, pContext);
            }
        }

        public float GetSpeed()
        {
            if (m_attrModifier != null)
            {
                return m_attrModifier.OnModifiedSpeed(this, m_speed);
            }

            return m_speed;
        }

        public void SetSpeed(float speed)
        {
            m_speed = speed;
        }

        public override T GetComponent<T>() where T : class
        {
            if (m_prefabPart != null && m_prefabPart.gameObject != null)
            {
                return m_prefabPart.gameObject.GetComponent<T>();
            }

            return null;
        }

        public void GetComponents<T>(List<T> list) where T : class
        {
            if (m_prefabPart != null && m_prefabPart.gameObject != null)
            {
                m_prefabPart.gameObject.GetComponents<T>(list);
            }

            list.Clear();
        }


        public override void OnReceiveEntityMessage(uint id, object data = null)
        {
            if (id == EntityMessageID.ResLoaded)
            {
                m_prefabPart.gameObject.BetterSetActive(true);
                Vector3 pos = m_dataPart.m_pos.Value;

                if (m_bLocalObject == false)
                {
                    pos = m_attrModifier.WorldPositionToBattlePosition(pos, false);
                }

                if (m_dataPart.m_isLocalPos.Value)
                {
                    pos = m_dataPart.m_localPos.Value;
                }

                SetParent(m_parent);

                SetFace(m_bFaceLeft);
                Vector3 forward = m_dataPart.m_forward.Value;
                if (m_dataPart.m_isLocalPos.Value)
                {
                    SetLocalPos(ref pos);
                }
                else
                {
                    SetPos(ref pos);
                }

                SetForward(ref forward);
                // SetRotation(m_rotate);

                // m_prefabPart.ResetColor();
                IDReco reco = m_prefabPart.gameObject.GetComponent<IDReco>();
                if (null == reco)
                {
                    reco = m_prefabPart.gameObject.AddComponent<IDReco>();
                }

                reco.camp = (ulong)m_dataPart.m_camp.Value;
                reco.entID = base.id;
                // reco.hitTrans = m_prefabPart.gameObject.GetComponent<SkillCompontBase>()
                // .GetSkillEffNode(ECreaturePos.Center).transform;
                reco.entType = base.type;

                //���ض�����Ա���Ҳ�ܹ���
                if (m_bLocalObject)
                {
                    reco.canAttack = true;
                    reco.beAttack = true;
                    reco.listFriendCamps = m_listFriendCamps;
                    reco.listEnemyCamps = m_listEnemyCamps;
                }
                else
                {
                    //Զ�̶����ܱ�����,�����ܰ���
                    reco.canAttack = false;
                    reco.beAttack = true;
                }

                InnerStateTrans();
                /*
                BuffBaseComponent buffComponent = m_prefabPart.gameObject.GetComponent<BuffBaseComponent>();
                if (null != buffComponent)
                {
                     buffComponent.Awake();
                    // buffComponent.effectActionCreate = KingBuffEffectCreator.Instance;
                    buffComponent.AddBuff(10004);
                }
                */

                Actor actor = m_prefabPart.gameObject.GetComponent<Actor>();
                if (null != actor)
                {
                    actor.SetMonsterCfg((cfg_Monster)config);
                    actor.SetCreatureEntity(this);
                }

                var bar = m_prefabPart.gameObject.GetComponentInChildren<HpBar>();
                if (null != bar)
                {
                    bar.SetEntity(this);
                }

                transform = m_prefabPart.transform;
                m_trLockTarget = transform.Find("LockTarget");
                if (m_trLockTarget == null)
                    m_trLockTarget = transform;

                m_refFace = transform.Find("Face")?.gameObject;
            }
        }

        public int GetIntAttr(int propID)
        {
            int val = 0;
            m_dicProp.TryGetValue(propID, out val);

            return val;
        }

        public void SetIntAttr(int propID, int val)
        {
            if (m_dicProp.ContainsKey(propID) == false)
            {
                m_dicProp.Add(propID, val);
            }
            else
            {
                m_dicProp[propID] = val;
            }
        }

        public void SetATTRModifier(IATTRModifier modifier)
        {
            m_attrModifier = modifier;
        }

        public CREATURE_VISIBLE_TYPE GetVisibleType()
        {
            return (CREATURE_VISIBLE_TYPE)m_dataPart.m_visibleType.Value;
        }

        public bool IsLoaded()
        {
            return m_prefabPart.gameObject != null;
        }

        public Transform GetTransform()
        {
            return m_prefabPart.transform;
        }

        public SkeletonGraphic GetSkeltonGraphic()
        {
            return GetComponent<SpineComponent>().skeGra;
        }

        public SkeletonAnimation GetSkeletonAnimation()
        {
            return GetComponent<SpineComponent>().skeAni;
        }

        private Transform m_stateTrans;

        public void SetStateTrans(Transform hpTrans)
        {
            m_stateTrans = hpTrans;
            InnerStateTrans();
        }

        private void InnerStateTrans()
        {
            if (null != m_stateTrans && m_prefabPart.transform != null)
            {
                m_stateTrans.BetterSetParent(m_prefabPart.transform);
                m_stateTrans.localPosition = new Vector3(0, 20);
                m_stateTrans.localScale = m_bFaceLeft ? s_vLeftScale : s_vRightScale;
            }
        }

        public void SetRoad(List<Vector3> road)
        {
            m_listRoads = road;
        }

        public List<Vector3> GetRoad()
        {
            return m_listRoads;
        }

        public void SetBoos()
        {
            m_bIsBoss = true;
        }

        public bool IsBoos()
        {
            return m_bIsBoss;
        }

        public int GetHatred()
        {
            return m_nHatred;
        }

        public void SetHatred(int value)
        {
            m_nHatred = value;
        }

        public Transform GetTr() => transform;

        public Transform GetLockTr() => m_trLockTarget;

        public void EludeBossSkill(Vector3 bossPos, Vector3 bossDir, float radius, float angleDeg)
        {

            if (null != m_refFace)
            {
                m_refFace.SetActive(true);
                GameManager.instance.AddTimer(2.5f, () => m_refFace.SetActive(false));
            }


            DodgeAndReturn(transform, bossPos, bossDir);
        }

        Vector2 GetSideDodgeDirection(Vector2 bossDir, Vector2 toNpc)
        {
            // 取 bossDir 的垂直方向（法向量）
            Vector2 side = Vector2.Perpendicular(bossDir).normalized;

            // 决定往左还是往右闪，依据 npc 在哪一侧
            float sign = Mathf.Sign(Vector2.Dot(side, toNpc));
            return side * sign;
        }

        public void DodgeAndReturn(Transform tr, Vector2 bossPos, Vector2 bossDir, float dodgeDistance = 7f, float waitTime = 1.5f, float duration = 1f)
        {
            Vector2 toNpc = (Vector2)tr.position - bossPos;
            Vector2 dodgeDir = GetSideDodgeDirection(bossDir, toNpc);

            Vector3 startPos = tr.position;
            Vector3 dodgeTarget = startPos + (Vector3)(dodgeDir * dodgeDistance);

            var anim = tr.GetComponent<Actor>().GetSkeleton();
            var animConfig = tr.GetComponent<Actor>().GetAnimConfig();
            anim.state.SetAnimation(1, animConfig.szHit, true);
            tr.DOKill();
            tr.DOMove(dodgeTarget, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // anim.state.SetAnimation(1, animConfig.szIdle, true);
                    // DOVirtual.DelayedCall(waitTime, () =>
                    // {
                    //     anim.state.SetAnimation(1, animConfig.szMove, true);
                    //     tr.DOMove(startPos, duration).SetEase(Ease.InQuad).OnComplete(() =>
                    //     {
                    //         anim.state.ClearTrack(1);
                    //         anim.state.SetAnimation(0, animConfig.szIdle, true);
                    //     });
                    // });
                    GameManager.instance.AddTimer(waitTime, () =>
                    {
                        anim.state.SetAnimation(1, animConfig.szMove, true);
                        tr.DOMove(startPos, duration).SetEase(Ease.InQuad).OnComplete(() =>
                        {
                            anim.state.ClearTrack(1);
                            anim.state.SetAnimation(0, animConfig.szIdle, true);
                        });
                    });
                });
        }

        public void ReceiveBossSelect(Vector3 bossPos)
        {
            Vector3 startPos = transform.position;
            float repulseDistance = 10f;
            Vector3 repulseDir = (transform.position - bossPos).normalized;
            Vector3 repulseTarget = startPos + (Vector3)(repulseDir * repulseDistance);
            transform.DOKill();
            transform.DOMove(repulseTarget, 0.3f)
               .SetEase(Ease.OutQuad)
               .OnComplete(() =>
               {
                   //    DOVirtual.DelayedCall(0.5f, () =>
                   //    {
                   //        transform.DOMove(startPos, 0.6f).SetEase(Ease.OutCirc);
                   //    });
                   GameManager.instance.AddTimer(0.5f, () =>
                                     {
                                         transform.DOMove(startPos, 0.6f).SetEase(Ease.OutCirc);
                                     });
               });
            transform.DOScale(1.4f, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
               {
                   GameManager.instance.AddTimer(0.5f, () =>
                  {
                      transform.DOScale(1, 0.6f).SetEase(Ease.OutCirc);
                  });
               });

        }


        public void RecordHarm(int addHarm)
        {
            m_totalHarm += addHarm;
        }

        public int GetTotalHarm() => m_totalHarm;


        public int GetHeroCls() => m_HeroCls;
    }
}