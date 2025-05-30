/*******************************************************************
** �ļ���:	MonsterLaucher.cs
** ��  Ȩ:	(C) ��������
** ������:	���¼�
** ��  ��:	2024.6.25
** ��  ��:	1.0
** ��  ��:	
** Ӧ  ��:  ˢ�ַ�����

**************************** �޸ļ�¼ ******************************
** �޸���: 
** ��  ��: 
** ��  ��: 
********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XClient.Common;
using XGame.EventEngine;
using XGame;

namespace XClient.Entity
{
    //����ˢ�µķ�ʽ
    public enum REFRESH_RECT_TYPE
    {
        RECT_TYPE_CIRCLE,
        RECT_TYPE_RECT,
    }


    //��������
    public enum PARAM_TYPE
    {
        PARAM_TYPE_COUNT,
        PARAM_TYPE_PROP,
    }


    public class MonsterLauncher : MonoBehaviour, IEventExecuteSink
    {
        //������Ӫ
        [Header("��Ӫ����")]
        public ulong camp = 1;

        //����ID
        [Header("����ID�б�")]
        public List<int> listMonsterIDs;

        [Header("ˢ�²���,�ο�PARAM_TYPE")]
        public List<int> listRefreshParam;

        [Header("ˢ�²�������")]
        public PARAM_TYPE paramType = PARAM_TYPE.PARAM_TYPE_COUNT;

        //ˢ�ּ��
        [Header("ˢ�ּ��")]
        public float refresh_Interval = 5.0f;

        //ˢ�ִ���,-1�������޴�
        [Header("ˢ�ִ���,-1�������޴�")]
        public int refresh_ExcuteCount = -1;

        //ˢ������
        [Header("��������,-1����������")]
        public int limit_MonsterCount = 100;

        //ÿ��ˢ�ָ���
        [Header("ÿ��ˢ�ָ���")]
        public int refresh_MonsterCount = 10;

        //ÿ��ˢ�ָ���
        [Header("ˢ�·�Χ��ʽ")]
        public REFRESH_RECT_TYPE rectType = REFRESH_RECT_TYPE.RECT_TYPE_CIRCLE;

        [Header("�ѷ�����Ӫ���")]
        public List<ulong> listFriendCamps;

        [Header("�з���Ӫ�б�,����������˼�����Ӫ,���ǵ���")]
        public List<ulong> listEnemyCamps;

        //�Ƿ����λ��
        public bool bRandomPos = true;

        //�̶�ˢ��λ��
        public Vector3 refreshPos;


        //�Ѿ�ˢ���˵Ĵ���
        private int m_refreshCount;

        //�ϴ�ˢ��ʱ��
        private float m_lastRefreshTime = 0;

        //λ����Ϣ
        private List<Vector3> m_listPos = new List<Vector3>();

        //����������
        private HashSet<ulong> m_hashEntIDs = new HashSet<ulong>();

        //ˢ�¸���
        private List<int> m_refreshMonsters = new List<int>();

        //����ˢ�ֵ���ʱ�б�
        private List<IMonster> m_listMonster = new List<IMonster>();

        // Start is called before the first frame update
        void Start()
        {
            //����������Ϣ
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            eventEngine?.Subscibe(this, DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, "MonsterLauncher:Start");// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
        }

        // Update is called once per frame
        void Update()
        {
            //������ˢ������
            if (MonsterSystem.Instance == null || GameGlobal.Role == null || refresh_ExcuteCount >= 0 && refresh_ExcuteCount <= m_refreshCount ||
                limit_MonsterCount >= 0 && limit_MonsterCount <= m_hashEntIDs.Count)
            {
                return;
            }

            float curTime = Time.realtimeSinceStartup;
            if (curTime - m_lastRefreshTime > refresh_Interval)
            {
                ++m_refreshCount;
                m_lastRefreshTime = curTime;
                RefreshMonster();
            }


        }

        public List<IMonster> RefreshMonster()
        {
            Vector3 pos = this.transform.position;
            Vector3 forward = this.transform.forward;

            IMonster monster = null;
            m_listMonster.Clear();
            __CalcRefreshMonsters();

            int nCount = m_refreshMonsters.Count;
            __RectOrder(nCount);
            for (int i = 0; i < nCount; ++i)
            {
                if (m_hashEntIDs.Count >= limit_MonsterCount)
                {
                    break;
                }

                // m_listPos[i]
                //pos.y = 0;

                CreateMonsterContext createContext = MonsterSystem.s_CreateContext;
                createContext.listFriendCamps = listFriendCamps;
                createContext.listEnemyCamps = listEnemyCamps;
                createContext.camp = camp;
                createContext.creatureID = m_refreshMonsters[i];
                createContext.displayID = m_refreshMonsters[i];
                createContext.forward = forward;
                createContext.pos = m_listPos[i];
                monster = MonsterSystem.Instance.CreateMonster(createContext);

                //monster = MonsterSystem.Instance.CreateMonster(m_refreshMonsters[i], m_listPos[i], forward,camp, listFriendCamps, listEnemyCamps);
                m_hashEntIDs.Add(monster.id);
                m_listMonster.Add(monster);
            }

            return m_listMonster;

        }


        // System.Random seedRd = new System.Random();
        private void __RectOrder(int nCount)
        {
            m_listPos.Clear();

            //�̶�λ��ˢ��
            if (bRandomPos == false)
            {
                for (int i = 0; i < nCount; ++i)
                {
                    m_listPos.Add(refreshPos);
                }
                return;
            }

            Vector3 vPos = this.transform.position;


            // int nCount = refresh_MonsterCount;


            Vector3 vScale = this.transform.localScale;
            Vector3 targetPos = vPos;

            //int nConcentratedCount = (int)(concentratedPer * nCount);
            float scale = 1.0f;//Mathf.Lerp(0.2f, 0.7f, ((float)nConcentratedCount) / 50.0f);
            float nSizeX = (vScale.x) / 2;
            float nScaleSizeX = (nSizeX * scale);
            float nSizeZ = vScale.z / 2;

            //Բ�ε���XΪ׼
            if (rectType == REFRESH_RECT_TYPE.RECT_TYPE_CIRCLE)
            {
                nSizeZ = nScaleSizeX;
            }


            for (int i = 0; i < nCount; ++i)
            {

                targetPos.x = vPos.x + Random.Range(-nScaleSizeX, nScaleSizeX);
                targetPos.z = vPos.z + Random.Range(-nSizeZ, nSizeZ);

                m_listPos.Add(targetPos);
            }


        }

        private void __CalcRefreshMonsters()
        {
            m_refreshMonsters.Clear();
            switch (paramType)
            {
                case PARAM_TYPE.PARAM_TYPE_COUNT:
                    {
                        int nMonsterID = 0;
                        int nMonsterCount = 0;
                        int nCount = listMonsterIDs.Count;
                        for (int i = 0; i < nCount; ++i)
                        {
                            nMonsterID = listMonsterIDs[i];
                            nMonsterCount = 1;
                            if (listRefreshParam.Count > i)
                            {
                                nMonsterCount = listRefreshParam[i];
                            }
                            //���ӹ������
                            for (int j = 0; j < nMonsterCount; ++j)
                            {
                                m_refreshMonsters.Add(nMonsterID);
                            }
                        }

                    }
                    break;
                case PARAM_TYPE.PARAM_TYPE_PROP:
                    {
                        int sumProp = GetPropSum();

                        int nMonsterID = 0;
                        for (int i = 0; i < refresh_MonsterCount; ++i)
                        {
                            nMonsterID = RandmonOneMonster(sumProp);
                            if (nMonsterID > 0)
                            {
                                m_refreshMonsters.Add(nMonsterID);
                            }

                        }
                    }
                    break;
                default:
                    break;


            }
        }

        //���ˢȡ������
        private int RandmonOneMonster(int sumProp)
        {
            int curSum = 0;
            int prop = Random.Range(0, sumProp);
            int nCount = listRefreshParam.Count;
            for (int i = 0; i < nCount; ++i)
            {
                curSum += listRefreshParam[i];
                if (curSum > prop)
                {
                    if (listMonsterIDs.Count > i)
                    {
                        return listMonsterIDs[i];
                    }
                    Debug.LogError("����ID�б��͸��ʸ�������Ӧ������");
                    break;
                }
            }

            return 0;
        }


        //��ȡ�ܸ���
        private int GetPropSum()
        {
            int sum = 0;
            int nCount = listRefreshParam.Count;
            for (int i = 0; i < nCount; ++i)
            {
                sum += listRefreshParam[i];
            }

            return sum;
        }

        private void OnDestroy()
        {
            //����������Ϣ
            IEventEngine eventEngine = XGameComs.Get<IEventEngine>();
            if (eventEngine != null)
            {
                eventEngine.UnSubscibe(this, DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0);// FireExecute(DGlobalEvent.EVENT_ENTITY_DESTROY, DEventSourceType.SOURCE_TYPE_ENTITY, 0, entity);
            }

        }

        public void OnExecute(ushort wEventID, byte bSrcType, uint dwSrcID, object pContext)
        {
            IMonster monster = pContext as IMonster;
            if (null != monster)
            {
                ulong entID = monster.id;
                m_hashEntIDs.Remove(entID);

            }
        }
    }

}

