using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace Laba6
{
    public partial class Form1 : Form
    {
        private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private const string SpreadsheetId = "1Kcvpqi-I6wY0HSFGehgdVp_tS70Fk2KQroZT39Z8S5Q";
        private const string GoogleCredentialsFileName = "google-credentials.json";
        private const string ReadRange = "Лист1!A:Z";

        double[,] abc;

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
                    dataGridView1.Rows[i].Cells[j].Value = rnd.Next(1, 10);
                }
                dataGridView2.Rows[i].Cells[0].Value = rnd.Next(1, 10);
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

            GausMethod gaussSolver = new GausMethod((uint)dataGridView1.Rows.Count, (uint)dataGridView1.Columns.Count);

            for (int row = 0; row < dataGridView1.Rows.Count; row++)
            {
                for (int column = 0; column < dataGridView1.Columns.Count; column++)
                {
                    gaussSolver.a_matrix[row][column] = Convert.ToInt32(dataGridView1.Rows[row].Cells[column].Value);
                }
            }

            for (int row = 0; row < dataGridView2.Rows.Count; row++)
            {
                gaussSolver.RightPart[row] = Convert.ToInt32(dataGridView2.Rows[row].Cells[0].Value);
            }

            int error = gaussSolver.Solvea_matrix();

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

        private void quad_square_button_Click(object sender, EventArgs e)
        {
            double[,] a = new double[dataGridView1.Rows.Count, dataGridView1.Columns.Count];
            double[] b = new double[dataGridView2.Rows.Count];
            double[] result = new double[dataGridView2.Rows.Count];



            for (int row = 0; row < a.GetLength(0); row++)
            {
                for (int column = 0; column < a.GetLength(1); column++)
                {
                   a[row,column] = Convert.ToInt32(dataGridView1.Rows[row].Cells[column].Value);
                }
            }
            for (int row = 0; row < dataGridView2.Rows.Count; row++)
            {
                b[row] = Convert.ToInt32(dataGridView2.Rows[row].Cells[0].Value);
            }


            Holeckiy hol = new Holeckiy(a, b, dataGridView1.Rows.Count);
            result = hol.vectorX();
           
            dataGridView3.Columns.Add("", "");
            for (int row = 0; row < result.GetLength(0); row++)
            {
                dataGridView3.Rows.Add();
                dataGridView3.Rows[row].Cells[0].Value = Math.Round(result[row], 3);
            }
        }


        async private void sheets_button_Click(object sender, EventArgs e)
        {
            var serviceValues = GetSheetsService().Spreadsheets.Values;
            await ReadAsync(serviceValues);
            for (int i = 0; i < abc.GetLength(1)-1; i++)
            {
                dataGridView1.Columns.Add("", "");
            }
            dataGridView1.Rows.Add(abc.GetLength(0)-1);

            for (int i = 0; i < abc.GetLength(1)-1; i++)
            {
                for (int j = 0; j < abc.GetLength(0); j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = abc[i,j];
                }
            }
            dataGridView2.Columns.Add("", "");
            for (int i = 0; i < abc.GetLength(0)-1; i++)
            {
                dataGridView2.Rows.Add();
            }
            for (int i = 0; i < abc.GetLength(0); i++)
            {
                dataGridView2.Rows[i].Cells[0].Value = abc[i, abc.GetLength(1)-1];
            }
        }

        private static SheetsService GetSheetsService()//получаем ответ от сервера
        {
            using (var stream = new FileStream(GoogleCredentialsFileName, FileMode.Open, FileAccess.Read))
            {
                var serviceInitializer = new BaseClientService.Initializer
                {
                    HttpClientInitializer = GoogleCredential.FromStream(stream).CreateScoped(Scopes)
                };
                return new SheetsService(serviceInitializer);
            }
        }

        private async Task ReadAsync(SpreadsheetsResource.ValuesResource valuesResource)//выполняем чтение
        {
            var response = await valuesResource.Get(SpreadsheetId, ReadRange).ExecuteAsync();
            var values = response.Values;
            if (values == null || !values.Any())
            {
                Console.WriteLine("No data found.");
                return;
            }

            abc = new double[values.Count, values.Count+1];

            for (int i = 0; i < values.Count; i++)
            {
                for (int j = 0; j < values.Count+1; j++)
                {
                    abc[i,j] = Convert.ToDouble(values[i][j]);
                }
                
            }

        }

        private void prognka_button_Click(object sender, EventArgs e)
        {
            double[,] a = new double[dataGridView1.Rows.Count, dataGridView1.Columns.Count];
            double[] b = new double[dataGridView2.Rows.Count];
            double[] result = new double[dataGridView2.Rows.Count];



            for (int row = 0; row < a.GetLength(0); row++)
            {
                for (int column = 0; column < a.GetLength(1); column++)
                {
                    a[row, column] = Convert.ToInt32(dataGridView1.Rows[row].Cells[column].Value);
                }
            }
            for (int row = 0; row < dataGridView2.Rows.Count; row++)
            {
                b[row] = Convert.ToInt32(dataGridView2.Rows[row].Cells[0].Value);
            }


            Run r = new Run(a, b, dataGridView1.Rows.Count);
            result = r.vectorX();

            dataGridView3.Columns.Add("", "");
            for (int row = 0; row < result.GetLength(0); row++)
            {
                dataGridView3.Rows.Add();
                dataGridView3.Rows[row].Cells[0].Value = Math.Round(result[row], 3);
            }
        }

        private void simple_itr_button_Click(object sender, EventArgs e)
        {
            double[,] a = new double[dataGridView1.Rows.Count, dataGridView1.Columns.Count];
            double[] b = new double[dataGridView2.Rows.Count];
            double[] result = new double[dataGridView2.Rows.Count];



            for (int row = 0; row < a.GetLength(0); row++)
            {
                for (int column = 0; column < a.GetLength(1); column++)
                {
                    a[row, column] = Convert.ToInt32(dataGridView1.Rows[row].Cells[column].Value);
                }
            }
            for (int row = 0; row < dataGridView2.Rows.Count; row++)
            {
                b[row] = Convert.ToInt32(dataGridView2.Rows[row].Cells[0].Value);
            }


            SimpleIteration simp = new SimpleIteration(a, b, dataGridView1.Rows.Count);
            result = simp.vectorX();

            dataGridView3.Columns.Add("", "");
            for (int row = 0; row < result.GetLength(0); row++)
            {
                dataGridView3.Rows.Add();
                dataGridView3.Rows[row].Cells[0].Value = Math.Round(result[row], 3);
            }
        }

        private void gradient_button_Click(object sender, EventArgs e)
        {
            double[,] a = new double[dataGridView1.Rows.Count, dataGridView1.Columns.Count];
            double[] b = new double[dataGridView2.Rows.Count];
            double[] result = new double[dataGridView2.Rows.Count];



            for (int row = 0; row < a.GetLength(0); row++)
            {
                for (int column = 0; column < a.GetLength(1); column++)
                {
                    a[row, column] = Convert.ToInt32(dataGridView1.Rows[row].Cells[column].Value);
                }
            }
            for (int row = 0; row < dataGridView2.Rows.Count; row++)
            {
                b[row] = Convert.ToInt32(dataGridView2.Rows[row].Cells[0].Value);
            }


            Gradient grad = new Gradient(a, b, dataGridView1.Rows.Count);
            result = grad.vectorX();

            dataGridView3.Columns.Add("", "");
            for (int row = 0; row < result.GetLength(0); row++)
            {
                dataGridView3.Rows.Add();
                dataGridView3.Rows[row].Cells[0].Value = Math.Round(result[row], 3);
            }
        }

        private void fast_button_Click(object sender, EventArgs e)
        {
            double[,] a = new double[dataGridView1.Rows.Count, dataGridView1.Columns.Count];
            double[] b = new double[dataGridView2.Rows.Count];
            double[] result = new double[dataGridView2.Rows.Count];



            for (int row = 0; row < a.GetLength(0); row++)
            {
                for (int column = 0; column < a.GetLength(1); column++)
                {
                    a[row, column] = Convert.ToInt32(dataGridView1.Rows[row].Cells[column].Value);
                }
            }
            for (int row = 0; row < dataGridView2.Rows.Count; row++)
            {
                b[row] = Convert.ToInt32(dataGridView2.Rows[row].Cells[0].Value);
            }


            FastGradient fastgrad = new FastGradient(a, b, dataGridView1.Rows.Count);
            result = fastgrad.vectorX();

            dataGridView3.Columns.Add("", "");
            for (int row = 0; row < result.GetLength(0); row++)
            {
                dataGridView3.Rows.Add();
                dataGridView3.Rows[row].Cells[0].Value = Math.Round(result[row], 3);
            }
        }
    }

    class FastGradient
    {
        double[,] a_matrix;
        double[] b_vector;
        public int n;
        double eps = 1e-9;
        int Iterations = 0;

        public FastGradient(double[,] a_matrixA, double[] vectorB, int size)
        {
            this.a_matrix = a_matrixA;
            this.b_vector = vectorB;
            this.n = size;
        }

        public double[] vectorX()
        {
            double[] TempX = new double[n];
            double[] X = new double[n];
            double[] r = new double[n];
            double[] r1 = new double[n];
            double s, s1 = 0;
            do
            {
                for (int i = 0; i < n; i++)
                {
                    r[i] = b_vector[i];
                    for (int j = 0; j < n; j++)
                    {
                        r[i] -= a_matrix[i, j] * TempX[j];
                    }
                }
                s = 0;
                for (int i = 0; i < n; i++)
                {
                    s += r[i] * r[i];
                }
                for (int i = 0; i < n; i++)
                {
                    r1[i] = 0;
                    for (int j = 0; j < n; j++)
                    {
                        r1[i] += a_matrix[i, j] * r[j];
                    }
                }
                s1 = 0;
                for (int i = 0; i < n; i++)
                {
                    s1 += r[i] * r1[i];
                }
                s /= s1;
                for (int i = 0; i < n; i++)
                {
                    X[i] += s * r[i];
                }
                s = 0;
                for (int i = 0; i < n; i++)
                {
                    s += (TempX[i] - X[i]) * (TempX[i] - X[i]);
                    TempX[i] = X[i];
                }
                Iterations++;
            }
            while (Math.Sqrt(s) > eps);
            return TempX;
        }

    }

    class Gradient
    {
        double[,] a_matrix;
        double[] b_vector;
        public int n;
        double eps = 1e-9;
        int Iterations = 0;

        public Gradient(double[,] a_matrixA, double[] vectorB, int size)
        {
            this.a_matrix = a_matrixA;
            this.b_vector = vectorB;
            this.n = size;
        }

        public double[] vectorX()
        {
            int i, j;
            double sumSq = 0;
            double alpha, beta;
            double Spr1;
            double[] X = new double[n];
            double[] Rk = new double[n];
            double[] Zk = new double[n];
            double[] Sz = new double[n];
            for (i = 0; i < n; i++)
            {
                sumSq += b_vector[i] * b_vector[i];
            }
            for (i = 0; i < n; i++)
            {
                for (Sz[i] = 0, j = 0; j < n; j++)
                {
                    Sz[i] += a_matrix[i, j] * X[j];
                }
                Rk[i] = b_vector[i] - Sz[i];
                Zk[i] = Rk[i];
            }
            do
            {
                double Spz = 0;
                double Spr = 0;
                for (i = 0; i < n; i++)
                {
                    for (Sz[i] = 0, j = 0; j < n; j++)
                    {
                        Sz[i] += a_matrix[i, j] * Zk[j];
                    }
                    Spz += Sz[i] * Zk[i];
                    Spr += Rk[i] * Rk[i];
                }
                alpha = Spr / Spz;
                Spr1 = 0;
                for (i = 0; i < n; i++)
                {
                    X[i] += alpha * Zk[i];
                    Rk[i] -= alpha * Sz[i];
                    Spr1 += Rk[i] * Rk[i];
                }

                beta = Spr1 / Spr;

                for (i = 0; i < n; i++)
                {
                    Zk[i] = Rk[i] + beta * Zk[i];
                }
                Iterations++;
            }

            while (Spr1 / sumSq > eps);
            return X;
        }


    }

    class SimpleIteration
    {
        double[,] a_matrix;
        double[] b_vector;
        public int n;
        double eps = 1e-9;
        int Iterations = 0;

        public SimpleIteration(double[,] a_matrixA, double[] vectorB, int size)
        {
            this.a_matrix = a_matrixA;
            this.b_vector = vectorB;
            this.n = size;
        }

        public double[] vectorX()
        {
            if (CheckMainDiagonal())
            {
                double[] TempX = new double[n];
                double norm;
                double[] X = new double[n];
                for (int i = 0; i < n; i++)
                {
                    X[i] = b_vector[i] / a_matrix[i, i];
                }
                do
                {
                    for (int i = 0; i < n; i++)
                    {
                        TempX[i] = b_vector[i];
                        for (int g = 0; g < n; g++)
                        {
                            if (i != g)
                                TempX[i] -= a_matrix[i, g] * X[g];
                        }
                        TempX[i] /= a_matrix[i, i];
                    }
                    norm = Math.Abs(X[0] - TempX[0]);
                    for (int h = 0; h < n; h++)
                    {
                        if (Math.Abs(X[h] - TempX[h]) > norm)
                            norm = Math.Abs(X[h] - TempX[h]);
                        X[h] = TempX[h];
                    }
                    Iterations++;
                } while (norm > eps);
                return X;
            }
            else
            {
                return null;
            }
        }

        public bool CheckMainDiagonal()
        {
            for (int i = 0; i < n; i++)
            {
                if (a_matrix[i, i] == 0)
                {
                    MessageBox.Show("Нулевые элементы на главной диагонали", "Ошибка!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }

    }

    class Run
    {
        double[,] a_matrix;
        double[] b_vector;
        public int n;

        public Run(double[,] a_matrixA, double[] vectorB, int size)
        {
            this.a_matrix = a_matrixA;
            this.b_vector = vectorB;
            this.n = size;
        }

        public  double[] vectorX()
        {
            if (checkMatrix())
            {
                //Прямой ход
                double[] v = new double[n];
                double[] u = new double[n];
                v[0] = a_matrix[0, 1] / (-a_matrix[0, 0]);
                u[0] = (-b_vector[0]) / (-a_matrix[0, 0]);
                for (int i = 1; i < n - 1; i++)
                {
                    v[i] = a_matrix[i, i + 1] / (-a_matrix[i, i] - a_matrix[i, i - 1] * v[i - 1]);
                    u[i] = (a_matrix[i, i - 1] * u[i - 1] - b_vector[i]) / (-a_matrix[i, i] - a_matrix[i, i - 1] * v[n - 2]);
                }
                v[n - 1] = 0;
                u[n - 1] = (a_matrix[n - 1, n - 2] * u[n - 2] - b_vector[n - 1]) / (-a_matrix[n - 1, n - 1] - a_matrix[n - 1, n - 2] * v[n - 2]);
                //Обратный ход
                double[] X = new double[n];
                X[n - 1] = u[n - 1];
                for (int i = n - 1; i > 0; i--)
                {
                    X[i - 1] = v[i - 1] * X[i] + u[i - 1];
                }
                return X;
            }
            else
            {
                return null;
            }
        }
        public bool checkMatrix()
        {
            for (int i = 1; i < n - 1; i++)
            {
                if (Math.Abs(a_matrix[i, i]) < Math.Abs(a_matrix[i, i - 1]) + Math.Abs(a_matrix[i, i + 1]))
                {
                    MessageBox.Show("Не выполнены условия достаточности", "Ошибка!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if ((Math.Abs(a_matrix[0, 0]) < Math.Abs(a_matrix[0, 1])) || Math.Abs(a_matrix[n - 1, n - 1]) < Math.Abs(a_matrix[n - 1, n - 2]))
                {
                    MessageBox.Show("Не выполнены условия достаточности", "Ошибка!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return CheckMainDiagonal();
        }
        public bool CheckMainDiagonal()
        {
            for (int i = 0; i < n; i++)
            {
                if (a_matrix[i, i] == 0)
                {
                    MessageBox.Show("Нулевые элементы на главной диагонали", "Ошибка!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }
    }

    class Holeckiy
    {
        double[,] a_a_matrix;
        double[] b_vector;
        public int n;
        public double[,] L;
        public double[,] LT;
        public double[] y;
        public double[] x;

        public Holeckiy(double[,] a_matrixA, double[] vectorB, int size)
        {
            this.a_a_matrix = a_matrixA;
            this.b_vector = vectorB;
            this.n = size;
        }

        public double[] vectorX()
        {
            L = Decomposition();
            y = Reverse(L, b_vector, true);
            LT = Transpose(L);
            x = Reverse(LT, y, false);
            return x;
        }

        public double[,] Decomposition()
        {
            double[,] L = new double[n,n];
            for (int i = 0; i < n; i++)
            {
                

                double temp;
                //Сначала вычисляем значения элементов слева от диагонального элемента,
                //так как эти значения используются при вычислении диагонального элемента.
                for (int j = 0; j < i; j++)
                {
                    temp = 0;
                    for (int k = 0; k < j; k++)
                    {
                        temp += L[i,k] * L[j,k];
                    }
                    L[i,j] = (a_a_matrix[i,j] - temp) / L[j,j];
                }

                //Находим значение диагонального элемента
                temp = a_a_matrix[i,i];
                for (int k = 0; k < i; k++)
                {
                    temp -= L[i,k] * L[i,k];
                }
                L[i,i] = Math.Sqrt(temp);
            }

            return L;
        }

        public double[] Reverse(double[,] trianglea_matrix, double[] B, bool DawnZero)
        {
            double[] X = new double[n];
            if (DawnZero)
            {
                X[0] = B[0] / trianglea_matrix[0, 0];
                for (int i = 1; i < n; i++)
                {
                    double temp = B[i];
                    for (int j = 0; j < i + 1; j++)
                    {
                        temp -= X[j] * trianglea_matrix[i, j];
                    }
                    X[i] = temp / trianglea_matrix[i, i];
                }
            }
            else
            {
                X[n - 1] = B[n - 1] / trianglea_matrix[n - 1, n - 1];
                for (int i = n - 2; i >= 0; i--)
                {
                    double temp = B[i];
                    for (int j = n - 1; j >= 0; j--)
                    {
                        temp -= X[j] * trianglea_matrix[i, j];
                    }
                    X[i] = temp / trianglea_matrix[i, i];
                }
            }
            return X;
        }

        public double[,] Transpose(double[,] a_matrix)
        {
            double[,] T = (double[,])a_matrix.Clone();
            double tmp;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    tmp = T[i, j];
                    T[i, j] = T[j, i];
                    T[j, i] = tmp;
                }
            }
            return T;
        }
    }

    class GausMethod
    {
        public uint RowCount;
        public uint ColumCount;
        public double[][] a_matrix { get; set; }
        public double[] RightPart { get; set; }
        public double[] Answer { get; set; }

        public GausMethod(uint Row, uint Colum)
        {
            RightPart = new double[Row];
            Answer = new double[Row];
            a_matrix = new double[Row][];
            for (int i = 0; i < Row; i++)
                a_matrix[i] = new double[Colum];
            RowCount = Row;
            ColumCount = Colum;

            //обнулим массив
            for (int i = 0; i < Row; i++)
            {
                Answer[i] = 0;
                RightPart[i] = 0;
                for (int j = 0; j < Colum; j++)
                    a_matrix[i][j] = 0;
            }
        }

        private void SortRows(int SortIndex)
        {

            double MaxElement = a_matrix[SortIndex][SortIndex];
            int MaxElementIndex = SortIndex;
            for (int i = SortIndex + 1; i < RowCount; i++)
            {
                if (a_matrix[i][SortIndex] > MaxElement)
                {
                    MaxElement = a_matrix[i][SortIndex];
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
                    Temp = a_matrix[MaxElementIndex][i];
                    a_matrix[MaxElementIndex][i] = a_matrix[SortIndex][i];
                    a_matrix[SortIndex][i] = Temp;
                }
            }
        }

        public int Solvea_matrix()
        {
            if (RowCount != ColumCount)
                return 1; //нет решения

            for (int i = 0; i < RowCount - 1; i++)
            {
                SortRows(i);
                for (int j = i + 1; j < RowCount; j++)
                {
                    if (a_matrix[i][i] != 0) //если главный элемент не 0, то производим вычисления
                    {
                        double MultElement = a_matrix[j][i] / a_matrix[i][i];
                        for (int k = i; k < ColumCount; k++)
                            a_matrix[j][k] -= a_matrix[i][k] * MultElement;
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
                    Answer[i] -= a_matrix[i][j] * Answer[j];

                if (a_matrix[i][i] == 0)
                    if (RightPart[i] == 0)
                        return 2; //множество решений
                    else
                        return 1; //нет решения

                Answer[i] /= a_matrix[i][i];
            }
            return 0;
        }
    }

}
