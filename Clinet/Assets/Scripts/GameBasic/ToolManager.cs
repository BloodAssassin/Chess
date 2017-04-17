using UnityEngine;
using System.Collections;
using System;

public class ToolManager : MonoBehaviour {

    /// <summary>
    /// 工具类：将坐标x的值转换为所在的列数
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static int xToCol(float x)
    {
        int col = (int)(x / 0.8f);
        col = col + 4;
        return col;       
    }

    /// <summary>
    /// 工具类：将坐标y的值转换为所在的行数
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public static int yToRow(float y)
    {
        int row;
        if (y > 0)
        {
            row=(int)(y/0.78f);
            row = 4 - row;
        }
        else
        {
            row = (int)(y / 0.78f);
            row = 5 - row;
        }
            return row;
    }

    /// <summary>
    /// 工具类：将所在的列数转换为对应的x坐标
    /// </summary>
    /// <param name="col"></param>
    /// <returns></returns>
    public static float colToX(int col)
    {
        float x;
        col = col - 4;
        x = col * 0.8f;
        return x;
    }

    /// <summary>
    /// 工具类：将所在的行数转换为对应的y坐标(因浮点数计算存在不精确问题，故先乘100，计算后再除100)
    /// </summary>
    /// <param name="row"></param>
    /// <returns></returns>
    public static float rowToY(int row)
    {
        float y;
        if (row < 5)
        {
            row = 4 - row;
            y =(float) (row * 78 + 38);
            y = y / 100;
        }
        else
        {
            row = 5 - row;
            y = (float)(row * 78 - 38);
            y = y / 100;
        }
        return y;
    }


    /// <summary>
    /// 计算选中的棋子的位置和要移动的位置之间的位置关系
    /// </summary>
    /// <param name="row1"></param>
    /// <param name="col1"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public static int Relation(int row1, int col1, int row, int col)
    {
        return Mathf.Abs(row1 - row) * 10 + Mathf.Abs(col1 - col);
    }

    /// <summary>
    /// 工具类：通过行列数来判断该位置上是否有棋子，若有则返回棋子的ID，若没有则返回-1
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public static int GetStoneId(int row, int col)
    {
        for (int i = 0; i < 32; ++i)
        {
            if (row == StoneManager.s[i]._row && col == StoneManager.s[i]._col && StoneManager.s[i]._dead == false)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// 工具类：判断要移动的棋子是否在棋盘的下方
    /// </summary>
    /// <param name="selectedId"></param>
    /// <returns></returns>
    public static bool IsBottomSide(int selectedId)
    {
        if (StoneManager.s[selectedId]._initRow > 5)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 工具类：判断两个位置所连成的一条直线上有多少个棋子
    /// </summary>
    /// <param name="row1"></param>
    /// <param name="col1"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public static int GetStoneCountAtLine(int row1, int col1, int row2, int col2)
    {
        int ret = 0;
        if (row1 != row2 && col1 != col2) return -1;
        if (row1 == row2 && col1 == col2) return -1;

        if (row1 == row2)
        {
            int min = col1 < col2 ? col1 : col2;
            int max = col1 < col2 ? col2 : col1;
            for (int col = min + 1; col < max; ++col)
            {
                if (GetStoneId(row1, col) != -1) ++ret;
            }
        }
        else
        {
            int min = row1 < row2 ? row1 : row2;
            int max = row1 < row2 ? row2 : row1;
            for (int row = min + 1; row < max; ++row)
            {
                if (GetStoneId(row, col1) != -1) ++ret;
            }
        }
        return ret;
    }


    /// <summary>
    /// 延时n秒执行某些行为
    /// </summary>
    /// <param name="action"></param>
    /// <param name="delaySeconds"></param>
    /// <returns></returns>
    public static IEnumerator DelayToInvokeDo(Action action, float delaySeconds)
    {

        yield return new WaitForSeconds(delaySeconds);

        action();
    }
}
