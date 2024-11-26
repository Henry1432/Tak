using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBoardWin : MonoBehaviour
{
    public Board currentBoard;
    public bool check = false;
    public int winDist;
    public int pathCount;
    public TileColor winning;
    public bool hasWinner;

    void Update()
    {
        Board.getCurrentBoard(out currentBoard);

        if (check)
        {
            check = false;
            currentBoard.checkPath();
            currentBoard.winState(out winDist, out pathCount, out winning, out hasWinner);
        }
    }
}
