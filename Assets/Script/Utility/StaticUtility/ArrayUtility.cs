using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayUtility 
{
	public static void Shuffle<T>(this T[] list)  
	{  
		int n = list.Length;  
		while (n > 1) 
		{  
			n--;
            int k = RandomUtility.GetRandomInt(0, n + 1);

            T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}
    }
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = RandomUtility.GetRandomInt(0, n + 1);

            var temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }
}
