using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class StoneManager : MonoBehaviour
{

    //初始化14种棋子的预制体
    public GameObject che_black;
    public GameObject ma_black;
    public GameObject xiang_black;
    public GameObject shi_black;
    public GameObject jiang_black;
    public GameObject pao_black;
    public GameObject bing_black;

    public GameObject che_red;
    public GameObject ma_red;
    public GameObject xiang_red;
    public GameObject shi_red;
    public GameObject jiang_red;
    public GameObject pao_red;
    public GameObject bing_red;

    //初始化32个棋子
    public static Stone[] s = new Stone[32];

    public struct Stone
    {
        public enum TYPE { JIANG, CHE, PAO, MA, BING, SHI, XIANG };

        //棋子是红子还是黑子,ID小于16的是红子，大于16是黑子
        public bool _red;

        //棋子是否死亡
        public bool _dead;

        //棋子在棋盘中的位置,
        public int _row;
        public int _col;

        //棋子在棋盘中的初始位置
        public int _initRow;
        public int _initCol;

        //棋子的类型
        public TYPE _type;

        //棋子初始化，赋予32个棋子对应的属性参数
        public void init(int id)
        {
            _red = id < 16;
            _dead = false;

            //每个点上的棋子的类型
            StonePos[] pos = {
                                 new StonePos(0,0,Stone.TYPE.CHE),
                                 new StonePos(0,1,Stone.TYPE.MA),
                                 new StonePos(0,2,Stone.TYPE.XIANG),
                                 new StonePos(0,3,Stone.TYPE.SHI),
                                 new StonePos(0,4,Stone.TYPE.JIANG),
                                 new StonePos(0,5,Stone.TYPE.SHI),
                                 new StonePos(0,6,Stone.TYPE.XIANG),
                                 new StonePos(0,7,Stone.TYPE.MA),
                                 new StonePos(0,8,Stone.TYPE.CHE),

                                 new StonePos(2,1,Stone.TYPE.PAO),
                                 new StonePos(2,7,Stone.TYPE.PAO),

                                 new StonePos(3,0,Stone.TYPE.BING),
                                 new StonePos(3,2,Stone.TYPE.BING),
                                 new StonePos(3,4,Stone.TYPE.BING),
                                 new StonePos(3,6,Stone.TYPE.BING),
                                 new StonePos(3,8,Stone.TYPE.BING),
                             };
            if (id < 16)
            {
                _row = pos[id].row;
                _col = pos[id].col;
                _initRow = _row;
                _initCol = _col;
                _type = pos[id].type;
            }
            else
            {
                _row = 9 - pos[id - 16].row;
                _col = 8 - pos[id - 16].col;
                _initRow = 9 - pos[id - 16].row;
                _initCol = 8 - pos[id - 16].col;
                _type = pos[id - 16].type;
            }
        }


        public void rotate()
        {
            _row = 9 - _row;
            _col = 8 - _col;
            _initRow = 9 - _initRow;
            _initCol = 8 - _initCol;
        }
    }


    /// <summary>
    /// 通过棋子的坐标，获得对应的类型，将三个值关联在一起
    /// </summary>
    public struct StonePos
    {
        public int row, col;
        public Stone.TYPE type;
        public StonePos(int _row, int _col, Stone.TYPE _type)
        {
            row = _row;
            col = _col;
            type = _type;
        }
    }


    /// <summary>
    /// 通过棋子的ID和类型赋予棋子对应的预制体
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public GameObject GetPrefab(bool isRed, StoneManager.Stone.TYPE type)
    {
        if (isRed)
        {
            switch (type)
            {
                case StoneManager.Stone.TYPE.CHE:
                    return che_red;
                case StoneManager.Stone.TYPE.MA:
                    return ma_red;
                case StoneManager.Stone.TYPE.XIANG:
                    return xiang_red;
                case StoneManager.Stone.TYPE.SHI:
                    return shi_red;
                case StoneManager.Stone.TYPE.JIANG:
                    return jiang_red;
                case StoneManager.Stone.TYPE.PAO:
                    return pao_red;
                case StoneManager.Stone.TYPE.BING:
                    return bing_red;
            }
        }
        else
        {
            switch (type)
            {
                case StoneManager.Stone.TYPE.CHE:
                    return che_black;
                case StoneManager.Stone.TYPE.MA:
                    return ma_black;
                case StoneManager.Stone.TYPE.XIANG:
                    return xiang_black;
                case StoneManager.Stone.TYPE.SHI:
                    return shi_black;
                case StoneManager.Stone.TYPE.JIANG:
                    return jiang_black;
                case StoneManager.Stone.TYPE.PAO:
                    return pao_black;
                case StoneManager.Stone.TYPE.BING:
                    return bing_black;
            }
        }

        return bing_black;
    }


    /// <summary>
    /// 初始并实例化棋子
    /// </summary>
    public void StoneInit(bool beRedSide)
    {
        for (int i = 0; i < 32; ++i)
        {
            s[i].init(i);
        }
        if (beRedSide)
        {
            for (int i = 0; i < 32; ++i)
            {
                s[i].rotate();
            }
        }
        //实例化32个棋子
        for (int i = 0; i < 32; ++i)
        {
            GameObject fabs = GetPrefab(s[i]._red, s[i]._type);
            GameObject Stone = Instantiate(fabs, transform.localPosition, Quaternion.identity) as GameObject;
            Stone.name = i.ToString();
            Stone.transform.parent = transform;
            Stone.transform.position = new Vector3(ToolManager.colToX(s[i]._col), ToolManager.rowToY(s[i]._row), 0);
            Stone.AddComponent<BoxCollider>();
        }
    }
}