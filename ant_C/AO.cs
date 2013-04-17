using System;
using System.Collections.Generic;
using System.Text;

namespace ant_C
{

    class AO
    {
        static void Main(string[] args)
        {
            //CTsp tsp = new CTsp();
            //tsp.InitData();
            //tsp.Search();

            //string strInfo = "";

            //strInfo = String.Format("best path length = {0} ", tsp.m_cBestAnt.m_dbPathLength);
            //Console.WriteLine(strInfo);

            //for (int i = 0; i < Common.N_CITY_COUNT; i++)
            //{
            //    strInfo = String.Format("{0} ", tsp.m_cBestAnt.m_nPath[i] + 1);
            //    Console.Write(strInfo);
            //}
            double length;
            double avglength = 0.0;
            for (int i = 0; i < 10; i++)
            {
                MTsp mtsp = new MTsp();
                mtsp.InitData();
                mtsp.Search();
                length = mtsp.m_cBestAnt.m_dbPathLength;
                Console.WriteLine("[{0}]: {1}", i, length);
                avglength += length;
            }

            Console.WriteLine("avg: {0}", avglength / 10);


            Console.Read();

        }
    }
}
