using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ant_C
{
    [Serializable]
    public class CAnt1 : ICloneable, IAnt
    {   
        /// <summary>
        /// 蚂蚁走的路径 
        /// </summary>     
	    public int[] m_nPath; 
        
        /// <summary>
        /// 没去过的城市
        /// </summary>
        int[] m_nAllowedCity;

        /// <summary>
        /// 当前所在城市编号
        /// </summary>
	    int m_nCurCityNo;
 
        /// <summary>
        /// 已经去过的城市数量
        /// </summary>
	    int m_nMovedCityCount;
        
        /// <summary>
        /// 蚂蚁走过的路径长度
        /// </summary>
        public double m_dbPathLength;

        public double Alpha, Beta;

        public CAnt1()
        {
            m_nPath=new int[Common.N_CITY_COUNT];
            m_nAllowedCity=new int[Common.N_CITY_COUNT]; //没去过的城市
        }

        /// <summary>
        /// 初始化函数，蚂蚁搜索前调用
        /// </summary>
        public void Init()
        {

            for (int i = 0; i < Common.N_CITY_COUNT; i++)
	        {
		        m_nAllowedCity[i]=1; //设置全部城市为没有去过
		        m_nPath[i]=0; //蚂蚁走的路径全部设置为0
	        }

	        //蚂蚁走过的路径长度设置为0
	        m_dbPathLength=0.0; 

	        //随机选择一个出发城市
            m_nCurCityNo = Common.rnd(0, Common.N_CITY_COUNT);

	        //把出发城市保存入路径数组中
	        m_nPath[0]=m_nCurCityNo;

	        //标识出发城市为已经去过了
	        m_nAllowedCity[m_nCurCityNo]=0; 

	        //已经去过的城市数量设置为1
	        m_nMovedCityCount=1; 

        }

        /// <summary>
        /// 选择下一个城市
        /// </summary>
        /// <returns>城市编号</returns>
        public int ChooseNextCity()
        {
	        int nSelectedCity=-1; //返回结果，先暂时把其设置为-1

	        //==============================================================================
	        //计算当前城市和没去过的城市之间的信息素总和
        	
	        double dbTotal=0.0;	
	        double[] prob=new double[Common.N_CITY_COUNT]; //保存各个城市被选中的概率

	        for (int i=0;i<Common.N_CITY_COUNT;i++)
	        {
		        if (m_nAllowedCity[i] == 1) //城市没去过
		        {
                    prob[i] = System.Math.Pow(Common.g_Trial_1[m_nCurCityNo,i], Common.ALPHA_1) * System.Math.Pow(1.0 / Common.g_Distance[m_nCurCityNo,i], Common.BETA_1); //该城市和当前城市间的信息素
			        dbTotal=dbTotal+prob[i]; //累加信息素，得到总和
		        }
		        else //如果城市去过了，则其被选中的概率值为0
		        {
			        prob[i]=0.0;
		        }
	        }

	        //==============================================================================
	        //进行轮盘选择
	        double dbTemp=0.0;
	        if (dbTotal > 0.0) //总的信息素值大于0
	        {
		        dbTemp=Common.rnd(0.0,dbTotal); //取一个随机数

		        for (int i=0;i<Common.N_CITY_COUNT;i++)
		        {
			        if (m_nAllowedCity[i] == 1) //城市没去过
			        {
				        dbTemp=dbTemp-prob[i]; //这个操作相当于转动轮盘，如果对轮盘选择不熟悉，仔细考虑一下
				        if (dbTemp < 0.0) //轮盘停止转动，记下城市编号，直接跳出循环
				        {
					        nSelectedCity=i;
					        break;
				        }
			        }
		        }
	        }

	        //==============================================================================
	        //如果城市间的信息素非常小 ( 小到比double能够表示的最小的数字还要小 )
	        //那么由于浮点运算的误差原因，上面计算的概率总和可能为0
	        //会出现经过上述操作，没有城市被选择出来
	        //出现这种情况，就把第一个没去过的城市作为返回结果
        	
	        //题外话：刚开始看的时候，下面这段代码困惑了我很长时间，想不通为何要有这段代码，后来才搞清楚。
	        if (nSelectedCity == -1)
	        {
                for (int i = 0; i < Common.N_CITY_COUNT; i++)
		        {
			        if (m_nAllowedCity[i] == 1) //城市没去过
			        {
				        nSelectedCity=i;
				        break;
			        }
		        }
	        }

	        //==============================================================================
	        //返回结果，就是城市的编号
	        return nSelectedCity;
        }

        /// <summary>
        /// 蚂蚁在城市间移动
        /// </summary>
        public void Move()
        {
	        int nCityNo=ChooseNextCity(); //选择下一个城市

	        m_nPath[m_nMovedCityCount]=nCityNo; //保存蚂蚁走的路径
	        m_nAllowedCity[nCityNo]=0;//把这个城市设置成已经去过了
	        m_nCurCityNo=nCityNo; //改变当前所在城市为选择的城市
	        m_nMovedCityCount++; //已经去过的城市数量加1
        }

        /// <summary>
        /// 计算蚂蚁走过的路径长度
        /// </summary>
        public void CalPathLength()
        {

	        m_dbPathLength=0.0; //先把路径长度置0
	        int m=0;
	        int n=0;

	        for (int i=1;i<Common.N_CITY_COUNT;i++)
	        {
		        m=m_nPath[i];
		        n=m_nPath[i-1];
		        m_dbPathLength=m_dbPathLength+Common.g_Distance[m,n];
	        }

	        //加上从最后城市返回出发城市的距离
	        n=m_nPath[0];
            m_dbPathLength = m_dbPathLength + Common.g_Distance[m,n];	

        }


        /// <summary>
        /// 蚂蚁进行搜索一次
        /// </summary>
        public void Search()
        {
	        Init(); //蚂蚁搜索前，先初始化

	        //如果蚂蚁去过的城市数量小于城市数量，就继续移动
	        while (m_nMovedCityCount < Common.N_CITY_COUNT)
	        {
		        Move();
	        }

	        //完成搜索后计算走过的路径长度
	        CalPathLength();
        }



        public object Clone()
        {
            MemoryStream ms = new MemoryStream();

            //以二进制格式进行序列化          

            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(ms, this);

            //反序列化当前实例到一个object    

            ms.Seek(0, 0);

            object obj = bf.Deserialize(ms);

            //关闭内存流            

            ms.Close();

            return obj;     
        }
    }
}
