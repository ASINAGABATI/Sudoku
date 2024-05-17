using System;
using System.Collections.Generic;
using System.Linq;

namespace Sudoku
{
    internal class Answer
    {
        public class Ban9x9
        {
            public int condition { get; set; } // 0:空白, 1:問題セル, 2:解答セル
            public int nn { get; set; }
            public List<int> candidate { get; set; }

            public Ban9x9() 
            {
                condition = 0;
                nn = 0;
                candidate = [];
            }
            public override string ToString()
            {
                string s = "";
                if (nn > 0)
                {
                    s += " (" + nn.ToString();
                }
                else
                {
                    s += " candidate:";
                    foreach(var c in candidate)
                    {
                        s += c.ToString() + " ";
                    }
                }
                foreach (var n in candidate)
                {
                    s += " " + nn.ToString();
                }
                return s;
            }
        }
        private struct Solution
        {
            public int i { get; set; }
            public int j { get; set; }
            public int n { get; set; }
            public Solution()
            {
                i = -1;
                j = -1;
                n = 0;
            }
            public override string ToString()
            {
                return "Solution:" + i + "-" + j + " " + n;
            }
        }

        private int mode;
        private readonly int LOOK_I = 1;
        private readonly int LOOK_J = 1;
        private readonly int LOOK_N = 6;
        
        private static readonly int[,] CHK6x6 = new int[,] { { 3, 4, 5, 6, 7, 8 }, { 0, 1, 2, 6, 7, 8 }, { 0, 1, 2, 3, 4, 5 } };

        private readonly Ban9x9[,] ban = new Ban9x9[9, 9];

        private class Cross
        {
            public int idx { get; set; }
            public int candi { get; set; }
            public Cross(int a, int b)
            {
                idx = a;
                candi = b;
            }
        }

