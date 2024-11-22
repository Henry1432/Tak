using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class BoardTile
{

    public TileColor owner;
    public float control; //who has control of the tile, 0 means total and opressive control by black, 1 means total and opressive control by white
    public int range; //distance this tile can cover
    public bool road; //if the tile is acting as a road
    public Direction dir;
    public List<TempStone> stonesOnTile = new List<TempStone>();
    public Vector2 boardPosition;
}

public class Board
{
    public Dictionary<(int, int), BoardTile> board;
    public float coverage; //0 is total black coverage, 1 is total white coverage
    public float totalControl; //avarage of control from board, same as above
    public float proximity; //how close to a win state we are
    public TileColor advantage; //who the proximity is for,
            //if white is close to a win prox would be 2 and advantage would be white.
            //if black and white are both 3 away from win prox would be avarage of both(3) and advantage would be none.
    public TileColor win = TileColor.None;
    public bool wCapstonePlaced;
    public bool bCapstonePlaced;
    public Board root = null;
    public List<Board> children = new List<Board>();

    public Moves saveMove;
    public float SaveScore;
    public Board()
    {
        board = new Dictionary<(int, int), BoardTile>();
        coverage = 0;
        totalControl = 0;
        proximity = 0;
        advantage = TileColor.None;
    }
    public Board(Board copy)
    {
        board = new Dictionary<(int, int), BoardTile>(copy.board);
        coverage = copy.coverage;
        totalControl = copy.totalControl;
        proximity = copy.proximity;
        advantage = copy.advantage;
    }


    //public float score;

    //assumes move is valid
    public static Board getNewBoard(Board baseBoard, Moves move)
    {
        Board newBoard = new Board(baseBoard);
        editBoard(newBoard, move);
        newBoard.root = baseBoard;
        newBoard.saveMove = move;
        baseBoard.children.Add(newBoard);

        return newBoard;
    }

