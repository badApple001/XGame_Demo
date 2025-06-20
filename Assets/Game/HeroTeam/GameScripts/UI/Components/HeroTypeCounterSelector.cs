using UnityEngine;
using UnityEngine.UI;
using XClient.Common;

namespace GameScripts.HeroTeam
{
    public class HeroTypeCounterSelector : MonoBehaviour
    {
        [SerializeField] private int m_cfgId;
        [SerializeField] private Button m_btnAdd, m_btnSub;
        [SerializeField] private Text m_textCount, m_textName;

        private int m_nCount = 0;

        public void SetCfgId(int id)
        {
            if (m_cfgId == id) return;
            m_cfgId = id;

            var cfg = GameGlobal.GameScheme.HeroTeamCreature_0(id);
            m_textName.text = cfg.szName;
        }

        private void Start()
        {
            m_btnAdd.onClick.AddListener(OnAdd);
            m_btnSub.onClick.AddListener(OnSub);
            Refresh();
        }


        private void OnAdd()
        {
            ++m_nCount;
            Refresh();
        }
        private void OnSub()
        {
            --m_nCount;
            Refresh();
        }

        private void Refresh()
        {
            //25后续配到单例里 统一数值
            m_nCount = Mathf.Min(25, m_nCount);
            m_textCount.text = $"({m_nCount})";
        }
    }

}