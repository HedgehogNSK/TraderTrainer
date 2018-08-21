using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ArrayExtensions
{

    public static T RandomItem<T>(this T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }

    public static T[] RandomArray<T>(this T[] array, int count)
    {
        T[] newArray = new T[count];
        for (int i = 0; i < count; i++)
        {
            if (count < array.Length)
            {
                T temp = array.RandomItem();
                while (newArray.Contains(temp))
                {
                    temp = array.RandomItem();
                }
                newArray[i] = temp;
            }
            else
            {
                newArray[i] = array.RandomItem();
            }
        }
        return newArray;
    }
    public static T[] RandomArray<T>(this T[] array, int count, bool canRepeat)
    {
        T[] newArray = new T[count];
        for (int i = 0; i < count; i++)
        {
            if (count < array.Length)
            {
                T temp = array.RandomItem();
                if (!canRepeat)
                {
                    while (newArray.Contains(temp))
                    {
                        temp = array.RandomItem();
                    }
                }
                newArray[i] = temp;
            }
            else
            {
                newArray[i] = array.RandomItem();
            }
        }
        return newArray;
    }

    public static bool Contains<T>(this T[] array, T obj)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                if (array[i].Equals(obj))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static Transform[] Childs(this Transform transform)
    {
        Transform[] childs = new Transform[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            childs[i] = transform.GetChild(i);
        }
        return childs;
    }
}
