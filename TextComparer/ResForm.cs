using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TextComparer
{
    public partial class ResForm : Form
    {
        public ResForm(string s)
        {
            InitializeComponent();
            wb.DocumentText = s;
        }
    }
}