using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Agent : MonoBehaviour
{
    public List<Moves> moves = new List<Moves>();
    public TileColor agentColor = TileColor.None;
    [SerializeField] private Stone Capstone;
    [SerializeField] private PlayerStoneController psc;
    [SerializeField] private PlayerStoneController epsc;
    [SerializeField] private bool check = false;
    private bool working = false;

    public Board TestBoard;
    public bool test;

    private void Start()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Capstone"))
        {
            Stone cap = obj.GetComponent<Stone>();
            if(cap != null)
            {
                if(cap.stoneColor == agentColor)
                {
                    Capstone = cap;
                    break;
                }
            }
        }
        foreach (PlayerStoneController pscTemp in GameObject.FindObjectsByType<PlayerStoneController>(FindObjectsSortMode.None))
        {
            if (pscTemp != null)
            {
                if (pscTemp.playerColor == agentColor)
                {
                    psc = pscTemp;
                }
                else
                {
                    epsc = pscTemp;
                }
            }
        }
    }

    private void Update()
    {
        if(test)
        {
            test = false;
            //TestBoard = new Board();
            DateTime save = DateTime.Now;
            //Board.getCurrentBoard(TestBoard);
            //TestBoard.quantifyBoard();

            /*
            moves.Clear();

            //System.DateTime timeTest = System.DateTime.Now;

            moves = getMoves();

            int moveIndex = UnityEngine.Random.Range(0, moves.Count - 1);
            
            Board tempBoard = Board.getNewBoard(TestBoard, moves[moveIndex]);

            if (tempBoard == TestBoard)
            {
                Debug.Log("fail");
            }
            else
                Debug.Log("sUcc");
            */

            Moves move = null;
            StartCoroutine(MCTSStrategy.GetNextMove(this, 5f, moveReturn =>
            {
                move = moveReturn;
            }));

            Debug.Log(DateTime.Now - save);

            if (move != null)
            {
                if (move.isPlaceStone())
                {
                    Debug.Log(move.getOrigin() + ", isWall: " + move.isWall() + ", isCapstone: " + move.isCapstone());
                }
                else
                {
                    Debug.Log(move.getOrigin() + ", Direction: " + move.getDirection() + ", Distance: " + move.getDist() + ", Abandon: " + move.getAbandon());
                }
            }
        }

        if (check && (agentColor == GameController.instance.currentTurn))
        {
            Moves nextMove = null;
            if(!working)
            {
                working = true;
                StartCoroutine(MCTSStrategy.GetNextMove(this, 10f, moveReturn =>
                {
                    Debug.Log("found");
                    nextMove = moveReturn;
                    working = false;
                    if (nextMove.isPlaceStone())
                    {
                        if (nextMove.getPlaceStone() == 'w' || nextMove.getPlaceStone() == 'b')
                        {
                            Stone stone;
                            if (!GameController.canWall())
                            {
                                stone = epsc.PlaceNextStone(nextMove.getOrigin());
                            }
                            else
                            {
                                stone = psc.PlaceNextStone(nextMove.getOrigin());
                            }

                            stone.currentTile = GenBoard.instance.board[(nextMove.getOriginX(), nextMove.getOriginY())];
                            stone.transform.position = new Vector3(stone.currentTile.transform.position.x, stone.currentTile.transform.position.y, transform.position.z);
                            stone.wall = nextMove.getWall() == 't' ? true : false;
                            GenBoard.instance.board[(stone.currentTile.boardPosition.x, stone.currentTile.boardPosition.y)].stonesOnTile.Add(stone);
                            stone.onTile = true;
                            stone.placed = true;
                            stone.follow = false;

                            stone.gameObject.SetActive(true);
                        }
                        else if ((nextMove.getPlaceStone() == 'W' || nextMove.getPlaceStone() == 'B'))
                        {
                            if (!GameController.canWall())
                            {
                                Stone stone = epsc.PlaceNextStone(nextMove.getOrigin());
                                stone.currentTile = GenBoard.instance.board[(nextMove.getOriginX(), nextMove.getOriginY())];
                                stone.transform.position = new Vector3(stone.currentTile.transform.position.x, stone.currentTile.transform.position.y, transform.position.z);
                                stone.wall = nextMove.getWall() == 't' ? true : false;
                                GenBoard.instance.board[(stone.currentTile.boardPosition.x, stone.currentTile.boardPosition.y)].stonesOnTile.Add(stone);
                                stone.onTile = true;
                                stone.placed = true;
                                stone.follow = false;
                            }
                            else
                            {
                                Capstone.currentTile = GenBoard.instance.board[(nextMove.getOriginX(), nextMove.getOriginY())];
                                Capstone.transform.position = new Vector3(Capstone.currentTile.transform.position.x, Capstone.currentTile.transform.position.y, transform.position.z);
                                GenBoard.instance.board[(Capstone.currentTile.boardPosition.x, Capstone.currentTile.boardPosition.y)].stonesOnTile.Add(Capstone);
                                Capstone.onTile = true;
                                Capstone.placed = true;
                                Capstone.follow = false;

                                Capstone.gameObject.SetActive(true);
                            }

                        }
                        GameController.placeStone();
                    }
                    else if (nextMove.isMoveStone())
                    {
                        int progress = 0;
                        Selecter selecter = GameObject.FindObjectOfType<Selecter>();
                        try
                        {

                            for (int i = 1; i <= nextMove.getDist(); i++)
                            {
                                if (nextMove.getDirection() == 'u')
                                {
                                    Tile activeTile = GenBoard.instance.board[(nextMove.getOriginX(), nextMove.getOriginY() + progress)];
                                    //Tile goalTile = GenBoard.instance.board[(moves[moveIndex].getOriginX(), moves[moveIndex].getOriginY() + i)];

                                    selecter.transform.position = activeTile.transform.position + selecter.offset;
                                    selecter.selectedTile = activeTile;
                                    int abandonCount = i == 1 ? nextMove.getAbandon() : 0;
                                    foreach (Stone stone in selecter.selectedTile.stonesOnTile)
                                    {
                                        if (abandonCount > 0)
                                        {
                                            selecter.leaveStones.Add(stone);
                                            abandonCount--;
                                        }
                                        if (!selecter.leaveStones.Contains(stone) && !selecter.movingStones.Contains(stone))
                                        {
                                            selecter.movingStones.Add(stone);
                                        }
                                    }

                                    selecter.MoveStones(Direction.Up);
                                }
                                else if (nextMove.getDirection() == 'r')
                                {
                                    Tile activeTile = GenBoard.instance.board[(nextMove.getOriginX() + progress, nextMove.getOriginY())];
                                    //Tile goalTile = GenBoard.instance.board[(moves[moveIndex].getOriginX() + i, moves[moveIndex].getOriginY())];

                                    selecter.transform.position = activeTile.transform.position + selecter.offset;
                                    selecter.selectedTile = activeTile;
                                    int abandonCount = i == 1 ? nextMove.getAbandon() : 0;
                                    foreach (Stone stone in selecter.selectedTile.stonesOnTile)
                                    {
                                        if (abandonCount > 0)
                                        {
                                            selecter.leaveStones.Add(stone);
                                            abandonCount--;
                                        }
                                        if (!selecter.leaveStones.Contains(stone) && !selecter.movingStones.Contains(stone))
                                        {
                                            selecter.movingStones.Add(stone);
                                        }
                                    }

                                    selecter.MoveStones(Direction.Right);
                                }
                                else if (nextMove.getDirection() == 'd')
                                {
                                    Tile activeTile = GenBoard.instance.board[(nextMove.getOriginX(), nextMove.getOriginY() - progress)];
                                    //Tile goalTile = GenBoard.instance.board[(moves[moveIndex].getOriginX(), moves[moveIndex].getOriginY() - i)];

                                    selecter.transform.position = activeTile.transform.position + selecter.offset;
                                    selecter.selectedTile = activeTile;
                                    int abandonCount = i == 1 ? nextMove.getAbandon() : 0;
                                    foreach (Stone stone in selecter.selectedTile.stonesOnTile)
                                    {
                                        if (abandonCount > 0)
                                        {
                                            selecter.leaveStones.Add(stone);
                                            abandonCount--;
                                        }
                                        if (!selecter.leaveStones.Contains(stone) && !selecter.movingStones.Contains(stone))
                                        {
                                            selecter.movingStones.Add(stone);
                                        }
                                    }

                                    selecter.MoveStones(Direction.Down);
                                }
                                else if (nextMove.getDirection() == 'l')
                                {
                                    Tile activeTile = GenBoard.instance.board[(nextMove.getOriginX() - progress, nextMove.getOriginY())];
                                    //Tile goalTile = GenBoard.instance.board[(moves[moveIndex].getOriginX() - i, moves[moveIndex].getOriginY())];

                                    selecter.transform.position = activeTile.transform.position + selecter.offset;
                                    selecter.selectedTile = activeTile;
                                    int abandonCount = i == 1 ? nextMove.getAbandon() : 0;
                                    foreach (Stone stone in selecter.selectedTile.stonesOnTile)
                                    {
                                        if (abandonCount > 0)
                                        {
                                            selecter.leaveStones.Add(stone);
                                            abandonCount--;
                                        }
                                        if (!selecter.leaveStones.Contains(stone) && !selecter.movingStones.Contains(stone))
                                        {
                                            selecter.movingStones.Add(stone);
                                        }
                                    }

                                    selecter.MoveStones(Direction.Left);
                                }
                                progress++;
                            }
                        }
                        catch
                        {
                            Debug.LogWarning("Attempt to move off the board, try again");
                        }
                        selecter.EndTurn();
                    }
                }));
            }
            //moves.Clear();

            //System.DateTime timeTest = System.DateTime.Now;

            /*moves = getMoves();
            check = false;

            int moveIndex = UnityEngine.Random.Range(0, moves.Count - 1);

            if (!GameController.canWall())
            {
                while (nextMove.isMoveStone())
                {
                    moveIndex = UnityEngine.Random.Range(0, moves.Count - 1);
                }
            }*/

            //Debug.Log(System.DateTime.Now - timeTest);
        }
    }

    public static List<Moves> getMoves(Board board, TileColor turn)
    {
        List<Moves> moves = new List<Moves>();

        List<BoardTile> check = getValidTiles(board, turn);

        char[] directions = { 'u', 'r', 'd', 'l' };

        foreach (BoardTile tile in check)
        {
            if (tile.stonesOnTile.Count == 0)
            {
                Moves place = new Moves(turn == TileColor.White ? 'w' : 'b', 'f', tile.boardPosition);
                moves.Add(place);

                place = new Moves(turn == TileColor.White ? 'w' : 'b', 't', tile.boardPosition);
                moves.Add(place);
                if ((turn == TileColor.White ? !board.wCapstonePlaced : !board.bCapstonePlaced))
                {
                    Moves capPlace = new Moves(turn == TileColor.White ? 'W' : 'B', 'f', tile.boardPosition);
                    moves.Add(capPlace);
                }
            }
            else
            {
                foreach (char dir in directions)
                {
                    for (short i = 1; i <= tile.stonesOnTile.Count; i++)
                    {
                        for (short a = 0; a < tile.stonesOnTile.Count; a++)
                        {
                            if(i + a <= tile.stonesOnTile.Count)
                            {
                                Moves moveStone = new Moves(tile.boardPosition, dir, i, a);
                                if (dir == 'u')
                                {
                                    try
                                    {
                                        //if (GenBoard.instance.board[(tile.boardPosition.x, tile.boardPosition.y + 1)])
                                        {
                                            if(board.getStonesOnTile((int)tile.boardPosition.x, (int)tile.boardPosition.y + 1) > 0)
                                            {
                                                if (!board.getLastStoneOnTile((int)tile.boardPosition.x, (int)tile.boardPosition.y + 1).cap && !board.getLastStoneOnTile((int)tile.boardPosition.x, (int)tile.boardPosition.y + 1).wall)
                                                    moves.Add(moveStone);
                                            }
                                            else
                                            {
                                                moves.Add(moveStone);
                                            }
                                        }
                                    }
                                    catch { }
                                }
                                else if (dir == 'r')
                                {
                                    try
                                    {
                                        //if (GenBoard.instance.board[(tile.boardPosition.x + 1, tile.boardPosition.y)])
                                        {
                                            if(board.getStonesOnTile((int)tile.boardPosition.x + 1, (int)tile.boardPosition.y) > 0)
                                            {
                                                if (!board.getLastStoneOnTile((int)tile.boardPosition.x + 1, (int)tile.boardPosition.y).cap && !board.getLastStoneOnTile((int)tile.boardPosition.x + 1, (int)tile.boardPosition.y).wall)
                                                    moves.Add(moveStone);
                                            }
                                            else
                                            {
                                                moves.Add(moveStone);
                                            }
                                        }
                                    }
                                    catch { }
                                }
                                if (dir == 'd')
                                {
                                    try
                                    {
                                        //if (GenBoard.instance.board[(tile.boardPosition.x, tile.boardPosition.y - 1)])
                                        {
                                            if (board.getStonesOnTile((int)tile.boardPosition.x, (int)tile.boardPosition.y - 1) > 0)
                                            {
                                                if (!board.getLastStoneOnTile((int)tile.boardPosition.x, (int)tile.boardPosition.y - 1).cap && !board.getLastStoneOnTile((int)tile.boardPosition.x, (int)tile.boardPosition.y - 1).wall)
                                                    moves.Add(moveStone);
                                            }
                                            else
                                            {
                                                moves.Add(moveStone);
                                            }
                                        }
                                    }
                                    catch { }
                                }
                                else if (dir == 'l')
                                {
                                    try
                                    {
                                        //if (GenBoard.instance.board[(tile.boardPosition.x - 1, tile.boardPosition.y)])
                                        {
                                            if (board.getStonesOnTile((int)tile.boardPosition.x - 1, (int)tile.boardPosition.y) > 0)
                                            {
                                                if (!board.getLastStoneOnTile((int)tile.boardPosition.x - 1, (int)tile.boardPosition.y).cap && !board.getLastStoneOnTile((int)tile.boardPosition.x + 1, (int)tile.boardPosition.y).wall)
                                                    moves.Add(moveStone);
                                            }
                                            else
                                            {
                                                moves.Add(moveStone);
                                            }
                                        }
                                    }
                                    catch { }
                                }
                            }
                        }
                    }
                }
            }

        }

        if (moves.Count <= 0)
        {
            moves = null;
        }
        return moves;
    }

    public List<Moves> getMoves()
    {
        List<Moves> moves = new List<Moves>();

        List<Tile> check = getValidTiles();

        char[] directions = { 'u', 'r', 'd', 'l' };

        foreach (Tile tile in check)
        {
            if(tile.stonesOnTile.Count == 0)
            {
                Moves place = new Moves(agentColor == TileColor.White ? 'w' : 'b', 'f', tile.boardPosition);
                moves.Add(place);
                
                place = new Moves(agentColor == TileColor.White ? 'w' : 'b', 't', tile.boardPosition);
                moves.Add(place);
                if (!Capstone.placed)
                {
                    Moves capPlace = new Moves(agentColor == TileColor.White ? 'W' : 'B', 'f', tile.boardPosition);
                    moves.Add(capPlace);
                }
            }
            else
            {
                foreach(char dir in directions)
                {
                    for(short i = 1;  i <= tile.stonesOnTile.Count; i++)
                    {
                        for(short a = 0; a <= tile.stonesOnTile.Count; a++)
                        {
                            Moves moveStone = new Moves(tile.boardPosition , dir, i, a);
                            if(dir == 'u')
                            {
                                try
                                {
                                    if (GenBoard.instance.board[(tile.boardPosition.x, tile.boardPosition.y + 1)])
                                    {
                                        if(!GenBoard.instance.board[(tile.boardPosition.x, tile.boardPosition.y + 1)].stonesOnTile.Last().cap && !GenBoard.instance.board[(tile.boardPosition.x, tile.boardPosition.y + 1)].stonesOnTile.Last().wall)
                                            moves.Add(moveStone);
                                    }
                                }
                                catch { }
                            }
                            else if (dir == 'r')
                            {
                                try
                                {
                                    if (GenBoard.instance.board[(tile.boardPosition.x + 1, tile.boardPosition.y)])
                                    {
                                        if (!GenBoard.instance.board[(tile.boardPosition.x + 1, tile.boardPosition.y)].stonesOnTile.Last().cap && !GenBoard.instance.board[(tile.boardPosition.x + 1, tile.boardPosition.y)].stonesOnTile.Last().wall)
                                            moves.Add(moveStone);
                                    }
                                }
                                catch { }
                            }
                            if (dir == 'd')
                            {
                                try
                                {
                                    if (GenBoard.instance.board[(tile.boardPosition.x, tile.boardPosition.y - 1)])
                                    {
                                        if (!GenBoard.instance.board[(tile.boardPosition.x, tile.boardPosition.y - 1)].stonesOnTile.Last().cap && !GenBoard.instance.board[(tile.boardPosition.x, tile.boardPosition.y - 1)].stonesOnTile.Last().wall)
                                            moves.Add(moveStone);
                                    }
                                }
                                catch { }
                            }
                            else if (dir == 'l')
                            {
                                try
                                {
                                    if (GenBoard.instance.board[(tile.boardPosition.x - 1, tile.boardPosition.y)])
                                    {
                                        if (!GenBoard.instance.board[(tile.boardPosition.x - 1, tile.boardPosition.y)].stonesOnTile.Last().cap && !GenBoard.instance.board[(tile.boardPosition.x - 1, tile.boardPosition.y)].stonesOnTile.Last().wall)
                                            moves.Add(moveStone);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }

        }

        if (moves.Count <= 0)
        {
            moves = null;
        }
        return moves;
    }

    //returns a list of tiles that are either empty or under apropriate controle
    private List<Tile> getValidTiles()
    {
        List<Tile> valid = new List<Tile>();

        foreach(KeyValuePair<(float, float), Tile> tile in GenBoard.instance.board)
        {
            if(tile.Value != null)
            {
                if (tile.Value.stonesOnTile.Count > 0)
                {
                    if(tile.Value.stonesOnTile.Last().stoneColor == agentColor)
                    {
                        valid.Add(tile.Value);
                    }
                }
                else
                {
                    valid.Add(tile.Value);
                }
            }
        }

        if(valid.Count <= 0 )
        {
            valid = null;
        }
        return valid;
    }

    private static List<BoardTile> getValidTiles(Board board, TileColor turn)
    {
        List<BoardTile> valid = new List<BoardTile>();

        foreach (KeyValuePair<(int, int), BoardTile> tile in board.board)
        {
            if (tile.Value != null)
            {
                if (tile.Value.stonesOnTile.Count > 0)
                {
                    if (tile.Value.stonesOnTile.Last().stoneColor == turn)
                    {
                        valid.Add(tile.Value);
                    }
                }
                else
                {
                    valid.Add(tile.Value);
                }
            }
        }

        if (valid.Count <= 0)
        {
            valid = null;
        }
        return valid;
    }

    public void Check() 
    { 
        if(agentColor == GameController.instance.currentTurn)
            check = true; 
    }
}
