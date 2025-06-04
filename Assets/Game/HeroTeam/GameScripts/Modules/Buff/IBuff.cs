using XClient.Entity;

namespace GameScripts.HeroTeam
{
    public interface IBuff
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="owner"> 拥有者 </param>
        void OnInit(IActor owner, cfg_HeroTeamBuff cfg);

        /// <summary>
        /// 下次可以更新的时间
        /// </summary>
        /// <returns></returns>
        float NextTime();

        /// <summary>
        /// 满足了 NextTime才会触发
        /// </summary>
        /// <param name="now"></param>
        void OnStep(float now);

        /// <summary>
        /// Buff状态是否可以结束
        /// </summary>
        /// <returns></returns>
        bool IsDone();

        /// <summary>
        /// Buff状态结束后回调
        /// </summary>
        void OnClear();

        /// <summary>
        /// 重置Buff时间
        /// </summary>
        void RePlay();

        /// <summary>
        /// 获取Buff配置
        /// </summary>
        /// <returns></returns>
        cfg_HeroTeamBuff GetCfg();

        /// <summary>
        /// 获取拥有者
        /// </summary>
        /// <returns></returns>
        IActor GetOwner();

    }
}
