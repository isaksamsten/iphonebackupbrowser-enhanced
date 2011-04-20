using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data.Common;
using IDevice.Plugins.Browsers.SQL;

namespace IDevice.Plugins.Browsers.SQL
{
    public partial class SQLiteBrowser : AbstractBrowsable
    {
        private static string m_DatabaseFilePath;
        private Dictionary<string, Table> m_Tables = new Dictionary<string, Table>();


        public SQLiteBrowser()
            : base("*.db")
        {
            InitializeComponent();
        }


        #region Available Tables Extraction

        /// <summary>
        /// Extract tables from the specified database by supplying the full path to the SQLite database file
        /// </summary>
        /// <param name="databaseFilePath">The full path to the database file</param>
        public void databaseTableExtraction(string databaseFilePath)
        {
            m_DatabaseFilePath = databaseFilePath;

            SQLiteConnection SQLiteConnection = new SQLiteConnection(@"Data Source=" + m_DatabaseFilePath);
            SQLiteConnection.Open();

            SQLiteCommand SQLiteCommand = new SQLiteCommand();
            SQLiteCommand.Connection = SQLiteConnection;
            SQLiteCommand.CommandText = "SELECT * FROM sqlite_master;";
            SQLiteCommand.CommandType = CommandType.Text;
            DbDataReader reader = SQLiteCommand.ExecuteReader();

            while (reader.Read())
            {
                addDatabaseTable(reader["name"].ToString());
            }

            SQLiteConnection.Close();
        }

        private void addDatabaseTable(string tableName)
        {
            m_Tables.Add(tableName, new Table(tableName));
            databaseTableList.Items.Add(tableName);
        }

        #endregion

        #region Columns From Table Extraction

        public void getColumnsInTable(string tableName)
        {
            clearLists();

            Table currentTable = m_Tables[tableName];

            if (currentTable.getSelectColumns().Length == 0)
                return;

            string[] columns = currentTable.getSelectColumns().Split(' ');

            SQLiteConnection SQLiteConnection = new SQLiteConnection(@"Data Source=" + m_DatabaseFilePath);
            SQLiteConnection.Open();

            SQLiteCommand SQLiteCommand = new SQLiteCommand();
            SQLiteCommand.Connection = SQLiteConnection;


            SQLiteCommand = new SQLiteCommand();
            SQLiteCommand.Connection = SQLiteConnection;

            SQLiteCommand.CommandText = ("PRAGMA table_info( " + tableName + " );");
            SQLiteCommand.CommandType = CommandType.Text;

            DbDataReader reader = SQLiteCommand.ExecuteReader();

            while (reader.Read())
            {
                currentTable.addNewColumn(new Column(reader[1].ToString()));
            }

            foreach (Column currentColumn in currentTable.getColumns())
            {
                tableColumnList.Items.Add(currentColumn.name, true);
                tableContentList.Columns.Add(currentColumn.name);
            }

            populateTableView(currentTable.getTableName());

            SQLiteConnection.Close();
        }

        public void populateTableView(string tableName)
        {
            try
            {
                Table currentTable = m_Tables[tableName];

                string query = "SELECT " + currentTable.getSelectColumnsForSelectStatement() + " FROM " + tableName + ";";

                SQLiteConnection SQLiteConnection = new SQLiteConnection(@"Data Source=" + m_DatabaseFilePath);
                SQLiteConnection.Open();

                SQLiteCommand SQLiteCommand = new SQLiteCommand();
                SQLiteCommand.Connection = SQLiteConnection;


                SQLiteCommand = new SQLiteCommand();
                SQLiteCommand.Connection = SQLiteConnection;

                SQLiteCommand.CommandText = (query);
                SQLiteCommand.CommandType = CommandType.Text;
                DbDataReader reader = SQLiteCommand.ExecuteReader();



                while (reader.Read())
                {

                    ListViewItem currentItem = new ListViewItem();



                    foreach (Column currentColumn in currentTable.getColumns())
                    {
                        try
                        {
                            if (currentItem.Text.Length == 0)
                                currentItem.Text = reader[currentColumn.name].ToString();
                            else
                                currentItem.SubItems.Add(reader[currentColumn.name].ToString());

                        }
                        catch
                        {
                            //don't prompt user
                        }

                    }
                    tableContentList.Items.Add(currentItem);
                }

                //Arrange column headers to display everything
                int numberOfColumns = currentTable.getColumns().Count;
                for (int i = 0; i < numberOfColumns; i++)
                {
                    tableContentList.Columns[i].Width = -2;
                }



                SQLiteConnection.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Auto-generated function, don't add logic here.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void databaseTableList_DoubleClick(object sender, EventArgs e)
        {
            ListBox tableListBox = (ListBox)sender;
            string tableName = tableListBox.SelectedItem.ToString();

            getColumnsInTable(tableName);
        }

        #endregion

        /// <summary>
        /// Clear all lists of old data
        /// </summary>
        private void clearLists()
        {
            tableContentList.Columns.Clear();
            tableContentList.Items.Clear();
            tableColumnList.Items.Clear();
        }

        #region Containers For The SQLite Database

        /** A container class for table specific elements */
        class Table
        {
            private string name;
            private List<Column> m_TableColumn = new List<Column>();

            public Table(String tableName)
            {
                name = tableName;
            }

            public void addNewColumn(Column column)
            {
                if (!m_TableColumn.Contains(column))
                    m_TableColumn.Add(column);
            }

            public List<Column> getColumns()
            {
                return m_TableColumn;
            }

            public String getTableName()
            {
                return name;
            }

            public String getSelectColumns()
            {
                String columns = "";


                foreach (Column column in m_TableColumn)
                {
                    columns += column.name + " ";
                }
                if (columns.Length > 2)
                    return columns.Substring(0, columns.Length - 2);
                return "*";
            }

            public void removeColumn(Column c)
            {
                m_TableColumn.Remove(c);
            }

            public string getSelectColumnsForSelectStatement()
            {
                string columns = "";


                foreach (Column column in m_TableColumn)
                {
                    columns += column.name + ",";
                }

                if (columns.Length > 0)
                    return columns.Substring(0, columns.Length - 1);
                return "*";
            }

        }

        /** A container for column specific elements */
        public struct Column
        {
            public string name;

            public Column(string cName)
            {
                name = cName;
            }
        }

        private void queryExecuteButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Sorry, not yet implemented!");
        }


        private void tableColumnList_ItemCheck(object sender, EventArgs e)
        {
            populateTableView(databaseTableList.SelectedItem.ToString());
        }

        private void helpButton_Click(object sender, EventArgs e)
        {
            DialogResult dr = new DialogResult();
            ManualQueryHelpForm mqhf = new ManualQueryHelpForm();
            dr = mqhf.ShowDialog();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public override Form Initialize(System.IO.FileInfo file)
        {
            return Initialize(file.FullName);
        }

        public override Form Initialize(string path)
        {
            databaseTableExtraction(path);

            return this;
        }
    }
        #endregion
}