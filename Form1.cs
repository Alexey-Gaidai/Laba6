using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Laba6
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox1.Text = textBox2.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            dataGridView2.Columns.Clear();
            Random rnd = new Random();
            for (int i = 0; i < Convert.ToInt32(textBox1.Text); i++)
            {
                dataGridView1.Columns.Add("", "");
            }
            dataGridView1.Rows.Add(Convert.ToInt32(textBox1.Text)-1);
            dataGridView2.Columns.Add("", "");
            dataGridView2.Rows.Add(Convert.ToInt32(textBox1.Text)-1);

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                for (int j = 0; j < dataGridView1.Columns.Count; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = rnd.Next(0, 100);
                }
                dataGridView2.Rows[i].Cells[0].Value = rnd.Next(0, 100);
            }
        }

        private void gauss_button_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count != dataGridView2.Rows.Count)
            {
                MessageBox.Show("Размер матриц несоответствующий");
                return;
            }

            if (dataGridView2.Columns.Count != 1)
            {
                MessageBox.Show("Размер матрицы B несоответствующий");
                return;
            }

            var gaussSolver = new GausMethod((uint)dataGridView1.Rows.Count, (uint)dataGridView1.Columns.Count);

            for (int row = 0; row < dataGridView1.Rows.Count; row++)
            {
                for (int column = 0; column < dataGridView1.Columns.Count; column++)
                {
                    gaussSolver.Matrix[row][column] = Convert.ToInt32(dataGridView1.Rows[row].Cells[column].Value);
                }
            }

            for (int row = 0; row < dataGridView2.Rows.Count; row++)
            {
                gaussSolver.RightPart[row] = Convert.ToInt32(dataGridView2.Rows[row].Cells[0].Value);
            }

            int error = gaussSolver.SolveMatrix();

            if (error == 0)
            {
                dataGridView3.Rows.Clear();
                dataGridView3.Columns.Clear();

                dataGridView3.Columns.Add("", "");

                for (int row = 0; row < gaussSolver.Answer.Length; row++)
                {
                    dataGridView3.Rows.Add();
                    dataGridView3.Rows[row].Cells[0].Value = Math.Round(gaussSolver.Answer[row],3);
                }
            }
            else if (error == 1)
            {
                MessageBox.Show("Нет решения");
            }
            else if (error == 2)
            {
                MessageBox.Show("Множество решений");
            }
        }
    }
    
    class GausMethod
    {
        public uint RowCount;
        public uint ColumCount;
        public double[][] Matrix { get; set; }
        public double[] RightPart { get; set; }
        public double[] Answer { get; set; }

        public GausMethod(uint Row, uint Colum)
        {
            RightPart = new double[Row];
            Answer = new double[Row];
            Matrix = new double[Row][];
            for (int i = 0; i < Row; i++)
                Matrix[i] = new double[Colum];
            RowCount = Row;
            ColumCount = Colum;

            //обнулим массив
            for (int i = 0; i < Row; i++)
            {
                Answer[i] = 0;
                RightPart[i] = 0;
                for (int j = 0; j < Colum; j++)
                    Matrix[i][j] = 0;
            }
        }

        private void SortRows(int SortIndex)
        {

            double MaxElement = Matrix[SortIndex][SortIndex];
            int MaxElementIndex = SortIndex;
            for (int i = SortIndex + 1; i < RowCount; i++)
            {
                if (Matrix[i][SortIndex] > MaxElement)
                {
                    MaxElement = Matrix[i][SortIndex];
                    MaxElementIndex = i;
                }
            }

            //теперь найден максимальный элемент ставим его на верхнее место
            if (MaxElementIndex > SortIndex)//если это не первый элемент
            {
                double Temp;

                Temp = RightPart[MaxElementIndex];
                RightPart[MaxElementIndex] = RightPart[SortIndex];
                RightPart[SortIndex] = Temp;

                for (int i = 0; i < ColumCount; i++)
                {
                    Temp = Matrix[MaxElementIndex][i];
                    Matrix[MaxElementIndex][i] = Matrix[SortIndex][i];
                    Matrix[SortIndex][i] = Temp;
                }
            }
        }

        public int SolveMatrix()
        {
            if (RowCount != ColumCount)
                return 1; //нет решения

            for (int i = 0; i < RowCount - 1; i++)
            {
                SortRows(i);
                for (int j = i + 1; j < RowCount; j++)
                {
                    if (Matrix[i][i] != 0) //если главный элемент не 0, то производим вычисления
                    {
                        double MultElement = Matrix[j][i] / Matrix[i][i];
                        for (int k = i; k < ColumCount; k++)
                            Matrix[j][k] -= Matrix[i][k] * MultElement;
                        RightPart[j] -= RightPart[i] * MultElement;
                    }
                    //для нулевого главного элемента просто пропускаем данный шаг
                }
            }

            //ищем решение
            for (int i = (int)(RowCount - 1); i >= 0; i--)
            {
                Answer[i] = RightPart[i];

                for (int j = (int)(RowCount - 1); j > i; j--)
                    Answer[i] -= Matrix[i][j] * Answer[j];

                if (Matrix[i][i] == 0)
                    if (RightPart[i] == 0)
                        return 2; //множество решений
                    else
                        return 1; //нет решения

                Answer[i] /= Matrix[i][i];
            }
            return 0;
        }
    }

}
