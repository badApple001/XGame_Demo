using System;
using XGameEngine.Player;

namespace GameScripts.HeroTeam
{

    [Serializable]
    public class HeroTeamLocalData : PlayerSerializableData
    {
        /// <summary>
        /// 最后一次登录游戏的时间戳
        /// </summary>
        public long LastLoginTimestamps;



    }
}
