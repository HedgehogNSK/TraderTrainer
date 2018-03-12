using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

namespace Hedge
{
    namespace Tools
    {
        static public class  ArrayTools
        {
            //Ищет ближайшее значение к искомому. Если искомое key меньше минимального вернёт -1
            static public int BinarySearch<T,T1>(T[] array, T1 key) where T: IComparable//,bool descendingOrder
            {
                int left = 0;
                int right = array.Length;
                int mid;
                
                while (!(left >= right))
                {
                    mid = left + (right - left) / 2;

                    if (array[left].Equals(key))
                        return left;

                    if (array[mid].Equals(key))
                    {
                        if (mid == left + 1)
                            return mid;
                        else
                            right = mid + 1;
                    }

                    else
                    {
                        if (array[mid].CompareTo(key) > 0)// ^ descendingOrder)
                            right = mid;
                        else
                            left = mid + 1;
                    }
                }

                return left-1;
            }

          /*  static int Find_Wrapper(int[] array, int key)
            {
                if (array.Length == 0)
                    return -1;

                bool descendingOrder = array[0] > array[array.Length - 1];
                return Find<int>(array, key);
            }*/

        }
    }
}