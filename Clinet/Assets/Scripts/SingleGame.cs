using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class SingleGame : GameManager {

    //enum TYPE { JIANG, CHE, PAO, MA, BING, SHI, XIANG };
    public static int[] chessScore = { 15000, 1000, 501, 499, 200, 100, 100 };

    public int _level = 3;

    public override void Click(int id, int row, int col)
    {
        if (base._beRedTurn)
        {
            base.Click(id, row, col);
            if (!base._beRedTurn && StoneManager.s[20]._dead == false)
            {
                StartCoroutine(ToolManager.DelayToInvokeDo(() => { ComputerMove(); }, 0.5f));
            }    
        }
    }

    /// <summary>
    /// 电脑移动棋子，利用最大最小值算法
    /// </summary>
    void ComputerMove()
    {
        step step = GetBestMove();
        base.MoveStone(step.moveId, step.killId,step.rowTo, step.colTo);   
    }

    /// <summary>
    /// 获取最好的移动方式
    /// </summary>
    /// <returns></returns>
    step GetBestMove()
    {
        step ret;

        //1.看看有哪些步骤可以走
        List<step> steps = new List<step>();
        GetAllPossibleMove(ref steps);

        //2.试着走一下,评估走的结果
        int maxInAllMinScore = -300000;
        ret = steps[steps.Count - 1];
        while(steps.Count!=0)
        {
            step tmpStep = steps[steps.Count-1];
            steps.RemoveAt(steps.Count - 1);

            FakeMove(tmpStep);
            int minScore = GetMinScore(_level - 1, maxInAllMinScore);
            UnFakeMove(tmpStep);

            if (minScore > maxInAllMinScore)
            {
                maxInAllMinScore = minScore;
                ret = tmpStep;
            }
        }
        steps.Clear();
        steps.TrimExcess();

        //3.取最好的结果作为参考
        return ret;
    }

    /// <summary>
    /// 获取最小分
    /// </summary>
    /// <param name="level"></param>
    /// <param name="curMin"></param>
    /// <returns></returns>
    int GetMinScore(int level,int curMin)
    {
        if (level == 0) return Score();

        //1.看看有哪些步骤可以走(红旗的)
        List<step> steps = new List<step>();
        GetAllPossibleMove(ref steps);

        int minInAllMaxScore = 300000;
        while (steps.Count != 0)
        {
            step tmpStep = steps[steps.Count - 1];
            steps.RemoveAt(steps.Count - 1);

            FakeMove(tmpStep);
            int maxScore = GetMaxScore(level - 1, minInAllMaxScore);
            UnFakeMove(tmpStep);

            if (maxScore <= curMin)
            {
                while (steps.Count != 0)
                {
                    steps.RemoveAt(steps.Count - 1);
                }
                steps.Clear();
                steps.TrimExcess();
                return maxScore;
            }
                

            if (maxScore < minInAllMaxScore)
            {
                minInAllMaxScore = maxScore;
            }
        }
        steps.Clear();
        steps.TrimExcess();

        return minInAllMaxScore;
    }

    /// <summary>
    /// 获取最大分
    /// </summary>
    /// <param name="level"></param>
    /// <param name="curMax"></param>
    /// <returns></returns>
    int GetMaxScore(int level,int curMax)
    {
        if (level == 0) return Score();

        List<step> steps = new List<step>();
        GetAllPossibleMove(ref steps);

        int maxInAllMinScore = -300000;
        while (steps.Count != 0)
        {
            step tmpStep = steps[steps.Count - 1];
            steps.RemoveAt(steps.Count - 1);

            FakeMove(tmpStep);
            int minScore = GetMinScore(level - 1, maxInAllMinScore);
            UnFakeMove(tmpStep);

            if (minScore >= curMax)
            {
                while (steps.Count != 0)
                {
                    steps.RemoveAt(steps.Count - 1);
                }
                steps.Clear();
                steps.TrimExcess();
                return minScore;
            }

            if (minScore > maxInAllMinScore)
            {
                maxInAllMinScore = minScore;
            }
        }
        steps.Clear();
        steps.TrimExcess();

        return maxInAllMinScore;
    }

    /// <summary>
    /// 做一次假的移动
    /// </summary>
    /// <param name="step"></param>
    void FakeMove(step step)
    {
        KillStone(step.killId);
        FakeMoveStone(step.moveId, step.rowTo, step.colTo);
    }

    /// <summary>
    /// 做一次假的返回移动
    /// </summary>
    /// <param name="step"></param>
    void UnFakeMove(step step)
    {
        ReliveStone(step.killId);
        FakeMoveStone(step.moveId,step.rowFrom, step.colFrom);
    }

    /// <summary>
    /// 模拟移动棋子
    /// </summary>
    /// <param name="moveId"></param>
    /// <param name="point"></param>
    void FakeMoveStone(int moveId, int row,int col)
    {
        StoneManager.s[moveId]._row = row;
        StoneManager.s[moveId]._col = col;

        _beRedTurn = !_beRedTurn;
    }

    /// <summary>
    /// 评价局面分
    /// </summary>
    /// <returns></returns>
    int Score()
    {
        //黑棋分的总数-红棋分的总数
        int scoreRed = 0;
        int scoreBlack = 0;

        for (int i = 0; i < 16; ++i)
        {
            if (StoneManager.s[i]._dead) continue;
            scoreRed += chessScore[Convert.ToInt32(StoneManager.s[i]._type)];          
        }
        for (int i = 16; i < 32; ++i)
        {
            if (StoneManager.s[i]._dead) continue;
            scoreBlack  += chessScore[Convert.ToInt32(StoneManager.s[i]._type)];
        }

        return scoreBlack - scoreRed;
    }

    /// <summary>
    /// 获取所有可行的移动方式
    /// </summary>
    /// <param name="steps"></param>
    void GetAllPossibleMove(ref List<step> steps)
    {
        int min, max;
        if (base._beRedTurn)
        {
            min = 0;
            max = 16;
        }
        else
        {
            min = 16;
            max = 32;
        }
        for (int i = min; i < max; ++i)
        {
            if (StoneManager.s[i]._dead) continue;

            for (int row = 0; row <= 9; ++row)
            {
                for (int col = 0; col <= 8; ++col)
                {
                    int killId = ToolManager.GetStoneId(row, col);
                    if (SameColor(killId, i)) continue;

                    if (CanMove(i, killId, row, col))
                    {
                        SaveStep(i, killId, row, col, ref steps);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 重新开始
    /// </summary>
    public override void Restart()
    {
        SceneManager.LoadScene("SingleGame");
    }

    /// <summary>
    /// 悔棋
    /// </summary>
    public override void Back()
    {
        BackOne();
        BackOne();
    }
}
