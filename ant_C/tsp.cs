using System;
using System.Collections.Generic;
using System.Text;

namespace ant_C
{
    class CTsp
    {
	
        CAnt1 []m_cAntAry; //蚂蚁数组
        public CAnt1 m_cBestAnt; //定义一个蚂蚁变量，用来保存搜索过程中的最优结果
	                                    //该蚂蚁不参与搜索，只是用来保存最优结果

        public CTsp()
        {
            Common.Init();
            m_cAntAry=new CAnt1[Common.N_ANT_COUNT];
            for (int i = 0; i < Common.N_ANT_COUNT; i++)
            {
                m_cAntAry[i] = new CAnt1();
            }
            m_cBestAnt=new CAnt1();
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        public void InitData()
        {
            //先把最优蚂蚁的路径长度设置成一个很大的值
            m_cBestAnt.m_dbPathLength = Common.DB_MAX;

            //计算两两城市间距离
            //double dbTemp = 0.0;
            //for (int i = 0; i < Common.N_CITY_COUNT; i++)
            //{
            //    for (int j = 0; j < Common.N_CITY_COUNT; j++)
            //    {
            //        dbTemp = (Common.x_Ary[i] - Common.x_Ary[j]) * (Common.x_Ary[i] - Common.x_Ary[j]) + (Common.y_Ary[i] - Common.y_Ary[j]) * (Common.y_Ary[i] - Common.y_Ary[j]);
            //        dbTemp = System.Math.Pow(dbTemp, 0.5);
            //        Common.g_Distance[i,j] = Common.ROUND(dbTemp);
            //    }
            //}

            //初始化环境信息素，先把城市间的信息素设置成一样
            //这里设置成1.0，设置成多少对结果影响不是太大，对算法收敛速度有些影响
            for (int i = 0; i < Common.N_CITY_COUNT; i++)
            {
                for (int j = 0; j < Common.N_CITY_COUNT; j++)
                {
                    Common.g_Trial[i,j] = 1.0;
                }
            }
        
        }


        /// <summary>
        /// 更新环境信息素
        /// </summary>
        public void UpdateTrial()
        {
	        //临时数组，保存各只蚂蚁在两两城市间新留下的信息素
            double[,] dbTempAry = new double[Common.N_CITY_COUNT, Common.N_CITY_COUNT];

            //先全部设置为0
            for (int i = 0; i < Common.N_ANT_COUNT; i++) //计算每只蚂蚁留下的信息素
            {
                for (int j = 1; j < Common.N_CITY_COUNT; j++)
                {
                    dbTempAry[i, j] = 0.0;
                }
            }


	        //计算新增加的信息素,保存到临时数组里
	        int m=0;
	        int n=0;

            for (int i = 0; i < Common.N_ANT_COUNT; i++) //计算每只蚂蚁留下的信息素
	        {
                for (int j = 1; j < Common.N_CITY_COUNT; j++)
			        {
				        m=m_cAntAry[i].m_nPath[j];
				        n=m_cAntAry[i].m_nPath[j-1];
                        dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cAntAry[i].m_dbPathLength;
				        dbTempAry[m,n]=dbTempAry[n,m];
			        }

			        //最后城市和开始城市之间的信息素
			        n=m_cAntAry[i].m_nPath[0];
                    dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cAntAry[i].m_dbPathLength;
			        dbTempAry[m,n]=dbTempAry[n,m];

	        }

	        //==================================================================
	        //更新环境信息素
            for (int i = 0; i < Common.N_CITY_COUNT; i++)
	        {
                for (int j = 0; j < Common.N_CITY_COUNT; j++)
		        {
                    Common.g_Trial[i,j] = Common.g_Trial[i,j] * Common.ROU + dbTempAry[i,j];  //最新的环境信息素 = 留存的信息素 + 新留下的信息素
		        }
	        }

        }


        /// <summary>
        /// 开始搜索
        /// </summary>
        public void Search()
        {

	        //char cBuf[256]; //打印信息用
            string strInfo="";
            
            bool blFlag = false;

	        //在迭代次数内进行循环
	        for (int i=0;i<Common.N_IT_COUNT;i++)
	        {
                blFlag = false;

		        //每只蚂蚁搜索一遍
                for (int j = 0; j < Common.N_ANT_COUNT; j++)
		        {
			        m_cAntAry[j].Search(); 
		        }

		        //保存最佳结果
                for (int j = 0; j < Common.N_ANT_COUNT; j++)
		        {
			        if (m_cAntAry[j].m_dbPathLength < m_cBestAnt.m_dbPathLength)
			        {				        
                        m_cBestAnt.m_dbPathLength=m_cAntAry[j].m_dbPathLength;
                        m_cBestAnt.m_nPath = m_cAntAry[j].m_nPath;

                        blFlag = true;

                    }
		        }

		        //更新环境信息素
		        UpdateTrial();

		        //输出目前为止找到的最优路径的长度
                if (blFlag == true)
                {
                    strInfo = String.Format("[{0}] {1}", i + 1, m_cBestAnt.m_dbPathLength);
                    Console.WriteLine(strInfo);
                }


	        }

        }
    }
}
