using System.Collections;
using System.Collections.Generic;
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

        public void SetCfgId(int id)
        {
            if (m_cfgId == id) return;
            m_cfgId = id;

            var cfg = GameGlobal.GameScheme.HeroTeamCreature_0(id);
            // m_textName.text = cfg.szAttackEffectPath;
        }



    }

}