using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CalcHeroGridCoord : MonoBehaviour
{
    public bool m_bEnableEditor = true;

    [Header( "±à¼­Æ÷ÊôÐÔ" )]
    public float __BossRadius = 5f;
    public Color __BossColor = Color.yellow;
    public float __CloseCombatantRadius = 8f;
    public Color __CloseCombatantColor = Color.gray;
    public float __LavaRadius = 11f;
    public Color __LavaColor = Color.yellow;
    public float __LongRangeAttackerRadius = 14f;
    public Color __LongRangeAttackerColor = Color.gray;
    public Vector3 __OffsetBossOrgin = Vector3.up * 2.35f;
    public float __LongRangeAttackerSeatRotate = 30f;


    private void OnValidate( )
    {
        Debug.Log( "OnValidate called" );
        if ( m_bEnableEditor )
        {
            Refrensh( );
        }
    }

    private void Refrensh( )
    {

        Vector3 ori = transform.position + __OffsetBossOrgin;

        if ( transform.childCount > 0 )
        {
            var tankRoot = transform.GetChild( 0 );
            var children = tankRoot.GetComponentsInChildren<SkillCompontBase>( ).ToList( );

            Vector3 dir = Vector3.down * ( __CloseCombatantRadius - 1 );
            Vector3 pos0 = ori + Quaternion.Euler( 0, 0, 30 ) * dir;
            Vector3 pos1 = ori + dir;
            Vector3 pos2 = ori + Quaternion.Euler( 0, 0, -30 ) * dir;

            List<Vector3> posList = new List<Vector3>( )
            {
                pos0,
                pos1,
                pos2
            };
            for ( int i = 0; i < posList.Count; i++ )
            {
                children[ i ].transform.position = posList[ i ];
            }
        }


        if ( transform.childCount > 1 )
        {

            var tankRoot = transform.GetChild( 1 );
            var children = tankRoot.GetComponentsInChildren<SkillCompontBase>( ).ToList( );

            Vector3 dir = Vector3.down * ( __LongRangeAttackerRadius - 1 );
            float unitAngle = 360 / children.Count;
            for ( int i = 0; i < children.Count; i++ )
            {
                var pos = ori + Quaternion.Euler( 0, 0, unitAngle * i ) * dir;
                children[ i ].transform.position = pos;
            }
        }

        if ( transform.childCount > 2 )
        {

            var tankRoot = transform.GetChild( 2 );
            var children = tankRoot.GetComponentsInChildren<SkillCompontBase>( ).ToList( );

            Vector3 dir = Vector3.up * ( __CloseCombatantRadius - 1 );
            float unitAngle = 25;
            for ( int i = 0; i < children.Count; i++ )
            {
                var pos = ori + Quaternion.Euler( 0, 0, unitAngle * -3 / 2 + unitAngle * i ) * dir;
                children[ i ].transform.position = pos;
            }

        }
    }

}
