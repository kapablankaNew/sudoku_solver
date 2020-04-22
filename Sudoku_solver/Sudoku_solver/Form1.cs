using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sudoku_solver
{
    public partial class Form1 : Form
    {
        TextBox[,] data;
        string global_filename;

        bool Auto = false;

        public Form1(string[] args)
        {
            InitializeComponent();
            data = new TextBox[9, 9];
            global_filename = "";
            //code for nice displayed
            foreach (TextBox tb in this.Controls.OfType<TextBox>())
            {
                //all textboks have names "textBox№", № - number of textbox
                var name = tb.Name;
                //find number of textbox
                var num = Convert.ToInt32(name.Substring(7)) - 1;
                //put the textbox in its cell
                data[num / 9, num % 9] = tb;
                //calculate coordinates of textbox
                var x = 15 + ((num % 9) / 3) * 15 + (num % 9) * 35;
                var y = 65 + ((num / 9) / 3) * 15 + (num / 9) * 35;
                //set parameters of textbox
                tb.Size = new Size(30, 30);
                tb.Location = new Point(x, y);
                tb.ForeColor = Color.Red;
                //add event handlers for validation
                tb.TextChanged += new System.EventHandler(Validation);
            }
            //this cicle for nice displayed of vertical and horizontal lines
            foreach (MyLine ml in this.Controls.OfType<MyLine>())
            {
                //all lines have names "myLine№", № - number of line
                //find number of line
                var num = Convert.ToInt32(ml.Name.Substring(6));
                //for same line set size and position
                switch (num)
                {
                    case 1:
                        ml.Size = new Size(350, 1);
                        ml.Location = new Point(10, 60);
                        break;
                    case 2:
                        ml.Size = new Size(350, 1);
                        ml.Location = new Point(10, 175);
                        break;
                    case 3:
                        ml.Size = new Size(350, 1);
                        ml.Location = new Point(10, 295);
                        break;
                    case 4:
                        ml.Size = new Size(350, 1);
                        ml.Location = new Point(10, 410);
                        break;
                    case 5:
                        ml.Size = new Size(1, 350);
                        ml.Location = new Point(10, 60);
                        break;
                    case 6:
                        ml.Size = new Size(1, 350);
                        ml.Location = new Point(125, 60);
                        break;
                    case 7:
                        ml.Size = new Size(1, 350);
                        ml.Location = new Point(245, 60);
                        break;
                    case 8:
                        ml.Size = new Size(1, 350);
                        ml.Location = new Point(360, 60);
                        break;
                }
            }

            if (args.Length > 0)
            {
                try
                {
                    var dat = Load_from_file(args[0]);
                    //write the read sudoku in field
                    for (int k = 0; k < 9; k++)
                    {
                        for (int h = 0; h < 9; h++)
                        {
                            data[k, h].ForeColor = Color.Red;
                            if (dat[k, h] != 0)
                                data[k, h].Text = dat[k, h].ToString();
                        }
                    }
                    global_filename = args[0];
                    //check corectness the read sudoku
                    Check_sudoku();
                }
                catch(Exception ea)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        for (int h = 0; h < 9; h++)
                        {
                            data[k, h].ForeColor = Color.Red;
                            data[k, h].Text = "";
                        }
                    }
                    MessageBox.Show("Attention! Error!\n" + ea.Message);
                }
            }
        }

        private void Validation(object sender, EventArgs e)
        {
            if (!Auto)
            {
                try
                {
                    Check_sudoku();
                }
                catch (Exception ea)
                {
                    MessageBox.Show("Attention! Error!\n" + ea.Message);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button_solve_Click(object sender, EventArgs e)
        {
            try
            {
                //numbers - array of input data
                var numbers = new int[9, 9];
                //sorting through all textboxs
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (data[i, j].Text != "")
                        {
                            //read number and check the range
                            var s = Convert.ToInt32(data[i, j].Text);
                            if (s < 1 || s > 9)
                                throw new Exception("Invalid number!");
                            numbers[i, j] = s;
                        }
                        //if in cell don't number - write 0
                        else
                        {
                            numbers[i, j] = 0;
                        }
                    }                   
                }
                //create new sudoku
                var sudoku = new Sudoku(numbers);
                //check the corectness of input data
                sudoku.Check();
                //find solve of sudoku
                var solve = sudoku.Solve();
                //print this solve
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        //if in cell don't number - write number in black
                        if (data[i, j].Text == "")
                        {
                            data[i, j].ForeColor = Color.Black;
                            data[i, j].Text = solve[i, j].ToString();
                        }
                        else
                        {
                            if (Convert.ToInt32(data[i, j].Text) != solve[i, j])
                                throw new Exception("Invalid Solve!");
                        }
                    }
                }
            }
            catch (Exception ea)
            {
                MessageBox.Show("Attention! Error!\n" + ea.Message);
            }
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            foreach (var s in data)
            {
                s.Text = "";
                s.ForeColor = Color.Red;
            }
        }

        private void button_clear_solve_Click(object sender, EventArgs e)
        {
            foreach (var s in data)
            {
                //if number is solve - clear this cell
                if (s.ForeColor == Color.Black)
                {
                    s.Text = "";
                    s.ForeColor = Color.Red;
                }
            }
        }

        private void button_auto_Click(object sender, EventArgs e)
        {
            //changing value to disable of validation
            Auto = true;
            Random rand = new Random();
            //clear all cells
            button_clear_Click(sender, e);
            for (; ; )
            {
                //sorting through creating sudoku while they are uncorrect
                try
                {
                    foreach (var s in data)
                    {
                        //in some cells add numbers from 1 to 9
                        double n = rand.NextDouble();
                        if (n > 0.75)
                        {
                            int dat = rand.Next(1, 10);
                            s.Text = dat.ToString();
                        }
                    }
                    //check correctness of sudoku
                    Check_sudoku();

                    //if Sudoku correct - go out
                    break;
                }
                catch (Exception ea)
                {
                    //if sudoku uncorrect, clear all cells and try again
                    button_clear_Click(sender, e);
                }
            }
            //enable validation
            Auto = false;
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            global_filename = "";
            button_clear_Click(sender, e);
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //check correctness of sudoku
                Check_sudoku();
                //save data in default file
                if (global_filename != "")
                {
                    Save_in_file(global_filename);
                    MessageBox.Show("Sudoku save in file");
                }
                else
                {
                    сохранитьКакToolStripMenuItem_Click(sender, e);
                }
            }
            catch (Exception ea)
            {
                MessageBox.Show("Attention! Error!\n" + ea.Message);
            }
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //check correctness of sudoku
                Check_sudoku();
                //user select file
                var saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "Sudoku files(*.sud)|*.sud";
                if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                //get name of selected file
                string filename = saveFileDialog1.FileName;
                //save data in file
                Save_in_file(filename);
                MessageBox.Show("Sudoku save in file");
                global_filename = filename;
            }
            catch (Exception ea)
            {
                MessageBox.Show("Attention! Error!\n" + ea.Message);
            }
        }

        private void загрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //user selest file
                var openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Sudoku files(*.sud)|*.sud";
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                //get name of selected file
                string filename = openFileDialog1.FileName;
                var dat = Load_from_file(filename);
                //clear field
                button_clear_Click(sender, e);
                //write the read sudoku in field
                for (int k = 0; k < 9; k++)
                {
                    for (int h = 0; h < 9; h++)
                    {
                        data[k, h].ForeColor = Color.Red;
                        if (dat[k, h] != 0)
                            data[k, h].Text = dat[k, h].ToString();
                    }
                }
                global_filename = filename;
                //check corectness the read sudoku
                Check_sudoku();
            }
            catch (Exception ea)
            {
                button_clear_Click(sender, e);
                MessageBox.Show("Attention! Error!\n" + ea.Message);
            }
        }

        private void Save_in_file(string file_name = null)
        {
            //find name of file
            string name;
            //default file
            if (file_name == null)
                name = "Sudoku.txt";
            else
                name = file_name;
            StreamWriter sw = new StreamWriter(name);
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //if number in textbox - write this in file
                    if (data[i, j].Text != "")
                    {
                        sw.Write(data[i, j].Text);
                    }
                    //empty cells replace with .
                    else
                    {
                        sw.Write(".");
                    }
                }
                sw.WriteLine();
            }
            //close file, clear resources
            sw.Close();
            sw.Dispose();
        }

        private int[,] Load_from_file(string file_name = null)
        {
            StreamReader sr = new StreamReader(file_name);
            //read file line by line and check data in file
            var dat = new int[9, 9];
            for (int i = 0; i < 9; i++)
            {
                var s = sr.ReadLine();
                if (s.Length != 9)
                    throw new Exception("Uncorrect file!");
                var j = 0;
                foreach (char elem in s)
                {
                    if (elem == '.')
                        dat[i, j] = 0;
                    else
                        dat[i, j] = Convert.ToInt32(elem.ToString());
                    j++;
                }
            }

            return dat;
        }

        private void Check_sudoku()
        {
            //numbers - array of input data
            var numbers = new int[9, 9];
            //sorting through all textboxs
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (data[i, j].Text != "")
                    {
                        //read number and check the range
                        var s = Convert.ToInt32(data[i, j].Text);
                        if (s < 1 || s > 9)
                            throw new Exception("Invalid number!");
                        numbers[i, j] = s;
                    }
                    //if in cell don't number - write 0
                    else
                    {
                        numbers[i, j] = 0;
                    }
                }
            }
            //create new sudoku
            var sudoku = new Sudoku(numbers);
            //check the corectness of input data
            sudoku.Check();
            //delete created sudoku
            sudoku.Dispose();
        }
    }

    //special class for drawing sample lines
    class MyLine : System.Windows.Forms.Control
    {
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            System.Drawing.Pen pen = new System.Drawing.Pen(this.ForeColor, this.Width);
            e.Graphics.DrawRectangle(pen, 0, 0, this.Width, this.Height);
            base.OnPaint(e);
        }
    }
}
