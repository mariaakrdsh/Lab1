using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace LabCalculator
{
    public class TableData
    {
        private int _columnNumber;
        private int _rowNumber;

        public Dictionary<string, MyCell> cellList = new Dictionary<string, MyCell>(); // map клеток
        public Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public TableData(int col, int row)
        {
            _columnNumber = col;
            _rowNumber = row;
        }
        public int  ColCount
        {
            get { return _columnNumber; }
            set { _columnNumber = value; }
        }
        public int RowCount
        {
            get { return _rowNumber; }
            set { _rowNumber = value; }
        }

        public void SetCellsInColumn(int nCol, DataGridView dataGridView1)
        {
            for (int i = 0; i < dataGridView1.RowCount; ++i)
            {
                string cellName = BasedSystem26.To26System(nCol) + Convert.ToString(i + 1);
                MyCell cell = new MyCell(nCol, i, cellName);
                try
                {
                    cellList.Add(cellName, cell);
                    dictionary.Add(cellName, "");
                }
                catch { }
            }
        }

        public void SetCellsInRow(DataGridView dataGridView1)
        {
            int number = dataGridView1.RowCount;
            for (int i = 0; i < dataGridView1.ColumnCount; ++i)
            {
                string cellName = BasedSystem26.To26System(i) + Convert.ToString(number);
                MyCell cell = new MyCell(i, number-1, cellName);
                try
                {
                    cellList.Add(cellName, cell);
                    dictionary.Add(cellName, "");
                }
                catch { }
            }
        }

        public void ChangeCellsAndPointers(DataGridView dataGridView1, string cellName, string expression)
        {
            cellList[cellName].DeletePointers();
            cellList[cellName].Exp = expression;
            cellList[cellName].new_referencesFromThis.Clear();

            if (expression != "")
            {
                if (expression[0] != '=')
                {
                    cellList[cellName].Value = expression;
                    dictionary[cellName] = expression;
                    foreach (MyCell cell in cellList[cellName].pointersToThis)
                    {
                        RefreshCellAndPointers(cell, dataGridView1);
                    }
                    return;
                }
            }

            string new_expression = ConvertReferences(cellName, expression);
            if (new_expression != "")
            {
                new_expression = new_expression.Remove(0, 1);
            }

            if (!cellList[cellName].CheckLoop(cellList[cellName].new_referencesFromThis))
            {
                MessageBox.Show("Помилка, змініть значення.");
                cellList[cellName].Exp = "";
                cellList[cellName].Value = "0";
                dataGridView1.CurrentCell.Value = "0";
                return;
            }

            cellList[cellName].AddPointers();
            string Value = Calculate(new_expression);
            if (Value == "Error")
            {
                MessageBox.Show("Помилка в клітинці " + cellName);
                cellList[cellName].Exp = "";
                cellList[cellName].Value = "0";
                dataGridView1.CurrentCell.Value = "0";
                return;
            }

            cellList[cellName].Value = Value;
            dictionary[cellName] = Value;
            foreach (MyCell cell in cellList[cellName].pointersToThis)
                RefreshCellAndPointers(cell, dataGridView1);
        }

        public bool RefreshCellAndPointers(MyCell cell, DataGridView dataGridView1)
        {
            cell.new_referencesFromThis.Clear();
            string new_expression = ConvertReferences(cell.Name, cell.Exp);
            new_expression = new_expression.Remove(0, 1);
            string Value = Calculate(new_expression);

            if (Value == "Error")
            {
                MessageBox.Show("Помилка в клітинці " + cell.Name);
                cell.Exp = "";
                cell.Value = "0";
                dataGridView1.CurrentCell.Value = "0";
                dictionary[cell.Name] = "0";
                dataGridView1[cell.Column, cell.Row].Value = 0;
                return false;
            }

            cellList[cell.Name].Value = Value;
            dictionary[cell.Name] = Value;
            dataGridView1[cell.Column, cell.Row].Value = Value;

            foreach (MyCell point in cell.pointersToThis)
            {
                if (!RefreshCellAndPointers(point, dataGridView1))
                    return false;
            }

            return true;
        }

        public string ConvertReferences(string cellName, string expression)
        {
            string cellPattern = @"[A-Z]+[0-9]+";
            Regex regex = new Regex(cellPattern, RegexOptions.IgnoreCase);

            foreach (Match match in regex.Matches(expression))
            {
                if (dictionary.ContainsKey(match.Value))
                {
                    cellList[cellName].new_referencesFromThis.Add(cellList[match.Value]);
                }
            }
            MatchEvaluator evaluator = new MatchEvaluator(referenceToValue);
            string new_expression = regex.Replace(expression, evaluator);
            return new_expression;
        }

        public string referenceToValue(Match m)
        {
            if (dictionary.ContainsKey(m.Value))
                if (dictionary[m.Value] == "")
                    return "0";
                else
                    return dictionary[m.Value];
            return m.Value;
        }

        public string Calculate(string expression)
        {
            string res = null;
            try
            {
                res = Convert.ToString(LabCalculator.Calculator.Evaluate(expression));
                if (res == "∞")
                {
                    res = "Помилка";
                }
                return res;
            }
            catch
            {
                return "Error";
            }
        }
        public bool ColumnDelete(DataGridView dataGridView1)
        {
            List<MyCell> lastCol = new List<MyCell>(); //cells that have references on the last col
            List<string> notEmptyCells = new List<string>();
            if (_columnNumber == 1 )
                return false;
            int curCount = _columnNumber - 1;
            for (int i = 0; i < _rowNumber; i++)
            {
                string name = BasedSystem26.To26System(curCount) + (i + 1).ToString();
                if (dictionary[name] != "0" && dictionary[name] != "" && dictionary[name] != " ")
                    notEmptyCells.Add(name);
                if (cellList[name].pointersToThis.Count != 0)
                    lastCol.AddRange(cellList[name].pointersToThis);
            }

            if (lastCol.Count != 0 || notEmptyCells.Count != 0)
            {
                string errorMessage = "";
                if (notEmptyCells.Count != 0)
                {
                    errorMessage = "Стовпчик має непорожню клітинку: ";
                    errorMessage += string.Join("; ", notEmptyCells.ToArray());
                }
                if (lastCol.Count != 0)
                {
                    errorMessage += "\nЄ клітинки, які мають посилання на клітинку, яка може бути видалена: ";
                    foreach (MyCell cell in lastCol)
                        errorMessage += string.Join("; ", cell.Name);
                }
                errorMessage += "\nВи впевнені, що хочете видалити цей стовпчик?";
                System.Windows.Forms.DialogResult res = System.Windows.Forms.MessageBox.Show(errorMessage, "УВАГА", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
                if (res == System.Windows.Forms.DialogResult.No)
                    return false;
            }
            for (int i = 0; i < _rowNumber; i++)
            {
                string name = BasedSystem26.To26System(curCount) + (i + 1).ToString();
                dictionary.Remove(name);
            }
            foreach (MyCell cell in lastCol)
                RefreshCellAndPointers(cell, dataGridView1);
            for (int i = 0; i < _rowNumber; i++)
            {
                string name = BasedSystem26.To26System(curCount) + (i + 1).ToString();
                cellList.Remove(name);
            }
            _columnNumber--;
            return true;
        }

        public bool RowDelete(DataGridView dataGridView1)
        {
            List<MyCell> lastRow = new List<MyCell>(); //cells that have references on the last row
            List<string> notEmptyCells = new List<string>();
            if (_rowNumber == 1)
                return false;
            int curRow = _rowNumber - 1;
            for (int i = 0; i < _columnNumber; i++)
            {
                string name = BasedSystem26.To26System(i) + (curRow + 1).ToString();
                if (dictionary[name] != "0" && dictionary[name] != "" && dictionary[name] != " ")
                    notEmptyCells.Add(name);
                if (cellList[name].pointersToThis.Count != 0)
                    lastRow.AddRange(cellList[name].pointersToThis);
            }

            if (lastRow.Count != 0 || notEmptyCells.Count != 0)
            {
                string errorMessage = "";
                if (notEmptyCells.Count != 0)
                {
                    errorMessage = "Рядок має непорожню клітинку: ";
                    errorMessage += string.Join("; ", notEmptyCells.ToArray());
                    errorMessage += ' ';
                }
                if (lastRow.Count != 0)
                {
                    errorMessage += "\nЄ клітинки, які мають посилання на клітинку, яка може бути видалена: ";
                    foreach (MyCell cell in lastRow)
                    {
                        errorMessage += string.Join("; ", cell.Name);
                        errorMessage += " ";
                    }

                }
                errorMessage += "\nВи впевнені, що хочете видалити цей рядок?";
                System.Windows.Forms.DialogResult res = 
                    System.Windows.Forms.MessageBox.Show(errorMessage, "УВАГА", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Warning);
                if (res == System.Windows.Forms.DialogResult.No)
                    return false;
            }
            for (int i = 0; i < _columnNumber; i++)
            {
                string name = BasedSystem26.To26System(i) + (curRow + 1).ToString();
                dictionary.Remove(name);
            }
            foreach (MyCell cell in lastRow)
                RefreshCellAndPointers(cell, dataGridView1);
            //cellList.Remove(curRow.ToString());
            for (int i = 0; i < _columnNumber; i++)
            {
                string name = BasedSystem26.To26System(i) + (curRow + 1).ToString();
                //dictionary.Remove(name);
                cellList.Remove(name);
            }
            _rowNumber--;
            return true;
        }
        public void Save(StreamWriter sw)
        {
            sw.WriteLine(_rowNumber);
            sw.WriteLine(_columnNumber);
            foreach (KeyValuePair<string, MyCell> element in cellList)
            {
                string name = element.Key;
                MyCell cell = element.Value;
                sw.WriteLine(cell.Name);
                sw.WriteLine(cell.Exp);
                sw.WriteLine(cell.Value);

                if (cell.referencesFromThis == null)
                {
                    sw.WriteLine("0");
                }
                else
                {
                    sw.WriteLine(cell.referencesFromThis.Count);
                    foreach (MyCell cellRef in cell.referencesFromThis)
                    {
                        sw.WriteLine(cellRef.Name);
                    }
                }
                if (cell.pointersToThis == null)
                {
                    sw.WriteLine("0");
                }
                else
                {
                    sw.WriteLine(cell.pointersToThis.Count);
                    foreach (MyCell cellPoint in cell.pointersToThis)
                    {
                        sw.WriteLine(cellPoint.Name);
                    }
                }
            }
        }
        public void Open(int row, int col, StreamReader sr, DataGridView dataGridView)
        {
            int cellCount = row * col;
            for (int i = 0; i < col * row; ++i)
            {
                string index = sr.ReadLine();
                string expression = sr.ReadLine();
                string value = sr.ReadLine();

                if (expression != "")
                    dictionary[index] = value;
                else
                    dictionary[index] = "";

                int refCount = Convert.ToInt32(sr.ReadLine());
                List<MyCell> newRef = new List<MyCell>();
                string refer;

                for (int k = 0; k < refCount; k++)
                {
                    refer = sr.ReadLine();
                    if (BasedSystem26.From26System(refer).row < _rowNumber
                        && BasedSystem26.From26System(refer).column < _columnNumber)
                        newRef.Add(cellList[refer]);
                }

                int pointCount = Convert.ToInt32(sr.ReadLine());
                List<MyCell> newPoint = new List<MyCell>();
                string point;

                for (int k = 0; k < pointCount; k++)
                {
                    point = sr.ReadLine();
                    newPoint.Add(cellList[point]);
                }

                string name = BasedSystem26.To26System(col) + (row + 1).ToString();
                cellList[index].setCell(expression, value, newRef, newPoint);

                int curCol = cellList[index].Column;
                int curRow = cellList[index].Row;
                dataGridView[curCol, curRow].Value = dictionary[index];

            }
        }

    }
}