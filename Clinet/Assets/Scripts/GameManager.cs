using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DG.Tweening;

public class GameManager : MonoBehaviour
{

    #region 变量
    /// <summary>
    /// 被选中的棋子的ID，若没有被选中的棋子，则ID为-1
    /// </summary>
    public int _selectedId=-1;

    /// <summary>
    /// 是否轮到红子的回合
    /// </summary>
    public bool _beRedTurn=true;

    public bool _beSide;

    /// <summary>
    /// 保存每一步走棋
    /// </summary>
    public struct step
    {
        public int moveId;
        public int killId;

        public int colFrom;
        public int rowFrom;
        public int colTo;
        public int rowTo;

        public step(int _moveId, int _killId, int _rowFrom, int _colFrom, int _rowTo, int _colTo)
        {
            moveId = _moveId;
            killId = _killId;
            colFrom = _colFrom;
            rowFrom = _rowFrom;
            colTo = _colTo;
            rowTo = _rowTo;
        }
    }
    public List<step> _steps=new List<step>();
    #endregion

    #region 游戏物体
    /// <summary>
    /// 正常状态下的棋子的图片资源
    /// </summary>
    public Object[] normalChess;

    /// <summary>
    /// 被选中状态下的棋子的图片资源
    /// </summary>
    public Object[] seletcedChess;

    /// <summary>
    /// 黑棋选框的GameObject
    /// </summary>
    public GameObject BlackSelected;

    /// <summary>
    /// 黑棋路径的GameObject
    /// </summary>
    public GameObject BlackPath;

    /// <summary>
    /// 红棋选框的GameObject
    /// </summary>
    public GameObject RedSelected;

    /// <summary>
    /// 红棋路径的GameObject
    /// </summary>
    public GameObject RedPath;

    /// <summary>
    /// 胜利界面
    /// </summary>
    public GameObject WinPlane;

    /// <summary>
    /// 失败界面
    /// </summary>
    public GameObject LosePlane;

    /// <summary>
    /// 功能界面
    /// </summary>
    public GameObject FunctionPlane;

    #endregion

    #region 音乐文件
    /// <summary>
    /// 放置棋子的音效
    /// </summary>
    public AudioSource clickMusic;

    /// <summary>
    /// 胜利的音效
    /// </summary>
    public AudioSource winMusic;

    /// <summary>
    /// 失败的音效
    /// </summary>
    public AudioSource loseMusic;
    #endregion

    void Awake()
    {
        //加载资源
        normalChess = Resources.LoadAll("_newNormalChess");
        seletcedChess = Resources.LoadAll("_newSelectChess");
        GameObject.Find("ChessBoard").GetComponent<StoneManager>().StoneInit(true);
        _beSide = true;
    }
	
	void Update () 
    {
        MainProcess();
	}


    #region 游戏流程的相关函数，包括：游戏的主流程、判断游戏结果（胜利或失败）

    /// <summary>
    /// 象棋的主要流程
    /// </summary>
    public void MainProcess()
    {
        //当鼠标点击时
        if (Input.GetMouseButtonDown(0))
        {
            //摄像机到点击位置的射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            //若点击的位置在棋盘内
            if (Physics.Raycast(ray, out hit) && InsideChessbord(hit))
            {
                Click(hit);
            }
        }
        //当手指点击屏幕时
        else if((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && InsideChessbord(hit))
            {
                Click(hit);
            }
        }
    }

    /// <summary>
    /// 判断胜负
    /// </summary>
    void JudgeVictory()
    {
        //如果是执红棋
        if (_beSide)
        {
            //黑色的将死了，则获胜；红色的将死了，则失败
            if (StoneManager.s[20]._dead == true)
            {
                //获胜
                WinPlane.SetActive(true);
                PlayMusic_Win();
            }

            if (StoneManager.s[4]._dead == true)
            {
                //失败
                LosePlane.SetActive(true);
                PlayMusic_Lose();
            }
        }
        //如果是执黑棋，则与红棋相反
        else
        {
            if (StoneManager.s[4]._dead == true)
            {
                //获胜
                WinPlane.SetActive(true);
                PlayMusic_Win();
            }

            if (StoneManager.s[20]._dead == true)
            {
                //失败
                LosePlane.SetActive(true);
                PlayMusic_Lose();
            }
        }
    }

