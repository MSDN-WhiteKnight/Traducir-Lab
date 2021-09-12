/* Traducir Lab
 * Copyright (c) 2021,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight)
 * License: MIT */
using System;
using System.Collections.Generic;
using System.Text;

namespace Traducir.Core.TextAnalysis
{
    public class DataVector // класс представляет один вектор классицирумого множества
    {
        public DataVector()
        {
            Name = string.Empty;
            Values = new List<double>(100);
        }

#pragma warning disable CA2227 // Collection properties should be read only
        public List<double> Values { get; set; }// список значений вектора
#pragma warning restore CA2227 // Collection properties should be read only

        public string Name { get; set; } // название вектора

        public static double Distance(DataVector v1, DataVector v2)// вычисление расстояния между двумя векторами
        {
            double d = 0.0;
            int i;
            if (v1.Values.Count != v2.Values.Count)
            {
                throw new Exception(
                    "Нельзя найти расстояния для векторов разных метрических пространств!");
            }

            for (i = 0; i < v1.Values.Count; i++)
            {
                d += Math.Pow(v1.Values[i] - v2.Values[i], 2);
            }

            return Math.Sqrt(d);
        }

        public DataVector Copy()// создание копии текущего вектора
        {
            DataVector v = new DataVector();
            v.Values = new List<double>(this.Values.Count);
            int i;

            for (i = 0; i < this.Values.Count; i++)
            {
                v.Values.Add(this.Values[i]);
            }

            v.Name = this.Name;
            return v;
        }
    }
}
