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

            HungSolving(priceMatrix, taskType);

        }

        private void HungSolving( int[,] priceMatrix, string taskType)
        {
            
            //Перевод для удобства работы
            string[,] workingMatrix = IntArrayToStringArray(priceMatrix);

            //Решение венгерским методом и заполнение второй таблицы

            //Ищем минимальный элемент в столбце и 
            //отнимаем от каждого элемента столбца
            if (taskType == "min")
            {
                DeductMinFromColumn(workingMatrix);
            }
            else if (taskType == "max")
            {
                DeductMaxFromColumn(workingMatrix);
            }
   
            String2DArrayToConsole(workingMatrix);

            //Ищем минимальный элемент в строке и 
            //отнимаем от каждого элемента строки
            DeductMinFromRow(workingMatrix);

            String2DArrayToConsole(workingMatrix);

            //Просматриваем стобцы матрицы  в поисках нуля
            //Если в одной строке с найденным нулем нет 0*,
            //то отмечаем его 0* 

            List<int> zeroStarColumnIndexes = new List<int>();
            List<int> zeroDashRowIndexes = new List<int>();
            int zeroStarCount = SetZeroStar(workingMatrix);
            Console.WriteLine("CountZeroStar: {0}", zeroStarCount);
            String2DArrayToConsole(workingMatrix);

            while (zeroStarCount < matrixSize)
            {
                //выделяем как + стобцы со 0*(их индексы в zeroStarColumnIndexes)
                selectZeroStarColumns(zeroStarColumnIndexes, workingMatrix);
                
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
                            FindZeroInUnselectedItems(workingMatrix, zeroStarColumnIndexes, zeroDashRowIndexes, ref zeroPos);
                        if (isZeroInUnselectedItems == false)
                        {
                            break;
                        }
                        //отмечаем этот 0 как 0'
                        workingMatrix[zeroPos.Y, zeroPos.X] = "0'";
                        String2DArrayToConsole(workingMatrix);

                        Point starZeroPos = new Point();
                        isStarZeroInRowOfZero = FindStarZeroInRowOfZero(workingMatrix, zeroPos, ref starZeroPos);

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
                                isStarZero = FindStarZeroInColumnOfDashZero(workingMatrix, dashZeroInLChain, ref starZeroInLChain);

                                if (isStarZero == true)
                                {
                                    
                                    isDashZero = FindDashZeroInRowOfStarZero(workingMatrix, starZeroInLChain, ref dashZeroInLChain);

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
                                if (workingMatrix[pos.Y, pos.X] == "0*")
                                {
                                    workingMatrix[pos.Y, pos.X] = "0";
                                }
                                if (workingMatrix[pos.Y, pos.X] == "0'")
                                {
                                    workingMatrix[pos.Y, pos.X] = "0*";
                                }
                            }

                            String2DArrayToConsole(workingMatrix);

                            //Снимаем все выделения
                            clearDashZeros(workingMatrix);
                            String2DArrayToConsole(workingMatrix);

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
                        int minH = FindMinHInUnselectedItems(workingMatrix, zeroStarColumnIndexes, zeroDashRowIndexes);
                        Console.WriteLine("minH = {0} \n", minH);
                        PrintSeparator();
                        //вычитаем h из невыделенных столцов
                        DeductHFromUnselectedColumns(workingMatrix, minH, zeroStarColumnIndexes, zeroDashRowIndexes);
                        String2DArrayToConsole(workingMatrix);
                        //добавляем h к выделенным строкам
                        PlusHToSelectedRows(workingMatrix, minH, zeroStarColumnIndexes, zeroDashRowIndexes);
                        String2DArrayToConsole(workingMatrix);
                    }

                }

            }

            //Записать матрицу X
            int FXOpt = DisplayEndPriceMatrixFromTo(workingMatrix, dataGridView2, priceMatrix);

            //Посчитать F(Xopt)
            textBoxFXOpt.Text = FXOpt.ToString();

        }

        private string[,] IntArrayToStringArray(int[,] priceMatrix)
        {
            string[,] goalMatrix = new string[5,5];

            for (int i = 0; i < priceMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < priceMatrix.GetLength(0); j++)
                {
                    goalMatrix[i, j] = priceMatrix[i, j].ToString();
                }
            }

            return goalMatrix;
        }

        private int DisplayEndPriceMatrixFromTo(string[,] goalMatrix,
                                                DataGridView dgv, 
                                                int[,] priceMatrix)
        {
            int FXOpt = 0;
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < goalMatrix.GetLength(0); j++)
                {
                    if (goalMatrix[j, i] == "0*")
                    {

                        dgv[i, j].Value = 1;
                        FXOpt += priceMatrix[j,i];
                    }
                    else
                    {
                        dgv[i, j].Value = 0;
                    }
                }
            }

            return FXOpt;
        }

        private void selectZeroStarColumns(List<int> zeroStarColumnIndexes, string[,] goalMatrix)
        {
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < goalMatrix.GetLength(0); j++)
                {
                    if (goalMatrix[j, i] == "0*")
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

        private void clearDashZeros(string[,] goalMatrix)
        {
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < goalMatrix.GetLength(0); j++)
                {
                    goalMatrix[j, i] = goalMatrix[j, i].Replace("'", string.Empty);
                }
            }
        }

        private void String2DArrayToConsole(string[,] matrix)
        {
            PrintSeparator();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(0); j++)
                {
                    Console.Write(matrix[i, j] + "\t");                    
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

        private bool FindDashZeroInRowOfStarZero(string[,] goalMatrix, Point starZeroInLChain, ref Point dashZeroInLChain)
        {
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                if (goalMatrix[starZeroInLChain.Y, i] == "0'")
                {
                    dashZeroInLChain = new Point(i, starZeroInLChain.Y);
                    return true;
                }
            }
            return false;
        }

        private bool FindStarZeroInColumnOfDashZero(string[,] goalMatrix, Point zeroPos, ref Point starZero )
        {
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                if (goalMatrix[i, zeroPos.X] == "0*")
                {
                    starZero = new Point(zeroPos.X, i);
                    return true;
                }
            }
            return false;
        }

        private void PlusHToSelectedRows(string[,] goalMatrix,
                                        int minH,
                                        List<int> zeroStarColumnIndexes,
                                        List<int> zeroDashRowIndexes)
        {
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                //Пропускаем невыделенные строки
                if (!zeroDashRowIndexes.Contains(i))
                {
                    continue;
                }
                else
                {

                    for (int j = 0; j < goalMatrix.GetLength(0); j++)
                    {

                        if (goalMatrix[i, j].Contains("*"))
                            {
                                goalMatrix[i, j] = (Convert.ToInt32(goalMatrix[i, j].Replace("*", string.Empty)) + minH).ToString() + "*";
                            }
                        else if (goalMatrix[i, j].Contains("'"))
                            {
                                goalMatrix[i, j] = (Convert.ToInt32(goalMatrix[i, j].Replace("'", string.Empty)) + minH).ToString() + "'";
                            }
                        else
                            goalMatrix[i, j] = (Convert.ToInt32(goalMatrix[i, j]) + minH).ToString();

                    }
                }
                
            }
        }

        private void DeductHFromUnselectedColumns(string[,] goalMatrix,
                                                  int minH, 
                                                  List<int> zeroStarColumnIndexes,
                                                  List<int> zeroDashRowIndexes)
        {
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                //пропускаем выделенные столбцы
                if (zeroStarColumnIndexes.Contains(i))
                {
                    continue;
                }
                else
                {

                    for (int j = 0; j < goalMatrix.GetLength(0); j++)
                    {
                        
                        if (goalMatrix[j, i].Contains("*"))
                        {
                            goalMatrix[j, i] = (Convert.ToInt32(goalMatrix[j, i].Replace("*", string.Empty)) - minH).ToString() + "*";
                        }
                        else if (goalMatrix[j, i].Contains("'"))
                        {
                            goalMatrix[j, i] = (Convert.ToInt32(goalMatrix[j, i].Replace("'", string.Empty)) - minH).ToString() + "'";
                        }
                        else
                            goalMatrix[j, i] = (Convert.ToInt32(goalMatrix[j, i]) - minH).ToString();

                    }
                }
            }
        }

        private int FindMinHInUnselectedItems(string[,] goalMatrix,
                                              List<int> zeroStarColumnIndexes,
                                              List<int> zeroDashRowIndexes)
        {
            //Первый из невыделенных элементов
            int minH = 0;

            bool firstElem = true;
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                //пропускаем выделенные столбцы
                if (zeroStarColumnIndexes.Contains(i))
                {
                    continue;
                }
                else
                {

                    for (int j = 0; j < goalMatrix.GetLength(0); j++)
                    {
                        //Пропускаем выделенные строки
                        if (zeroDashRowIndexes.Contains(j))
                        {
                            continue;
                        }
                        else
                        {
                            if (Convert.ToInt32(goalMatrix[j, i]) > 0)
                            {
                                if (firstElem == true)
                                {
                              
                                    minH = Convert.ToInt32(goalMatrix[j, i]);
                                    firstElem = false;
                                
                                }
                                else if (Convert.ToInt32(goalMatrix[j, i]) < minH)
                                {
                                    minH = Convert.ToInt32(goalMatrix[j, i]);

                                }
                            }
                            

                        }
                    }
                }
            }

            return minH;
        }

        private bool FindStarZeroInRowOfZero(string[,] goalMatrix, Point zeroPos, ref Point starZeroPos)
        {
            for (int m = 0; m < goalMatrix.GetLength(0); m++)
            {
                //В одной строке с этим нулем есть 0* ?
                if (goalMatrix[zeroPos.Y, m] == "0*")
                {
                    starZeroPos = new Point(m, zeroPos.Y);
                    return true;
                }
            }

            return false;
        }

        private bool FindZeroInUnselectedItems(string[,] goalMatrix,
                                               List<int> zeroStarColumnIndexes, 
                                               List<int> zeroDashRowIndexes, 
                                               ref Point zeroPos)
        {

            //-------------------------------------
            //среди невыделенных элементов есть 0?
            //-------------------------------------

            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                //пропускаем выделенные столбцы
                if (zeroStarColumnIndexes.Contains(i))
                {
                    continue;
                }
                else
                {

                    for (int j = 0; j < goalMatrix.GetLength(0); j++)
                    {
                        //Пропускаем выделенные строки
                        if (zeroDashRowIndexes.Contains(j))
                        {
                            continue;
                        }
                        else
                        {
                            //среди невыделенных элементов есть 0?
                            if (goalMatrix[j, i] == "0")
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
        private int SetZeroStar(string[,] goalMatrix)
        {
            Console.WriteLine("SetAndCountZeroStar");
            int zeroStarCount = 0;
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                
                for (int j = 0; j < goalMatrix.GetLength(0); j++)
                {
                    
                    //Просматриваем столбцы в поисках 0
                    if (goalMatrix[j, i] == "0")
                    {
                        bool isZeroStar = false;
                        //Просматриваем строку с 0 на 0*
                        for (int m = 0; m < goalMatrix.GetLength(0); m++)
                        {
                            if (goalMatrix[j, m] == "0*")
                            {
                                //0* есть в одной строке с 0 
                                isZeroStar = true;
                                
                                break;
                            }
                        }
                        
                        //Если 0* не найден, отмечаем найденный 0 как 0*
                        if (isZeroStar == false)
                        {
                            goalMatrix[j, i] = "0*";
                            zeroStarCount++;
                            
                            break;
                            
                        }

                        
                    }
                }
            }
            return zeroStarCount;
        }

        private void DeductMinFromColumn(string[,] goalMatrix)
        {
            Console.WriteLine("DeductMinFromColumn");
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                int min = Convert.ToInt32(goalMatrix[0, i]);

                for (int k = 0; k < goalMatrix.GetLength(0); k++)
                {
                    if (Convert.ToInt32(goalMatrix[k, i]) < min)
                    {
                        min = Convert.ToInt32(goalMatrix[k, i]);
                    }
                }
                Console.Write(min + "\t");
                for (int j = 0; j < goalMatrix.GetLength(0); j++)
                {

                    goalMatrix[j, i] =
                        (Convert.ToInt32(goalMatrix[j, i]) - min).ToString();
                }
            }
            Console.WriteLine();
            PrintSeparator();

        }

        private void DeductMaxFromColumn(string[,] goalMatrix)
        {
            Console.WriteLine("DeductMinFromColumn");
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                int max = Convert.ToInt32(goalMatrix[0, i]);

                for (int k = 0; k < goalMatrix.GetLength(0); k++)
                {
                    if (Convert.ToInt32(goalMatrix[k, i]) > max)
                    {
                        max = Convert.ToInt32(goalMatrix[k, i]);
                    }
                }
                
                Console.Write(max + "\t");
                for (int j = 0; j < goalMatrix.GetLength(0); j++)
                {

                    goalMatrix[j, i] =
                        (max - Convert.ToInt32(goalMatrix[j, i])).ToString();
                }
            }
            Console.WriteLine();
            PrintSeparator();

        }

        private void DeductMinFromRow(string[,] goalMatrix)
        {
            PrintSeparator();
            Console.WriteLine("DeductMinFromRow");
            for (int i = 0; i < goalMatrix.GetLength(0); i++)
            {
                int min = Convert.ToInt32(goalMatrix[i,0]);
                for (int j = 0; j < goalMatrix.GetLength(0); j++)
                {
                    if (min > Convert.ToInt32(goalMatrix[i,j]))
                    {
                        min = Convert.ToInt32(goalMatrix[i,j]);
                    }

                }
                Console.Write(min + "\t");

                for (int j = 0; j < goalMatrix.GetLength(0); j++)
                {

                    goalMatrix[i, j] = 
                        (Convert.ToInt32(goalMatrix[i, j]) - min).ToString();
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

        }
    }
}
