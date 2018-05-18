using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace TextComparer
{
    public partial class MainForm : Form
    {
        Grammar g;
        public MainForm()
        {
            InitializeComponent();
            string[] arrText1 = Compare.ReadTexts("1.txt");
            string[] arrText2 = Compare.ReadTexts("2.txt");
            tb1.Text = arrText1[0];
            tb2.Text = arrText2[0];
            g = new Grammar(true, 1);
        }
        void open1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.Default);
                string s = sr.ReadToEnd();
                sr.Close();
                tb1.Text = s;
            }
            catch
            {
                MessageBox.Show("Ошибка открытия");
            }
        }
        void open2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (ofd.ShowDialog() != DialogResult.OK)
                    return;
                FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.Default);
                string s = sr.ReadToEnd();
                sr.Close();
                tb2.Text = s;
            }
            catch
            {
                MessageBox.Show("Ошибка открытия");
            }
        }
        void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        void cmpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Refresh();
                string[][] arrSh1, arrSh2;
                string s;
                double x = Compare.CompareTexts(tb1.Text, tb2.Text, 3, 4, g, pb,
                    out arrSh1, out arrSh2, out s);
                pb.Value = 0;
                s = string.Format("Результат сравнения: {0}<br>{1}", Math.Round(x, 3), s);
                ResForm rf = new ResForm(s);
                rf.ShowDialog();
            }
            catch
            {
                MessageBox.Show("Ошибка сравнения");
            }
        }
    }
}