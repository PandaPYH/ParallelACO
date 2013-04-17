using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ant_C
{
    public class MTsp : IDisposable
    {
        #region 参数定义

        CAnt1[] m_cAntAry_1; //蚂蚁种群1数组
        CAnt2[] m_cAntAry_2; //蚂蚁种群2数组

        CAnt1 localBest_1;
        CAnt2 localBest_2;
        public CAnt1 m_cBestAnt; //定义一个蚂蚁变量，用来保存搜索过程中的最优结果
                                 //该蚂蚁不参与搜索，只是用来保存最优结果
        public List<double> BestPathLengthList = new List<double>();

        //存储一次迭代后得到的解
        List<CAnt1> popLocalBest_1 = new List<CAnt1>();
        List<CAnt2> popLocalBest_2 = new List<CAnt2>();

        double ballance = 1.0;

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
            localBest_1 = new CAnt1();
            localBest_2 = new CAnt2();
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
            localBest_1.m_dbPathLength = Common.DB_MAX;
            localBest_2.m_dbPathLength = Common.DB_MAX;
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
                //dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / Math.Pow(m_cBestAnt_1.m_dbPathLength, 2);
                dbTempAry[m, n] = dbTempAry[n, m];
            }

                //最后城市和开始城市之间的信息素
            n = m_cBestAnt_1.m_nPath[0];
            dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cBestAnt_1.m_dbPathLength;
            //dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / Math.Pow(m_cBestAnt_1.m_dbPathLength, 2);
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
                //dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / Math.Pow(m_cBestAnt_2.m_dbPathLength, 2);
                dbTempAry[m, n] = dbTempAry[n, m];
            }

            //最后城市和开始城市之间的信息素
            n = m_cBestAnt_2.m_nPath[0];
            dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / m_cBestAnt_2.m_dbPathLength;
            //dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / Math.Pow(m_cBestAnt_2.m_dbPathLength, 2);
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
            //Common.MaxTrial_1 = 1 / Common.ROU * (1 / m_cBestAnt_1.m_dbPathLength);
            //Common.MinTrial_1 = Common.MaxTrial_1 / 5;
            //临时数组，保存各只蚂蚁在两两城市间新留下的信息素
            double[,] dbTempAry = new double[Common.N_CITY_COUNT, Common.N_CITY_COUNT];
            double lengthTemp = 0;

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
                    n = m_cAntAry_1[i].m_nPath[j];
                    m = m_cAntAry_1[i].m_nPath[j - 1];
                    dbTempAry[n, m] = dbTempAry[n, m] + (Common.DBQ / m_cAntAry_1[i].m_dbPathLength) * ballance;
                    //dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / Math.Pow(m_cAntAry_1[i].m_dbPathLength, 2);
                    dbTempAry[m, n] = dbTempAry[n, m];
                }

                //最后城市和开始城市之间的信息素
                n = m_cAntAry_1[i].m_nPath[0];
                dbTempAry[n, m] = dbTempAry[n, m] + (Common.DBQ / m_cAntAry_1[i].m_dbPathLength) * ballance;
                //dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / Math.Pow(m_cAntAry_1[i].m_dbPathLength, 2);
                dbTempAry[m, n] = dbTempAry[n, m];
            }

            //==================================================================
            //更新环境信息素
            for (int i = 0; i < Common.N_CITY_COUNT; i++)
            {
                for (int j = 0; j < Common.N_CITY_COUNT; j++)
                {
                    Common.g_Trial_1[i, j] = Common.g_Trial_1[i, j] * Common.ROU + (1 - Common.ROU) * dbTempAry[i, j];  //最新的环境信息素 = 留存的信息素 + 新留下的信息素
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
        /// 动态信息素更新
        /// </summary>
        //public void DynamicUpdateTrial()
        //{
        //    //临时数组，保存各只蚂蚁在两两城市间新留下的信息素
        //    double[,] dbTempAry = new double[Common.N_CITY_COUNT, Common.N_CITY_COUNT];
        //    double lengthTemp = 0;

        //    //先全部设置为0
        //    for (int i = 0; i < Common.N_ANT_COUNT; i++) //计算每只蚂蚁留下的信息素
        //    {
        //        for (int j = 1; j < Common.N_CITY_COUNT; j++)
        //        {
        //            dbTempAry[i, j] = 0.0;
        //        }
        //    }


        //    for (int k = 1; k < Common.N_CITY_COUNT; k++)
        //    {
        //        //计算新增加的信息素,保存到临时数组里
        //        int m = 0;
        //        int n = 0;
        //        lengthTemp = 0.0;

        //        //先全部设置为0
        //        for (int i = 0; i < Common.N_ANT_COUNT; i++) //计算每只蚂蚁留下的信息素
        //        {
        //            for (int j = 1; j < Common.N_CITY_COUNT; j++)
        //            {
        //                dbTempAry[i, j] = 0.0;
        //            }
        //        }

        //        for (int i = 0; i < Common.N_ANT_COUNT; i++)
        //        {
        //            for (int j = 0; j < k; j++)
        //            {
        //                n = m_cAntAry_1[i].m_nPath[j];
        //                m = m_cAntAry_1[i].m_nPath[j + 1];
        //                lengthTemp += Common.g_Distance[n, m];
        //            }
        //        }

        //        for (int i = 0; i < Common.N_ANT_COUNT; i++) //计算每只蚂蚁留下的信息素
        //        {
        //            for (int j = 0; j < k; j++)
        //            {
        //                n = m_cAntAry_1[i].m_nPath[j];
        //                m = m_cAntAry_1[i].m_nPath[j + 1];
        //                dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ1 / lengthTemp;
        //                //dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / Math.Pow(m_cAntAry_1[i].m_dbPathLength, 2);
        //                dbTempAry[m, n] = dbTempAry[n, m];
        //            }
        //            //dbTempAry[m, n] = dbTempAry[n, m];
        //        }

        //        //==================================================================
        //        //更新环境信息素
        //        for (int i = 0; i < Common.N_ANT_COUNT; i++)
        //        {
        //            for (int j = 0; j < k; j++)
        //            {
        //                n = m_cAntAry_1[i].m_nPath[j];
        //                m = m_cAntAry_1[i].m_nPath[j + 1];
        //                Common.g_Trial_1[n, m] = Common.g_Trial_1[n, m] * Common.ROU + (1 - Common.ROU) * dbTempAry[n, m];  //最新的环境信息素 = 留存的信息素 + 新留下的信息素
        //                Common.g_Trial_1[m, n] = Common.g_Trial_1[n, m];
        //                //if (Common.g_Trial_1[i, j] > Common.MaxTrial_1)
        //                //{
        //                //    Common.g_Trial_1[i, j] = Common.MaxTrial_1;
        //                //}
        //                //if (Common.g_Trial_1[i, j] < Common.MinTrial_1)
        //                //{
        //                //    Common.g_Trial_1[i, j] = Common.MinTrial_1;
        //                //}
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 动态信息素最优更新
        /// </summary>
        //public void DynamicUpdateTrialBest()
        //{
        //    //临时数组，保存各只蚂蚁在两两城市间新留下的信息素
        //    double[,] dbTempAry = new double[Common.N_CITY_COUNT, Common.N_CITY_COUNT];

        //    //先全部设置为0
        //    for (int i = 0; i < Common.N_ANT_COUNT; i++) //计算每只蚂蚁留下的信息素
        //    {
        //        for (int j = 1; j < Common.N_CITY_COUNT; j++)
        //        {
        //            dbTempAry[i, j] = 0.0;
        //        }
        //    }


        //    //计算新增加的信息素,保存到临时数组里
        //    int m = 0;
        //    int n = 0;
        //    for (int j = 1; j < Common.N_CITY_COUNT; j++)
        //    {
        //        m = m_cBestAnt_1.m_nPath[j];
        //        n = m_cBestAnt_1.m_nPath[j - 1];
        //        dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ2 / m_cBestAnt_1.m_dbPathLength;
        //        dbTempAry[m, n] = dbTempAry[n, m];
        //    }

        //    //最后城市和开始城市之间的信息素
        //    n = m_cBestAnt_1.m_nPath[0];
        //    dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ2 / m_cBestAnt_1.m_dbPathLength;
        //    dbTempAry[m, n] = dbTempAry[n, m];



        //    //==================================================================
        //    //更新环境信息素
        //    for (int i = 0; i < Common.N_CITY_COUNT; i++)
        //    {
        //        for (int j = 0; j < Common.N_CITY_COUNT; j++)
        //        {
        //            Common.g_Trial_1[i, j] = Common.g_Trial_1[i, j] * Common.ALPHA + (1 - Common.ALPHA) * dbTempAry[i, j];  //最新的环境信息素 = 留存的信息素 + 新留下的信息素
        //        }
        //    }
        //}

        /// <summary>
        /// 更新种群2的环境信息素
        /// </summary>
        public void UpdateTrial_2()
        {
            //Common.MaxTrial_2 = (1 / Common.ROU) * (1 / m_cBestAnt_2.m_dbPathLength);
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
                    dbTempAry[n, m] = dbTempAry[n, m] + (Common.DBQ / m_cAntAry_2[i].m_dbPathLength) * ballance;
                    //dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / Math.Pow(m_cAntAry_2[i].m_dbPathLength, 2);
                    dbTempAry[m, n] = dbTempAry[n, m];
                }

                //最后城市和开始城市之间的信息素
                n = m_cAntAry_2[i].m_nPath[0];
                dbTempAry[n, m] = dbTempAry[n, m] + (Common.DBQ / m_cAntAry_2[i].m_dbPathLength) * ballance;
                //dbTempAry[n, m] = dbTempAry[n, m] + Common.DBQ / Math.Pow(m_cAntAry_2[i].m_dbPathLength, 2);
                dbTempAry[m, n] = dbTempAry[n, m];
            }

            //==================================================================
            //更新环境信息素
            for (int i = 0; i < Common.N_CITY_COUNT; i++)
            {
                for (int j = 0; j < Common.N_CITY_COUNT; j++)
                {
                    Common.g_Trial_2[i, j] = Common.g_Trial_2[i, j] * Common.ROU + (1 - Common.ROU) * dbTempAry[i, j];  //最新的环境信息素 = 留存的信息素 + 新留下的信息素
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

        /// <summary>
        /// 平衡信息素
        /// </summary>
        private void TrialBallance()
        {
        }

        #endregion

        #region 搜索

        /// <summary>
        /// 蚂蚁种群1进行搜索
        /// </summary>
        public void Search_1()
        {
            for (int i = 0; i < Common.N_ANT_COUNT; i++)
            {
                m_cAntAry_1[i].Search();
            }
            //Parallel.ForEach(m_cAntAry_1, ant => ant.Search());
            
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
            //DynamicUpdateTrial();
            //DynamicUpdateTrialBest();
            
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

            //Parallel.ForEach(m_cAntAry_2, ant => ant.Search());

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
            //ant_1 = new Thread(Search_1);
            //ant_2 = new Thread(Search_2);
            //ant_1.Start();
            //ant_2.Start();
            //ant_1.Join();
            //ant_2.Join();
            Search_1();
            Search_2();
        }

        /// <summary>
        /// 开始搜索
        /// </summary>
        public void Search()
        {
            //char cBuf[256]; //打印信息用
            string strInfo = "";

            bool blFlag = false;
            //Mant = new Thread(MantStart);
            //在迭代次数内进行循环
            for (int i = 0; i < Common.N_IT_COUNT; i++)
            {
                MantStart();

                //蚁群交叉
                if (BestPathLengthList.Count > 2)
                {
                    double ramd = Common.rnd(0.9, 1);
                    if (ramd < BestPathLengthList[BestPathLengthList.Count - 1]
                        / BestPathLengthList[BestPathLengthList.Count - 2])
                    {
                        AntCross();
                    }

                }

                if (i % 10 == 0)
                {
                    UpdateBestTrial();
                    //AntCross();
                }

                localBest_2.m_dbPathLength = Common.DB_MAX;
                for (int a = 0; a < Common.N_ANT_COUNT; a++)
                {
                    if (m_cAntAry_2[a].m_dbPathLength < localBest_2.m_dbPathLength)
                    {
                        localBest_2.m_dbPathLength = m_cAntAry_2[a].m_dbPathLength;
                    }
                }
                Common.CAnt_2List.Add(localBest_2.m_dbPathLength);

                localBest_1.m_dbPathLength = Common.DB_MAX;
                for (int b = 0; b < Common.N_ANT_COUNT; b++)
                {
                    if (m_cAntAry_1[b].m_dbPathLength < localBest_1.m_dbPathLength)
                    {
                        localBest_1.m_dbPathLength = m_cAntAry_1[b].m_dbPathLength;
                        localBest_1.m_nPath = m_cAntAry_1[b].m_nPath;
                    }
                }
                Common.CAnt_1List.Add(localBest_1.m_dbPathLength);

                //Common.CAnt_1List.Add(m_cBestAnt_1.m_dbPathLength);
                //Common.CAnt_2List.Add(m_cBestAnt_2.m_dbPathLength);

                if (m_cBestAnt.m_dbPathLength > m_cBestAnt_2.m_dbPathLength)
                {
                    //m_cBestAnt.m_nPath = m_cBestAnt_2.m_nPath;
                    m_cBestAnt.m_nPath = (int[])INTSergesion.DeepClone(m_cBestAnt_2.m_nPath);
                    m_cBestAnt.m_dbPathLength = m_cBestAnt_2.m_dbPathLength;
                    blFlag = true;
                }
                if (m_cBestAnt.m_dbPathLength > m_cBestAnt_1.m_dbPathLength)
                {
                    m_cBestAnt.m_nPath = (int[])INTSergesion.DeepClone(m_cBestAnt_1.m_nPath);
                    m_cBestAnt.m_dbPathLength = m_cBestAnt_1.m_dbPathLength;
                    blFlag = true;
                }

                //BestPathLengthList.Add(m_cBestAnt.m_dbPathLength);
                //if (BestPathLengthList.Count > 2
                //    && BestPathLengthList[BestPathLengthList.Count - 1] != BestPathLengthList[BestPathLengthList.Count - 2])
                //{
                //    ballance = 1 - (BestPathLengthList[BestPathLengthList.Count - 1] / BestPathLengthList[BestPathLengthList.Count - 2]);
                //    //TrialBallance();
                //}
                //AntCross();
                if (BestPathLengthList.Count == 0)
                {
                    BestPathLengthList.Add(m_cBestAnt.m_dbPathLength);
                }
                else
                {
                    if (BestPathLengthList[BestPathLengthList.Count - 1] != m_cBestAnt.m_dbPathLength)
                    {
                        BestPathLengthList.Add(m_cBestAnt.m_dbPathLength);
                    }
                }

                if (blFlag)
                {
                    //Console.WriteLine("\nall:[{0}]: {1}", i, m_cBestAnt.m_dbPathLength);
                    //for (int j = 0; j < Common.N_CITY_COUNT; j++)
                    //{
                    //    Console.Write("{0}, ", m_cBestAnt.m_nPath[j] + 1);
                    //}
                    //Console.WriteLine();
                    blFlag = false;
                }
            }

            //Console.WriteLine("\nbest path length = {0} ", m_cBestAnt.m_dbPathLength);
            //for (int i = 0; i < Common.N_CITY_COUNT; i++)
            //{
            //    strInfo = String.Format("{0} ", m_cBestAnt.m_nPath[i] + 1);
            //    Console.Write(strInfo);
            //}

            //Console.WriteLine("\nbest_1 path length = {0} ", m_cBestAnt_1.m_dbPathLength);
            //for (int i = 0; i < Common.N_CITY_COUNT; i++)
            //{
            //    strInfo = String.Format("{0} ", m_cBestAnt_1.m_nPath[i] + 1);
            //    Console.Write(strInfo);
            //}

            //Console.WriteLine();

            //Console.WriteLine("best_2 path length = {0} ", m_cBestAnt_2.m_dbPathLength);
            //for (int i = 0; i < Common.N_CITY_COUNT; i++)
            //{
            //    strInfo = String.Format("{0} ", m_cBestAnt_2.m_nPath[i] + 1);
            //    Console.Write(strInfo);
            //}
        }

        /// <summary>
        /// 种群较优解交配
        /// </summary>
        private void AntCross()
        {
            double length1;
            double length2;
            int[] tempPath1_1 = new int[Common.N_CITY_COUNT * 2];
            int[] tempPath2_1 = new int[Common.N_CITY_COUNT * 2];
            int[] tempPath1 = new int[Common.N_CITY_COUNT];
            int[] tempPath2 = new int[Common.N_CITY_COUNT];
            int loopCount = 0;
            while (loopCount < 100)
            {
                int ramdPoint = Common.rnd(10, 50);
                int ChromCount = ramdPoint;

                for (int i = ramdPoint, j = 0; i < Common.N_CITY_COUNT + ramdPoint; i++)
                {
                    tempPath1_1[i] = m_cBestAnt_1.m_nPath[i - ramdPoint];
                    tempPath2_1[i] = m_cBestAnt_2.m_nPath[i - ramdPoint];
                }

                for (int i = 0; i < ramdPoint; i++)
                {
                    tempPath1_1[i] = m_cBestAnt_2.m_nPath[i];
                    tempPath2_1[i] = m_cBestAnt_1.m_nPath[i];
                    tempPath1[i] = m_cBestAnt_2.m_nPath[i];
                    tempPath2[i] = m_cBestAnt_1.m_nPath[i];
                }

                int count;
                for (int i = ramdPoint; i < Common.N_CITY_COUNT + ramdPoint; i++)
                {
                    count = 0;
                    bool flag = true; //默认为没有相同的值

                    //确认是否有相同的值
                    while (count < ramdPoint)
                    {
                        if (tempPath1_1[count] == tempPath1_1[i])
                        {
                            flag = false;
                            break;
                        }
                        count++;
                    }

                    if (flag)
                    {
                        tempPath1[ChromCount] = tempPath1_1[i];
                        ChromCount++;
                    }
                }

                length1 = GetLength(tempPath1);
                if (length1 < m_cBestAnt_1.m_dbPathLength)
                {
                    m_cBestAnt_1.m_dbPathLength = length1;
                    m_cBestAnt_1.m_nPath = (int[])INTSergesion.DeepClone(tempPath1);
                    //Console.WriteLine("Use");
                    //throw new Exception("Success");
                }

                ChromCount = ramdPoint;
                for (int i = ramdPoint; i < Common.N_CITY_COUNT + ramdPoint; i++)
                {
                    count = 0;
                    bool flag = true; //默认为没有相同的值

                    //确认是否有相同的值
                    while (count < ramdPoint)
                    {
                        if (tempPath2_1[count] == tempPath2_1[i])
                        {
                            flag = false;
                            break;
                        }
                        count++;
                    }

                    if (flag)
                    {
                        tempPath2[ChromCount] = tempPath2_1[i];
                        ChromCount++;
                    }
                }

                length2 = GetLength(tempPath2);
                if (length2 < m_cBestAnt_2.m_dbPathLength)
                
                {
                    m_cBestAnt_2.m_dbPathLength = length2;
                    m_cBestAnt_2.m_nPath = (int[])INTSergesion.DeepClone(tempPath2);
                    //Console.WriteLine("Use");
                    //throw new Exception("Success");
                }
                loopCount++;
            }
        }

        /// <summary>
        /// 计算路径长度
        /// </summary>
        /// <param name="PathArray"></param>
        /// <returns></returns>
        private double GetLength(int[] PathArray)
        {
            double PathLength = 0.0; //先把路径长度置0
            int m = 0;
            int n = 0;

            for (int i = 1; i < PathArray.Length; i++)
            {
                m = PathArray[i];
                n = PathArray[i - 1];
                PathLength = PathLength + Common.g_Distance[m, n];
            }

            //加上从最后城市返回出发城市的距离
            n = PathArray[0];
            PathLength = PathLength + Common.g_Distance[m, n];
            return PathLength;
        }

        #endregion
        
        #region IDisposable 成员

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
