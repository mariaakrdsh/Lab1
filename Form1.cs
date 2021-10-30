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

namespace LabCalculator
{
    public partial class Form1 : Form
    {
        private int columnnum = 3;
        private int rownum = 3;
        private TableData _table;
        public Form1()
        {
            InitializeComponent();
            _table = new TableData(columnnum, rownum);

            CreateDataGrid(rownum, columnnum);
            WindowState = FormWindowState.Maximized;
        }

        private void CreateDataGrid(int rows, int column)
        {
           
            for (int i = 0; i < column; ++i)
            {
                DataGridViewColumn exelColumn = new DataGridViewColumn();
                MyCell cell = new MyCell();
                exelColumn.CellTemplate = cell;
                exelColumn.HeaderText = BasedSystem26.To26System(i);
                exelColumn.Name = BasedSystem26.To26System(i);
                dataGridView1.Columns.Add(exelColumn);
            }
            for (int i = 0; i < rows ; ++i)
            {
                dataGridView1.Rows.Add();
            }
            for (int i = 0; i <dataGridView1.ColumnCount; ++ i)
            {
                _table.SetCellsInColumn(i, dataGridView1);
            }
            SetRowNum(dataGridView1);
            dataGridView1.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }

        public void SetRowNum (DataGridView dataGridView)
        {
            for(int i = 0; i < dataGridView.RowCount; ++i)
            {
                dataGridView.Rows[i].HeaderCell.Value = (i +1).ToString();
            }
        }
        private void AddRow_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add();
            SetRowNum(dataGridView1);
            _table.SetCellsInRow(dataGridView1);
            _table.RowCount++;
        }


        private void DeleteRow_Click(object sender, EventArgs e)
        {
            if (!_table.RowDelete(dataGridView1))
                return;
            dataGridView1.Rows.RemoveAt(_table.RowCount);
            SetRowNum(dataGridView1);
        }


        private void AddColumn_Click(object sender, EventArgs e)
        {
        DataGridViewColumn excelColumn = new DataGridViewColumn();
        MyCell cell = new MyCell();
        excelColumn.CellTemplate = cell;
        excelColumn.HeaderText = BasedSystem26.To26System(dataGridView1.ColumnCount);
        excelColumn.Name = BasedSystem26.To26System(dataGridView1.ColumnCount);

        dataGridView1.Columns.Add(excelColumn);
            _table.SetCellsInColumn(dataGridView1.ColumnCount-1, dataGridView1);
            _table.ColCount++;
    }

        private void DeleteColumn_Click(object sender, EventArgs e)
        {
            if (!_table.ColumnDelete(dataGridView1))
                return;
            dataGridView1.Columns.RemoveAt(_table.ColCount);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "File|*.txt";
            saveFileDialog.Title = "Save file";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                FileStream fs = (FileStream)saveFileDialog.OpenFile();
                StreamWriter sw = new StreamWriter(fs);
                _table.Save(sw);
                sw.Close();
                fs.Close();
            }
        }


        private void Form1_Load(object sender, FormClosingEventArgs e)
        {


            DialogResult result = MessageBox.Show(
                     "Ви впевнені, що хочете закрити?",
                     "Увага",
                     MessageBoxButtons.OKCancel,
                     MessageBoxIcon.Information,
                     MessageBoxDefaultButton.Button1,
                     MessageBoxOptions.DefaultDesktopOnly);

            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void CalculateExcel()
        {

            int currRow = dataGridView1.CurrentCell.RowIndex;
            int currCol = dataGridView1.CurrentCell.ColumnIndex;
            string cellName = BasedSystem26.To26System(currCol) + (currRow + 1).ToString();
            if (dataGridView1.CurrentCell.Value == null)
            {
                _table.cellList[cellName].Clear();
                MessageBox.Show("Увага, клітинка порожня");
                return;
            }
            string expression;
            if (_table.cellList[cellName].Exp == "" || dataGridView1.CurrentCell.Value.ToString() != _table.cellList[cellName].Exp)
            {
                expression = (dataGridView1.CurrentCell.Value).ToString();
            }
            else
            {
                expression = _table.cellList[cellName].Exp;
            }
            _table.ChangeCellsAndPointers(dataGridView1, cellName, expression);
            dataGridView1.CurrentCell.Value = _table.cellList[cellName].Value;
            textBox1.Text = _table.cellList[cellName].Exp;
        }
        private void Calculate_Click(object sender, EventArgs e)
        {
            CalculateExcel();
        }

        private void helpToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Наша програма має наступні операції:\n\t" +
                "1.'+, -, *,/'\n\t" +
                "2. '^'\n\t " +
                "3. 'mod, div'",
                "Допоміжне меню");
        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
            int row = dataGridView1.CurrentCell.RowIndex;
            int col = dataGridView1.CurrentCell.ColumnIndex;
            string cellName = BasedSystem26.To26System(col) + (row + 1).ToString();
            textBox1.Text = _table.cellList[cellName].Exp;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "File|*.txt";
                openFileDialog.Title = "Open File";
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;
                StreamReader sr = new StreamReader(openFileDialog.FileName);
                dataGridView1.Rows.Clear();
                dataGridView1.Columns.Clear();
                int row; int column;
                Int32.TryParse(sr.ReadLine(), out row);
                Int32.TryParse(sr.ReadLine(), out column);
                CreateDataGrid(row, column);
                _table.Open(row, column, sr, dataGridView1);
                sr.Close();
            }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            CalculateExcel();
        }
    }
}
