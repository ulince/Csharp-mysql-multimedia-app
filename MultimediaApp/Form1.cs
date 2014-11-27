using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace MultimediaApp
{
    public partial class Form1 : Form
    {
        string filePath = null, strFileName;
        int FileSize;
        DBConnection conn = new DBConnection();
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'multimediaDataSet.file' table. You can move, or remove it, as needed.
            this.fileTableAdapter.Fill(this.multimediaDataSet.file);

        }

        //Browse dialog
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = @"C:\BLOBS2";
            openFileDialog1.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.Multiselect = false;

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePath = @openFileDialog1.FileName;
                strFileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
                //MessageBox.Show(filePath);
            }
        }

        //Save image toDB
        private void button2_Click(object sender, EventArgs e)
        {
            if (filePath != null)
            {
                string SQL;
                MySqlCommand cmd = new MySqlCommand();
                FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                FileSize = (int)fs.Length;
                BinaryReader reader = new BinaryReader(fs);
                byte[] BlobValue = reader.ReadBytes((int)fs.Length);

                fs.Close();
                reader.Close();

                if (conn.OpenConnection() == true)
                {
                    try
                    {
                        SQL = "INSERT INTO file VALUES(NULL, @FileName, @FileSize, @File)";
                        cmd.Connection = conn.connection;
                        cmd.CommandText = SQL;
                        cmd.Parameters.AddWithValue("@FileName", strFileName);
                        cmd.Parameters.AddWithValue("@FileSize", FileSize);
                        cmd.Parameters.AddWithValue("@File", BlobValue);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("File Inserted into database successfully!",
                            "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                        this.fileTableAdapter.Fill(this.multimediaDataSet.file);

                        conn.CloseConnection();
                    }
                    catch (MySql.Data.MySqlClient.MySqlException ex)
                    {
                        MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        //Retrieve image from DB
        private void button3_Click(object sender, EventArgs e)
        {
            MySqlCommand cmd = new MySqlCommand();
            MySqlDataReader reader;
            string SQL = "SELECT file FROM file where file_name=@FileName";
            FileStream fs;
            BinaryWriter writer;
            int bufferSize = 1024;
            byte[] BlobValue = new byte[bufferSize];
            long bytesReturned, startIndex = 0;

            DataGridViewCell cell = dataGridView1.SelectedCells[0];
            
            if (cell.Value != null)
            {
                strFileName = cell.Value.ToString();
             
                if (conn.OpenConnection() == true)
                {
                    try
                    {
                        cmd.Connection = conn.connection;
                        cmd.CommandText = SQL;
                        cmd.Parameters.AddWithValue("@FileName", strFileName);

                        reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);

                        if (!Directory.Exists(@"C:\BLOBS"))
                        {
                            Directory.CreateDirectory(@"C:\BLOBS");
                        }

                        while (reader.Read())
                        {
                            fs = new FileStream(@"C:\BLOBS\" + strFileName , FileMode.OpenOrCreate, FileAccess.Write);
                            writer = new BinaryWriter(fs);
                            startIndex = 0;
                            bytesReturned = reader.GetBytes(0,startIndex,BlobValue,0,bufferSize);

                            while(bytesReturned == bufferSize)
                            {
                                writer.Write(BlobValue);
                                writer.Flush();
                                startIndex += bufferSize;
                                bytesReturned = reader.GetBytes(0, startIndex, BlobValue,0,bufferSize);
                            }

                            writer.Write(BlobValue, 0, (int)bytesReturned - 1);
                            writer.Close();
                            fs.Close();

                        }

                        reader.Close();

                        MessageBox.Show("File retreived from database successfully!",
                            "Success!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                        conn.CloseConnection();
                    }
                    catch (MySql.Data.MySqlClient.MySqlException ex)
                    {
                        MessageBox.Show("Error " + ex.Number + " has occurred: " + ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
