using UnityEngine;
using System.Collections;

public class RuleManager : MonoBehaviour
{

    /// <summary>
    /// 将的走棋规则
    /// </summary>
    /// <returns></returns>
    public static bool moveJiang(int selectedId, int row, int col, int destoryId)
    {
        /*
         * 1.首先目标位置在九宫格内
         * 2.移动的步长是一个格子
         * 3.将和帅不准在同一直线上直接对面（中间无棋子），如一方已先占据位置，则另一方必须回避，否则就算输了
        */
        if (destoryId != -1 && StoneManager.s[destoryId]._type == StoneManager.Stone.TYPE.JIANG)
            return moveChe(selectedId, row, col, destoryId);

        if (col < 3 || col > 5) return false;
        if (ToolManager.IsBottomSide(selectedId))
        {
            if (row < 7) return false;
        }
        else
        {
            if (row > 2) return false;
        }

        int row1 = StoneManager.s[selectedId]._row;
        int col1 = StoneManager.s[selectedId]._col;
        int d = ToolManager.Relation(row1, col1, row, col);
        if (d != 1 && d != 10) return false;

        return true;
    }


    /// <summary>
    /// 士的走棋规则
    /// </summary>
    /// <returns></returns>
    public static bool moveShi(int selectedId, int row, int col, int destoryId)
    {
        /*
         * 1.目标位置在九宫格内
         * 2.只许沿着九宫中的斜线行走一步（方格的对角线）
        */
        if (ToolManager.IsBottomSide(selectedId))
        {
            if (row < 7) return false;
        }
        else
        {
            if (row > 2) return false;
        }
        if (col < 3 || col > 5) return false;

        int row1 = StoneManager.s[selectedId]._row;
        int col1 = StoneManager.s[selectedId]._col;
        int d = ToolManager.Relation(row1, col1, row, col);
        if (d != 11) return false;

        return true;
    }


    /// <summary>
    /// 相的走棋规则
    /// </summary>
    /// <returns></returns>
    public static bool moveXiang(int selectedId, int row, int col, int destoryId)
    {
        /*
         * 1.目标位置不能越过河界走入对方的领地
         * 2.只能斜走（两步），可以使用汉字中的田字形象地表述：田字格的对角线，即俗称象（相）走田字
         * 3.当象（相）行走的路线中，及田字中心有棋子时（无论己方或者是对方的棋子），则不允许走过去，俗称：塞象（相）眼。
        */
        int row1 = StoneManager.s[selectedId]._row;
        int col1 = StoneManager.s[selectedId]._col;
        int d = ToolManager.Relation(row1, col1, row, col);
        if (d != 22) return false;

        int rEye = (row + row1) / 2;
        int cEye = (col + col1) / 2;

        if (ToolManager.GetStoneId(rEye, cEye) != -1) return false;

        if (ToolManager.IsBottomSide(selectedId))
        {
            if (row < 4) return false;
        }
        else
        {
            if (row > 5) return false;
        }

        return true;
    }


    /// <summary>
    /// 车的走棋规则
    /// </summary>
    /// <returns></returns>
    public static bool moveChe(int selectedId, int row, int col, int destoryId)
    {
        /*
         * 1.每行一步棋可以上、下直线行走（进、退）；左、右横走
         * 2.中间不能隔棋子
         * 3.行棋步数不限
         */
        int row1 = StoneManager.s[selectedId]._row;
        int col1 = StoneManager.s[selectedId]._col;
        int ret = ToolManager.GetStoneCountAtLine(row1, col1, row, col);
        if (ret == 0) return true;
        return false;
    }


    /// <summary>
    /// 马的走棋规则
    /// </summary>
    /// <returns></returns>
    public static bool moveMa(int selectedId, int row, int col, int destoryId)
    {
        /*
         * 1.马走日字（斜对角线）
         * 2.可以将马走日分解为：先一步直走（或一横）再一步斜走
         * 3.如果在要去的方向，第一步直行处（或者横行）有别的棋子挡住，则不许走过去（俗称：蹩马腿）
         */
        int row1 = StoneManager.s[selectedId]._row;
        int col1 = StoneManager.s[selectedId]._col;
        int d = ToolManager.Relation(row1, col1, row, col);
        if (d != 12 && d != 21) return false;

        if (d == 12)
        {
            if (ToolManager.GetStoneId(row1, (col + col1) / 2) != -1)
                return false;
        }
        else
        {
            if (ToolManager.GetStoneId((row + row1) / 2, col1) != -1)
                return false;
        }

        return true;
    }


    /// <summary>
    /// 炮的走棋规则
    /// </summary>
    /// <returns></returns>
    public static bool movePao(int selectedId, int row, int col, int destoryId)
    {
        /*
         * 1.此棋的行棋规则和车（車）类似，横平、竖直，只要前方没有棋子的地方都能行走
         * 2.但是，它的吃棋规则很特别，必须跳过一个棋子（无论是己方的还是对方的）去吃掉对方的一个棋子。俗称：隔山打炮
         */
        int row1 = StoneManager.s[selectedId]._row;
        int col1 = StoneManager.s[selectedId]._col;
        int ret = ToolManager.GetStoneCountAtLine(row1, col1, row, col);
        if (destoryId != -1)
        {
            if (ret == 1)
                return true;
        }
        else
        {
            if (ret == 0)
                return true;
        }
        return false;
    }


    /// <summary>
    /// 兵的走棋规则
    /// </summary>
    /// <returns></returns>
    public static bool moveBing(int selectedId, int row, int col, int destoryId)
    {
        /*
         * 1.在没有过河界前，此棋每走一步棋只许向前直走一步（不能后退）
         * 2.过了河界之后，每行一步棋可以向前直走，或者横走（左、右）一步，但也是不能后退的
         */
        int row1 = StoneManager.s[selectedId]._row;
        int col1 = StoneManager.s[selectedId]._col;
        int d = ToolManager.Relation(row1, col1, row, col);
        if (d != 1 && d != 10) return false;

        if (ToolManager.IsBottomSide(selectedId))
        {
            if (row > row1) return false;
            if (row1 >= 5 && row == row1) return false;
        }
        else
        {
            if (row < row1) return false;
            if (row1 <= 4 && row == row1) return false;
        }
        return true;
    }

}