    #endregion


    #region 移动棋子相关动画特效函数，包括：改变棋子图片、显示/隐藏棋子移动路径、移动错误的动画特效

    /// <summary>
    /// 当鼠标选择棋子时，改变棋子的Sprite
    /// </summary>
    void ChangeSpriteToSelect(int moveId)
    {
        GameObject Stone = GameObject.Find(moveId.ToString());
        SpriteRenderer spr = Stone.GetComponent<SpriteRenderer>();
        int i=1;
        if (StoneManager.s[moveId]._red)
        {
            switch (StoneManager.s[moveId]._type)
            {
                case StoneManager.Stone.TYPE.JIANG: i = 8;
                    break;
                case StoneManager.Stone.TYPE.SHI: i = 9;
                    break;
                case StoneManager.Stone.TYPE.XIANG: i = 10;
                    break;
                case StoneManager.Stone.TYPE.MA: i = 11;
                    break;
                case StoneManager.Stone.TYPE.CHE: i = 12;
                    break;
                case StoneManager.Stone.TYPE.PAO: i = 13;
                    break;
                case StoneManager.Stone.TYPE.BING: i = 14;
                    break;
            }
        }
        else
        {
            switch (StoneManager.s[moveId]._type)
            {
                case StoneManager.Stone.TYPE.JIANG: i = 1;
                    break;
                case StoneManager.Stone.TYPE.SHI: i = 2;
                    break;
                case StoneManager.Stone.TYPE.XIANG: i = 3;
                    break;
                case StoneManager.Stone.TYPE.MA: i = 4;
                    break;
                case StoneManager.Stone.TYPE.CHE: i = 5;
                    break;
                case StoneManager.Stone.TYPE.PAO: i = 6;
                    break;
                case StoneManager.Stone.TYPE.BING: i = 7;
                    break;
            }
        }
        spr.sprite = seletcedChess[i] as Sprite;   
    }

    /// <summary>
    /// 当鼠标取消选择时，再次改变其Sprite
    /// </summary>
    void ChangeSpriteToNormal(int moveId)
    {
        GameObject Stone = GameObject.Find(moveId.ToString());
        SpriteRenderer spr = Stone.GetComponent<SpriteRenderer>();
        int i = 1;
        if (StoneManager.s[moveId]._red)
        {
            switch (StoneManager.s[moveId]._type)
            {
                case StoneManager.Stone.TYPE.JIANG: i = 8;
                    break;
                case StoneManager.Stone.TYPE.SHI: i = 9;
                    break;
                case StoneManager.Stone.TYPE.XIANG: i = 10;
                    break;
                case StoneManager.Stone.TYPE.MA: i = 11;
                    break;
                case StoneManager.Stone.TYPE.CHE: i = 12;
                    break;
                case StoneManager.Stone.TYPE.PAO: i = 13;
                    break;
                case StoneManager.Stone.TYPE.BING: i = 14;
                    break;
            }
        }
        else
        {
            switch (StoneManager.s[moveId]._type)
            {
                case StoneManager.Stone.TYPE.JIANG: i = 1;
                    break;
                case StoneManager.Stone.TYPE.SHI: i = 2;
                    break;
                case StoneManager.Stone.TYPE.XIANG: i = 3;
                    break;
                case StoneManager.Stone.TYPE.MA: i = 4;
                    break;
                case StoneManager.Stone.TYPE.CHE: i = 5;
                    break;
                case StoneManager.Stone.TYPE.PAO: i = 6;
                    break;
                case StoneManager.Stone.TYPE.BING: i = 7;
                    break;
            }
        }
        spr.sprite = normalChess[i] as Sprite;
    }

