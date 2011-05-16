using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using IDevice.IPhone;
using IDevice.Managers;
using System.IO;
using LevDan.Exif;
using System.Data.SQLite;
using System.Data.Common;

namespace IDevice.Plugins.Analyzers.SMS
{
    public partial class SMSAnalyzer : Form
    {
        private string _smsDbFilePath;
        
        private Array _smsText;
        private Array _smsFilterText;
        private Array _selectStrings;
        
        private IPhoneBackup _backup;
        private BackgroundWorker _worker;

        public SMSAnalyzer(IPhoneBackup backup, string smsDbFilePath)
        {
            _smsDbFilePath = smsDbFilePath;
            _backup = backup;
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(_worker_DoWork);
            _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
            _worker.WorkerReportsProgress = true;

            InitializeComponent();

            if (_backup != null)
                this.Text += " [" + _backup.DisplayName + "]";
        }

        
        List<T> extractTableCollumn<T>(string tableName, string collumnName, Type type) 
        {
            List<T> list = new List<T>();

            // Extract collumn from table
            SQLiteConnection SQLiteConnection = new SQLiteConnection(@"Data Source=" + _smsDbFilePath);
            SQLiteConnection.Open();

            SQLiteCommand SQLiteCommand = new SQLiteCommand();
            SQLiteCommand.Connection = SQLiteConnection;
            SQLiteCommand.CommandText = "SELECT " + collumnName + " FROM " + tableName + ";";
            SQLiteCommand.CommandType = CommandType.Text;
            DbDataReader reader = SQLiteCommand.ExecuteReader();

            // Fill the list with the rows of the collumn
            while (reader.Read())
            {
                if (reader.HasRows && !reader.IsClosed)
                {
                    object obj = new object();
                    obj = reader.GetValue(0);
                    Type objType = obj.GetType();

                    // Error handling
                    if (!objType.Equals(type))
                        obj = Convert.ChangeType(obj,type);
                    

                    list.Insert(0,(T)obj);
                }
            }

            // Close reader and connection
            reader.Close();
            SQLiteConnection.Close();

            return list;
        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var x = e.Result as dynamic;
            smsListBox.BeginInvoke(new MethodInvoker(delegate()
            {
                smsListBox.Items.AddRange(x.ListBox);
            }));

            // Save for later use
            _selectStrings = x.ListBox;
            _smsText = _smsFilterText = x.TextMessages;
        }

        void _worker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<String>    text = extractTableCollumn<String>("message", "text",Type.GetType("System.String"));
            List<Int64>     date = extractTableCollumn<Int64>("message", "date", Type.GetType("System.Int64"));
            List<String>    address = extractTableCollumn<String>("message", "address", Type.GetType("System.String"));
            List<Int64>     flags = extractTableCollumn<Int64>("message", "flags", Type.GetType("System.Int64"));

            // Construct message selection strings
            List<String> selectStrings = new List<String>();
            List<Int64>.Enumerator dateEnumer = date.GetEnumerator();
            List<Int64>.Enumerator flagsEnumer = flags.GetEnumerator();
            DateTime dateTime;
            foreach(string s in address)
            {
                if (dateEnumer.MoveNext() && flagsEnumer.MoveNext())
                {
                    String concatString = "";
                    dateTime = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(dateEnumer.Current);

                    concatString += dateTime.ToString()+"        ";
                    switch (flagsEnumer.Current)
                    {
                        case 2:
                            concatString += "<FROM> ";
                            break;
                        case 3:
                            concatString += "<TO> ";
                            break;
                        default:
                            concatString += "<Flag(" + flagsEnumer.Current + ")> ";
                            break;
                    }
                    concatString += "\x9"+s;

                    selectStrings.Add(concatString);
                }
            }

            e.Result = new { ListBox = selectStrings.ToArray(), TextMessages = text.ToArray() };
        }

        private void SMSAnalyzer_Load(object sender, EventArgs e)
        {
            _worker.RunWorkerAsync(_backup);
        }

        private void smsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string text = smsListBox.GetItemText(smsListBox.SelectedItem);
            int index = smsListBox.FindString(text);

            if (index != -1)
            {
                smsTextBox.Clear();
                smsTextBox.AppendText(_smsFilterText.GetValue(index).ToString());
            }
        }

        private void filterButton_Click(object sender, EventArgs e)
        {
            List<string> resultSelectList = new List<string>();
            List<string> resultTextList = new List<string>();
            string filterString = filterTextBox.Text;

            // Filter
            int index = 0;
            foreach (string s in _selectStrings)
            {
                if (filterString.Length == 0 || s.ToLower().Contains(filterString.ToLower()))
                {
                    resultSelectList.Add(s);
                    resultTextList.Add(_smsText.GetValue(index).ToString());
                }
                index++;
            }

            _smsFilterText = resultTextList.ToArray();
            // Update selection list
            smsTextBox.Clear();
            smsListBox.BeginInvoke(new MethodInvoker(delegate()
            {
                smsListBox.Items.Clear();
                smsListBox.Items.AddRange(resultSelectList.ToArray());
            }));
        }
    }
}
