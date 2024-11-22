using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static Unity.Collections.AllocatorManager;

public class MCTSStrategy
{
    static Board current = new Board();
    public static float aggression = 0.5f; //a value between 0 and 1 to show whos on the defensive and whos on the offensive. used when compairing board states to decide what is actually better
                                   //if there is a move that lowers control but has a higher score the agression decides if its worth the risk
    public const float ADVANTAGE = 10f;
    public const int DEPTH = 2;
    public static /*Moves*/ void GetNextMove(Agent agent) // void is temp
    {
        //look back at chess to relearn
        return;
    }
    //check if we can make this smaller
    public static float Score(Board board, bool maximizing)
    {
        board.quantifyBoard();

        float score = 0;
        bool boost = true;
        if(maximizing)
            if(board.advantage == TileColor.White)
            {
                aggression *= 1.25f;
                Mathf.Clamp(aggression, 0.1f, 0.9f);
                if (current.proximity > board.proximity)
                    boost = true;
                else
                    boost = false;
            }
            else if(board.advantage == TileColor.Black)
            {
                aggression *= 0.75f;
                Mathf.Clamp(aggression, 0.1f, 0.9f);
                if (current.proximity < board.proximity)
                    boost = true;
                else
                    boost = false;
            }
        else
            if (board.advantage == TileColor.White)
            {
                aggression *= 0.75f;
                Mathf.Clamp(aggression, 0.1f, 0.9f);
                if (current.proximity < board.proximity)
                    boost = true;
                else
                    boost = false;
            }
            else if (board.advantage == TileColor.Black)
            {
                aggression *= 1.25f;
                Mathf.Clamp(aggression, 0.1f, 0.9f);
                if (current.proximity > board.proximity)
                    boost = true;
                else
                    boost = false;
            }

        if (board.win == TileColor.White)
        {
            score = 100;
        }
        else if(board.win == TileColor.Black)
        {
            score = -100;
        }
        else
        {
            //aggression = 0.5f;
            float position = (board.coverage * aggression) + (board.totalControl * (1 - aggression));

            score = (GenBoard.getSize() - board.proximity) + 5f;
            score += boost ? 3 : -3;
            if (board.advantage != TileColor.None)
                score *= board.advantage == TileColor.White ? 1 : -1;
            else
            {
                if (position == 0.5) 
                {
                    score = 0;
                }
                else
                {
                    score *= position > 0.5f ? 1 : -1;
                }
            }

            score *= 1 + position;

            List<BoardTile> neighbors = new List<BoardTile>();
            foreach(KeyValuePair<(int, int), BoardTile> tile in board.board)
            {
                neighbors.Clear();
                if(tile.Key.Item1 > 0)
                {
                    neighbors.Add(board.board[(tile.Key.Item1 - 1, tile.Key.Item2)]);
                }
                if (tile.Key.Item1 < GenBoard.getSize()-1)
                {
                    neighbors.Add(board.board[(tile.Key.Item1 + 1, tile.Key.Item2)]);
                }
                if (tile.Key.Item2 > 0)
                {
                    neighbors.Add(board.board[(tile.Key.Item1, tile.Key.Item2 - 1)]);
                }
                if (tile.Key.Item2 < GenBoard.getSize() - 1)
                {
                    neighbors.Add(board.board[(tile.Key.Item1, tile.Key.Item2 + 1)]);
                }

                foreach(BoardTile n in neighbors)
                {
                    if(maximizing)
                    {
                        if(tile.Value.owner == TileColor.White)
                        {
                            if(n.owner == TileColor.White)
                            {
                                score *= 1.2f;
                                if(n.road)
                                {
                                    score *= 1.2f;
                                }
                            }
                            else if(n.owner == TileColor.Black)
                            {
                                if(n.stonesOnTile.Last().wall)
                                {
                                    score /= 1.2f;
                                }
                                if (n.road)
                                {
                                    score *= 1.1f;
                                    if(n.dir == Direction.Up)
                                    {
                                        if (n.boardPosition.y < tile.Key.Item2)
                                        {
                                            score *= 1.5f;
                                        }
                                    }
                                    else if (n.dir == Direction.Down)
                                    {
                                        if (n.boardPosition.y > tile.Key.Item2)
                                        {
                                            score *= 1.5f;
                                        }
                                    }
                                    else if( n.dir == Direction.Left)
                                    {
                                        if (n.boardPosition.x < tile.Key.Item1)
                                        {
                                            score *= 1.5f;
                                        }
                                    }
                                    else if(n.dir == Direction.Right)
                                    {
                                        if (n.boardPosition.x > tile.Key.Item1)
                                        {
                                            score *= 1.5f;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (tile.Value.owner == TileColor.Black)
                        {
                            if (n.owner == TileColor.Black)
                            {
                                score *= 1.2f;
                                if (n.road)
                                {
                                    score *= 1.2f;
                                }
                            }
                            else if (n.owner == TileColor.White)
                            {
                                if (n.stonesOnTile.Last().wall)
                                {
                                    score /= 1.2f;
                                }
                                if (n.road)
                                {
                                    score *= 1.1f;
                                    if (n.dir == Direction.Up)
                                    {
                                        Debug.Log(n.boardPosition + ", u");
                                        if (n.boardPosition.y < tile.Value.boardPosition.y)
                                        {
                                            score *= 1.5f;
                                        }
                                    }
                                    else if (n.dir == Direction.Down)
                                    {
                                        if (n.boardPosition.y > tile.Value.boardPosition.y)
                                        {
                                            score *= 1.5f;
                                        }
                                    }
                                    else if (n.dir == Direction.Left)
                                    {
                                        Debug.Log(n.boardPosition);
                                        if (n.boardPosition.x < tile.Value.boardPosition.x)
                                        {
                                            score *= 1.5f;
                                        }
                                    }
                                    else if (n.dir == Direction.Right)
                                    {
                                        if (n.boardPosition.x > tile.Value.boardPosition.x)
                                        {
                                            score *= 1.5f;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return score;
    }
}