    private static void editBoard(Board target, Moves move)
    {
        if (move.isPlaceStone())
        {
            BoardTile boardTile = new BoardTile();
            boardTile.owner = move.getPlaceStoneColor();
            if (move.isWall())
            {
                if (boardTile.owner == TileColor.White)
                {
                    boardTile.control = getTileControl(1, 0, true, false, boardTile.owner);
                }
                else
                {
                    boardTile.control = getTileControl(0, 1, true, false, boardTile.owner);
                }
            }
            else if(move.isCapstone())
            {
                if (boardTile.owner == TileColor.White)
                {
                    boardTile.control = getTileControl(1, 0, false, true, boardTile.owner);
                    target.wCapstonePlaced = true;
                }
                else
                {
                    boardTile.control = getTileControl(0, 1, false, true, boardTile.owner);
                    target.bCapstonePlaced= true;
                }
            }
            else
            {
                if (boardTile.owner == TileColor.White)
                {
                    boardTile.control = getTileControl(1, 0, false, false, boardTile.owner);
                }
                else
                {
                    boardTile.control = getTileControl(0, 1, false, false, boardTile.owner);
                }
            }
            boardTile.range = 1;
            boardTile.boardPosition = move.getOrigin();

            bool success; 
            Tile tile = GenBoard.instance.board[((float)move.getOriginX(), (float)move.getOriginY())];
            TempStone stone = new TempStone(boardTile.owner, move.isWall(), move.isCapstone(), tile);
            if(stone != null)
            {
                boardTile.stonesOnTile.Add(stone);
            }

            target.board.Remove(((int)move.getOriginX(), (int)move.getOriginY()));
            target.board.Add(((int)move.getOriginX(), (int)move.getOriginY()), boardTile);
        }
        else
        {
            List<TempStone> movingTiles = new List<TempStone>(target.board[((int)move.getOriginX(), (int)move.getOriginY())].stonesOnTile), abandonedTiles = new List<TempStone>();
            for (int a = 0; a < move.getAbandon(); a++)
            {
                if (movingTiles.Count > 0)
                {
                    abandonedTiles.Add(movingTiles.First());
                    movingTiles.Remove(movingTiles.First());
                }
            }
            int xDist = 0, yDist = 0;
            for(int i = 0; i < move.getDist(); i++)
            {
                if(movingTiles.Count > 0)
                {
                    BoardTile thisTile = new BoardTile(), nextTile = new BoardTile();
                    if(abandonedTiles.Count > 0)
                    {
                        thisTile.stonesOnTile.AddRange(abandonedTiles);

                        if(i == 0)
                        {
                            for(int a = 0; a < move.getAbandon(); a++)
                                if(abandonedTiles.Count > 0)
                                    abandonedTiles.Remove(abandonedTiles.First());
                        }
                        else
                        {
                            abandonedTiles.Remove(abandonedTiles.First());
                        }
                    }
                    if(movingTiles.Count > 0)
                    {
                        nextTile.stonesOnTile.AddRange(movingTiles);

                        abandonedTiles.Add(movingTiles.First());
                        movingTiles.Remove(movingTiles.First());
                    }

                    if(thisTile.stonesOnTile.Count > 0)
                    {
                        thisTile.owner = thisTile.stonesOnTile.Last().stoneColor;
                        thisTile.control = getTileControl(thisTile.stonesOnTile);
                    }
                    else
                    {
                        thisTile.owner = TileColor.None;
                        thisTile.control = 0.5f;
                    }

                    if (nextTile.stonesOnTile.Count > 0)
                    {
                        nextTile.owner = nextTile.stonesOnTile.Last().stoneColor;
                        nextTile.control = getTileControl(nextTile.stonesOnTile);
                    }
                    else
                    {
                        nextTile.owner = TileColor.None;
                        nextTile.control = 0.5f;
                    }

                    if (move.getDirection() == 'u')
                    {
                        yDist += 1;
                        /*try
                        {
                            thisTile.stonesOnTile.AddRange(target.board[(move.getOriginX() + xDist, move.getOriginY() + yDist - 1)].stonesOnTile);
                        }
                        catch { }*/
                        try 
                        {
                            nextTile.stonesOnTile.AddRange(target.board[(move.getOriginX() + xDist, move.getOriginY() + yDist)].stonesOnTile);
                        }
                        catch{}

                        thisTile.range = thisTile.stonesOnTile.Count;
                        nextTile.range = nextTile.stonesOnTile.Count;

                        thisTile.boardPosition = target.board[((int)Mathf.Clamp(move.getOriginX() + xDist, 0, GenBoard.getSize() - 1), (int)Mathf.Clamp(move.getOriginY() + yDist - 1, 0, GenBoard.getSize() - 1))].boardPosition;
                        nextTile.boardPosition = target.board[((int)Mathf.Clamp(move.getOriginX() + xDist, 0, GenBoard.getSize() - 1), (int)Mathf.Clamp(move.getOriginY() + yDist, 0, GenBoard.getSize() - 1))].boardPosition;
                    }
                    else if(move.getDirection() == 'r')
                    {
                        xDist += 1;
                        /*try 
                        {
                            thisTile.stonesOnTile.AddRange(target.board[(move.getOriginX() + xDist - 1, move.getOriginY() + yDist)].stonesOnTile);
                        }
                        catch{}*/
                        try 
                        {
                            nextTile.stonesOnTile.AddRange(target.board[(move.getOriginX() + xDist, move.getOriginY() + yDist)].stonesOnTile);
                        }
                        catch{}

                        thisTile.range = thisTile.stonesOnTile.Count;
                        nextTile.range = nextTile.stonesOnTile.Count;

                        thisTile.boardPosition = target.board[((int)Mathf.Clamp(move.getOriginX() + xDist - 1, 0, GenBoard.getSize() - 1), (int)Mathf.Clamp(move.getOriginY() + yDist, 0, GenBoard.getSize() - 1))].boardPosition;
                        nextTile.boardPosition = target.board[((int)Mathf.Clamp(move.getOriginX() + xDist, 0, GenBoard.getSize() - 1), (int)Mathf.Clamp(move.getOriginY() + yDist, 0, GenBoard.getSize() - 1))].boardPosition;
                    }
                    else if (move.getDirection() == 'd')
                    {
                        yDist -= 1;
                        /*try 
                        {
                            thisTile.stonesOnTile.AddRange(target.board[(move.getOriginX() + xDist, move.getOriginY() + yDist + 1)].stonesOnTile);
                        }
                        catch{}*/
                        try 
                        {                        
                            nextTile.stonesOnTile.AddRange(target.board[(move.getOriginX() + xDist, move.getOriginY() + yDist)].stonesOnTile);
                        }
                        catch{}

                        thisTile.range = thisTile.stonesOnTile.Count;
                        nextTile.range = nextTile.stonesOnTile.Count;

                        thisTile.boardPosition = target.board[((int)Mathf.Clamp(move.getOriginX() + xDist, 0, GenBoard.getSize() - 1), (int)Mathf.Clamp(move.getOriginY() + yDist + 1, 0, GenBoard.getSize() - 1))].boardPosition;
                        nextTile.boardPosition = target.board[((int)Mathf.Clamp(move.getOriginX() + xDist, 0, GenBoard.getSize() - 1), (int)Mathf.Clamp(move.getOriginY() + yDist, 0, GenBoard.getSize() - 1))].boardPosition;
                    }
                    else
                    {
                        xDist -= 1;
                        /*try
                        {
                            thisTile.stonesOnTile.AddRange(target.board[(move.getOriginX() + xDist + 1, move.getOriginY() + yDist)].stonesOnTile);
                        }
                        catch{}*/
                        try 
                        {
                            nextTile.stonesOnTile.AddRange(target.board[(move.getOriginX() + xDist, move.getOriginY() + yDist)].stonesOnTile);
                        }
                        catch{}

                        thisTile.range = thisTile.stonesOnTile.Count;
                        nextTile.range = nextTile.stonesOnTile.Count;


                        thisTile.boardPosition = target.board[((int)Mathf.Clamp(move.getOriginX() + xDist + 1, 0, GenBoard.getSize() - 1), (int)Mathf.Clamp(move.getOriginY() + yDist, 0, GenBoard.getSize() - 1))].boardPosition;
                        nextTile.boardPosition = target.board[((int)Mathf.Clamp(move.getOriginX() + xDist, 0, GenBoard.getSize() - 1), (int)Mathf.Clamp(move.getOriginY() + yDist, 0, GenBoard.getSize() - 1))].boardPosition;
                    }

                    target.board.Remove(((int)thisTile.boardPosition.x, (int)thisTile.boardPosition.y));
                    target.board.Add(((int)thisTile.boardPosition.x, (int)thisTile.boardPosition.y), thisTile);
                    target.board.Remove(((int)nextTile.boardPosition.x, (int)nextTile.boardPosition.y));
                    target.board.Add(((int)nextTile.boardPosition.x, (int)nextTile.boardPosition.y), nextTile);
                }
            }
        }
    }

