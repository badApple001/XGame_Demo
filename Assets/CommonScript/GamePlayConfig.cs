/*******************************************************************
** 文件名:	GamePlayConfig.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2024.11.7
** 版  本:	1.0
** 描  述:	
** 应  用:  游戏列表配置

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XGame.Utils;

namespace CommonScript
{

    [Serializable]
    public class GamePlayItem
    {
        //demo项的资源路径
        public string gameResDir = "Game/ImmortalFamily/GameResources";

        //基础脚本目录
        public string gameScriptDir = "Game/ImmortalFamily/GameScripts";

        //分辨率宽
        public int resolutionWidth = 1080;

        //分辨率高
        public int resolutionHeight = 1920;

        //名空间
        public string strNamespace = "ImmortalFamily";

        //启动场景名字
        //public string setupScenePath = "";

        public ResourceRef setupScenePath;


    }

    public class GamePlayConfig : MonoBehaviour
    {

        //游戏配置选项
        [Header("游戏列表相关配置")]
        public List<GamePlayItem> gamePlayItems;

        //启用的demo下标
        public int setupGamePlayIndex;

        // Start is called before the first frame update
        void Start()
        {

        

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


