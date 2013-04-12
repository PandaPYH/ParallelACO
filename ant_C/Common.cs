using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ant_C
{
    public class Common
    {
        #region 参数定义

        public static double ALPHA = 1.0; //启发因子，信息素的重要程度
        public static double BETA = 3.0;   //期望因子，城市间距离的重要程度

        public static double ALPHA_1 = 2.0; //种群1启发因子，信息素的重要程度
        public static double BETA_1 = 4.0;   //种群1期望因子，城市间距离的重要程度

        public static double ALPHA_2 = 1.0; //种群2启发因子，信息素的重要程度
        public static double BETA_2 = 6.0;   //种群2期望因子，城市间距离的重要程度

        public static double ROU = 0.7; //信息素残留参数

        public static int N_ANT_COUNT = 40; //蚂蚁数量
        public static int N_IT_COUNT = 1000; //迭代次数
        public static int N_CITY_COUNT = 76; //城市数量

        public static TSPpoint[] tspPoint = new TSPpoint[N_CITY_COUNT];
        public static double DBQ = 100.0; //总的信息素
        public static double DB_MAX = 10e9; //一个标志数，10的9次方
        public static string filePath = "D:\\eil76.tsp";

        public static List<double> CAnt_1List = new List<double>();
        public static List<double> CAnt_2List = new List<double>();
        public static double MaxTrial_1 = 0.0;
        public static double MinTrial_1 = 0.0;
        public static double MaxTrial_2 = 0.0;
        public static double MinTrial_2 = 0.0;

        public static double[,] g_Trial = new double[N_CITY_COUNT, N_CITY_COUNT]; //两两城市间信息素，就是环境信息素
        public static double[,] g_Trial_1 = new double[N_CITY_COUNT, N_CITY_COUNT];//种群1的环境信息素
        public static double[,] g_Trial_2 = new double[N_CITY_COUNT, N_CITY_COUNT];//种群2的环境信息素
        public static readonly double[,] g_Distance = new double[N_CITY_COUNT, N_CITY_COUNT]; //两两城市间距离

        #endregion
        

        //public Common()
        //{
        //    ReadTspTypeFile(filePath);
        //    N_CITY_COUNT = TSPtype.CityCount;
        //    tspPoint = new TSPpoint[N_CITY_COUNT];
        //    g_Trial = new double[N_CITY_COUNT, N_CITY_COUNT];
        //    g_Distance = new double[N_CITY_COUNT, N_CITY_COUNT];
        //}

        /// <summary>
        /// eil51.tsp城市坐标数据
        /// </summary>
        public static double[]  x_Ary=new double[]
        {
	        37,49,52,20,40,21,17,31,52,51,
	        42,31,5,12,36,52,27,17,13,57,
	        62,42,16,8,7,27,30,43,58,58,
	        37,38,46,61,62,63,32,45,59,5,
	        10,21,5,30,39,32,25,25,48,56,
	        30
        };
    
        public static double[] y_Ary=new double[]
        {
            52,49,64,26,30,47,63,62,33,21,
            41,32,25,42,16,41,23,33,13,58,
            42,57,57,52,38,68,48,67,48,27,
            69,46,10,33,63,69,22,35,15,6,
            17,10,64,15,10,39,32,55,28,37,
            40
        };

        #region 生成随机数

        static int RAND_MAX = 0x7fff; //随机数最大值

        static Random rand = new Random(System.DateTime.Now.Millisecond);

        /// <summary>
        /// 返回指定范围内的随机整数
        /// </summary>
        /// <param name="nLow"></param>
        /// <param name="nUpper"></param>
        /// <returns></returns>
        public static int rnd(int nLow, int nUpper)
        {
            return nLow + (nUpper - nLow) * rand.Next(RAND_MAX) / (RAND_MAX + 1);
        }

        /// <summary>
        /// 返回指定范围内的随机浮点数
        /// </summary>
        /// <param name="dbLow"></param>
        /// <param name="dbUpper"></param>
        /// <returns></returns>
        public static double rnd(double dbLow, double dbUpper)
        {
            double dbTemp = (double)rand.Next(RAND_MAX) / ((double)RAND_MAX + 1.0);
            return dbLow + dbTemp * (dbUpper - dbLow);
        }

        /// <summary>
        /// 返回浮点数四舍五入取整后的浮点数
        /// </summary>
        /// <param name="dbA"></param>
        /// <returns></returns>
        public static double ROUND(double dbA)
        {
            return (double)((int)(dbA + 0.5));
        }

        #endregion

        #region 文件读取

        public static void ReadTspTypeFile(string filePath)
        {
            FileStream fs;
            fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
            StreamReader r = new StreamReader(fs);
            string str;

            while (true)
            {
                str = r.ReadLine();
                string[] temp = str.Split(' ');
                if (temp[0] == "NODE_COORD_SECTION")
                    break;
                switch (temp[0])
                {
                    case "NAME": TSPtype.problemName = temp[2]; break;
                    case "COMMENT": TSPtype.Comment = temp[2]; break;
                    case "TYPE": TSPtype.Type = temp[2]; break;
                    case "DIMENSION": TSPtype.CityCount = Convert.ToInt32(temp[2]); break;
                }
            }
            fs.Close();
        }

        public static int[,] ReadTspPoint(string filePath)
        {
            int cityCount;
            int[,] Mat;

            FileStream fs;
            fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
            StreamReader r = new StreamReader(fs);
            string str;

            while (true)
            {
                str = r.ReadLine();
                string[] temp = str.Split(' ');
                if (temp[0] == "DIMENSION")
                {
                    cityCount = Convert.ToInt32(temp[2]);
                    break;
                }
            }

            Mat = new int[cityCount, 3];

            while (true)
            {
                str = r.ReadLine();
                string[] temp = str.Split(' ');
                if (temp[0] == "NODE_COORD_SECTION")
                {
                    break;
                }
            }

            int count = 0;
            while (true)
            {
                str = r.ReadLine();
                string[] temp = str.Split(' ');
                if (temp[0] == "EOF")
                {
                    break;
                }
                for (int i = 0; i < 3; i++)
                {
                    Mat[count, i] = Convert.ToInt32(temp[i]);
                }
                count++;
            }
            fs.Close();

            return Mat;
        }

        public static void ReadTspPointById(string filePath, int id)
        {
            ReadTspTypeFile(filePath);
            tspPoint = new TSPpoint[N_IT_COUNT];
            FileStream fs;
            fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
            StreamReader r = new StreamReader(fs);
            string str;
            while (true)
            {
                str = r.ReadLine();
                string[] temp = str.Split(' ');
                if (temp[0] == "NODE_COORD_SECTION")
                {
                    break;
                }
            }

            for (int i = 0; i < N_IT_COUNT; i++)
            {
                str = r.ReadLine();
                string[] temp1 = str.Split(' ');
                if (temp1[0] == id.ToString())
                {
                    tspPoint[i].id = Convert.ToInt32(temp1[0]);
                    tspPoint[i].x = Convert.ToInt32(temp1[1]);
                    tspPoint[i].y = Convert.ToInt32(temp1[2]);
                }
            }
            fs.Close();
        }

        public static void ReadTspPointById(string filePath)
        {
            FileStream fs;
            fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
            StreamReader r = new StreamReader(fs);
            string str;
            while (true)
            {
                str = r.ReadLine();
                string[] temp = str.Split(' ');
                if (temp[0] == "NODE_COORD_SECTION")
                {
                    break;
                }
            }

            for (int i = 0; i < N_CITY_COUNT; i++)
            {
                str = r.ReadLine();
                string[] temp1 = str.Split(' ');
                tspPoint[i].id = Convert.ToInt32(temp1[0]);
                tspPoint[i].x = Convert.ToInt32(temp1[1]);
                tspPoint[i].y = Convert.ToInt32(temp1[2]);

            }
            fs.Close();
        }

        #endregion

        /// <summary>
        /// 生成距离矩阵
        /// </summary>
        /// <returns>距离矩阵</returns>
        private static void CreatgDistance()
        {
            for (int i = 0; i < N_CITY_COUNT; i++)
            {
                for (int j = 0; j < N_CITY_COUNT; j++)
                {
                    if (i == j)
                    {
                        g_Distance[i, j] = 0;
                    }
                    else
                    {
                        g_Distance[i, j] = Common.ROUND(Math.Sqrt(Math.Pow((double)(tspPoint[i].x - tspPoint[j].x), 2)
                            + Math.Pow((double)(tspPoint[i].y - tspPoint[j].y), 2)));
                    }
                }
            }
        }

        /// <summary>
        /// 定义数据
        /// </summary>
        public static void Init()
        {
            for (int i = 0; i < N_CITY_COUNT; i++)
            {
                tspPoint[i] = new TSPpoint();
            }
            ReadTspPointById(filePath);
            CreatgDistance();
        }

    }
}