        public Answer(int mode_, int[,] q)
        {
            mode = mode_;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    ban[i, j] = new()
                    {
                        condition = q[i, j] == 0 ? 0 : 1,
                        nn = q[i, j],
                        candidate = []
                    };
                }
            }
            constructCandidate();
        }
        private bool constructCandidate()
        {
            List<int> candidateFromAnser(int i, int j)  // i:行(0,1,..8), j:列(0,1,..8)
            {
                int[] nine = [1, 1, 1, 1, 1, 1, 1, 1, 1];  // 候補1～9すべて
                int bi = i / 3 * 3;
                int bj = j / 3 * 3;
                // (1) roi3x3マスで 問題/解答セル値 を除外する
                for (int ii = bi; ii < bi + 3; ii++)
                {
                    for (int jj = bj; jj < bj + 3; jj++)
                    {
                        if (ban[ii,jj].condition > 0)
                        {
                            nine[ban[ii, jj].nn - 1] = 0;
                        }
                    }
                }

                // (2) roi3x3マス外の垂直/水平位置の 問題/解答セル値 を除外する
                int[] roi_v6 = [1, 2, 3, 4, 5, 6];
                int[] roi_h6 = [ 0,0,0,0,0,0];
                for (int iijj = 0; iijj < 6; iijj++)
                {
                    roi_v6[iijj] = CHK6x6[i / 3, iijj];
                    roi_h6[iijj] = CHK6x6[j / 3, iijj];
                }
                foreach (int jj in roi_h6)
                {
                    var idx = ban[i, jj].nn;  // 水平位置を見る
                    if (idx > 0)
                    {
                        nine[idx - 1] = 0;
                    }
                }
                foreach (int ii in roi_v6)
                {
                    var idx = ban[ii, j].nn;  // 垂直位置を見る
                    if (idx > 0)
                    {
                        nine[idx - 1] = 0;
                    }
                }

                List<int> arr = []; // 候補
                foreach(var item in nine.Select((value, index) => new { value, index } ))
                {
                    if (item.value == 1)
                    {
                        arr.Add(1 + item.index);
                    }
                }
                return arr;// 候補
            } // candidateFromAnser

            // ban[,].candidate を更新
            (List<Cross>vertical, List<Cross>horizontal) makeXlist(int m, int n) // 
            {
                List<Cross>vd = new ();
                List<Cross>hd = new ();
                int[] vin3 = { m * 3, m * 3 + 1, m * 3 + 2 };
                int[] hin3 = { n * 3, n * 3 + 1, n * 3 + 2 };

                List<int> lstN = new();
                foreach (int i in vin3)
                {
                    foreach (int j in hin3)
                    {
                        if (ban[i, j].candidate.Count > 1)
                        {
                            foreach (int cn in ban[i, j].candidate)
                            {
                                if (!lstN.Contains(cn))
                                {
                                    lstN.Add(cn);
                                }
                            }
                        }
                    }
                }

                int[] vout6 = { CHK6x6[m, 0], CHK6x6[m, 1], CHK6x6[m, 2], CHK6x6[m, 3], CHK6x6[m, 4], CHK6x6[m, 5] };
                foreach (int candi in lstN)
                {
                    int[,] step1 = { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } };
                    for (int i = 0; i < step1.GetLength(0); i++)
                    {
                        foreach (int j in hin3)
                        {
                            if (ban[vout6[i], j].candidate.Contains(candi))
                            {
                                step1[i, j % 3] = 1;
                            }
                        }
                    }
                    for (int i1 = 0; i1 < 2; i1++)
                    {
                        int[] step2 = { 0, 0, 0 };
                        for (int j1 = 0; j1 < 3; j1++)
                        {
                            for (int i2 = 0; i2 < 3; i2++)
                            {
                                if (step1[i1 * 3 + i2, j1] != 0)
                                {
                                    step2[j1] = 1;
                                }
                            }
                        }
                        if (step2.Sum() == 1)
                        {
                            int i = 0;
                            for (; i < 3; i++)
                            {
                                if (step2[i] == 1)
                                {
                                    break;
                                }
                            }
                            vd.Add(new Cross(i, candi));
                        }
                    }
                }

                int[] hout6 = { CHK6x6[n, 0], CHK6x6[n, 1], CHK6x6[n, 2], CHK6x6[n, 3], CHK6x6[n, 4], CHK6x6[n, 5] };
                foreach (int candi in lstN)
                {
                    int[,] step1 = { { 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0 } };
                    foreach (int i in vin3)
                    {
                        for (int j = 0; j < step1.GetLength(1); j++)
                        {
                            if (ban[i, hout6[j]].candidate.Contains(candi))
                            {
                                step1[i % 3, j] = 1;
                            }
                        }
                    }
                    for(int j1 = 0; j1 < 2; j1++)
                    {
                        int[] step2 = { 0, 0, 0 };
                        for (int i1 = 0; i1 < 3; i1++)
                        {
                            for(int j2 = 0; j2 < 3; j2++)
                            {
                                if (step1[i1, j1 * 3 + j2] != 0)
                                {
                                    step2[i1] = 1;
                                }
                            }
                        }
                        if (step2.Sum() == 1)
                        {
                            int j = 0;
                            for(; j < 3; j++)
                            {
                                if (step2[j] == 1)
                                {
                                    break;
                                }
                            }
                            hd.Add(new Cross(j, candi));
                        }
                    }
                }

                return (vd, hd);
            } // makeXlist

            bool updateCandidate3x3(bool isHorizontal, int m, int n, Cross crs)
            {
                bool brk = false;
                List<int> hlist = new();
                List<int> vlist = new();
                if (isHorizontal)
                {
                    vlist.Add(m * 3 + crs.idx);
                    hlist.Add(n * 3);
                    hlist.Add(n * 3 + 1);
                    hlist.Add(n * 3 + 2);
                }
                else
                {
                    hlist.Add(n * 3 + crs.idx);
                    vlist.Add(m * 3);
                    vlist.Add(m * 3 + 1);
                    vlist.Add(m * 3 + 2);
                }
                foreach(var i in vlist)
                {
                    foreach(var j in hlist)
                    {
                        if (ban[i, j].candidate.Contains(crs.candi))
                        {
                            var idx = ban[i, j].candidate.IndexOf(crs.candi);
                            ban[i, j].candidate.RemoveAt(idx);
                            brk = true;
                        }
                    }
                }
                return brk;
            } // updateCandidate3x3

            bool removeDandidate2pair(int m, int n)
            {
                var rm_pair = new Dictionary<int, List<int> >();
                foreach (int i in new int[] { m * 3 + 0, m * 3 + 1, m * 3 + 2 })
                {
                    foreach(int j in new int[] { n * 3 + 0, n * 3 + 1, n * 3 + 2 })
                    {
                        if (ban[i,j].candidate.Count == 2)
                        {
                            int pair_k = ban[i, j].candidate[0] * 10 + ban[i, j].candidate[1];
                            int itm = i * 10 + j;
                            if (rm_pair.ContainsKey(pair_k))
                            {
                                rm_pair[pair_k].Add(itm);
                            }
                            else
                            {
                                rm_pair.Add(pair_k, new List<int> { itm} );
                            }
                        }
                    }
                }

                bool brk = false;
                foreach (KeyValuePair<int, List<int>> kvp in rm_pair)
                {
                    if (kvp.Value.Count == 2)
                    {
                        int na = kvp.Key / 10;
                        int nb =kvp.Key % 10;
                        int i0 = kvp.Value[0] / 10;
                        int j0 = kvp.Value[1] % 10;

                        foreach (int i in new int[] { m * 3 + 0, m * 3 + 1, m * 3 + 2 })
                        {
                            foreach (int j in new int[] { n * 3 + 0, n * 3 + 1, n * 3 + 2 })
                            {
                                if (i0 != i || j0 != j)
                                {
                                    List<int> n_lst = new ();
                                    foreach(var nc in ban[i, j].candidate)
                                    {
                                        if (nc != na && nc != nb)
                                        {
                                            n_lst.Add(nc);
                                        }
                                    }
                                    if (n_lst.Count > 0)
                                    {
                                        ban[i, j].candidate = n_lst;
                                        brk = true;
                                    }
                                }
                            }
                        }
                    }
                }
                return brk;
            } // removeDandidate2pair
            void removeCandidateOne(int m, int n)
            {
                int[] nine = [0, 0, 0, 0, 0, 0, 0, 0, 0];
                int[] coord = [-1, -1, -1, -1, -1, -1, -1, -1, -1]; 
                foreach (int i in new int[] { m * 3 + 0, m * 3 + 1, m * 3 + 2 })
                {
                    foreach (int j in new int[] { n * 3 + 0, n * 3 + 1, n * 3 + 2 })
                    {
                        foreach (var nc in ban[i, j].candidate)
                        {
                            nine[nc - 1]++;
                            coord[nc - 1] = i * 10 + j;
                        }
                    }
                }
                for(int i = 0; i < nine.Length; i++)
                {
                    if (nine[i] == 1)
                    {
                        int i0 = coord[i] / 10;
                        int j0 = coord[i] % 10;
                        ban[i0, j0].candidate = new List<int> { 1 + i };
                    }
                }
            }

            for (int i = 0; i < ban.GetLength(0); i++)
            {
                for (int j = 0; j < ban.GetLength(1); j++)
                {
                    ban[i, j].candidate = [];
                    if (ban[i, j].nn == 0)
                    {
                        ban[i, j].candidate = candidateFromAnser(i, j); // 3x3セル内で候補を拾う
                    }
                }
            }

            bool brk = false;
            foreach (int m in new int[] { 0, 1, 2} )
            {
                foreach (int n in new int[] { 0, 1, 2 } )
                {
                    // 3x3セルないでの作業
                    var lst = makeXlist(m, n);
                    foreach (var vertical in lst.vertical)
                    {
                        brk = updateCandidate3x3(false, m, n, vertical); // 垂直方向 2列 に 他3x3セル に垂直性候補 があれば候補から除外する
                    }
                    foreach (var horizontal in lst.horizontal)
                    {
                        brk = updateCandidate3x3(true, m, n, horizontal); // 水平方向 2行 に 他3x3セル に水平性候補 があれば候補から除外する
                    }

                    brk = removeDandidate2pair(m, n); // 2つだけの候補が2セルあれば 他の候補列から その2つの候補を除外する 

                    removeCandidateOne(m, n);
                }
            }
            return brk;
        } // constructCandidate

        public int Prn(bool print = true)
        {
            int nzero = 0;
            for (int i = 0; i < ban.GetLength(0); i++)
            {
                string s = "";
                for (int j = 0; j < ban.GetLength(1); j++)
                {
                    var n = ban[i, j].nn;
                    if (n == 0)
                    {
                        nzero++;
                    }
                    s += n + " ";
                }
                if (print)
                {
                    Console.WriteLine(s);
                }
            }
            return nzero;
        }

        private bool nextSpaceCell(ref int i, ref int j)
        {
            j++;
            if (j == 9)
            {
                i++;
                if (i == 9)
                {
                    return false;
                }
                j = 0;
            }
            for (; i < 9; i++)
            {
                for (; j < 9; j++)
                {
                    if (ban[i, j].nn == 0)
                    {
                        return true; // 空き
                    }
                }
                j = 0;
            }
            if (i == 9)
            {
                return false; // すべて解答すみ
            }
            return false;
        }

        public void Test()
        {
            for (; ;)
            {
                int upd = 0;

                for ( int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (ban[i, j].candidate.Count() == 1)
                        {
                            ban[i, j].nn = ban[i, j].candidate[0];
                            ban[i, j].condition = 2;
                            ban[i, j].candidate = [];
                            upd++;
                        }
                    }
                }
                if (upd == 0)
                {
                    break; // ギブアップ or すべて解答
                }
                constructCandidate();
            }
        }

        public int TestStepByStep()
        {
            int upd = 0;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (ban[i, j].candidate.Count() == 1)
                    {
                        ban[i, j].nn = ban[i, j].candidate[0];
                        ban[i, j].condition = 2;
                        ban[i, j].candidate = [];
                        upd++;
                    }
                }
            }
            if (upd > 0)
            {
                constructCandidate();
            }
            return upd;  // 0:next 1:end
        }

        public Ban9x9[,] getResult()
        {
            return ban;
        }
    }
}
