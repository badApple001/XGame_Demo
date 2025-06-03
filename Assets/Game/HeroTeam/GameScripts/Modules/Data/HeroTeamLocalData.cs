using System;
using System.Collections.Generic;
using XGameEngine.Player;

namespace GameScripts.HeroTeam
{

    [Serializable]
    public class HeroTeamLocalData : PlayerSerializableData
    {
        /// <summary>
        /// ���һ�ε�¼��Ϸ��ʱ���
        /// </summary>
        public long LastLoginTimestamps;

        public string testString = "";
    }
}
