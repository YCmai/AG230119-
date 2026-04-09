using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskSim
{
    public partial class Form1 : Form
    {

        Dictionary<string, BookFatory> bookFatories = new Dictionary<string, BookFatory>();

        public Form1()
        {
            InitializeComponent();

            bookFatories.Add("OP001", new BookFatory() { Id = "OP001" });

            bookFatories.Add("OP002", new BookFatory() { Id = "OP002" });

            bookFatories.Add("OP003", new BookFatory() { Id = "OP003" });

            bookFatories.Add("OP004", new BookFatory() { Id = "OP004" });
        }
        #region checkBox 点击一下，更改在线情况。

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                bookFatories["OP001"].IsOnline = true;
            }
            else
            {
                bookFatories["OP001"].IsOnline = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                bookFatories["OP002"].IsOnline = true;
            }
            else
            {
                bookFatories["OP002"].IsOnline = false;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                bookFatories["OP003"].IsOnline = true;
            }
            else
            {
                bookFatories["OP003"].IsOnline = false;
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                bookFatories["OP004"].IsOnline = true;
            }
            else
            {
                bookFatories["OP004"].IsOnline = false;
            }
        }

        #endregion


        #region 按键，点击一下，随机生成一本书
        Random random = new Random();
        private void bnt_AddBook1_Click(object sender, EventArgs e)
        {
            bookFatories["OP001"].AddBook(Guid.NewGuid().ToString(), "图书分类" + random.Next(1, 40));
        }

        private void bnt_AddBook2_Click(object sender, EventArgs e)
        {
            bookFatories["OP002"].AddBook(Guid.NewGuid().ToString(), "图书分类" + random.Next(1, 40));
        }

        private void bnt_AddBook3_Click(object sender, EventArgs e)
        {
            bookFatories["OP003"].AddBook(Guid.NewGuid().ToString(), "图书分类" + random.Next(1, 40));
        }

        private void bnt_AddBook4_Click(object sender, EventArgs e)
        {
            bookFatories["OP004"].AddBook(Guid.NewGuid().ToString(), "图书分类" + random.Next(1, 40));
        }

        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var bookf in bookFatories.Values)
            {
                stringBuilder.AppendLine(bookf.Id +":" + bookf.State.ToString()+":"+bookf.BookSortDecribtion);
            }
            textBox1.Text = stringBuilder.ToString();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {

            bookFatories["OP001"].AutoAddBook = checkBox5.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {

            bookFatories["OP002"].AutoAddBook = checkBox5.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {

            bookFatories["OP003"].AutoAddBook = checkBox5.Checked;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {

            bookFatories["OP004"].AutoAddBook = checkBox5.Checked;
        }
    }







}
