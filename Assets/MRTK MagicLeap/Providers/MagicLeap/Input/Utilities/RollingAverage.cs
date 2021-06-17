/// -------------------------------------------------------------------------------
// MRTK - MagicLeap
// https://github.com/magicleap/MRTK-MagicLeap
// -------------------------------------------------------------------------------
//
// MIT License
//
// Copyright(c) 2021 Magic Leap, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// -------------------------------------------------------------------------------
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeap.MRTK.DeviceManagement
{

    ///<summary>
    /// This class can give you an average of data, without having to store the data itself!
    /// For example, instead of using a float[] and averaging all the values, we just track a "sum",
    /// and "numValues (_n)". Then avg = sum/numValues.
    ///</summary>
    public abstract class RollingAverage<T>
    {

        public T Average => _average;

        protected T _sum;
        protected int _n = 0;
        protected int _nLimit = 1000000;
        protected T _average;

        //add another value to the rolling average
        public void AddData(T value)
        {
            _sum = Add(_sum,value);
            _n++;

            _average = Divide(_sum,_n);

            if(_n > _nLimit)
            {
                Reset();
            }
        }

        public abstract T Add(T a, T b);

        public abstract T Divide(T a, int i);

        //clear the data
        public abstract void Reset();

    }

    public class RollingAverageFloat : RollingAverage<float>
    {

        public RollingAverageFloat()
        {
           Reset();
        }

        public override float Add(float a, float b)
        {
            return a + b;
        }

        public override float Divide(float a, int i)
        {
            return a/i;
        }

        public override void Reset()
        {
            _sum = 0.0f;
            _average = 0.0f;
        }
    }

    public class RollingAverageVector3 : RollingAverage<Vector3>
    {

        public RollingAverageVector3()
        {
            Reset();
        }

        public override Vector3 Add(Vector3 a, Vector3 b)
        {
            return a + b;
        }

        public override Vector3 Divide(Vector3 a, int i)
        {
            return a / i;
        }

        public override void Reset()
        {
            _sum = Vector3.zero;
            _average = Vector3.zero;
        }
    }

}