    public static void getCurrentBoard(Board target)
    {
        if(target.board == null)
        {
            target.board = new Dictionary<(int, int), BoardTile> ();
        }
        target.board.Clear ();

        foreach(KeyValuePair<(float,float), Tile> tile in GenBoard.instance.board)
        {
            BoardTile boardTile = new BoardTile();
            if(tile.Value.stonesOnTile.Count > 0)
            {
                boardTile.owner = tile.Value.stonesOnTile.Last().stoneColor;
                boardTile.control = getTileControl(tile.Value);
                boardTile.range = tile.Value.stonesOnTile.Count;
                foreach(Stone stone in  tile.Value.stonesOnTile)
                {
                    boardTile.stonesOnTile.Add(new TempStone(stone)); 
                }

                if(tile.Value.stonesOnTile.Last().cap)
                {
                    if(boardTile.owner == TileColor.White)
                        target.wCapstonePlaced = true;
                    else
                        target.bCapstonePlaced = true;
                }
            }
            else
            {
                boardTile.owner = TileColor.None;
                boardTile.control = 0.5f;
                boardTile.range = 0;
            }
            boardTile.boardPosition = tile.Value.boardPosition;
            
            target.board.Add(((int)tile.Key.Item1, (int)tile.Key.Item2), boardTile);
            target.coverage += boardTile.owner == TileColor.White ? 1 : 0;
            target.totalControl += boardTile.control;
        }
        target.coverage /= target.board.Count;
        target.totalControl /= target.board.Count;

        //target.quantifyBoard();
        target.root = target;
    }

