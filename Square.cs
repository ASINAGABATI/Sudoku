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
        public void updateCell(int index, int row, int col, int num, int cond)
        {
            string key = String.Format("C{0}R{1}", col, row);
            Square squre = new(col, row, num, cond);
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
            return playsIO[idx].today;
        }
    }

}
