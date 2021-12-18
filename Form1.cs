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

        private void GaussMethod()
        {
            int[,] matrix = new int[Convert.ToInt32(textBox1.Text), Convert.ToInt32(textBox1.Text)+1];
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1)-1; j++)
                {
                    matrix[i, j] = (int)dataGridView1.Rows[i].Cells[j].Value;
                }
                for (int j = matrix.GetLength(1) - 1; j < matrix.GetLength(1); j++)
                {
                    matrix[i, j] = (int)dataGridView2.Rows[i].Cells[0].Value;
                }
            }
            for (int i = 0; i < matrix.GetLength(1); i++)
            {
                dataGridView3.Columns.Add("", "");
            }
            dataGridView3.Rows.Add(matrix.GetLength(0)-1);
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    dataGridView3.Rows[i].Cells[j].Value = matrix[i,j];
                }
            }
        }

        private void gauss_button_Click(object sender, EventArgs e)
        {
            GaussMethod();
        }
    }
}
