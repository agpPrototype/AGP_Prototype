using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AGPMonoBehavior : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public delegate int GetTypeDelegate();

    public class Int : IComparable
    {
        public int value;

        public Int(int val)
        {
            value = val;
        }

        int IComparable.CompareTo(object obj)
        {
            Int other = (Int)obj;
            if (value > other.value)
            {
                return 1;
            }
            else if (value < other.value)
            {
                return -1;
            }
            else
                return 0;
        }

    }

}
