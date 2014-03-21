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
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread newSolving = new Thread(HungSolving);
            newSolving.Start();

        }

        private void HungSolving()
        {
            
            int matrixSize = 5;
            this.dataGridView1.ColumnCount = matrixSize;
            this.dataGridView1.RowCount = matrixSize;

            int[,] priceMatrix = { 
                                 { 2, 4, 1, 3 ,3 },
                                 { 1, 5, 4, 1, 2 },
                                 { 3, 5, 2, 2, 4 },
                                 { 1, 4, 3, 1, 4 },
                                 { 3, 2, 5, 3, 5 }
                                 };
            SetMatrixToDataGridView(dataGridView1, priceMatrix);

            //Решение венгерским методом и заполнение второй таблицы

            this.dataGridView2.ColumnCount = matrixSize;
            this.dataGridView2.RowCount = matrixSize;


            //Ищем минимальный элемент в столбце и 
            //отнимаем от каждого элемента столбца
            DeductMinFromColumn();

            //Ищем минимальный элемент в строке и 
            //отнимаем от каждого элемента строки
            DeductMinFromRow();

            //Просматриваем стобцы матрицы  в поисках нуля
            //Если в одной строке с найденным нулем нет 0*,
            //то отмечаем его 0* 

            List<int> zeroStarColumnIndexes = new List<int>();
            List<int> zeroDashRowIndexes = new List<int>();
            int zeroStarCount = SetAndCountZeroStar(zeroStarColumnIndexes);

            while (zeroStarCount < matrixSize)
            {
                //выделяем как + стобцы со 0*(их индексы в zeroStarColumnIndexes)

                bool isStarZeroInRowOfZero = true;

                while (isStarZeroInRowOfZero == true)
                {

                    Point zeroPos = new Point();
                    bool isZeroInUnselectedItems =
                        FindZeroInUnselectedItems(zeroStarColumnIndexes, zeroDashRowIndexes, ref zeroPos);

                    //Среди невыделенных элементов есть 0?
                    while (isZeroInUnselectedItems == true)
                    {
                        //отмечаем этот 0 как 0'
                        dataGridView1[zeroPos.X, zeroPos.Y].Value = "0'";

                        Point starZeroPos = new Point();
                        isStarZeroInRowOfZero = FindStarZeroInRowOfZero(zeroPos, ref starZeroPos);

                        //В одной строке с этим 0 есть 0*
                        if (isStarZeroInRowOfZero == true)
                        {
                            //Снимаем выделение со столбца с 0*
                            zeroStarColumnIndexes.Remove(starZeroPos.X);
                            //Выделяем строку, в которой есть этот 0' & 0*
                            if (!zeroDashRowIndexes.Contains(starZeroPos.Y))
                            {
                                zeroDashRowIndexes.Add(starZeroPos.Y);
                            }
                        }
                        else
                        {
                            isStarZeroInRowOfZero = false;

                            List<Point> LChain = new List<Point>();
                            //строим L цепь (должна начинаться и заканчиваться 0')
                            //должна быть непродолжаемой
                            //от тек 0'(zeroPos) по стоблцу к 0*, от него по строке к 0' и тд
                            LChain.Add(zeroPos); // текущий 0'

                            bool isStarZero = true;
                            bool isDashZero = true;

                            while (isStarZero && isDashZero)
                            {

                                Point starZeroInLChain = new Point();
                                isStarZero = FindStarZeroInColumnOfDashZero(zeroPos, ref starZeroInLChain);

                                if (isStarZero == true)
                                {
                                    Point dashZeroInLChain = new Point();
                                    isDashZero = FindDashZeroInRowOfStarZero(starZeroInLChain, ref dashZeroInLChain);

                                    if (isDashZero == true)
                                    {
                                        LChain.Add(starZeroInLChain);
                                        LChain.Add(dashZeroInLChain);
                                    }
                                    else
                                    {
                                        isStarZero = false;
                                        isDashZero = false;
                                    }
                                }
                                else
                                {
                                    isStarZero = false;
                                    isDashZero = false;
                                }

                            }
                            //на выходе из цикла получаем L-цепь

                            //Затем заменяем в ней все 0* на 0, а 0’ на 0*.Снимаем все выделения.
                            foreach (Point pos in LChain)
                            {
                                if (Convert.ToString(dataGridView1[pos.X, pos.Y].Value) == "0*")
                                {
                                    dataGridView1[pos.X, pos.Y].Value = 0;
                                }
                                if (Convert.ToString(dataGridView1[pos.X, pos.Y].Value) == "0'")
                                {
                                    dataGridView1[pos.X, pos.Y].Value = "0'";
                                }
                            }

                            //Снимаем все выделения
                            zeroStarColumnIndexes.Clear();
                            zeroDashRowIndexes.Clear();
                            zeroStarCount++;
                            isZeroInUnselectedItems = false;
                            //isStarZeroInRowOfZero = false;
                        }


                    }

                    if (isZeroInUnselectedItems == false)
                    {
                        //Находим минимальный элемент h > 0 среди не выделенных элементов
                        int minH = FindMinHInUnselectedItems(zeroStarColumnIndexes, zeroDashRowIndexes);
                        //вычитаем h из невыделенных столцов
                        DeductHFromUnselectedColumns(minH, zeroStarColumnIndexes, zeroDashRowIndexes);
                        //добавляем h к выделенным строкам
                        PlusHToSelectedRows(minH, zeroStarColumnIndexes, zeroDashRowIndexes);

                    }
                    else
                    {

                    }

                }





            }



            //Записать матрицу X

            //SetMatrixToDataGridView(dataGridView2, priceMatrix);

            //Посчитать F(Xopt)


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

        private bool FindDashZeroInRowOfStarZero(Point starZeroInLChain, ref Point dashZeroInLChain)
        {
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                if (Convert.ToString(dataGridView1[i, starZeroInLChain.Y].Value) == "0'")
                {
                    dashZeroInLChain = new Point(i, starZeroInLChain.Y);
                    return true;
                }
            }
            return false;
        }

        private bool FindStarZeroInColumnOfDashZero(Point zeroPos, ref Point starZero )
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (Convert.ToString(dataGridView1[zeroPos.X,i].Value) == "0*")
                {
                    starZero = new Point(zeroPos.X, i);
                    return true;
                }
            }
            return false;
        }

        private void PlusHToSelectedRows(int minH, List<int> zeroStarColumnIndexes, List<int> zeroDashRowIndexes)
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                //пропускаем невыделенные столбцы
                if (!zeroStarColumnIndexes.Contains(i))
                {
                    continue;
                }
                else
                {

                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        //Пропускаем невыделенные строки
                        if (!zeroDashRowIndexes.Contains(j))
                        {
                            continue;
                        }
                        else
                        {

                            dataGridView1[j, i].Value = Convert.ToInt32(dataGridView1[i, j].Value) + minH;

                        }
                    }
                }
            }
        }

        private void DeductHFromUnselectedColumns(int minH, List<int> zeroStarColumnIndexes,
                                              List<int> zeroDashRowIndexes)
        {
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                //пропускаем выделенные столбцы
                if (zeroStarColumnIndexes.Contains(i))
                {
                    continue;
                }
                else
                {

                    for (int j = 0; j < dataGridView1.RowCount; j++)
                    {
                        //Пропускаем выделенные строки
                        if (zeroDashRowIndexes.Contains(j))
                        {
                            continue;
                        }
                        else
                        {

                            dataGridView1[i, j].Value = Convert.ToInt32(dataGridView1[i, j].Value) - minH;

                        }
                    }
                }
            }
        }

        private int FindMinHInUnselectedItems(List<int> zeroStarColumnIndexes,
                                              List<int> zeroDashRowIndexes)
        {
            //Первый из невыделенных элементов
            int minH = 0;//= Convert.ToInt32(dataGridView1[zeroStarColumnIndexes[0], zeroDashRowIndexes[0]].Value);

            bool firstElem = true;
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                //пропускаем выделенные столбцы
                if (zeroStarColumnIndexes.Contains(i))
                {
                    continue;
                }
                else
                {

                    for (int j = 0; j < dataGridView1.RowCount; j++)
                    {
                        //Пропускаем выделенные строки
                        if (zeroDashRowIndexes.Contains(j))
                        {
                            continue;
                        }
                        else
                        {
                            if (Convert.ToInt32(dataGridView1[i, j].Value) > 0)
                            {
                                if (firstElem == true)
                                {
                              
                                        minH = Convert.ToInt32(dataGridView1[i, j].Value);
                                        firstElem = false;
                                
                                }
                                else if (Convert.ToInt32(dataGridView1[i, j].Value) < minH)
                                {
                                        minH = Convert.ToInt32(dataGridView1[i, j].Value);

                                }
                            }
                            

                        }
                    }
                }
            }

            return minH;
        }

        private bool FindStarZeroInRowOfZero(Point zeroPos, ref Point starZeroPos)
        {
            for (int m = 0; m < dataGridView1.ColumnCount; m++)
            {
                //В одной строке с этим нулем есть 0* ?
                if (Convert.ToString(dataGridView1[m, zeroPos.Y].Value) == "0*")
                {
                    starZeroPos = new Point(m, zeroPos.Y);
                    return true;
                }
            }

            return false;
        }

        private bool FindZeroInUnselectedItems(List<int> zeroStarColumnIndexes, 
                                               List<int> zeroDashRowIndexes, 
                                               ref Point zeroPos)
        {

            //-------------------------------------
            //среди невыделенных элементов есть 0?
            //-------------------------------------

            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                //пропускаем выделенные столбцы
                if (zeroStarColumnIndexes.Contains(i))
                {
                    continue;
                }
                else
                {

                    for (int j = 0; j < dataGridView1.RowCount; j++)
                    {
                        //Пропускаем выделенные строки
                        if (zeroDashRowIndexes.Contains(j))
                        {
                            continue;
                        }
                        else
                        {
                            //среди невыделенных элементов есть 0?
                            if (Convert.ToString(dataGridView1[i, j].Value) == "0")
                            {
                                zeroPos = new Point(i, j);
                                return true;
                                
                            }
                        }
                    }
                }
            }//среди невыделенных элементов есть 0?

            return false;
        }

        private int SetAndCountZeroStar(List<int> zeroStarColumnIndexes)
        {
            int zeroStarCount = 0;
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                for (int j = 0; j < dataGridView1.RowCount; j++)
                {
                    if (Convert.ToString(dataGridView1[i, j].Value) == "0")
                    {
                        bool isZeroStar = false;
                        //Просматриваем строку с 0 на 0*
                        for (int m = 0; m < dataGridView1.ColumnCount; m++)
                        {
                            if (Convert.ToString(dataGridView1[m, j].Value) == "0*")
                            {
                                isZeroStar = true;
                                zeroStarCount++;
                                break;
                            }
                        }
                        //Если 0* не найден, отмечаем найденный 0 как 0*
                        if (!isZeroStar)
                        {
                            dataGridView1[i, j].Value = "0*";
                            if (!zeroStarColumnIndexes.Contains(i))
                            {
                                zeroStarColumnIndexes.Add(i);
                            }
                            
                        }
                    }
                }
            }
            return zeroStarCount;
        }

        private void DeductMinFromColumn()
        {
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                int min = this.dataGridView1.Rows.Cast<DataGridViewRow>().Min(r => Convert.ToInt32(r.Cells[i].Value));

                for (int j = 0; j < dataGridView1.RowCount; j++)
                {

                    dataGridView1[i, j].Value =
                        Convert.ToInt32(dataGridView1[i, j].Value) - min;
                }
            }
        }

        private void DeductMinFromRow()
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                int min = Convert.ToInt32(dataGridView1[0, i].Value);
                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                {
                    if (min > Convert.ToInt32(dataGridView1[j, i].Value))
                    {
                        min = Convert.ToInt32(dataGridView1[j, i].Value);
                    }

                }

                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                {

                    dataGridView1[j, i].Value =
                        Convert.ToInt32(dataGridView1[j, i].Value) - min;
                }
            }
        }
    }
}