    public void quantifyBoard()
    {
        proximity = Mathf.Infinity;
        advantage = TileColor.None;

        int whiteRoadCount = 0;
        int blackRoadCount = 0;
        int totalPoxi = 0;

        int edge = (int)(GenBoard.getSize() - 1);
        List<int> poxi = new List<int>();
        for (int i = 0; i < GenBoard.getSize(); i++)
        {
            if (board[(0, i)].owner != TileColor.None)
            {
                int tempProx = (int)GenBoard.getSize() - checkPath(0, i, 0, Direction.Right);
                if (tempProx != (int)GenBoard.getSize())
                {
                    if (board[(0, i)].owner == TileColor.White)
                        whiteRoadCount++;
                    else
                        blackRoadCount++;

                    if (tempProx < proximity)
                    {
                        proximity = tempProx;
                    }

                    totalPoxi += tempProx;
                    poxi.Add(tempProx);
                }
            }

            if (board[(i, edge)].owner != TileColor.None)
            {
                int tempProx = (int)GenBoard.getSize() - checkPath(i, edge, 0, Direction.Down);
                if (tempProx != (int)GenBoard.getSize())
                {
                    if (board[(i, edge)].owner == TileColor.White)
                        whiteRoadCount++;
                    else
                        blackRoadCount++;

                    if (tempProx < proximity)
                    {
                        proximity = tempProx;
                    }

                    totalPoxi += tempProx;
                    poxi.Add(tempProx);
                }
            }

            if (board[(edge, edge - i)].owner != TileColor.None)
            {
                int tempProx = (int)GenBoard.getSize() - checkPath(edge, edge - i, 0, Direction.Left);
                if (tempProx != (int)GenBoard.getSize())
                {
                    if (board[(edge, edge - i)].owner == TileColor.White)
                        whiteRoadCount++;
                    else
                        blackRoadCount++;

                    if (tempProx < proximity)
                    {
                        proximity = tempProx;
                    }

                    totalPoxi += tempProx;
                    poxi.Add(tempProx);
                }
            }

            if (board[(i, 0)].owner != TileColor.None)
            {
                int tempProx = (int)GenBoard.getSize() - checkPath(i, 0, 0, Direction.Up);
                if (tempProx != (int)GenBoard.getSize())
                {
                    if (board[(i, 0)].owner == TileColor.White)
                        whiteRoadCount++;
                    else
                        blackRoadCount++;

                    if (tempProx < proximity)
                    {
                        proximity = tempProx;
                    }

                    totalPoxi += tempProx;
                    poxi.Add(tempProx);
                }
            }
        }

        float totalRoadCount = whiteRoadCount + blackRoadCount;
        float position = (totalControl + coverage + (whiteRoadCount/totalRoadCount)) / 2;
        advantage = TileColor.None;
        if(position > 0.5f)
        {
            advantage = TileColor.White;
        }
        else if(position < 0.5f)
        {
            advantage = TileColor.Black;
        }
    }

    private void checkPath()
    {
        //currently this is a recursive board shifting pile of bs, this can be inproved to a list of neighbor groups, an unordered list of unordered maps that can be checked against
            //add some handling funcitons like win state count and neighbor group count. run checkPath at top of quantify board and check the info it got throughout quantification
    }

