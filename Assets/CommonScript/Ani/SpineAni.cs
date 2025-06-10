/*******************************************************************
** �ļ���:	SpineAni.cs
** ��  Ȩ:	(C) ��������
** ������:	���¼�
** ��  ��:	2024.9.03
** ��  ��:	1.0
** ��  ��:	
** Ӧ  ��:  ����spine���Ŷ���

**************************** �޸ļ�¼ ******************************
** �޸���: �ʱ���
** ��  ��: 2024.12.03
** ��  ��: ֧��SkeletonGraphic
********************************************************************/


using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Network;
using static GameScripts.SpineAni;

namespace GameScripts
{
    public class SpineAni : NetObjectBehaviour<SpineAniData>
    {
        public class SpineAniData : MonoNetObject
        {
            //�����������а汾��
            public NetVarLong m_version;

            //���ڲ��ŵĶ���
            public NetVarString m_aniName;

            //�Ƿ�ѭ��
            public NetVarBool m_loop;

            //�Ƿ�ͣ�����һ������
            public NetVarBool m_stay;


            protected override void OnSetupVars()
            {
                //IsDebug = true;
                m_version = SetupVar<NetVarLong>("m_version");
                m_aniName = SetupVar<NetVarString>("m_aniName");
                m_loop = SetupVar<NetVarBool>("m_loop");
                m_stay = SetupVar<NetVarBool>("m_stay");
                m_loop.Value = false;
                m_stay.Value = false;
                m_version.Value = 0;
            }
        }

        //2D��������
        public SkeletonAnimation skeletonAnimation;
        public SkeletonGraphic skeletonGraphic;
        private IAnimationStateComponent skeletonAnimationState;

        //Ĭ�϶���
        public string defaultAniName = "idle";

        //��ǰ���Ŷ����İ汾��
        private long m_playVersion = 0;
        //add by ������
        private bool isAddEvent = false;

        // Start is called before the first frame update
        void Start()
        {
            if (null == skeletonAnimation && null == skeletonGraphic)
            {
                skeletonAnimation = this.GetComponentInChildren<SkeletonAnimation>();
            }

            if (null == skeletonAnimation && null == skeletonGraphic)
            {
                skeletonGraphic = this.GetComponentInChildren<SkeletonGraphic>();
            }

            if (null != skeletonAnimation)
            {
                // skeletonAnimation.freeze = false;
                skeletonAnimationState = skeletonAnimation as IAnimationStateComponent;
            }

            if (null != skeletonGraphic)
            {
                skeletonGraphic.freeze = false;
                skeletonAnimationState = skeletonGraphic as IAnimationStateComponent;
            }

            if (skeletonAnimationState != null && skeletonAnimationState.AnimationState != null)
            {
                skeletonAnimationState.AnimationState.Complete += completeEvent;
                isAddEvent = true;
            }
        }

        // Update is called once per frame
        protected override void OnUpdate()
        {
            if (m_playVersion != NetObj.m_version.Value)
            {
                m_playVersion = NetObj.m_version.Value;

                if (skeletonAnimationState == null || skeletonAnimationState.AnimationState == null)
                {
                    return;
                }
                if (!isAddEvent)
                {
                    skeletonAnimationState.AnimationState.Complete += completeEvent;
                    isAddEvent = true;
                }
                if (string.IsNullOrEmpty(NetObj.m_aniName.Value) == false)
                {
                    if (skeletonAnimationState.AnimationState.Data.SkeletonData
                            .FindAnimation(NetObj.m_aniName.Value) == null)
                    {
                        Debug.LogError("�����ڵĶ���: " + NetObj.m_aniName.Value);
                        return;
                    }

                    /*
                    if(NetObj.m_aniName.Value!="move")
                    {
                        Debug.LogError("���Ŷ���: " + this.gameObject.GetHashCode() + "��������" + NetObj.m_aniName.Value);
                    }*/


                    skeletonAnimationState.AnimationState.SetAnimation(0, NetObj.m_aniName.Value, NetObj.m_loop.Value);

                    /*
                    if(skeletonAnimation.AnimationState.GetCurrent(0).TrackTime>0)
                    {
                        Debug.LogError("skeletonAnimation.AnimationState.GetCurrent(0).TrackTime==" + skeletonAnimation.AnimationState.GetCurrent(0).TrackTime);
                    }
                    */

                    skeletonAnimationState.AnimationState.GetCurrent(0).TrackTime = 0;
                }
            }
        }

        private void OnDestroy()
        {
            if (skeletonAnimationState != null && skeletonAnimationState.AnimationState != null)
            {
                skeletonAnimationState.AnimationState.Complete -= completeEvent;
            }
            isAddEvent = false;
            skeletonAnimationState = null;
        }

        public void DoAction(string name, bool loop = false, bool stay = false,bool force = false, string endAniName = null)
        {
            if (null == endAniName)
            {
                endAniName = defaultAniName;
            }
            if (NetObj.m_aniName.Value != name || NetObj.m_loop.Value != loop || NetObj.m_stay.Value != stay|| force)
            {
                NetObj.m_version.Value = NetObj.m_version.Value + 1;
                NetObj.m_loop.Value = loop;
                NetObj.m_stay.Value = stay;
                NetObj.m_aniName.Value = name;
                if (defaultAniName != endAniName && !string.IsNullOrEmpty(endAniName))
                {
                    defaultAniName = endAniName;
                }
                /*
                if (NetObj.m_aniName.Value != "move")
                {
                    Debug.LogError("���Ŷ���: " + this.gameObject.GetHashCode() + "��������" + NetObj.m_aniName.Value);
                }
                */
            }
        }

        public void completeEvent(Spine.TrackEntry trackEntry)
        {
            if (NetObj.m_loop.Value == false)
            {
                if (NetObj.m_stay.Value == false && NetObj.m_aniName.Value == trackEntry.Animation.Name &&
                    skeletonAnimationState.AnimationState.Data.SkeletonData.FindAnimation(defaultAniName) != null)
                {
                    DoAction(defaultAniName, true);
                }
            }
        }

        public void SetFreezing(bool freeze)
        {
            if (null != skeletonAnimation)
            {
                // skeletonAnimation.freeze = freeze;
               
            }else if (null != skeletonGraphic)
            {
                skeletonGraphic.freeze = freeze;
              
            }
        }

        //�Ƿ���ʾ
        public void EnableSpineView(bool enable)
        {
            if (null != skeletonAnimation)
            {
                skeletonAnimation.enabled = enable;

            }
            else if (null != skeletonGraphic)
            {
                skeletonGraphic.enabled = enable;

            }
        }
    }
}