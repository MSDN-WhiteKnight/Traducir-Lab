/* Traducir Lab
 * Copyright (c) 2021,  MSDN.WhiteKnight (https://github.com/MSDN-WhiteKnight)
 * License: MIT */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
#pragma warning disable CA2227 // Collection properties should be read only

/*Модуль таксономии.
 *
 * Использование:
 * 1) Создать новый объект-классификатор Tax
 * 2) Заполнить поля R и InputSet входными данными
 * 3) Запустить метод Classify
 * 4) Возвращенное значение - список таксонов
*/

namespace Traducir.Core.TextAnalysis
{
    public class Tax // объект-классификатор
    {
        public Tax(List<DataVector> input, double r)
        {
            this.InputSet = input;
            this.R = r;
        }

        // входные параметры:
        public List<DataVector> InputSet { get; set; }// множество классифицируемых векторов

        public double R { get; set; }// пороговое расстояние

        private void ProcessVector(DataVector v, List<DataVector> group)
        {
            // обрабатывает вектор, находя ближайшие к нему в исходном множестве и включая их в таксон
            // (функция рекурсивная)
            if (this.InputSet == null)
            {
                throw new Exception("Не задано входное множество");
            }

            if (this.R <= 0.0 || double.IsNaN(this.R) || double.IsInfinity(this.R))
            {
                throw new Exception("Неверное пороговое расстояние");
            }

            DataVector w;
            int i = 0; // счетчик, пробегающий все значения в исходном множестве
            double d;
            List<DataVector> list = new List<DataVector>(group.Count); // список ближайших векторов

            group.Add(v); // добавляем вектор в таксон

            while (true)
            {
                if (i >= InputSet.Count)
                {
                    break; // если пройдено все множество, выходим
                }

                d = DataVector.Distance(v, InputSet[i]); // находим расстояние от обрабатываемого вектора до текущего
                if (d <= this.R)
                {
                    // если расстояние не превышает пороговое...
                    w = InputSet[i].Copy(); // копируем текущий вектор
                    InputSet.RemoveAt(i); // убираем его из дальнейшего рассмотрения
                    list.Add(w); // включаем его в список ближайших векторов
                }
                else
                {
                    // если расстояние превышает пороговое...
                    i++; // пропускаем вектор и переходим к следующему вектору исходного множества
                }
            }

            if (list.Count == 0)
            {
                return; // если вектора перестали находится, завершение рекурсивной функции
            }

            for (i = 0; i < list.Count; i++)
            {
                ProcessVector(list[i], group); // обрабатываем каждый из найденных векторов...
            }
        }

        public List<VectorGroup> Classify()// запуск процесса классификации
        {
            List<VectorGroup> groups = new List<VectorGroup>(); // наш список таксонов
            VectorGroup gr;
            DataVector v;

            int k = 0;
            while (true)
            {
                gr = new VectorGroup(); // создаем новый таксон
                v = InputSet[0].Copy(); // берем первый вектор исходного множества и копируем его
                InputSet.RemoveAt(0); // убираем этот вектор из дальнейшего рассмотрения
                ProcessVector(v, gr.Vectors); // обрабатываем этот вектор (запуск рекурсивного процесса)
                groups.Add(gr); // добавляем заполненный таксон в выходной список

                if (InputSet.Count == 0)
                {
                    break; // если в исходном множестве не осталось векторов - процесс завершен
                }

                k++;
                if (k > 10000000)
                {
                    // что-то пошло не так?
                    throw new Exception("Слишком много итераций!"); // в эту ветку программа не должна заходить
                }
            }

            return groups; // возвращенное значение - список таксонов
        }
    }

    public class VectorGroup // класс, представляющий таксон, состоящий из одного или нескольких векторов
    {
        public VectorGroup()
        {
            Vectors = new List<DataVector>(50);
        }

        public List<DataVector> Vectors { get; set; }// список векторов таксона

        public static string PrintList(IList<VectorGroup> groups, double r)
        {
            StringBuilder sb = new StringBuilder(1000);
            StringWriter wr = new StringWriter(sb);

            wr.Write("При пороговом расстоянии ");
            wr.Write(r.ToString(CultureInfo.InvariantCulture));
            wr.Write(" выделено ");
            wr.Write(groups.Count);
            wr.Write(" таксонов:");
            wr.WriteLine();

            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].Vectors.Count <= 1)
                {
                    // ignore taxons made up from a single item
                    continue;
                }

                string first = groups[i].Vectors[0].Name;
                bool unique = false;

                for (int j = 1; j < groups[i].Vectors.Count; j++)
                {
                    if (!first.Equals(groups[i].Vectors[j].Name, StringComparison.Ordinal))
                    {
                        unique = true;
                        break;
                    }
                }

                if (!unique)
                {
                    // ignore taxons made up from equal items
                    continue;
                }

                // добавляем список векторов каждого таксона в текст
                wr.Write("- Таксон #");
                wr.Write((i + 1).ToString(CultureInfo.InvariantCulture));
                wr.Write(" содержит ");
                wr.Write(groups[i].Vectors.Count + " векторов: ");
                wr.Write(groups[i].ToString());
                wr.WriteLine();
            }

            wr.Flush();
            return sb.ToString();
        }

        public override string ToString()// формирование текстового описания таксона
        {
            string s = string.Empty;
            int i;
            s += Vectors[0].Name;
            for (i = 1; i < Vectors.Count; i++)
            {
                s += ", " + Vectors[i].Name; // добавляем название каждого вектора к строке
            }

            return s;
        }
    }
}
