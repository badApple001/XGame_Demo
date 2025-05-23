using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( CalcHeroGridCoord ) ), CanEditMultipleObjects]
public class CalcHeroGridCoordEditor : Editor
{
    Vector3 m_v3OriginPos = Vector3.zero;
    Vector3 m_v3OffsetBossOrgin = Vector3.zero;
    private void OnEnable( )
    {
        CalcHeroGridCoord _target = ( target as CalcHeroGridCoord );
        m_v3OriginPos = _target.transform.position;
    }

    private void OnSceneGUI( )
    {
        CalcHeroGridCoord _target = ( target as CalcHeroGridCoord );

        if ( _target == null || ( _target != null && !_target.m_bEnableEditor ) ) return;

        m_v3OffsetBossOrgin = _target.__OffsetBossOrgin;
        DrawSolid( _target.__LongRangeAttackerColor, _target.__LongRangeAttackerRadius );
        DrawSolid( _target.__LavaColor, _target.__LavaRadius );
        DrawSolid( _target.__CloseCombatantColor, _target.__CloseCombatantRadius );
        DrawSolid( _target.__BossColor, _target.__BossRadius );

        DrawSceneCameraViewBorder( );
    }



    public void DrawSolid( Color color, float radius )
    {
        Handles.color = color;
        Handles.DrawSolidDisc( m_v3OriginPos + m_v3OffsetBossOrgin, Vector3.forward, radius );
    }



    public void DrawSceneCameraViewBorder( )
    {
        Camera cam = Camera.main;
        if ( cam == null ) return;

        Handles.color = Color.yellow;

        float z = cam.orthographic ? ( 0 - cam.transform.position.z ) : cam.farClipPlane;

        Vector3[] corners = new Vector3[ 4 ];
        corners[ 0 ] = cam.ViewportToWorldPoint( new Vector3( 0, 0, z ) ); // Bottom-left
        corners[ 1 ] = cam.ViewportToWorldPoint( new Vector3( 1, 0, z ) ); // Bottom-right
        corners[ 2 ] = cam.ViewportToWorldPoint( new Vector3( 1, 1, z ) ); // Top-right
        corners[ 3 ] = cam.ViewportToWorldPoint( new Vector3( 0, 1, z ) ); // Top-left

        for ( int i = 0; i < 4; i++ )
        {
            Handles.DrawLine( corners[ i ], corners[ ( i + 1 ) % 4 ] );
        }
    }
}