    /// <summary>
    /// 设置上一步棋子走过的路径，即将上一步行动的棋子的位置留下标识，并标识该棋子
    /// </summary>
    void ShowPath(Vector3 oldPosition, Vector3 newPosition,bool isRed)
    {
        if (isRed)
        {
            RedSelected.transform.position = new Vector3(newPosition.x + 0.05f, newPosition.y - 0.06f, 0);
            RedSelected.SetActive(true);

            RedPath.transform.position = oldPosition;
            RedPath.SetActive(true);
        }
        else
        {
            BlackSelected.transform.position = new Vector3(newPosition.x, newPosition.y - 0.04f, 0);
            BlackSelected.SetActive(true);

            BlackPath.transform.position = oldPosition;
            BlackPath.SetActive(true);
        }
       
    }

    /// <summary>
    /// 隐藏路径
    /// </summary>
    void HidePath()
    {
        RedSelected.SetActive(false);
        RedPath.SetActive(false);

        BlackSelected.SetActive(false);
        BlackPath.SetActive(false);                  
    }

    /// <summary>
    /// 播放移动错误的动画特效
    /// </summary>
    void MoveError(int moveId,int row,int col)
    {
        GameObject Stone = GameObject.Find(moveId.ToString());
        Vector3 oldPosition = new Vector3(ToolManager.colToX(StoneManager.s[moveId]._col),ToolManager.rowToY(StoneManager.s[moveId]._row),0);
        Vector3 newPosition = new Vector3(ToolManager.colToX(col), ToolManager.rowToY(row), 0);
        Vector3[] paths = new Vector3[3];
        paths[0] = oldPosition;
        paths[1] = newPosition;
        paths[2] = oldPosition;
        Stone.transform.DOPath(paths,0.8f); 
    }

    #endregion


    #region 播放音效的相关函数，包括：放置棋子音效、胜利音效、失败音效

    /// <summary>
    /// 播放放置棋子时的音效
    /// </summary>
    void PlayMusic_Move()
    {
        if (!clickMusic.isPlaying)
        {
            clickMusic.Play();
        }
    }

    /// <summary>
    /// 播放胜利时的音效
    /// </summary>
    void PlayMusic_Win()
    {
        if (!winMusic.isPlaying)
        {
            winMusic.Play();
        }
    }

    /// <summary>
    /// 播放失败时的音效
    /// </summary>
    void PlayMusic_Lose()
    {
        if (!loseMusic.isPlaying)
        {
            loseMusic.Play();
        }
    }

    #endregion 


    #region 工具帮助类函数

    bool IsRed(int id)
    {
        return StoneManager.s[id]._red;
    }

    bool IsDead(int id)
    {
        if (id == -1) return true;
        return StoneManager.s[id]._dead;
    }

    public bool SameColor(int id1, int id2)
    {
        if (id1 == -1 || id2 == -1) return false;

        return IsRed(id1) == IsRed(id2);
    }

    /// <summary>
    /// 设置棋子死亡
    /// </summary>
    /// <param name="id"></param>
    public void KillStone(int id)
    {
        if (id == -1) return;

        StoneManager.s[id]._dead = true;
        GameObject Stone = GameObject.Find(id.ToString());
        Stone.SetActive(false);     
    }

    /// <summary>
    /// 复活棋子
    /// </summary>
    /// <param name="id"></param>
    public void ReliveStone(int id)
    {
        if (id == -1) return;

        //因GameObject.Find();函数不能找到active==false的物体，故先找到其父物体，再找到其子物体才可以找到active==false的物体
        StoneManager.s[id]._dead = false;
        GameObject Background = GameObject.Find("ChessBoard");
        GameObject Stone = Background.transform.Find(id.ToString()).gameObject;
        Stone.SetActive(true);           
    }

    /// <summary>
    /// 判断点击的位置是否在棋盘内
    /// </summary>
    /// <param name="hit"></param>
    /// <returns></returns>
    bool InsideChessbord(RaycastHit hit)
    {
        if ((hit.point.x > -4 && hit.point.x < 4) && (hit.point.y > -4.3 && hit.point.y < 4.3) )
            return true;
        else
            return false;
    }

