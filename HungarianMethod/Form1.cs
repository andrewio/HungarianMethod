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
                                 { 7, 2, 8, 2 ,5 },
                                 { 1, 2, 3, 1, 2 },
                                 { 7, 2, 5, 3, 6 },
                                 { 6, 2, 12, 3, 6 },
                                 { 4, 7, 11, 1, 9 }
                                 };
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
            HungSolving();

        }

        private void HungSolving()
        {
            

            //Решение венгерским методом и заполнение второй таблицы

            //Ищем минимальный элемент в столбце и 
            //отнимаем от каждого элемента столбца
            if (radioButton1.Checked)
            {
                DeductMinFromColumn();
            }
            else if (radioButton2.Checked)
            {
                DeductMaxFromColumn();
            }


            
     
            DGVToConsole(dataGridView1);

            //Ищем минимальный элемент в строке и 
            //отнимаем от каждого элемента строки
            DeductMinFromRow();

            DGVToConsole(dataGridView1);

            //Просматриваем стобцы матрицы  в поисках нуля
            //Если в одной строке с найденным нулем нет 0*,
            //то отмечаем его 0* 

            List<int> zeroStarColumnIndexes = new List<int>();
            List<int> zeroDashRowIndexes = new List<int>();
            int zeroStarCount = SetZeroStar();
            Console.WriteLine("CountZeroStar: {0}",zeroStarCount);
            DGVToConsole(dataGridView1);

            while (zeroStarCount < matrixSize)
            {
                //выделяем как + стобцы со 0*(их индексы в zeroStarColumnIndexes)
                selectZeroStarColumns(zeroStarColumnIndexes,dataGridView1);
                
                bool isStarZeroInRowOfZero = true;

                while (isStarZeroInRowOfZero == true)
                {
                    bool isZeroInUnselectedItems = true;
                    bool isLChain = false;
                    //Среди невыделенных элементов есть 0?
                    while (isZeroInUnselectedItems == true)
                    {

                        Point zeroPos = new Point();
                        isZeroInUnselectedItems =
                            FindZeroInUnselectedItems(zeroStarColumnIndexes, zeroDashRowIndexes, ref zeroPos);
                        if (isZeroInUnselectedItems == false)
                        {
                            break;
                        }
                        //отмечаем этот 0 как 0'
                        dataGridView1[zeroPos.X, zeroPos.Y].Value = "0'";
                        DGVToConsole(dataGridView1);

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
                            Point dashZeroInLChain = zeroPos;
                            while (isStarZero && isDashZero)
                            {
                                
                                Point starZeroInLChain = new Point();
                                isStarZero = FindStarZeroInColumnOfDashZero(dashZeroInLChain, ref starZeroInLChain);

                                if (isStarZero == true)
                                {
                                    
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

                            //Затем заменяем в ней все 0* на 0, а 0’ на 0*. Снимаем все выделения.
                            foreach (Point pos in LChain)
                            {
                                if (Convert.ToString(dataGridView1[pos.X, pos.Y].Value) == "0*")
                                {
                                    dataGridView1[pos.X, pos.Y].Value = 0;
                                }
                                if (Convert.ToString(dataGridView1[pos.X, pos.Y].Value) == "0'")
                                {
                                    dataGridView1[pos.X, pos.Y].Value = "0*";
                                }
                            }

                            DGVToConsole(dataGridView1);

                            //Снимаем все выделения
                            clearDashZeros(dataGridView1);
                            DGVToConsole(dataGridView1);

                            zeroStarColumnIndexes.Clear();
                            zeroDashRowIndexes.Clear();
                            zeroStarCount++;
                            isZeroInUnselectedItems = false;
                            isStarZeroInRowOfZero = false;
                            isLChain = true;
                        }


                    }

                    if (isZeroInUnselectedItems == false && isLChain == false)
                    {
                        //Находим минимальный элемент h > 0 среди не выделенных элементов
                        int minH = FindMinHInUnselectedItems(zeroStarColumnIndexes, zeroDashRowIndexes);
                        Console.WriteLine("minH = {0} \n", minH);
                        PrintSeparator();
                        //вычитаем h из невыделенных столцов
                        DeductHFromUnselectedColumns(minH, zeroStarColumnIndexes, zeroDashRowIndexes);
                        DGVToConsole(dataGridView1);
                        //добавляем h к выделенным строкам
                        PlusHToSelectedRows(minH, zeroStarColumnIndexes, zeroDashRowIndexes);
                        DGVToConsole(dataGridView1);
                    }

                }

            }

            //Записать матрицу X
            int FXOpt = DisplayEndPriceMatrixFrom(dataGridView1);

            SetMatrixToDataGridView(dataGridView1, priceMatrix);

            //Посчитать F(Xopt)
            textBoxFXOpt.Text = FXOpt.ToString();

        }

        private int DisplayEndPriceMatrixFrom(DataGridView dgv)
        {
            int FXOpt = 0;
            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                for (int j = 0; j < dgv.RowCount; j++)
                {
                    if (dgv[i, j].Value.ToString() == "0*")
                    {

                        dataGridView2[i, j].Value = 1;
                        FXOpt += priceMatrix[j,i];
                    }
                    else
                    {
                        dataGridView2[i, j].Value = 0;
                    }
                }
            }

            return FXOpt;
        }

        private void selectZeroStarColumns(List<int> zeroStarColumnIndexes, DataGridView dgv)
        {
            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                for (int j = 0; j < dgv.RowCount; j++)
                {
                    if (dgv[i,j].Value.ToString() == "0*")
                    {


                        if (!zeroStarColumnIndexes.Contains(i))
                        {
                            zeroStarColumnIndexes.Add(i);
                            break;
                        }
                    }
                }
            }

        }

        private void clearDashZeros(DataGridView dgv)
        {
            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                for (int j = 0; j < dgv.RowCount; j++)
                {
                    dgv[i, j].Value = dgv[i, j].Value.ToString().Replace("'", string.Empty);
                }
            }
        }

        private void DGVToConsole(DataGridView dgv)
        {
            PrintSeparator();
            for (int i = 0; i < dgv.RowCount; i++)
            {
                for (int j = 0; j < dgv.ColumnCount; j++)
                {
                    Console.Write(dgv[j, i].Value  + "\t");
                    
                }
                Console.WriteLine();
                
            }
            PrintSeparator();
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
                //Пропускаем невыделенные строки
                if (!zeroDashRowIndexes.Contains(i))
                {
                    continue;
                }
                else
                {

                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {

                        if (Convert.ToString(dataGridView1[j, i].Value).Contains("*"))
                            {
                                dataGridView1[j, i].Value = (Convert.ToInt32(dataGridView1[j, i].Value.ToString().Replace("*", string.Empty)) + minH).ToString() + "*";
                            }
                        else if (Convert.ToString(dataGridView1[j, i].Value).Contains("'"))
                            {
                                dataGridView1[j, i].Value = (Convert.ToInt32(dataGridView1[j, i].Value.ToString().Replace("'", string.Empty)) + minH).ToString() + "'";
                            }
                        else
                            dataGridView1[j, i].Value = Convert.ToInt32(dataGridView1[j, i].Value) + minH;

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
                        //if (zeroDashRowIndexes.Contains(j))
                        //{
                        //    continue;
                        //}
                        //else
                        //{
                            if (Convert.ToString(dataGridView1[i, j].Value).Contains("*"))
                            {
                                dataGridView1[i, j].Value = (Convert.ToInt32(dataGridView1[i, j].Value.ToString().Replace("*",string.Empty)) - minH).ToString() + "*";
                            }
                            else if (Convert.ToString(dataGridView1[i, j].Value).Contains("'"))
                            {
                                dataGridView1[i, j].Value = (Convert.ToInt32(dataGridView1[i, j].Value.ToString().Replace("'",string.Empty)) - minH).ToString() + "'";
                            }
                            else
                                dataGridView1[i, j].Value = Convert.ToInt32(dataGridView1[i, j].Value) - minH;

                        //}
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

        void PrintSeparator ()
        {
            Console.WriteLine("--------------");
        }
        private int SetZeroStar()
        {
            Console.WriteLine("SetAndCountZeroStar");
            int zeroStarCount = 0;
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                //DGVToConsole(dataGridView1);
                for (int j = 0; j < dataGridView1.RowCount; j++)
                {
                    
                    //Просматриваем столбцы в поисках 0
                    if (Convert.ToString(dataGridView1[i, j].Value) == "0")
                    {
                        bool isZeroStar = false;
                        //Просматриваем строку с 0 на 0*
                        for (int m = 0; m < dataGridView1.ColumnCount; m++)
                        {
                            if (Convert.ToString(dataGridView1[m, j].Value) == "0*")
                            {
                                //0* есть в одной строке с 0 
                                isZeroStar = true;
                                
                                break;
                            }
                        }
                        
                        //Если 0* не найден, отмечаем найденный 0 как 0*
                        if (isZeroStar == false)
                        {
                            dataGridView1[i, j].Value = "0*";
                            zeroStarCount++;
                            
                            break;
                            
                        }

                        
                    }
                }
            }
            return zeroStarCount;
        }

        private void DeductMinFromColumn()
        {
            Console.WriteLine("DeductMinFromColumn");
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                int min = this.dataGridView1.Rows.Cast<DataGridViewRow>().Min(r => Convert.ToInt32(r.Cells[i].Value));
                Console.Write(min + "\t");
                for (int j = 0; j < dataGridView1.RowCount; j++)
                {

                    dataGridView1[i, j].Value =
                        Convert.ToInt32(dataGridView1[i, j].Value) - min;
                }
            }
            Console.WriteLine();
            PrintSeparator();

        }

        private void DeductMaxFromColumn()
        {
            Console.WriteLine("DeductMinFromColumn");
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                int max = this.dataGridView1.Rows.Cast<DataGridViewRow>().Max(r => Convert.ToInt32(r.Cells[i].Value));
                Console.Write(max + "\t");
                for (int j = 0; j < dataGridView1.RowCount; j++)
                {

                    dataGridView1[i, j].Value =
                        max - Convert.ToInt32(dataGridView1[i, j].Value);
                }
            }
            Console.WriteLine();
            PrintSeparator();

        }

        private void DeductMinFromRow()
        {
            PrintSeparator();
            Console.WriteLine("DeductMinFromRow");
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
                Console.Write(min + "\t");

                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                {

                    dataGridView1[j, i].Value =
                        Convert.ToInt32(dataGridView1[j, i].Value) - min;
                }
            }
            Console.WriteLine();
            PrintSeparator();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.RowCount = dataGridView1.ColumnCount = this.matrixSize;
            dataGridView2.RowCount = dataGridView2.ColumnCount = this.matrixSize;

            SetMatrixToDataGridView(dataGridView1, this.priceMatrix);

            DGVToConsole(dataGridView1);

        }
    }
}
