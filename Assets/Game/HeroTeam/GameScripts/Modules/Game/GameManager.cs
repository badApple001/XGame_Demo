using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using XClient.Common;
using GameScripts.Monster;

namespace GameScripts.HeroTeam
{
    public class GameManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start( )
        {
            //GameGlobal.GameScheme.



            CreateHeros( );
            CreateBoss( );
        }



        private void CreateHeros( )
        {

        }

        private void CreateBoss( )
        {
            RefreshMonsterMgr.Instance.RefreshBoss( 3, 1, 1, 1 );
        }





    }

}