    /// <summary>
    /// 通过鼠标点击的位置，获取距离当前坐标点最近的中心点的位置
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    Vector3 Center(Vector3 point)
    {
        //x,y,z为要返回的三维坐标
        //将象棋分为9列（i）和10行（j）
        //计算距离鼠标所指坐标点的最近的行列的序号（tpmi、tmpj）
        //通过行列的序号算出位于该行该列的中心点的坐标位置并返回

        float x, y, z = 0;
        int i, tmpi = 1, j, tmpj = 1;
        float min = 80;

        for (i = 0; i < 9; ++i)
        {
            if (System.Math.Abs(point.x * 100 - ToolManager.colToX(i) * 100) < min)
            {
                min = System.Math.Abs(point.x * 100 - ToolManager.colToX(i) * 100);
                tmpi = i;
            }
        }
        x = ToolManager.colToX(tmpi);


        min = 51;
        for (j = 0; j < 10; ++j)
        {
            if (System.Math.Abs(point.y * 100 - ToolManager.rowToY(j) * 100) < min)
            {
                min = System.Math.Abs(point.y * 100 - ToolManager.rowToY(j) * 100);
                tmpj = j;
            }
        }
        y = ToolManager.rowToY(tmpj);

        return new Vector3(x, y, z);
    }

    #endregion


    #region 移动棋子

    /// <summary>
    /// 获取点击的中心位置,若该位置上有棋子，则获取该棋子ID，否则id为-1
    /// </summary>
    /// <param name="hit"></param>
    void Click(RaycastHit hit)
    {
        int col = ToolManager.xToCol(Center(hit.point).x);
        int row = ToolManager.yToRow(Center(hit.point).y);
        int id = ToolManager.GetStoneId(row,col);
        Click(id,row,col);
    }

    /// <summary>
    /// 若当前没有选中棋子，则尝试选中点击的棋子；若当前已有选中的棋子，则尝试移动棋子
    /// </summary>
    /// <param name="id"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public virtual void Click(int id, int row, int col)
    {
        if (_selectedId == -1)
        {
            TrySelectStone(id);
        }
        else
        {
            TryMoveStone(id, row, col);
        }
    }

    /// <summary>
    /// 尝试选择棋子；若id=-1或者不是处于移动回合的棋子，则返回；否则，将该棋子设为选中的棋子，并更新图片
    /// </summary>
    /// <param name="id"></param>
    void TrySelectStone(int id)
    {
        if (id == -1) return;

        if (!CanSelect(id)) return;

        _selectedId = id;

        ChangeSpriteToSelect(id);
    }

    /// <summary>
    /// 尝试移动棋子
    /// 若要移动的目标位置有棋子（kiillId）且和当前选中的棋子同色，则换选择
    /// 若可以移动，则移动；若不能移动，则播放移动错误的提示动画
    /// </summary>
    /// <param name="killId"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void TryMoveStone(int killId, int row, int col)
    {
        if (killId != -1 && SameColor(killId, _selectedId))
        {
            ChangeSpriteToNormal(_selectedId);
            TrySelectStone(killId);
            return;
        }

        bool ret = CanMove(_selectedId, killId, row, col);

        if (ret)
        {
            MoveStone(_selectedId, killId, row, col);
            _selectedId = -1;
        }
        else
        {
            MoveError(_selectedId, row,col);          
        }
    }

    /// <summary>
    /// 走棋并吃棋
    /// </summary>
    /// <param name="hit"></param>
    public void MoveStone(int moveId, int killId, int row,int col)
    {
        // 1.若移动到的位置上有棋子，将其吃掉
        // 2.隐藏上一步的路径
        // 3.将棋子移动到目标位置,将移动棋子的路径显示出来
        // 4.播放音效
        // 5.改变精灵的渲染图片
        // 6.判断是否符合胜利或者失败的条件

        SaveStep(moveId, killId, row, col, ref _steps);

        KillStone(killId);

        HidePath();

        ShowPath(new Vector3(ToolManager.colToX(StoneManager.s[moveId]._col), ToolManager.rowToY(StoneManager.s[moveId]._row), 0), new Vector3(ToolManager.colToX(col), ToolManager.rowToY(row), 0), StoneManager.s[moveId]._red); 
        MoveStone(moveId, row, col);
        

        PlayMusic_Move();

        ChangeSpriteToNormal(moveId);

        JudgeVictory();
    }

