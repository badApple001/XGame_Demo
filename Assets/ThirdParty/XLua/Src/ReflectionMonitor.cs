/**************************************************************************    
文　　件：ReflectionMonitor
作　　者：许德纪
创建时间：2022/2/10 12:52:33
描　　述： 监测没有xlua 没有生成wrap的类
***************************************************************************/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if UNITY_EDITOR

namespace XLua
{

    public class ReflectionMonitor
    {
        static private ReflectionMonitor _Instance = null ;

        //存储类名
        private HashSet<string> m_setTypes = new HashSet<string>();

        private string m_path = UnityEngine.Application.dataPath + "/ReflectionMonitor.txt";

        static public ReflectionMonitor Instance()
        {
            if(null== _Instance)
            {
                _Instance = new ReflectionMonitor();
               
            }

            return _Instance;
        }



        public void MonitorType(string type)
        {
            if(m_setTypes.Contains(type))
            {
                return;
            }

            m_setTypes.Add(type);

           
        }

        //保存反射
        public void Save()
        {

            StreamWriter sw = new StreamWriter(m_path);

            foreach (string item in m_setTypes)
            {
                //写文件
                string genItem = "typeof(" + item + "),";
                sw.WriteLine(genItem);
            }

            sw.Close();
        }
    }
}

#endif