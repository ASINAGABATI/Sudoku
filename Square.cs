using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace Sudoku
{
    static class Constants
    {
        public const string CONF_FILENAME = "sudoku.json";

        public static readonly string[,] CELLNAME = {
              { "C9R1","C8R1","C7R1",  "C6R1","C5R1","C4R1",  "C3R1","C2R1","C1R1" },
              { "C9R2","C8R2","C7R2",  "C6R2","C5R2","C4R2",  "C3R2","C2R2","C1R2" },
              { "C9R3","C8R3","C7R3",  "C6R3","C5R3","C4R3",  "C3R3","C2R3","C1R3" },

              { "C9R4","C8R4","C7R4",  "C6R4","C5R4","C4R4",  "C3R4","C2R4","C1R4" },
              { "C9R5","C8R5","C7R5",  "C6R5","C5R5","C4R5",  "C3R5","C2R5","C1R5" },
              { "C9R6","C8R6","C7R6",  "C6R6","C5R6","C4R6",  "C3R6","C2R6","C1R6" },

              { "C9R7","C8R7","C7R7",  "C6R7","C5R7","C4R7",  "C3R7","C2R7","C1R7" },
              { "C9R8","C8R8","C7R8",  "C6R8","C5R8","C4R8",  "C3R8","C2R8","C1R8" },
              { "C9R9","C8R9","C7R9",  "C6R9","C5R9","C4R9",  "C3R9","C2R9","C1R9" }
        };
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
    public class PlayIO
    {
        public string today;
        public Dictionary<string, Square> squares;

        public PlayIO()
        {
            today = "";
            squares= new Dictionary<string, Square>();
        }

    }
    public class NumPlace
    {
        private int save;
        public int number { get; set; }
        public List<Play> plays { get; set; }
        
        private string confPath = System.IO.Path.Combine(Environment.CurrentDirectory, Constants.CONF_FILENAME);
        private List<PlayIO> playsIO;

        public NumPlace()
        {
            number = 0;
            plays = new List<Play>();

            save = 0;
            playsIO = new List<PlayIO>();
        }
        public void final()
        {
            if (save != 0)
            {
                try
                {
                    plays = new List<Play>();
                    foreach (var playIO in playsIO)
                    {
                        Play play = new();
                        play.today = playIO.today;
                        foreach(var square in playIO.squares)
                        {
                            Square square2 = new Square();
                            square2.col = square.Value.col;
                            square2.row = square.Value.row;
                            square2.num = square.Value.num;
                            square2.cond = square.Value.cond;
                            play.squares.Add(square2);
                        }
                        plays.Add(play);
                    }

                    var json_str = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

                    string confPath = System.IO.Path.Combine(Environment.CurrentDirectory, Constants.CONF_FILENAME);
                    File.WriteAllText(confPath, json_str);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        public void first()
        {
            load();
        }
        private void load()
        {
            try
            {
                if (File.Exists(confPath))
                {
                    string jsonString = File.ReadAllText(confPath);
                    if (jsonString != null)
                    {
                        var rdata = JsonSerializer.Deserialize<NumPlace>(jsonString);
                        if (rdata != null)
                        {
                            number = rdata.number;
                            plays = rdata.plays;
                        }
                        if (plays.Count > 0)
                        {
                            foreach (var play in plays)
                            {
                                PlayIO pi = new PlayIO();
                                pi.today = play.today;
                                foreach (var squre in play.squares)
                                {
                                    string key = String.Format("C{0}R{1}", squre.col, squre.row);
                                    pi.squares[key] = squre;
                                }
                                playsIO.Add(pi);
                            }
                        }
                    }
                }
                else
                {
                    PlayIO pi = new PlayIO();
                    pi.today = DateTime.Now.ToString();
                    pi.squares = new Dictionary<string, Square>();
                    playsIO.Add(pi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, confPath);
            }
        }

        public Play loadPlay(int index)
        {
            Play play = new Play();
            if (plays.Count > 0)
            {
                int idx = index < 0 ? playsIO.Count - 1 : index;

                play.today = playsIO[idx].today;
                foreach(var pio in playsIO[idx].squares)
                {
                    play.squares.Add(pio.Value);
                }
            }
            return play;
        }
        public void removePlay(int index)
        {
            playsIO.RemoveAt(index);

            save = 2;
        }
        public void updatePlay(int idx, Play play)
        {
            save = 1;
            if (idx < 0)
            {
                PlayIO playIO = new PlayIO();
                playIO.today = play.today;
                foreach (var p in play.squares)
                {
                    string key = String.Format("C{0}R{1}", p.col, p.row);
                    playIO.squares[key] = p;
                }
                playsIO.Add(playIO);
            }
            else
            {
                playsIO[idx].today = play.today;
                playsIO[idx].squares = new Dictionary<string, Square>();
                foreach (var p in play.squares)
                {
                    string key = String.Format("C{0}R{1}", p.col, p.row);
                    playsIO[idx].squares[key] = p;
                }
            }
        }
        public void updateCell(int index, string key, int num, int cond)
        {
            string[] name = key.Substring(1).Split("R");
            Square squre = new (Int32.Parse(name[0]), Int32.Parse(name[1]), num, cond);
            playsIO[index].squares[key] = squre;

            save = 11;
        }
        public void modifyCaption(int index, string cap)
        {
            playsIO[index].today = cap;
            
            save = 11;
        }
        public List<string> listGame()
        {
            List<string> lst = new();
            foreach (var p in playsIO)
            {
                lst.Add(p.today);
            }

            return lst;
        }
        public string lookToday(int idx)
        {
            //return plays[idx].today;
            return playsIO[idx].today;
        }
    }

}
