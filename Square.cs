using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Security.Policy;

namespace Sudoku
{
    static class Constants
    {
        public const string CONF_FILENAME = "sudoku.json";

        public static readonly string[,] CELLNAME = {
                            { "C9_1", "C8_1", "C7_1", "C6_1", "C5_1", "C4_1", "C3_1", "C2_1", "C1_1" },
                            { "C9_2", "C8_2", "C7_2", "C6_2", "C5_2", "C4_2", "C3_2", "C2_2", "C1_2" },
                            { "C9_3", "C8_3", "C7_3", "C6_3", "C5_3", "C4_3", "C3_3", "C2_3", "C1_3" },

                            { "C9_4", "C8_4", "C7_4", "C6_4", "C5_4", "C4_4", "C3_4", "C2_4", "C1_4" },
                            { "C9_5", "C8_5", "C7_5", "C6_5", "C5_5", "C4_5", "C3_5", "C2_5", "C1_5" },
                            { "C9_6", "C8_6", "C7_6", "C6_6", "C5_6", "C4_6", "C3_6", "C2_6", "C1_6" },

                            { "C9_7", "C8_7", "C7_7", "C6_7", "C5_7", "C4_7", "C3_7", "C2_7", "C1_7" },
                            { "C9_8", "C8_8", "C7_8", "C6_8", "C5_8", "C4_8", "C3_8", "C2_8", "C1_8" },
                            { "C9_9", "C8_9", "C7_9", "C6_9", "C5_9", "C4_9", "C3_9", "C2_9", "C1_9" }
            };
    }

    internal class OnePeace
    {
        public int col { get; set; }    // 左から 1,2,3,4,5,6,7,8,9
        public int row { get; set; }    // 上から 1,2,3,4,5,6,7,8,9
        public int num { get; set; }    // 0:KURO(Question), 1:AKA(Answer)

        public OnePeace()
        {
            col = 0;
            row = 0;
            num = 0;
        }
    }
    internal class OneNumPeace
    {
        public string today { get; set; }
        public List<OnePeace> onePeace { get; set; }

        public OneNumPeace()
        {
            today = DateTime.Now.ToString();
            onePeace = new List<OnePeace> { };
        }
    }

    public class Square
    {
        public int col { get; set; }    // 左から 1,2,3,4,5,6,7,8,9
        public int row { get; set; }    // 上から 1,2,3,4,5,6,7,8,9
        public int num { get; set; }    // 1,2,3,4,5,6,7,8,9
        public int cond { get; set; }   // 0:KURO(Question), 1:AKA(Answer)

        public Square()
        {
            col = 0;
            row = 0;
            num = 0;
            cond = 0;
        }
        public Square(int a, int b, int c, int d)
        {
            col = a;
            row = b;
            num = c;
            cond = d;
        }
    }
    public class Play
    {
        public string? today { get; set; }
        public List<Square> squares { get; set; }

        public Play()
        {
            today = "";
            squares = new List<Square>();
        }
    }
    public class NumPlace
    {
        public int number { get; set; }
        public List<Play> plays { get; set; }
        private string confPath = System.IO.Path.Combine(Environment.CurrentDirectory, Constants.CONF_FILENAME);

        public NumPlace()
        {
            number = 0;
            plays = new List<Play>();
        }

        public Play Load(int index = -1)
        {
            Play r = new Play();
            try
            {
                string jsonString = File.ReadAllText(confPath);
                if (jsonString != null)
                {
                    var rdata = JsonSerializer.Deserialize<NumPlace>(jsonString);
                    if (rdata != null)
                    {
                        number = rdata.number;
                        plays = rdata.plays;

                        if (index < 0)
                        {
                            r = plays[plays.Count - 1];
                        }
                        else
                        {
                            if (index < plays.Count)
                            {
                                r = plays[index];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            return r;
        }
        public bool SaveSudoku2(NumPlace np)
        {
            bool r = true;
            try
            {
                var json_str = JsonSerializer.Serialize(np, new JsonSerializerOptions { WriteIndented = true });

                string confPath = System.IO.Path.Combine(Environment.CurrentDirectory, Constants.CONF_FILENAME);
                File.WriteAllText(confPath, json_str);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                r = false;
            }
            return r;
        }
        public List<string> listGame()
        {
            List<string> lst = new();
            foreach(var p in plays)
            {
                lst.Add(p.today);
            }
            return lst;
        }
    }

}
