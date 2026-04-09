using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AciModule.Domain.Shared
{
    public class GetRandom
    {
       /// <summary>
       /// 判断数组是否重复，并且重新获取指定范围内的随机数
       /// </summary>
       /// <param name="arrNum">判断的数组</param>
       /// <param name="minValue">下限</param>
       /// <param name="maxValue">上限</param>
       /// <returns></returns>
        public static int GetId(int[] arrNum,int minValue=1,int maxValue=1000)
        {
            Random ra = new Random(unchecked((int)DateTime.Now.Ticks));
            int value = 0;
            for (int i = 0; i < arrNum.Length; i++)
            {
               int tmp = ra.Next(minValue, maxValue); //随机取数,取指定范围内的随机数，例如Next(100,1000),取从100开始到1000的随机数
                value = getNum(arrNum, tmp, ra); //得到的值取出值赋到给变量
                if (value != 0) break;
            }
            return value;

        }
        /// <summary>
        ///  递归，用它来检测生成的随机数是否有重复，如果取出来的数字和已取得的数字有重复就重新随机获取。
        /// </summary>
        /// <param name="arrNum">判断的数组</param>
        /// <param name="tmp">产生的随机数</param>
        /// <param name="ra">随机数</param>
        /// <returns></returns>
        private static int getNum(int[] arrNum, int tmp, Random ra)
        {
            int n = 0;
            while (n <= arrNum.Length - 1)
            {
                if (arrNum[n] == tmp) //利用循环判断是否有重复
                {
                    int minValue = arrNum.Max();//重复就以当前，正在执行的任务中取到的最大的值为起始值，再取1000个
                    int maxValue = 1000;
                    tmp = ra.Next(minValue, maxValue); //重新随机获取。
                    getNum(arrNum, tmp, ra);//递归:如果取出来的数字和已取得的数字有重复就重新随机获取。
                }
                n++;
            }
            return tmp;
        }
        /// <summary>
        /// 检查所有的数，如果存在跟tmp一样的，则返回0，否则返回tmp。
        /// </summary>
        /// <param name="arrNum"></param>
        /// <param name="tmp"></param>
        /// <returns></returns>
        public static int getIds(int[] arrNum,  int tmp)
        {
            int n = 0;
            while (n <= arrNum.Length - 1)
            {
                if (arrNum[n] == tmp)
                {
                    return 0;
                }
                n++;
            }
            return tmp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="IdHasUsed"></param>
        /// <param name="tmp"></param>
        /// <returns></returns>
        public static int getIds(List<int>IdHasUsed, int min,int max)
        {
            for(int i=min ;i<max;i++)
            {
                if (!IdHasUsed.Contains(i))
                    return i;
            }
            return 0;
        }

        /// <summary>
        /// 用种子产生相对不重复的随机数
        /// </summary>
        /// <returns></returns>
        public static int GenerateRandomSeed()
        {
            int num = 0;
            num = Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
            if (num <= 0)
            {
                while (num == 0)
                {
                    num = Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
                }
            }
            while (num >= 65535)
            {
                num = Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
            }
            return num;
            //return Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
            //  return (int)DateTime.Now.Ticks;
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="intArray"></param>
        /// <param name="nLower"></param>
        /// <param name="nUpper"></param>
        public static void Sort(int[] intArray, int nLower, int nUpper)
         {
             if(nLower<nUpper)
             {
                 int nSplit = Partition(intArray, nLower, nUpper);
                 ///递归排序
                 Sort(intArray, nLower, nSplit - 1);
                 Sort(intArray, nSplit + 1, nUpper);
             }
        }
        /// <summary>
       /// 方法参数：原始数组、第一个元素位置、最后元素位置
       /// 方法功能：完成一趟快速排序
       /// </summary>
       /// <param name="intArray"></param>
       /// <param name="nLower"></param>
       /// <param name="nUpper"></param>
       /// <returns></returns>
       public static int Partition(int[] intArray, int nLower, int nUpper)
       {
           int nLeft = nLower + 1;
           ///以数组第一个元素值作为支点
           int nPivot = intArray[nLower];
           int nRight = nUpper;

           int nSwap;
           while(nLeft <= nRight)
           {
               ///从左向右寻找大于支点元素
               while(nLeft <= nRight && intArray[nLeft] < nPivot)
                   nLeft++;
               ///从右向左寻找小于支点元素
               while(nLeft <= nRight && intArray[nRight] >= nPivot)
                   nRight--;
               ///交换nLeft和nRight位置元素值
               if(nLeft<nRight)
               {
                   nSwap = intArray[nLeft];
                   intArray[nLeft] = intArray[nRight];
                   intArray[nRight] = nSwap;
                   nLeft++;
                   nRight--;
               }
           }
           ///以intArray[nRight]为新支点 
           nSwap = intArray[nLower];
           intArray[nLower] = intArray[nRight];
           intArray[nRight] = nSwap;
           return nRight;
       }
    }
}
