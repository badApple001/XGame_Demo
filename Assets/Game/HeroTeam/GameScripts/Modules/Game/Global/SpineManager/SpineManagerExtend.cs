using Spine.Unity;
using UnityEngine;

namespace GameScripts.HeroTeam
{
    /// <summary>
    /// SpineManager的业务逻辑扩展类，用于封装与具体游戏业务相关的Spine操作。
    /// 
    /// 设计理念：
    /// - SpineManager 专注于Spine动画的性能优化、实例创建与回收管理，不直接参与具体游戏逻辑。
    /// - 各个游戏项目对Spine动画的需求可能不同，业务逻辑应由扩展类负责实现，避免SpineManager臃肿。
    /// - 虽然有些团队喜欢创建大量Utils类（如XXXGameSpineUtils.cs），
    ///   但过多分散的工具类会增加项目复杂度和维护成本。
    /// - 采用注入式扩展（Extension Method）方式，将业务逻辑附加于SpineManager，方便管理和调用。
    /// 
    /// 主要功能：
    /// - Spawn：从对象池租用SkeletonAnimation实例，并设置父节点和初始位置。
    /// - DeSpawn：清理SkeletonAnimation动画状态和Transform属性，清除事件监听，归还对象池。
    /// 
    /// 备注：
    /// - 业务扩展方法应保持简洁，避免修改SpineManager核心逻辑。
    /// - 该设计模式提升代码整洁性和维护性，方便后续根据不同游戏需求定制扩展。
    /// </summary>
    public static class SpineManagerExtend
    {
        /// <summary>
        /// 从对象池租用SkeletonAnimation实例，并将其挂载到指定父节点下，位置重置为零。
        /// </summary>
        /// <param name="spineManager">SpineManager实例</param>
        /// <param name="szSkeletonDataAssetPath">骨骼数据资源路径</param>
        /// <param name="parent">父Transform</param>
        /// <returns>租用的SkeletonAnimation实例</returns>
        public static SkeletonAnimation Spawn(this SpineManager spineManager, string szSkeletonDataAssetPath, Transform parent)
        {
            var skeleton = spineManager.RentSkeletonAnimation(szSkeletonDataAssetPath);
            skeleton.transform.SetParent(parent);
            skeleton.transform.localPosition = Vector3.zero;
            return skeleton;
        }

        /// <summary>
        /// 清理SkeletonAnimation的动画状态、Transform和事件监听，归还至对象池。
        /// </summary>
        /// <param name="spineManager">SpineManager实例</param>
        /// <param name="skeletonAnimation">待回收的SkeletonAnimation实例</param>
        public static void DeSpawn(this SpineManager spineManager, SkeletonAnimation skeletonAnimation)
        {
            // 清理动画状态，恢复默认姿势
            skeletonAnimation.AnimationState.ClearTracks();
            skeletonAnimation.AnimationState.TimeScale = 1f;
            skeletonAnimation.Skeleton?.SetToSetupPose();
            skeletonAnimation.Skeleton?.SetSlotsToSetupPose();
            skeletonAnimation.Skeleton?.UpdateWorldTransform();
          
            //颜色
            skeletonAnimation.Skeleton?.SetColor(Color.white);

            // 重置Transform，确保对象状态一致
            skeletonAnimation.transform.localPosition = Vector3.zero;
            skeletonAnimation.transform.localRotation = Quaternion.identity;
            skeletonAnimation.transform.localScale = Vector3.one;

            // 清除所有动画事件监听，避免遗留回调
            skeletonAnimation.AnimationState.ClearAllAnimationEventHandlers();
            skeletonAnimation.AnimationState.ClearListenerNotifications();

            // 归还对象池，会挂载到隐藏节点并停止自动更新
            spineManager.RemandSkeletonAnimation(skeletonAnimation);
        }
    }
}
