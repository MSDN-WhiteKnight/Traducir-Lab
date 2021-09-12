using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classifier
{
    public class VECTOR //класс представляет один вектор классицирумого множества
    {
        public List<double> values=new List<double>();//список значений вектора
        public string name="";//название вектора

        public VECTOR Copy()//создание копии текущего вектора
        {
            VECTOR v = new VECTOR();
            v.values = new List<double>(this.values.Count);
            int i;
            for (i = 0; i < this.values.Count; i++)
            {
                v.values.Add(this.values[i]);
            }
            v.name = this.name;
            return v;

        }

        public static double Distance(VECTOR v1, VECTOR v2)//вычисление расстояния между двумя векторами
        {
            double d = 0.0;
            int i;
            if (v1.values.Count != v2.values.Count)
            {
                throw new Exception(
                    "Нельзя найти расстояния для векторов разных метрических пространств!"
                    );
            }
            for (i = 0; i < v1.values.Count; i++)
            {
                d += Math.Pow(v1.values[i] - v2.values[i], 2);

            }
            return Math.Sqrt(d);
        }
    }
}
