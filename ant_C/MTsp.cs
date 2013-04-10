using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ant_C
{
    class MTsp : IDisposable
    {
        #region 参数定义

        CAnt1[] m_cAntAry_1; //蚂蚁种群1数组
        CAnt2[] m_cAntAry_2; //蚂蚁种群2数组
        public CAnt1 m_cBestAnt; //定义一个蚂蚁变量，用来保存搜索过程中的最优结果
        //该蚂蚁不参与搜索，只是用来保存最优结果
        /// <summary>
        /// 种群1的最优蚂蚁
        /// </summary>
        public static CAnt1 m_cBestAnt_1;
        /// <summary>
        /// 种群2的最优蚂蚁
        /// </summary>
        public static CAnt2 m_cBestAnt_2;

        Thread ant_1;
        Thread ant_2;
        Thread Mant;

        #endregion
        

        public MTsp()
        {
            Common.Init();
            m_cAntAry_1 = new CAnt1[Common.N_ANT_COUNT];
            m_cAntAry_2 = new CAnt2[Common.N_ANT_COUNT];
            for (int i = 0; i < Common.N_ANT_COUNT; i++)
            {
                m_cAntAry_1[i] = new CAnt1();
            }
            for (int i = 0; i < Common.N_ANT_COUNT; i++)
            {
                m_cAntAry_2[i] = new CAnt2();
            }
            m_cBestAnt=new CAnt1();
            m_cBestAnt_1 = new CAnt1();
            m_cBestAnt_2 = new CAnt2();
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        public void InitData()
        {
            //先把最优蚂蚁的路径长度设置成一个很大的值
            m_cBestAnt.m_dbPathLength = Common.DB_MAX;
            m_cBestAnt_1.m_dbPathLength = Common.DB_MAX;
            m_cBestAnt_2.m_dbPathLength = Common.DB_MAX;

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
                    Common.g_Trial_1[i, j] = 1.0;
                    Common.g_Trial_2[i, j] = 1.0;
                }
            }
        
        }

        #region 信息素更新

        /// <summary>
        /// 交换更新两种群中最优路径的环境信息素
        /// </summary>
        public void UpdateBestTrial()
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
            int m = 0;
            int n = 0;
            for (int j = 1; j < Common.N_CITY_COUNT; j++)
            {
                m = m_cBestAnt_1.m_nPath[j];
                n = m_cBestAnt_1.m_nPath[j - 1];
                dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cBestAnt_1.m_dbPathLength;
                dbTempAry[m, n] = dbTempAry[n, m];
            }

                //最后城市和开始城市之间的信息素
            n = m_cBestAnt_1.m_nPath[0];
            dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cBestAnt_1.m_dbPathLength;
            dbTempAry[m, n] = dbTempAry[n, m];

            

            //==================================================================
            //更新环境信息素
            for (int i = 0; i < Common.N_CITY_COUNT; i++)
            {
                for (int j = 0; j < Common.N_CITY_COUNT; j++)
                {
                    Common.g_Trial_2[i, j] = Common.g_Trial_2[i, j] * Common.ROU + dbTempAry[i, j];  //最新的环境信息素 = 留存的信息素 + 新留下的信息素
                }
            }

            for (int j = 1; j < Common.N_CITY_COUNT; j++)
            {
                m = m_cBestAnt_2.m_nPath[j];
                n = m_cBestAnt_2.m_nPath[j - 1];
                dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cBestAnt_2.m_dbPathLength;
                dbTempAry[m, n] = dbTempAry[n, m];
            }

            //最后城市和开始城市之间的信息素
            n = m_cBestAnt_2.m_nPath[0];
            dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cBestAnt_2.m_dbPathLength;
            dbTempAry[m, n] = dbTempAry[n, m];

            for (int i = 0; i < Common.N_CITY_COUNT; i++)
            {
                for (int j = 0; j < Common.N_CITY_COUNT; j++)
                {
                    Common.g_Trial_1[i, j] = Common.g_Trial_1[i, j] * Common.ROU + dbTempAry[i, j];  //最新的环境信息素 = 留存的信息素 + 新留下的信息素
                }
            }
            

        }

        /// <summary>
        /// 更新种群1的环境信息素
        /// </summary>
        public void UpdateTrial_1()
        {
            //Common.MaxTrial_1 = (1 / (1 - Common.ROU)) * (1 / m_cBestAnt_1.m_dbPathLength);
            //Common.MinTrial_1 = Common.MaxTrial_1 / 5;
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
            int m = 0;
            int n = 0;

            for (int i = 0; i < Common.N_ANT_COUNT; i++) //计算每只蚂蚁留下的信息素
            {
                for (int j = 1; j < Common.N_CITY_COUNT; j++)
                {
                    m = m_cAntAry_1[i].m_nPath[j];
                    n = m_cAntAry_1[i].m_nPath[j - 1];
                    dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cAntAry_1[i].m_dbPathLength;
                    dbTempAry[m, n] = dbTempAry[n, m];
                }

                //最后城市和开始城市之间的信息素
                n = m_cAntAry_1[i].m_nPath[0];
                dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cAntAry_1[i].m_dbPathLength;
                dbTempAry[m, n] = dbTempAry[n, m];

            }

            //==================================================================
            //更新环境信息素
            for (int i = 0; i < Common.N_CITY_COUNT; i++)
            {
                for (int j = 0; j < Common.N_CITY_COUNT; j++)
                {
                    Common.g_Trial_1[i, j] = Common.g_Trial_1[i, j] * Common.ROU + dbTempAry[i, j];  //最新的环境信息素 = 留存的信息素 + 新留下的信息素
                    //if (Common.g_Trial_1[i, j] > Common.MaxTrial_1)
                    //{
                    //    Common.g_Trial_1[i, j] = Common.MaxTrial_1;
                    //}
                    //if (Common.g_Trial_1[i, j] < Common.MinTrial_1)
                    //{
                    //    Common.g_Trial_1[i, j] = Common.MinTrial_1;
                    //}
                }
            }
        }

        /// <summary>
        /// 更新种群2的环境信息素
        /// </summary>
        public void UpdateTrial_2()
        {
            //Common.MaxTrial_2 = (1 / (1 - Common.ROU)) * (1 / m_cBestAnt_2.m_dbPathLength);
            //Common.MinTrial_2 = Common.MaxTrial_2 / 5;
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
            int m = 0;
            int n = 0;

            for (int i = 0; i < Common.N_ANT_COUNT; i++) //计算每只蚂蚁留下的信息素
            {
                for (int j = 1; j < Common.N_CITY_COUNT; j++)
                {
                    m = m_cAntAry_2[i].m_nPath[j];
                    n = m_cAntAry_2[i].m_nPath[j - 1];
                    dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cAntAry_2[i].m_dbPathLength;
                    dbTempAry[m, n] = dbTempAry[n, m];
                }

                //最后城市和开始城市之间的信息素
                n = m_cAntAry_2[i].m_nPath[0];
                dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cAntAry_2[i].m_dbPathLength;
                dbTempAry[m, n] = dbTempAry[n, m];

            }

            //==================================================================
            //更新环境信息素
            for (int i = 0; i < Common.N_CITY_COUNT; i++)
            {
                for (int j = 0; j < Common.N_CITY_COUNT; j++)
                {
                    Common.g_Trial_2[i, j] = Common.g_Trial_2[i, j] * Common.ROU + dbTempAry[i, j];  //最新的环境信息素 = 留存的信息素 + 新留下的信息素
                    //if (Common.g_Trial_2[i, j] > Common.MaxTrial_2)
                    //{
                    //    Common.g_Trial_2[i, j] = Common.MaxTrial_2;
                    //}
                    //if (Common.g_Trial_2[i, j] < Common.MinTrial_2)
                    //{
                    //    Common.g_Trial_2[i, j] = Common.MinTrial_2;
                    //}
                }
            }
        }

        #endregion
        
        /// <summary>
        /// 蚂蚁种群1进行搜索
        /// </summary>
        public void Search_1()
        {
            for (int i = 0; i < Common.N_ANT_COUNT; i++)
            {
                m_cAntAry_1[i].Search();
            }

            //保存最佳结果
            for (int j = 0; j < Common.N_ANT_COUNT; j++)
            {
                if (m_cAntAry_1[j].m_dbPathLength < m_cBestAnt_1.m_dbPathLength)
                {
                    m_cBestAnt_1.m_dbPathLength = m_cAntAry_1[j].m_dbPathLength;
                    m_cBestAnt_1.m_nPath = m_cAntAry_1[j].m_nPath;
                }
            }
            //Console.WriteLine("1:");
            //for (int i = 0; i < Common.N_CITY_COUNT; i++)
            //{
            //    Console.Write("{0} ", m_cBestAnt_1.m_nPath[i] + 1);
            //}
            //Console.WriteLine();
            UpdateTrial_1();
        }

        /// <summary>
        /// 蚂蚁种群2进行搜索
        /// </summary>
        public void Search_2()
        {
            for (int i = 0; i < Common.N_ANT_COUNT; i++)
            {
                m_cAntAry_2[i].Search();
            }

            //保存最佳结果
            for (int j = 0; j < Common.N_ANT_COUNT; j++)
            {
                if (m_cAntAry_2[j].m_dbPathLength < m_cBestAnt_2.m_dbPathLength)
                {
                    m_cBestAnt_2.m_dbPathLength = m_cAntAry_2[j].m_dbPathLength;
                    m_cBestAnt_2.m_nPath = m_cAntAry_2[j].m_nPath;
                }
            }
            //Console.WriteLine("2:");
            //for (int i = 0; i < Common.N_CITY_COUNT; i++)
            //{
            //    Console.Write("{0} ", m_cBestAnt_2.m_nPath[i] + 1);
            //}
            //Console.WriteLine();
            UpdateTrial_2();
        }

        /// <summary>
        /// 并行开始
        /// </summary>
        public void MantStart()
        {
            ant_1 = new Thread(Search_1);
            ant_2 = new Thread(Search_2);
            ant_1.Start();
            ant_2.Start();
            ant_1.Join();
            ant_2.Join();
        }

        /// <summary>
        /// 开始搜索
        /// </summary>
        public void Search()
        {
	        //char cBuf[256]; //打印信息用
            string strInfo="";
            
            bool blFlag = false;
            int flag = 0;
            //Mant = new Thread(MantStart);
	        //在迭代次数内进行循环
            for (int i = 0; i < Common.N_IT_COUNT; i++)
            {
                MantStart();

                if (m_cBestAnt.m_dbPathLength > m_cBestAnt_2.m_dbPathLength)
                {
                    m_cBestAnt.m_nPath = m_cBestAnt_2.m_nPath;
                    m_cBestAnt.m_dbPathLength = m_cBestAnt_2.m_dbPathLength;
                    blFlag = true;
                }
                if (m_cBestAnt.m_dbPathLength > m_cBestAnt_1.m_dbPathLength)
                {
                    m_cBestAnt.m_nPath = m_cBestAnt_1.m_nPath;
                    m_cBestAnt.m_dbPathLength = m_cBestAnt_1.m_dbPathLength;
                    blFlag = true;
                }

                if (i % 10 == 0)
                {
                    UpdateBestTrial();
                }
                if (blFlag)
                {
                    Console.WriteLine("[{0}]: {1}", i, m_cBestAnt.m_dbPathLength);
                    for (int j = 0; j < Common.N_CITY_COUNT; j++)
                    {
                        Console.Write("{0}, ", m_cBestAnt.m_nPath[j]);
                    }
                    Console.WriteLine();
                    blFlag = false;
                }
            }

            Console.WriteLine("best_1 path length = {0} ", m_cBestAnt_1.m_dbPathLength);
            for (int i = 0; i < Common.N_CITY_COUNT; i++)
            {
                strInfo = String.Format("{0} ", m_cBestAnt_1.m_nPath[i] + 1);
                Console.Write(strInfo);
            }

            Console.WriteLine();

            Console.WriteLine("best_2 path length = {0} ", m_cBestAnt_2.m_dbPathLength);
            for (int i = 0; i < Common.N_CITY_COUNT; i++)
            {
                strInfo = String.Format("{0} ", m_cBestAnt_2.m_nPath[i] + 1);
                Console.Write(strInfo);
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
