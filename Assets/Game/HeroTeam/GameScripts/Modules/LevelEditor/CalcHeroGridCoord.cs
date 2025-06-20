using UnityEngine;

namespace GameScripts.HeroTeam
{
    public class CalcHeroGridCoord : MonoBehaviour
    {
        public bool m_bEnableEditor = true;

        [Header("团本攻击站位范围配置")]
        [SerializeField, InspectorName("不允许放置角色")] public float BossCollision = 5f;
        [SerializeField] public PoisitionScope TankScope = new PoisitionScope(270, 150, 8);
        [SerializeField] public PoisitionScope WarriorScope = new PoisitionScope(270, 150, 8);
        [SerializeField] public PoisitionScope HunterScope = new PoisitionScope(270, 150, 8);
        [SerializeField, InspectorName("基于Boss位置偏移")] public Vector3 m_v3OrginOffset = new Vector3();


        [Header("肉盾参数设置")]
        public SectorCellGenerator tankCellSector = new SectorCellGenerator();
        public Vector2 tankCellSize = 2f * Vector2.one;
        public float tankCellAngleStepDeg = 10f;

        [Header("近战或刺客参数设置")]
        public SectorCellGenerator warriorCellSector = new SectorCellGenerator();
        public Vector2 warriorCellSize = 1f * Vector2.one;
        public float warriorCellAngleStepDeg = 10f;


        [Header("射手和奶妈参数设置")]
        public SectorCellGenerator hunterCellSector = new SectorCellGenerator();
        public Vector2 hunterCellSize = 1f * Vector2.one;
        public float hunterCellAngleStepDeg = 10f;

        // public float __BossRadius = 5f;
        // public float __CloseCombatantRadius = 8f;
        // public Color __CloseCombatantColor = Color.gray;
        // public float __LavaRadius = 11f;
        // public Color __LavaColor = Color.yellow;
        // public float __LongRangeAttackerRadius = 14f;
        // public Color __LongRangeAttackerColor = Color.gray;
        // public Vector3 __OffsetBossOrgin = Vector3.up * 2.35f;
        // public float __LongRangeAttackerSeatRotate = 30f;


        private void OnValidate()
        {
            Debug.Log("OnValidate called");
            if (m_bEnableEditor)
            {
                Refrensh();
            }
        }
        private void Refrensh()
        {
            var origin = transform.position + m_v3OrginOffset;

            tankCellSector.Init(origin, BossCollision, TankScope.radius, TankScope.rangeAngle, TankScope.forwardAngle, tankCellSize, tankCellAngleStepDeg);
            warriorCellSector.Init(origin, BossCollision, WarriorScope.radius, WarriorScope.rangeAngle, WarriorScope.forwardAngle, warriorCellSize, warriorCellAngleStepDeg);
            hunterCellSector.Init(origin, HunterScope.radius - 2f, HunterScope.radius, HunterScope.rangeAngle, HunterScope.forwardAngle, hunterCellSize, hunterCellAngleStepDeg);
        }

        // private void Refrensh( )
        // {

        //     Vector3 ori = transform.position + __OffsetBossOrgin;

        //     if ( transform.childCount > 0 )
        //     {
        //         var tankRoot = transform.GetChild( 0 );
        //         var children = tankRoot.GetComponentsInChildren<SkillCompontBase>( ).ToList( );

        //         Vector3 dir = Vector3.down * ( __CloseCombatantRadius - 1 );
        //         Vector3 pos0 = ori + Quaternion.Euler( 0, 0, 30 ) * dir;
        //         Vector3 pos1 = ori + dir;
        //         Vector3 pos2 = ori + Quaternion.Euler( 0, 0, -30 ) * dir;

        //         List<Vector3> posList = new List<Vector3>( )
        //         {
        //             pos0,
        //             pos1,
        //             pos2
        //         };
        //         for ( int i = 0; i < posList.Count; i++ )
        //         {
        //             children[ i ].transform.position = posList[ i ];
        //         }
        //     }


        //     if ( transform.childCount > 1 )
        //     {

        //         var tankRoot = transform.GetChild( 1 );
        //         var children = tankRoot.GetComponentsInChildren<SkillCompontBase>( ).ToList( );

        //         Vector3 dir = Vector3.down * ( __LongRangeAttackerRadius - 1 );
        //         float unitAngle = 360 / children.Count;
        //         for ( int i = 0; i < children.Count; i++ )
        //         {
        //             var pos = ori + Quaternion.Euler( 0, 0, unitAngle * i ) * dir;
        //             children[ i ].transform.position = pos;
        //         }
        //     }

        //     if ( transform.childCount > 2 )
        //     {

        //         var tankRoot = transform.GetChild( 2 );
        //         var children = tankRoot.GetComponentsInChildren<SkillCompontBase>( ).ToList( );

        //         Vector3 dir = Vector3.up * ( __CloseCombatantRadius - 1 );
        //         float unitAngle = 25;
        //         for ( int i = 0; i < children.Count; i++ )
        //         {
        //             var pos = ori + Quaternion.Euler( 0, 0, unitAngle * -3 / 2 + unitAngle * i ) * dir;
        //             children[ i ].transform.position = pos;
        //         }

        //     }
        // }

    }

}