    private int checkPath(int checkX, int checkY, int dist, Direction dir)
    {
        dist++;
        if (dir == Direction.Up)
        {
            try
            {
                if (board[(checkX, checkY)].owner == board[(checkX, checkY + 1)].owner && !board[(checkX, checkY)].stonesOnTile.Last().wall)
                {
                    board[(checkX, checkY)].road = true;
                    board[(checkX, checkY)].dir = dir;
                    if (dist >= GenBoard.getSize() - 1)
                        win = board[(checkX, checkY)].owner;
                    return checkPath(checkX, checkY + 1, dist, dir);
                }
                else
                {
                    board[(checkX, checkY)].road = false;
                    dist--;
                    return dist;
                }

            }
            catch
            {
                return dist;
            }
        }
        else if (dir == Direction.Right)
        {
            try
            {
                if (board[(checkX, checkY)].owner == board[(checkX + 1, checkY)].owner && !board[(checkX, checkY)].stonesOnTile.Last().wall)
                {
                    board[(checkX, checkY)].road = true;
                    board[(checkX, checkY)].dir = dir;
                    if (dist >= GenBoard.getSize() - 1)
                        win = board[(checkX, checkY)].owner;
                    return checkPath(checkX + 1, checkY, dist, dir);
                }
                else
                {
                    board[(checkX, checkY)].road = false;
                    dist--;
                    return dist;
                }

            }
            catch
            {
                return dist;
            }
        }
        else if (dir == Direction.Down)
        {
            try
            {
                if (board[(checkX, checkY)].owner == board[(checkX, checkY - 1)].owner && !board[(checkX, checkY)].stonesOnTile.Last().wall)
                {
                    board[(checkX, checkY)].road = true;
                    board[(checkX, checkY)].dir = dir;
                    if (dist >= GenBoard.getSize() - 1)
                        win = board[(checkX, checkY)].owner;
                    return checkPath(checkX, checkY - 1, dist, dir);
                }
                else
                {
                    board[(checkX, checkY)].road = false;
                    dist--;
                    return dist;
                }

            }
            catch
            {
                return dist;
            }
        }
        else if (dir == Direction.Left)
        {
            try
            {
                if (board[(checkX, checkY)].owner == board[(checkX + 1, checkY)].owner && !board[(checkX, checkY)].stonesOnTile.Last().wall)
                {
                    board[(checkX, checkY)].road = true;
                    if (dist >= GenBoard.getSize() - 1)
                        win = board[(checkX, checkY)].owner;
                    return checkPath(checkX - 1, checkY, dist, dir);
                }
                else
                {
                    board[(checkX, checkY)].road = false;
                    dist--;
                    return dist;
                }

            }
            catch
            {
                return dist;
            }
        }
        return dist;
    }

    private static float getTileControl(Tile tile)
    {
        float control = ((tile.getWhiteStonesOnTile() / tile.stonesOnTile.Count) * 0.4f) + 0.3f;
        if (tile.stonesOnTile.Last().cap)
        {
            if (tile.stonesOnTile.Last().stoneColor == TileColor.White)
            {
                control += 0.3f;
            }
            else
            {
                control -= 0.3f;
            }
        }
        else if (tile.stonesOnTile.Last().wall)
        {
            if (tile.stonesOnTile.Last().stoneColor == TileColor.White)
            {
                control += 0.2f;
            }
            else
            {
                control -= 0.2f;
            }
        }

        return control;
    }

    private static float getTileControl(List<TempStone> stones)
    {
        int whiteStonesOnTile = 0;
        foreach (TempStone stone in stones)
        {
            if (stone.stoneColor == TileColor.White)
            {
                whiteStonesOnTile++;
            }
        }
        float control = ((whiteStonesOnTile / stones.Count) * 0.4f) + 0.3f;
        if (stones.Last().cap)
        {
            if (stones.Last().stoneColor == TileColor.White)
            {
                control += 0.3f;
            }
            else
            {
                control -= 0.3f;
            }
        }
        else if (stones.Last().wall)
        {
            if (stones.Last().stoneColor == TileColor.White)
            {
                control += 0.2f;
            }
            else
            {
                control -= 0.2f;
            }
        }

        return control;
    }
    private static float getTileControl(int whiteStones, int blackStones, bool wall, bool cap, TileColor owner)
    {
        float control = ((whiteStones / (whiteStones + blackStones)) * 0.4f) + 0.3f;
        if (whiteStones + blackStones == 0)
            control = 0.5f;
        if (cap)
        {
            if (owner == TileColor.White)
            {
                control += 0.3f;
            }
            else
            {
                control -= 0.3f;
            }
        }
        else if (wall)
        {
            if (owner == TileColor.White)
            {
                control += 0.2f;
            }
            else
            {
                control -= 0.2f;
            }
        }

        return control;
    }

    public int getStonesOnTile(Vector2 tilePos)
    {
        return board[((int)tilePos.x, (int)tilePos.y)].stonesOnTile.Count;
    }

    public int getStonesOnTile(int x, int y)
    {
        return board[(x, y)].stonesOnTile.Count;
    }

    public TempStone getLastStoneOnTile(Vector2 tilePos)
    {
        return board[((int)tilePos.x, (int)tilePos.y)].stonesOnTile.Last();
    }

    public TempStone getLastStoneOnTile(int x, int y)
    {
        return board[(x, y)].stonesOnTile.Last();
    }
}
