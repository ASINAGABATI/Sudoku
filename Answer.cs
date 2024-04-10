using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        private readonly int LOOK_I = 0;
        private readonly int LOOK_J = 6;
        private readonly int LOOK_N = 6;
        
        private static readonly int[,] CHK6x6 = new int[,] { { 3, 4, 5, 6, 7, 8 }, { 0, 1, 2, 6, 7, 8 }, { 0, 1, 2, 3, 4, 5 } };

        private readonly Ban9x9[,] ban = new Ban9x9[9, 9];

        public Answer(int[,] q)
        {
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
        private void constructCandidate()
        {
            List<int> candiFromAnswer(int i, int j)
            {
                int[] nine = [1, 1, 1, 1, 1, 1, 1, 1, 1];
                int bi = i / 3 * 3;
                int bj = j / 3 * 3;

                // roi3x3マスで 問題/決定セルを除外する
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

                // roi3x3マス外の 問題/決定セルを除外
                int[] i_arr6 = [1, 2, 3, 4, 5, 6];
                int[] j_arr6 = [ 0,0,0,0,0,0];
                for (int iijj = 0; iijj < 6; iijj++)
                {
                    i_arr6[iijj] = CHK6x6[i / 3, iijj];
                    j_arr6[iijj] = CHK6x6[j / 3, iijj];
                }
                foreach (int jj in j_arr6)
                {
                    var idx = ban[i, jj].nn - 1;
                    if (idx >= 0)
                    {
                        nine[idx] = 0;
                    }
                }
                foreach (int ii in i_arr6)
                {
                    var idx = ban[ii, j].nn - 1;
                    if (idx >= 0)
                    {
                        nine[idx] = 0;
                    }
                }

                List<int> arr = [];
                for (var ix = 0; ix < 9; ix++)
                {
                    if (nine[ix] == 1)
                    {
                        arr.Add(1 + ix);
                    }
                }
                return arr;
            } // candiFromAnswer

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    ban[i, j].candidate = [];
                    if (ban[i,j].nn == 0)
                    {
                        ban[i,j].candidate = candiFromAnswer(i,j);
                    }
                }
            }


        }

        public int Prn(bool print = true)
        {
            int nzero = 0;
            for (int i = 0; i < 9; i++)
            {
                string s = "";
                for (int j = 0; j < 9; j++)
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
        private Solution solutionCell(int i, int j)
        {
            int[] array6(int x)
            {
                int x0 = x / 3;
                int[] i_array = new int[6];
                for (int x1 = 0; x1 < 6; x1++)
                {
                    i_array[x1] = CHK6x6[x0, x1];
                }
                return i_array;
            }
            int[] array2(int x)
            {
                int x0 = x / 3 * 3;
                List<int> i_array = [x0, x0 + 1, x0 + 2];
                i_array.RemoveAt(x % 3);

                return [.. i_array];
            }

            List<int> i_3 = [];
            List<int> j_3 = [];
            int ix = i / 3 * 3;
            int jx = j / 3 * 3;
            for (int idx = 0; idx < 3; idx++)
            {
                i_3.Add(ix + idx);
                j_3.Add(jx + idx);
            }

            bool oneCandidate(int n)
            {
                int nc = 0;
                for (int idx = 0; idx < 3; idx++)
                {
                    for (int jdx = 0; jdx < 3; jdx++)
                    {
                        int ii = i_3[idx];
                        int jj = j_3[jdx];
                        if (ban[ii,jj].candidate.Contains(n))
                        {
                            nc++;
                        }
                    }
                }

                return nc == 1;
            }
            int[,] pick3x3(int i0, int j0, int n)
            {
                int[,] w3x3 = new int[3, 3];
                int ii = (i0 / 3) * 3;
                int jj = (j0 / 3) * 3;
                for (int i1 = 0; i1 < 3; i1++)
                {
                    for (int j1 = 0; j1 < 3; j1++)
                    {
                        w3x3[i1, j1] = ban[ii + i1, jj + j1].nn == 0 ? 1 : 0; // 0:決定していない 1:決定している
                    }
                }

                int[] iarr2 = array2(i0);
                int[] jarr6 = array6(j0);
                for (int i1 = 0; i1 < 2; i1++)
                {
                    for (int j1 = 0; j1 < 6; j1++)
                    {
                        if (ban[iarr2[i1], jarr6[j1]].nn == n)
                        {
                            w3x3[iarr2[i1] % 3, 0] = 0; // 水平方向 roi3x3外 6列に内に n がある. 
                            w3x3[iarr2[i1] % 3, 1] = 0;
                            w3x3[iarr2[i1] % 3, 2] = 0;
                        }
                    }
                }
                int[] iarr6 = array6(i0);
                int[] jarr2 = array2(j0);
                for (int i1 = 0; i1 < 6; i1++)
                {
                    for (int j1 = 0; j1 < 2; j1++)
                    {
                        if (ban[iarr6[i1], jarr2[j1]].nn == n)
                        {
                            w3x3[0, jarr2[j1] % 3] = 0; // 垂直方向 roi3x3外 6行に内に n がある. 
                            w3x3[1, jarr2[j1] % 3] = 0;
                            w3x3[2, jarr2[j1] % 3] = 0;
                        }
                    }
                }

                return w3x3;
            }

            int[] array3(int x)
            {
                int x0 = x / 3 * 3;
                int[] i_array = [x0, x0 + 1, x0 + 2];
                return i_array;
            }
            bool directionCandidate(int n)
            {
                int[,] p3x3 = pick3x3(i, j, n);
                // h-direction
                int[] iarr2 = array2(i);
                int[] iarr3 = array3(i);
                int[] jarr6 = array6(j);
                int[,] h_dir = { { 0, 0 }, { 0, 0 }, { 0, 0 } };
                for (int ii = 0; ii < 3; ii++)
                {
                    for (int jj = 0; jj < 6; jj++)
                    {
                        if (ban[iarr3[ii], jarr6[jj]].candidate.Contains(n))
                        {
                            h_dir[ii, jj / 3] = 1;
                        }
                    }
                }
                for (int jj = 0; jj < 2; jj++)
                {
                    if (h_dir[0, jj] + h_dir[1, jj] + h_dir[2, jj] > 1)
                    {
                        h_dir[0, jj] = h_dir[1, jj] = h_dir[2, jj] = 0;
                    }
                }
                for (int ii = 0; ii < 3; ii++)
                {
                    if (h_dir[ii, 0] + h_dir[ii, 1] > 0)
                    {
                        if (iarr2.Contains(iarr3[ii]))
                        {
                            p3x3[ii, 0] = 0; p3x3[ii, 1] = 0; p3x3[ii, 2] = 0;
                        }
                    }
                }

                // v-direction
                int[] jarr2 = array2(j);
                int[] jarr3 = array3(j);
                int[] iarr6 = array6(i);
                int[,] v_dir = { { 0, 0, 0 }, { 0, 0, 0 } };
                for (int jj = 0; jj < 3; jj++)
                {
                    for (int ii = 0; ii < 6; ii++)
                    {
                        if (ban[iarr6[ii], jarr3[jj]].candidate.Contains(n))
                        {
                            v_dir[ii / 3, jj] = 1;
                        }
                    }
                }
                for (int ii = 0; ii < 2; ii++)
                {
                    if (v_dir[ii, 0] + v_dir[ii, 1] + v_dir[ii, 2] > 1)
                    {
                        v_dir[ii, 0] = v_dir[ii, 1] = v_dir[ii, 2] = 0;
                    }
                }
                for (int jj = 0; jj < 3; jj++)
                {
                    if (v_dir[0, jj] + v_dir[1, jj] > 0)
                    {
                        if (jarr2.Contains(jarr3[jj]))
                        {
                            p3x3[0, jj] = 0; p3x3[1, jj] = p3x3[2, jj] = 0;
                        }
                    }
                }

                int s = 0;
                for (int ii = 0; ii < 3; ii++)
                {
                    for(int jj = 0; jj < 3; jj++)
                    {
                        s += p3x3[ii, jj];
                    }
                }
                return s == 1;　// 候補が１つなら n で決定
            }

            bool directionCandidate2(int n)
            {
                int[,] p3x3 = pick3x3(i, j, n);

                // h-direction
                int[] iarr3 = array3(i);
                int[] jarr6 = array6(j);
                int[,] h_dir = { { 0, 0 }, { 0, 0 }, { 0, 0 } };
                for (int ii = 0; ii < 3; ii++)
                {
                    for (int jj = 0; jj < 6; jj++)
                    {
                        if (ban[iarr3[ii], jarr6[jj]].candidate.Contains(n))
                        {
                            h_dir[ii, jj / 3] = 1;
                        }
                    }
                }
                int[] iii = [0, 0, 0];
                iii[i % 3] = 1;
                int h_x = 0;
                for(int ii = 0; ii < 3; ii++)
                {
                    if (iii[ii] == 1)
                    {
                        if (h_dir[ii, 0] == 0 && h_dir[ii, 1] == 0)
                        {
                            h_x++;
                        }
                    }
                    else
                    {
                        if (h_dir[ii, 0] == 1 || h_dir[ii, 1] == 1)
                        {
                            h_x++;
                        }
                    }
                }
                if (h_x == 3)
                {
                    for(int ii = 0; ii < 3; ii++)
                    {
                        if (iii[ii] == 0)
                        {
                            p3x3[ii, 0] = p3x3[ii, 1] = p3x3[ii, 2] = 0;
                        }
                    }
                }

                // v-direction
                int[] jarr3 = array3(j);
                int[] iarr6 = array6(i);
                int[,] v_dir = { { 0, 0, 0 }, { 0, 0, 0 } };
                for (int ii = 0; ii < 6; ii++)
                {
                    for (int jj = 0; jj < 3; jj++)
                        {
                        if (ban[iarr6[ii], jarr3[jj]].candidate.Contains(n))
                        {
                            v_dir[ii / 3, jj] = 1;
                        }
                    }
                }
                int[] jjj = [0, 0, 0];
                jjj[j % 3] = 1;
                int v_x = 0;
                for (int jj = 0; jj < 3; jj++)
                {
                    if (jjj[jj] == 1)
                    {
                        if (v_dir[0, jj] == 0 && v_dir[1, jj] == 0)
                        {
                            v_x++;
                        }
                    }
                    else
                    {
                        if (v_dir[0, jj] == 1 || v_dir[1, jj] == 1)
                        {
                            v_x++;
                        }
                    }
                }
                if (v_x == 3)
                {
                    for (int jj = 0; jj < 3; jj++)
                    {
                        if (jjj[jj] == 0)
                        {
                            p3x3[0, jj] = p3x3[1, jj] = p3x3[2, jj] = 0;
                        }
                    }
                }

                int s = 0;
                for (int ii = 0; ii < 3; ii++)
                {
                    for (int jj = 0; jj < 3; jj++)
                    {
                        s += p3x3[ii, jj];
                    }
                }
                return s == 1;　// 候補が１つなら n で決定
            }

            Solution solution = new();
            foreach(int candi in ban[i, j].candidate)
            {
                // 3x3マス内で candi は roiセルだけ...candiで決定
                if (oneCandidate(candi))
                {
                    solution.i = i;
                    solution.j = j;
                    solution.n = candi;
                    break;
                }

                // 3x3マス外 6マスにh方向,v方向性があれば 3x3のh,v方向を除外できる
                if (directionCandidate(candi))
                {
                    solution.i = i;
                    solution.j = j;
                    solution.n = candi;
                    break;
                }

                // 
                if (directionCandidate2(candi))
                {
                    solution.i = i;
                    solution.j = j;
                    solution.n = candi;
                    break;
                }
            }

            return solution;
        } // solutionCell

        public int Test()
        {
            int cnt = 0;
            int curr_i;
            int curr_j;
            void startAddr()
            {
                curr_i = -1;
                curr_j = 8;
            }
            startAddr();

            for(; ; )
            {
                if ( !nextSpaceCell(ref curr_i, ref curr_j) )
                {
                    break;
                }
                //if (curr_i == LOOK_I && curr_j == LOOK_J)
                //{
                //    Console.WriteLine("   (" + curr_i + "," + curr_j + ")");
                //}
                Solution solution = solutionCell(curr_i, curr_j);
                if (solution.n > 0)
                {
                    Console.WriteLine(++cnt + " i:" + curr_i + " j:" + curr_j + " n:" + solution.n);
                    ban[curr_i, curr_j].nn = solution.n;
                    ban[curr_i, curr_j].condition = 2;

                    constructCandidate();
                    startAddr();
                }
            }

            return Prn(false);
        }
        public Ban9x9[,] getResult()
        {
            return ban;
        }
    }
}
