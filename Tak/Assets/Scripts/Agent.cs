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
        if (check && (agentColor == GameController.instance.currentTurn))
        {
            moves.Clear();

            //System.DateTime timeTest = System.DateTime.Now;

            moves = getMoves();
            check = false;

            int moveIndex = UnityEngine.Random.Range(0, moves.Count - 1);

            if (!GameController.canWall())
            {
                while (moves[moveIndex].isMoveStone())
                {
                    moveIndex = UnityEngine.Random.Range(0, moves.Count - 1);
                }
            }

            if (moves[moveIndex] != null)
            {
                if (moves[moveIndex].isPlaceStone())
                {
                    if (moves[moveIndex].getPlaceStone() == 'w' || moves[moveIndex].getPlaceStone() == 'b')
                    {
                        Stone stone;
                        if (!GameController.canWall())
                        {
                            stone = epsc.PlaceNextStone(moves[moveIndex].getOrigin());
                        }
                        else
                        {
                            stone = psc.PlaceNextStone(moves[moveIndex].getOrigin());
                        }

                        stone.currentTile = GenBoard.instance.board[(moves[moveIndex].getOriginX(), moves[moveIndex].getOriginY())];
                        stone.transform.position = new Vector3(stone.currentTile.transform.position.x, stone.currentTile.transform.position.y, transform.position.z);
                        stone.wall = moves[moveIndex].getWall() == 't' ? true : false;
                        GenBoard.instance.board[(stone.currentTile.boardPosition.x, stone.currentTile.boardPosition.y)].stonesOnTile.Add(stone);
                        stone.onTile = true;
                        stone.placed = true;
                        stone.follow = false;

                        stone.gameObject.SetActive(true);
                    }
                    else if ((moves[moveIndex].getPlaceStone() == 'W' || moves[moveIndex].getPlaceStone() == 'B'))
                    {
                        if(!GameController.canWall())
                        {
                            Stone stone = epsc.PlaceNextStone(moves[moveIndex].getOrigin());
                            stone.currentTile = GenBoard.instance.board[(moves[moveIndex].getOriginX(), moves[moveIndex].getOriginY())];
                            stone.transform.position = new Vector3(stone.currentTile.transform.position.x, stone.currentTile.transform.position.y, transform.position.z);
                            stone.wall = moves[moveIndex].getWall() == 't' ? true : false;
                            GenBoard.instance.board[(stone.currentTile.boardPosition.x, stone.currentTile.boardPosition.y)].stonesOnTile.Add(stone);
                            stone.onTile = true;
                            stone.placed = true;
                            stone.follow = false;
                        }
                        else
                        {
                            Capstone.currentTile = GenBoard.instance.board[(moves[moveIndex].getOriginX(), moves[moveIndex].getOriginY())];
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
                else if (moves[moveIndex].isMoveStone())
                {
                    int progress = 0;
                    Selecter selecter = GameObject.FindObjectOfType<Selecter>();
                    try
                    {

                        for (int i = 1; i <= moves[moveIndex].getDist(); i++)
                        {
                            if (moves[moveIndex].getDirection() == 'u')
                            {
                                Tile activeTile = GenBoard.instance.board[(moves[moveIndex].getOriginX(), moves[moveIndex].getOriginY() + progress)];
                                //Tile goalTile = GenBoard.instance.board[(moves[moveIndex].getOriginX(), moves[moveIndex].getOriginY() + i)];

                                selecter.transform.position = activeTile.transform.position + selecter.offset;
                                selecter.selectedTile = activeTile;

                                foreach (Stone stone in selecter.selectedTile.stonesOnTile)
                                {
                                    if (!selecter.leaveStones.Contains(stone) && !selecter.movingStones.Contains(stone))
                                    {
                                        selecter.movingStones.Add(stone);
                                    }
                                }

                                selecter.MoveStones(Direction.Up);
                            }
                            else if (moves[moveIndex].getDirection() == 'r')
                            {
                                Tile activeTile = GenBoard.instance.board[(moves[moveIndex].getOriginX() + progress, moves[moveIndex].getOriginY() )];
                                //Tile goalTile = GenBoard.instance.board[(moves[moveIndex].getOriginX() + i, moves[moveIndex].getOriginY())];

                                selecter.transform.position = activeTile.transform.position + selecter.offset;
                                selecter.selectedTile = activeTile;

                                foreach (Stone stone in selecter.selectedTile.stonesOnTile)
                                {
                                    if (!selecter.leaveStones.Contains(stone) && !selecter.movingStones.Contains(stone))
                                    {
                                        selecter.movingStones.Add(stone);
                                    }
                                }

                                selecter.MoveStones(Direction.Right);
                            }
                            else if (moves[moveIndex].getDirection() == 'd')
                            {
                                Tile activeTile = GenBoard.instance.board[(moves[moveIndex].getOriginX(), moves[moveIndex].getOriginY() - progress)];
                                //Tile goalTile = GenBoard.instance.board[(moves[moveIndex].getOriginX(), moves[moveIndex].getOriginY() - i)];

                                selecter.transform.position = activeTile.transform.position + selecter.offset;
                                selecter.selectedTile = activeTile;

                                foreach (Stone stone in selecter.selectedTile.stonesOnTile)
                                {
                                    if (!selecter.leaveStones.Contains(stone) && !selecter.movingStones.Contains(stone))
                                    {
                                        selecter.movingStones.Add(stone);
                                    }
                                }

                                selecter.MoveStones(Direction.Down);
                            }
                            else if (moves[moveIndex].getDirection() == 'l')
                            {
                                Tile activeTile = GenBoard.instance.board[(moves[moveIndex].getOriginX() - progress, moves[moveIndex].getOriginY())];
                                //Tile goalTile = GenBoard.instance.board[(moves[moveIndex].getOriginX() - i, moves[moveIndex].getOriginY())];

                                selecter.transform.position = activeTile.transform.position + selecter.offset;
                                selecter.selectedTile = activeTile;

                                foreach (Stone stone in selecter.selectedTile.stonesOnTile)
                                {
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
            }

            //Debug.Log(System.DateTime.Now - timeTest);
        }
    }

    private List<Moves> getMoves()
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
                        Moves moveStone = new Moves(tile.boardPosition , dir, i);
                        if(dir == 'u')
                        {
                            try
                            {
                                if (GenBoard.instance.board[(tile.boardPosition.x, tile.boardPosition.y + 1)])
                                {
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
                                    moves.Add(moveStone);
                                }
                            }
                            catch { }
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

    public void Check() 
    { 
        if(agentColor == GameController.instance.currentTurn)
            check = true; 
    }
}
