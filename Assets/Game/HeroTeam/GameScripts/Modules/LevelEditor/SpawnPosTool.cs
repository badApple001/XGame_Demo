using UnityEngine;

public class SpawnPosTool : MonoBehaviour
{
    public float m_fXInterval = 5f;
    public float m_fYInterval = 5f;
    public int m_nCols = 5;
    public int m_nRows = 5;
    public Vector3 m_vec3SpawnPosBase = Vector3.zero;
    public bool m_bEnableEditor = true;


    private void OnValidate( )
    {
        Debug.Log( "OnValidate called" );
        if( m_bEnableEditor )
        {
            Refrensh( );
        }
    }

    private void Refrensh( )
    {
        while ( transform.childCount < m_nCols * m_nRows )
        {
            GameObject.Instantiate( transform.GetChild( 0 ).gameObject, transform );
        }

        while ( transform.childCount > m_nCols * m_nRows )
        {
            DestroyImmediate( transform.GetChild( 0 ) );
        }


        int iLeftScale = m_nCols / 2;
        Vector3 spawnPos = new Vector3( iLeftScale * -m_fXInterval + m_vec3SpawnPosBase.x, m_vec3SpawnPosBase.y, m_vec3SpawnPosBase.z );

        for ( int i = 0; i < transform.childCount; i++ )
        {
            transform.GetChild( i ).name = GetNameByIndex( i );

            int yScale = i / m_nCols;
            int xScale = i % m_nCols;

            Vector3 pos = spawnPos + Vector3.right * xScale * m_fXInterval + Vector3.down * yScale * m_fYInterval;
            transform.GetChild( i ).position = pos;
        }

    }


    private string GetNameByIndex( int i ) => string.Format( "__SpawnPos_{0:D2}", i );
}
