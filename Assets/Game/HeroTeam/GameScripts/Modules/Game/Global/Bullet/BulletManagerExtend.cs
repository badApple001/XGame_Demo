using UnityEngine;

namespace GameScripts.HeroTeam
{   
    /// <summary>
    /// 子弹管理的 业务扩展, 不想 BulletManager参与业务逻辑
    /// BulletManager 应该只处理子弹的获取和回收，以及hit特效
    /// 
    /// 但是我不想让太多乱七八糟的Utils让开发工程复杂度变高，更希望一个Manager处理它相关的一切， 那使用注入式的扩展类来处理再好不过。
    /// 
    /// </summary>
    public static class BulletManagerExtend
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bulletManager"></param>
        /// <param name="cfg"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Bullet GetBullet(this BulletManager bulletManager, cfg_HeroTeamBullet cfg, Vector3 pos)
        {
            int iType = cfg.iType;
            return iType switch
            {
                1 => bulletManager.Get<Bullet>(cfg, pos),
                2 => bulletManager.Get<LightBeamBullet>(cfg, pos),
                3 => bulletManager.Get<EventBullet>(cfg, pos),
                _ => bulletManager.Get<Bullet>(cfg, pos),
            };
        }

    }

}