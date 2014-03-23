using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace HungarianMethod
{
    public partial class Form1 : Form
    {
        private int matrixSize = 5;

        private int[,] priceMatrix = { 
                                 { 7, 7, 9, 6 ,3 },
                                 { 9, 9, 6, 8, 7 },
                                 { 6,11, 4, 6, 5 },
                                 { 5,10, 3, 10, 10 },
                                 { 9, 8,10, 8, 5 }
                                 };
        //private int[,] priceMatrix = { 
        //                         { 7, 2, 8, 2 ,5 },
        //                         { 1, 2, 3, 1, 2 },
        //                         { 7, 2, 5, 3, 6 },
        //                         { 6, 2, 12, 3, 6 },
        //                         { 4, 7, 11, 1, 9 }
        //                         };
        //private int[,] priceMatrix = { 
        //                         { 8, 4, 5, 7 ,2 },
        //                         { 7, 4, 3, 8, 2 },
        //                         { 1, 2, 4, 7, 2 },
        //                         { 3, 9, 9, 2, 5 },
        //                         { 4, 5, 1, 6, 3 }
        //                         };

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Выбор типа задачи
            string taskType = string.Empty;
            if (radioButton1.Checked)
            {
                taskType = "min";
            }
            else if (radioButton2.Checked)
            {
                taskType = "max";
            }

            HungarianMethod HM = new HungarianMethod(priceMatrix, taskType);

            //Записать матрицу X
            int FXOpt = HM.DisplayResultToDGV(dataGridView2, priceMatrix);

            //Посчитать F(Xopt)
            textBoxFXOpt.Text = FXOpt.ToString();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.RowCount = dataGridView1.ColumnCount = this.matrixSize;
            dataGridView2.RowCount = dataGridView2.ColumnCount = this.matrixSize;

            SetMatrixToDataGridView(dataGridView1, this.priceMatrix);

        }

        private void SetMatrixToDataGridView(DataGridView dgv, int[,] priceMatrix)
        {
            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                for (int j = 0; j < dgv.RowCount; j++)
                {
                    dgv[i, j].Value = priceMatrix[j, i];
                }

            }
        }
    }
}