    /// <summary>
    /// 移动棋子到目标位置
    /// </summary>
    /// <param name="point"></param>
    public void MoveStone(int moveId,int row,int col)
    {
        GameObject Stone = GameObject.Find(moveId.ToString());
        Stone.transform.DOMove(new Vector3(ToolManager.colToX(col), ToolManager.rowToY(row), 0), 0.5f);
        
        StoneManager.s[moveId]._row = row;
        StoneManager.s[moveId]._col = col;

        _beRedTurn = !_beRedTurn;
    }

    /// <summary>
    /// 将移动的棋子ID、吃掉的棋子ID以及棋子从A点的坐标移动到B点的坐标都记录下来
    /// </summary>
    /// <param name="moveId"></param>
    /// <param name="killId"></param>
    /// <param name="bx"></param>
    /// <param name="by"></param>
    public void SaveStep(int moveId, int killId, int row, int col,ref List<step> steps)
    {
        step tmpStep = new step();

        tmpStep.moveId = moveId;
        tmpStep.killId = killId;
        tmpStep.rowFrom = StoneManager.s[moveId]._row;
        tmpStep.colFrom = StoneManager.s[moveId]._col;
        tmpStep.rowTo = row;
        tmpStep.colTo = col;

        steps.Add(tmpStep);
    }

    /// <summary>
    /// 通过记录的步骤结构体来返回上一步
    /// </summary>
    /// <param name="_step"></param>
    void Back(step _step)
    {
        ReliveStone(_step.killId);
        MoveStone(_step.moveId, _step.rowFrom,_step.colFrom);
        HidePath();
        if (_selectedId != -1)
        {
            ChangeSpriteToNormal(_selectedId);
            _selectedId = -1;
        }
    }

    /// <summary>
    /// 悔棋，退回一步
    /// </summary>
    public void BackOne()
    {
        if (_steps.Count == 0) return;

        step tmpStep = _steps[_steps.Count - 1];
        _steps.RemoveAt(_steps.Count - 1);
        Back(tmpStep);
        FunctionPlane.SetActive(false);
    }

    public virtual void Back()
    {
        BackOne();
    }

    #endregion


    #region 规则

    /// <summary>
    /// 判断走棋是否符合走棋的规则
    /// </summary>
    /// <param name="selectedId"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public bool CanMove(int moveId, int killId, int row,int col)
    {
        if (SameColor(moveId, killId)) return false;

        switch (StoneManager.s[moveId]._type)
        {
            case StoneManager.Stone.TYPE.JIANG:
                return RuleManager.moveJiang(moveId, row, col, killId);
            case StoneManager.Stone.TYPE.SHI:
                return RuleManager.moveShi(moveId, row, col, killId);
            case StoneManager.Stone.TYPE.XIANG:
                return RuleManager.moveXiang(moveId, row, col, killId);
            case StoneManager.Stone.TYPE.CHE:
                return RuleManager.moveChe(moveId, row, col, killId);
            case StoneManager.Stone.TYPE.MA:
                return RuleManager.moveMa(moveId, row, col, killId);
            case StoneManager.Stone.TYPE.PAO:
                return RuleManager.movePao(moveId, row, col, killId);
            case StoneManager.Stone.TYPE.BING:
                return RuleManager.moveBing(moveId, row, col, killId);
        }

        return true;
    }

    /// <summary>
    /// 判断点击的棋子是否可以被选中，即点击的棋子是否在它可以移动的回合
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    bool CanSelect(int id)
    {
        return _beRedTurn == StoneManager.s[id]._red;
    }

    #endregion


    #region 设置相关的按钮响应

    /// <summary>
    /// 设置按钮
    /// </summary>
    public void SettingButton()
    {
        Debug.Log("点击到设置按钮");
        FunctionPlane.SetActive(false);
    }

    /// <summary>
    /// 隐藏设置面板
    /// </summary>
    public void SetPanelDisable()
    {
        FunctionPlane.SetActive(false);
    }

    /// <summary>
    /// 显示设置面板
    /// </summary>
    public void SetPanelEnable()
    {
        FunctionPlane.SetActive(true);
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public virtual void Restart()
    {
        SceneManager.LoadScene("Main");
    }

    /// <summary>
    /// 返回主菜单
    /// </summary>
    public virtual void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    #endregion
}
