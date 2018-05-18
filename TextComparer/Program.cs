using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SolarixGrammarEngineNET;
using System.IO;
using System.Text;

namespace TextComparer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());            
        }        
    }

    class Grammar
    {
        bool allowDynForms;
        int jumpCount;
        IntPtr hEngine;
        public Grammar(bool allowDynForms, int jumpCount)
        {
            this.allowDynForms = allowDynForms;
            this.jumpCount = jumpCount;
            hEngine = GrammarEngine.sol_CreateGrammarEngineW("dictionary.xml");
        }
        public bool CheckSyn(string wX, string wY)
        {
            IntPtr hCoordX = GrammarEngine.sol_ProjectWord(hEngine, wX, allowDynForms);
            int pCountX = GrammarEngine.sol_CountProjections(hCoordX);
            IntPtr hCoordY = GrammarEngine.sol_ProjectWord(hEngine, wY, allowDynForms);
            int pCountY = GrammarEngine.sol_CountProjections(hCoordY);
            for (int i = 0; i < pCountX; i++)
            {
                int eIndexX = GrammarEngine.sol_GetIEntry(hCoordX, i);
                int eClassX = GrammarEngine.sol_GetEntryClass(hEngine, eIndexX);
                for (int j = 0; j < pCountY; j++)
                {
                    int eIndexY = GrammarEngine.sol_GetIEntry(hCoordY, j);
                    int eClassY = GrammarEngine.sol_GetEntryClass(hEngine, eIndexY);
                    if (eClassX != eClassY)
                        break;
                    IntPtr hLink = GrammarEngine.sol_SeekThesaurus(hEngine, eIndexY,
                        true, false, false, true, jumpCount);
                    if (hLink.ToInt32() != 0)
                    {
                        int linkCount = GrammarEngine.sol_CountInts(hLink);
                        if (linkCount > 0)
                        {
                            for (int k = 0; k < linkCount; k++)
                                if (GrammarEngine.sol_GetInt(hLink, k) == eIndexX)
                                    return true;
                            GrammarEngine.sol_DeleteInts(hLink);
                        }
                    }
                }
            }
            return false;
        }
    }
    static class Compare
    {
        static public string[][] MakeShingles(string text, int n)
        {
            string[] arrSep = new string[] {
                " в ", " на ", " под ", " из-за ", " вследствие ", " благодаря ", " через ", " из ", " из-под ",
                " для ", " в течение ", " в продолжение ", " несмотря на ", " до ", " после ",
                " ", ".", ",", "?", ":", " - ", "\t", "\"", "\n", "\r", "«", "»"
            };
            string[] arrWord = text.Split(arrSep, StringSplitOptions.RemoveEmptyEntries);
            int shCount = arrWord.Length - n + 1;
            string[][] arrSh = new string[shCount][];
            for (int i = 0; i < shCount; i++)
            {
                arrSh[i] = new string[n];
                for (int j = 0; j < n; j++)
                    arrSh[i][j] = arrWord[i + j];
            }
            return arrSh;
        }
        static public double CompareShingles(string[] sh1, string[] sh2, Grammar g)
        {
            double res = 0;
            for (int i = 0; i < sh1.Length; i++)
            {
                int j;
                for (j = 0; j < sh2.Length; j++)
                    if (sh1[i] == sh2[j] ||
                        g.CheckSyn(sh1[i], sh2[j]))
                        break;
                if (j < sh2.Length)
                    res++;
            }
            return res / sh1.Length;
        }
        static public double CompareTexts(string text1, string text2, int n, int m,
            Grammar g, ToolStripProgressBar pb,
            out string[][] arrSh1, out string[][] arrSh2, out string s)
        {
            s = "Соответствие шинглов:<br>";
            arrSh1 = MakeShingles(text1, n);
            arrSh2 = MakeShingles(text2, n);
            if (arrSh1.Length > arrSh2.Length)
            {
                string[][] arrShTmp = arrSh2;
                arrSh2 = arrSh1;
                arrSh1 = arrShTmp;
            }
            if (pb != null)
                pb.Maximum = arrSh1.Length;
            double res = 0;
            for (int i = 0; i < arrSh1.Length; i++)
            {
                int k = i * arrSh2.Length / arrSh1.Length - m / 2;
                if (k < 0)
                    k = 0;
                double max = 0;
                int jMax = 0;
                for (int j = 0; j < m && k + j < arrSh2.Length; j++)
                {
                    double x = CompareShingles(arrSh1[i], arrSh2[k + j], g);
                    if (x >= max)
                    {
                        max = x;
                        jMax = j;
                    }
                }
                res += max;
                s += ShingleToString(arrSh1[i]) + " <-> " +
                    ShingleToString(arrSh2[k + jMax]) +
                    string.Format(" {0}<br>", Math.Round(max, 3));
                if (pb != null)
                    pb.Value = i;
            }
            return res / arrSh1.Length;
        }
        static public string[] ReadTexts(string file)
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.Default);
            string s = sr.ReadToEnd();
            sr.Close();
            string[] arrText = s.Split(new string[] { "<text>" },
                StringSplitOptions.RemoveEmptyEntries);
            return arrText;
        }
        static public double Teach(string file1, string file2, int n, int m, Grammar g, int intervCount,
            out double[] arrX, out double[] arrN)
        {
            string[] arrText1 = ReadTexts(file1), arrText2 = ReadTexts(file2);
            double[] arrC = new double[arrText1.Length];
            double cMin = 1, cMax = 0;
            for (int i = 0; i < arrText1.Length; i++)
            {
                string[][] arrSh1, arrSh2;
                string s;
                arrC[i] = CompareTexts(arrText1[i], arrText2[i], n, m, g, null,
                    out arrSh1, out arrSh2, out s);
                if (arrC[i] > cMax)
                    cMax = arrC[i];
                if (arrC[i] < cMin)
                    cMin = arrC[i];
            }
            double h = (cMax - cMin) / intervCount;
            arrX = new double[intervCount];
            arrN = new double[intervCount];
            for (int i = 0; i < intervCount; i++)
			{
                arrX[i] = cMin + h / 2 + h * i;
                arrN[i] = 0;			 
			}
            double sum = 0;
            for (int i = 0; i < arrC.Length; i++)
            {
                int j;
                for (j = 0; j < intervCount - 1; j++)
                    if (arrC[i] < cMin + h * (j + 1))
                        break;
                arrN[j]++;
                sum += arrC[i];
            }
            return sum / arrC.Length;
        }
        static public string ShingleToString(string[] sh)
        {
            string res = "{ ";
            foreach (string s in sh)
                res += s + " ";
            return res + "}";
        }
    }
}