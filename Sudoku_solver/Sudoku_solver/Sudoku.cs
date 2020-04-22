using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku_solver
{
    //special class for storage of sudoku
    public class Sudoku : IDisposable
    {
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            //suppres finalization
            //this prevents the system from executing the method finalize() 
            //for the current object
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                { 
                    //release the managed resources
                }
                //release the unmanaged resources
                disposed = true;
            }
        }
        //array for storage of sudoku itself 
        public int[,] Field { get; private set; }

        //array for probably variants in each cell
        public List<int>[,] Probably { get; private set; }

        public Sudoku(int[,] data)
        {
            //check input data
            if (data.GetLength(0) != 9 || data.GetLength(1) != 9)
            {
                throw new Exception("Размер поля не соответствует стандартному размеру!");
            }
            Field = data;
            Probably = new List<int>[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //for each cell get all probably variants
                    Probably[i, j] = Options(i, j);
                }
            }
        }

        public Sudoku(int[,] data, List<int>[,] maybe)
        {
            //check input data
            if (data.GetLength(0) != 9 || data.GetLength(1) != 9 || maybe.GetLength(0) != 9 || maybe.GetLength(1) != 9)
            {
                throw new Exception("Размер поля не соответствует стандартному размеру!");
            }
            Field = data;
            Probably = maybe;
        }

        //destructor
        ~Sudoku()
        {
            Dispose(true);
        }

        //this metod return list of all posible number for cell with the specified coordinates
        private List<int> Options(int row, int column)
        {
            //all possible variants for empty sudoku
            var List_of_options = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            //if in cell is number (not zero), this number is only possible number
            if (Field[row, column] != 0)
            {
                List_of_options.Clear();
                List_of_options.Add(Field[row, column]);
            }
            else
            {
                //sorting through the element in row and column
                for (int i = 0; i < 9; i++)
                {
                    //if same number is in this row and column - delete him from list of possible numbers
                    if (List_of_options.Contains(Field[row, i]) && i != column)
                        List_of_options.Remove(Field[row, i]);
                    if (List_of_options.Contains(Field[i, column]) && i != row)
                        List_of_options.Remove(Field[i, column]);
                }
                //calculate, in which square is located element
                var Square_X = row / 3;
                var Square_Y = column / 3;
                for (int j = 0; j < 3; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        //check all elements of this square
                        var elem = Field[Square_X * 3 + k, Square_Y * 3 + j];
                        if (Square_X * 3 + k != row || Square_Y * 3 + j != column)
                        {
                            if (List_of_options.Contains(elem))
                                List_of_options.Remove(elem);
                        }
                    }
                }
            }
            return List_of_options;
        }

        public int[,] Solve()
        {
            //new_data - solve of sudoku
            var new_data = new int[9, 9];
            //check - true if all cells of sudoku are filled
            bool check = true;
            //check_1 - true, if there aren't cells that has filled
            bool check1 = true;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //if in cell can stand only one number
                    if (Probably[i, j].Count == 1)
                    {
                        new_data[i, j] = Probably[i, j][0];
                        //if we fill at lest one cell - reset check_1
                        if (Field[i, j] == 0)
                            check1 = false;
                    }
                    else
                    {
                        //if in same cell aren't possible variants, sudoku hasn't solve
                        if (Probably[i, j].Count == 0)
                            throw new Exception("No solves!");
                        //if in the same cell can stand several numbers - reset check
                        new_data[i, j] = 0;
                        check = false;
                    }
                }
            }
            //if all cells of sudoku are filled - this is solve, return his 
            if (check)
                return new_data;
            if (check1)
            {
                // if there aren't cells that has filled, find cell with a minimum number of possible options
                var num = Least_Options();
                foreach (var nums in Probably[num[0], num[1]])
                {
                    //try fill this cell in turn all possible options
                    try
                    {
                        new_data[num[0], num[1]] = nums;
                        //create new sudoku
                        var new_sud = new Sudoku(new_data);
                        //check his correctness
                        new_sud.Check();
                        //find solve
                        var res = new_sud.Solve();
                        //check his correctness
                        var sud_2 = new Sudoku(res);
                        sud_2.Check();
                        return res;
                    }
                    //if in this case aren't solves - try next option
                    catch (Exception ea)
                    { }
                }
                throw new Exception("No solves!");
            }
            //if they are at least one cell that had filled - fill the cells again, into account the changes
            else
            {
                var sud = new Sudoku(new_data);
                return sud.Solve();
            }
        }

        //this method find in which cell there is a minimum number of possible options other than one
        private int[] Least_Options()
        {
            
            var coord = new int[2] { 0, 0};
            var min = 9;
            //check all cells
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Probably[i, j].Count > 1)
                    {
                        if (Probably[i, j].Count < min)
                        {
                            //update coordinates
                            min = Probably[i, j].Count;
                            coord[0] = i;
                            coord[1] = j;
                        }
                    }
                    //if not possible options - sudoku isn't correct
                    if (Probably[i, j].Count == 0)
                        throw new Exception("No solves!");
                }
            }
            //return coordinates of cell
            return coord;
        }

        public void Check()
        {
            //this method check the corectness of sudoku
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //for each not zero element check 
                    if (Field[i, j] != 0)
                    {
                        //check, are there identical elements in row and column 
                        for (int k = 0; k < 9; k++)
                        {
                            if (Field[i, k] == Field[i, j] && k != j)
                                throw new Exception("Uncorrect sudoku!");
                            if (Field[k, j]== Field[i, j] && k != i)
                                throw new Exception("Uncorrect sudoku!");
                        }
                        //calculate, in which square is located element 
                        var Square_X = i / 3;
                        var Square_Y = j / 3;
                        //check, are there identical elements in this square
                        for (int h = 0; h < 3; h++)
                        {
                            for (int g = 0; g < 3;  g++)
                            {
                                var elem = Field[Square_X * 3 + g, Square_Y * 3 + h];
                                if (Square_X * 3 + g != i || Square_Y * 3 + h != j)
                                {
                                    if (elem == Field[i, j])
                                        throw new Exception("Uncorrect sudoku!");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
