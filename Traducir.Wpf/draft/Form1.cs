using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
/*
15 chars: 40
80 chars: 120*/

namespace Classifier
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //добавление тестового набора векторов в таблицу
            dataGridView1.Rows.Add();
            dataGridView1.Rows[0].Cells[0].Value = "Hide hot network questions";
            dataGridView1.Rows[0].Cells[2].Value = (2.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[1].Cells[0].Value = "hide hot Network questions";
            dataGridView1.Rows[1].Cells[2].Value = (3.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[2].Cells[0].Value = "Download activity data stuff";
            dataGridView1.Rows[2].Cells[2].Value = (3.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[3].Cells[0].Value = "You need at least 10 reputation.";
            dataGridView1.Rows[3].Cells[2].Value = (4.0).ToString();
            /*dataGridView1.Rows.Add();
            dataGridView1.Rows[4].Cells[1].Value = (3.0).ToString();
            dataGridView1.Rows[4].Cells[2].Value = (4.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[5].Cells[1].Value = (2.0).ToString();
            dataGridView1.Rows[5].Cells[2].Value = (5.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[6].Cells[1].Value = (6.0).ToString();
            dataGridView1.Rows[6].Cells[2].Value = (1.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[7].Cells[1].Value = (5.0).ToString();
            dataGridView1.Rows[7].Cells[2].Value = (2.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[8].Cells[1].Value = (6.0).ToString();
            dataGridView1.Rows[8].Cells[2].Value = (2.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[9].Cells[1].Value = (7.0).ToString();
            dataGridView1.Rows[9].Cells[2].Value = (2.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[10].Cells[1].Value = (5.0).ToString();
            dataGridView1.Rows[10].Cells[2].Value = (1.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[11].Cells[1].Value = (7.0).ToString();
            dataGridView1.Rows[11].Cells[2].Value = (3.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[12].Cells[1].Value = (5.0).ToString();
            dataGridView1.Rows[12].Cells[2].Value = (4.0).ToString();
            dataGridView1.Rows.Add();
            dataGridView1.Rows[13].Cells[1].Value = (5.0).ToString();
            dataGridView1.Rows[13].Cells[2].Value = (5.0).ToString();*/
                        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double r;
            bool b;
            b = Double.TryParse(textBox1.Text, out r);
            if (b == false)
            {
                MessageBox.Show(this, "Введите нормальное пороговое расстояние!","Ошибка");
                return;
            }

            if (Double.IsInfinity(r) || Double.IsNaN(r))
            {
                MessageBox.Show(this, "Пороговое расстояние должно быть действительным числом", "Ошибка");
                return;
            }

            if (r <= 0)
            {
                MessageBox.Show(this, "Пороговое расстояние должно быть положительным", "Ошибка");
                return;
            }

            int i;
            List<VECTOR> vectors = new List<VECTOR>(dataGridView1.Rows.Count);
            VECTOR v;
            int j;
            
            try
            {
                for (i = 0; i < dataGridView1.Rows.Count-1; i++) //считывание векторов из таблицы
                {
                    v = new VECTOR();
                    v.name = (string)dataGridView1.Rows[i].Cells[0].Value;//первый столбец - имя вектора

                    string str = v.name;

                    for (j = 0; j < str.Length; j++)
                    {
                        v.values.Add((double)str[j]);
                    }

                    if (v.values.Count < 300)
                    {
                        while (true)
                        {
                            v.values.Add(0.0);
                            if (v.values.Count >= 300) break;
                        }
                    }
                                        
                    vectors.Add(v);
                }


                Tax tax = new Tax(vectors, r);//новый объект-классификатор
                List<GROUP> groups = tax.Classify();//запуск процесса классификации

                //формируем текст для отображения результата:
                string s = "При пороговом расстоянии "+r.ToString()+" выделено "+groups.Count+" таксонов:";
                s += Environment.NewLine;
                for (i = 0; i < groups.Count; i++)//добавляем список векторов каждого таксона в текст
                {
                    s += "- Таксон #" + (i + 1).ToString() + " содержит ";
                    s += groups[i].vectors.Count + " векторов: ";
                    s += groups[i].ToString();
                    s += Environment.NewLine;
                }

                MessageBox.Show(this, s, "Результат классификации");//отображаем результат классификации пользователю

            }
            catch (Exception ex)
            {
                MessageBox.Show(this,ex.ToString(), "Ошибка");
            }

        }

        private void bAddColumn_Click(object sender, EventArgs e)//добавление новой колонки в таблицу
        {
            int c = dataGridView1.ColumnCount;
            string s="Признак " + c.ToString();
            dataGridView1.Columns.Add(s, s);
        }

        private void bDropColumn_Click(object sender, EventArgs e) //удаление колонки из таблицы
        {
            int c = dataGridView1.ColumnCount;
            if (c <= 2) return;
            dataGridView1.Columns.RemoveAt(c - 1);
        }
    }
}
