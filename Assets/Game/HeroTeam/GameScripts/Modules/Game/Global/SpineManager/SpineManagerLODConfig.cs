using UnityEngine;

namespace GameScripts.HeroTeam
{
    [CreateAssetMenu(fileName = nameof(SpineManagerLODConfig), menuName = "SpineConfig/" + nameof(SpineManagerLODConfig))]
    public class SpineManagerLODConfig : ScriptableObject
    {
        [Header("LOD 配置")]

        [InspectorName("远距离范围触发的 LOD 显示距离")]
        public float highDetailDistance = 10f;

        [InspectorName("中等距离范围触发的 LOD 显示距离")]
        public float midDetailDistance = 25f;

        [Header("不同等级下更新的帧间隔（帧数间隔越大越省性能）")]
        [InspectorName("近距离单位更新频率（帧间隔）")]
        [Range(1, 16)]
        public int highFrequency = 1;

        [InspectorName("中距离单位更新频率（帧间隔）")]
        [Range(1, 16)]
        public int midFrequency = 2;

        [InspectorName("远距离单位更新频率（帧间隔）")]
        [Range(1, 16)]
        public int lowFrequency = 4;

        [Header("分区激活")]
        [InspectorName("激活分区数量")]
        [Tooltip("将激活的 Spine 分为 groupCount 个分区，每帧仅激活其中一部分以减小开销")]
        [Range(1, 8)]
        public int groupCount = 4;
    }

}