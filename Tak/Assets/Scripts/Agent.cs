using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public List<Moves> moves = new List<Moves>();
    public TileColor agentColor = TileColor.None;
    [SerializeField] private Stone Capstone;
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
    }

    private void Update()
    {
        if(check)
        {
            moves.Clear();

            System.DateTime timeTest = System.DateTime.Now;

            moves = getMoves();
            check = false;

            foreach(Moves move in moves)
            {
                Debug.Log(move.isPlaceStone());
            }

            Debug.Log(System.DateTime.Now - timeTest);
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
                        Moves moveStone = new Moves(tile.boardPosition, dir, i);
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
}
