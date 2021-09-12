/*Модуль таксономии.
 * 
 * Использование:
 * 1) Создать новый объект-классификатор Tax
 * 2) Заполнить поля R и InputSet входными данными
 * 3) Запустить метод Classify
 * 4) Возвращенное значение - список таксонов
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Classifier
{
    public class Tax //объект-классификатор
    {
        //входные параметры:
        public List<VECTOR> InputSet=null;//множество классифицируемых векторов
        public double R=0.0;//пороговое расстояние

        public Tax(List<VECTOR> input, double r)
        {
            this.InputSet = input;
            this.R = r;
        }

        void ProcessVector(VECTOR v, List<VECTOR> group)
        //обрабатывает вектор, находя ближайшие к нему в исходном множестве и включая их в таксон 
            //(функция рекурсивная)
        {

            if (this.InputSet == null) throw new Exception("Не задано входное множество");
            if(this.R<=0.0||Double.IsNaN(this.R)||Double.IsInfinity(this.R))
                throw new Exception("Неверное пороговое расстояние");

            VECTOR w; 
            int i=0;//счетчик, пробегающий все значения в исходном множестве
            double d;
            List<VECTOR> list = new List<VECTOR>();//список ближайших векторов

            group.Add(v);//добавляем вектор в таксон           

            while (true)
            {
                if (i >= InputSet.Count) break;//если пройдено все множество, выходим
                d = VECTOR.Distance(v, InputSet[i]);//находим расстояние от обрабатываемого вектора до текущего
                if (d <= this.R)//если расстояние не превышает пороговое...
                {
                    w = InputSet[i].Copy();//копируем текущий вектор
                    InputSet.RemoveAt(i);//убираем его из дальнейшего рассмотрения
                    list.Add(w);//включаем его в список ближайших векторов
                }
                else //если расстояние превышает пороговое...
                {
                    i++;//пропускаем вектор и переходим к следующему вектору исходного множества
                }
            }

            if (list.Count == 0) return;//если вектора перестали находится, завершение рекурсивной функции

            for (i = 0; i < list.Count; i++)
            {
                ProcessVector(list[i], group);//обрабатываем каждый из найденных векторов...
            }
        }

        public List<GROUP> Classify()//запуск процесса классификации
        {
            List<GROUP> groups=new List<GROUP>();//наш список таксонов
            GROUP gr;
            VECTOR v;

            int k = 0;
            while (true)
            {
                gr = new GROUP();//создаем новый таксон
                v = InputSet[0].Copy();//берем первый вектор исходного множества и копируем его
                InputSet.RemoveAt(0);//убираем этот вектор из дальнейшего рассмотрения
                ProcessVector(v, gr.vectors);//обрабатываем этот вектор (запуск рекурсивного процесса)
                groups.Add(gr);//добавляем заполненный таксон в выходной список
                if (InputSet.Count == 0) break;//если в исходном множестве не осталось векторов - процесс завершен

                k++;
                if (k > 10000000)//что-то пошло не так? 
                {
                    throw new Exception("Слишком много итераций!");//в эту ветку программа не должна заходить
                }

            }
            
            return groups;//возвращенное значение - список таксонов
        }
    }

    public class GROUP //класс, представляющий таксон, состоящий из одного или нескольких векторов
    {
       public List<VECTOR> vectors=new List<VECTOR>();//список векторов таксона

       public override string ToString()//формирование текстового описания таксона
       {
           string s = "";
           int i;
           s += vectors[0].name;
           for (i = 1; i < vectors.Count; i++)
           {
               s += ", "+vectors[i].name;//добавляем название каждого вектора к строке               
           }

           return s;
       }

    }

}
