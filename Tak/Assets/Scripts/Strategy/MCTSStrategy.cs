using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using System.Linq;

using static Unity.Collections.AllocatorManager;
using Unity.Collections;
using static MCTSStrategy;
using System;



public class MCTSStrategy
{
    public struct MCTSNode
    {
        public Board nodeBoard;
        public int parentIndex;
        public float potential;
        public Moves rootMove;
        public TileColor moveColor;
        public HashSet<Moves> openMoves;
        public HashSet<Moves> closeMoves;

        public MCTSNode(Board rootBoard, Moves move, TileColor turnColor, int parentIndex, float potential)
        {
            this.nodeBoard = Board.getNewBoard(rootBoard, move);
            this.rootMove = move;
            this.parentIndex = parentIndex;
            this.potential = potential;
            this.moveColor = turnColor;

            openMoves = new HashSet<Moves>();
            closeMoves = new HashSet<Moves>();
            openMoves.AddRange(Agent.getMoves(nodeBoard, turnColor).ToArray());
        }

        public MCTSNode(MCTSNode rootNode, Moves move, int parentIndex, float potential)
            : this(rootNode.nodeBoard, move, (rootNode.moveColor == TileColor.White ? TileColor.Black : TileColor.White), parentIndex, potential) { }
        public MCTSNode(Board rootBoard, TileColor turnColor, float potential)
        {
            this.nodeBoard = rootBoard;
            this.parentIndex = -1;
            this.potential = potential;
            this.rootMove = null;
            this.moveColor = turnColor;

            openMoves = new HashSet<Moves>();
            closeMoves = new HashSet<Moves>();
            openMoves.AddRange(Agent.getMoves(nodeBoard, turnColor).ToArray());

        }

        public float getUCB(ref List<MCTSNode> nodes)
        {
            if (closeMoves.Count > 0 && parentIndex != -1)
                return potential + 1 * Mathf.Sqrt(Mathf.Log(nodes[parentIndex].closeMoves.Count / closeMoves.Count));
            else
                return float.MaxValue;
        }
    }
    static Board current = new Board();
    public static float aggression = 0.5f; //a value between 0 and 1 to show whos on the defensive and whos on the offensive. used when compairing board states to decide what is actually better
    private float currentTime = 0f;                                       //if there is a move that lowers control but has a higher score the agression decides if its worth the risk
    public const float ADVANTAGE = 10f;
    public const int DEPTH = 2;

    private static float startTime = -1;
    private static List<MCTSNode> nodes = new List<MCTSNode>();



    public static IEnumerator GetNextMove(Agent agent, float processingTime = 10f, Action<Moves> callback = null)
    {
        Board.getCurrentBoard(out current);

        if (startTime == -1)
        {
            startTime = Time.time;
            nodes.Clear();
            current.quantifyBoard();
            Debug.Log(current.neighborGroups.Count);
        }
        //Debug.Log("start:" + startTime);
        while (Time.time - startTime < processingTime)
        {
            Selection(current, nodes, agent.agentColor);
            //Debug.Log("running" + (Time.time - startTime) + "...");
            yield return null;
        }
        //Debug.Log("end:" + Time.time);

        MCTSNode pickNode = nodes[1];


        for (int i = 2; i < nodes.Count; i++)
        {
            //find the best node
            if(nodes[i].parentIndex != -1)
            {
                if (pickNode.potential < nodes[i].potential && nodes[nodes[i].parentIndex].parentIndex == -1)
                {
                    pickNode = nodes[i];
                }
            }
        }
        while (nodes[pickNode.parentIndex].parentIndex != -1)
        {
            if (pickNode.parentIndex < nodes.Count)
            {
                pickNode = nodes[pickNode.parentIndex];
            }
        }

        startTime = -1;
        callback?.Invoke(pickNode.rootMove);
    }

    //given the mcts trees and the root node select the node to explore
    private static void Selection(Board start, List<MCTSNode> nodes, TileColor agentColor)
    {
        if (nodes.Count == 0)
        {
            MCTSNode tempNode = new MCTSNode(start, agentColor, 0);
            nodes.Add(tempNode);
        }
        else
        {
            List<MCTSNode> bestNode = new List<MCTSNode>();
            float UCB = 0;
            for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
            {
                float tempUCB = nodes[nodeIndex].getUCB(ref nodes);

                if (bestNode.Count == 0)
                {
                    bestNode.Add(nodes[nodeIndex]);
                    UCB = tempUCB;
                }
                else if (nodes[nodeIndex].openMoves.Count > 0)
                {
                    if (tempUCB == UCB)
                    {
                        bestNode.Add(nodes[nodeIndex]);
                    }
                    else
                    {
                        if (UCB < tempUCB)
                        {
                            bestNode.Clear();
                            bestNode.Add(nodes[nodeIndex]);
                            UCB = tempUCB;
                        }
                    }
                }
            }
            System.Random rand = new System.Random();
            int randomIndex = rand.Next(bestNode.Count);

            if (randomIndex < nodes.Count)
            {
                Expansion(randomIndex, nodes, agentColor);
            }
        }
    }

    //actually taking the board then making the move
    private static void Expansion(int nodeIndex, List<MCTSNode> nodes, TileColor agentColor)
    {
        if (nodeIndex >= 0)
        {
            if (nodes[nodeIndex].openMoves.Count > 0)
            {
                //std::random_device rd; //commented for open tasting
                Moves move;

                System.Random rand = new System.Random();
                int randomIndex = rand.Next(nodes[nodeIndex].openMoves.Count);
                move = nodes[nodeIndex].openMoves.ElementAt(randomIndex);
                MCTSNode tempNode = new MCTSNode(nodes[nodeIndex], move, nodeIndex, 0);

                float potential = Score(tempNode.nodeBoard, tempNode.moveColor, agentColor); //change how score works, inprove huristic to be faster and better
                //Simulation(tempBoard, tempBoard.sideToMove());

                tempNode.potential += potential;

                MCTSNode backPropNode = nodes[nodeIndex];
                while (backPropNode.parentIndex != -1)
                {
                    backPropNode.potential += potential;
                    backPropNode = nodes[backPropNode.parentIndex];
                }

                nodes[nodeIndex].closeMoves.Add(move);
                nodes[nodeIndex].openMoves.Remove(move);
                tempNode.parentIndex = nodeIndex;
                nodes.Add(tempNode);
            }
        }
    }

    public static float Score(Board board, TileColor moveColor, TileColor agentColor)
    {
        current.quantifyBoard();
        board.quantifyBoard();

        board.winState(out int winDist, out int whitePathCount, out int blackPathCount, out TileColor winning, out bool hasWinner, out List<Vector2> wallPoints);

        float score = (((int)GenBoard.getSize() - 1) - winDist);
        score += board.CalculateGroupScore();



        if (winning == TileColor.None)
        {
        }
        else if (winning == agentColor)
        {
            score += (((int)GenBoard.getSize() - 1) - winDist) * 2;
            aggression *= 1.25f;
        }
        else
        {
            aggression *= 0.75f;
            score -= (((int)GenBoard.getSize() - 1) - winDist);
        }

        if(board.saveMove.isPlaceStone())
        {
            if(wallPoints.Contains(board.saveMove.getOrigin()))
            {
                if(board.saveMove.isWall())
                {
                    score += 8;
                }
                else
                {
                    score += 4;
                }
            }
        }

        
        if(hasWinner && winning == agentColor)
        {
            score += 100000;
        }
        else if(hasWinner && winning != agentColor)
        {
            score -= 100000;
        }
        Math.Clamp(aggression, 0.2f, 1f);


        //float position = ((board.coverage * aggression) + (board.totalControl * (1 - aggression))) * 10;
        //if(moveColor == agentColor)
        //{
        //    score += position;
        //}
        //else
        //{
        //    score -= position;
        //}


        return score;
    }

   


    //this is currently not working as this was not from the right algorithm, recheck chess to make sure it is still minimizing on black, etc, then re implement here
    //obvously I there arent chess pieces to easily quantify, i have a feeling it will be similar but not the same
    public static float Score(Board board, bool maximizing)
    {
        current.quantifyBoard();
        board.quantifyBoard();

        float score = 0;
        bool boost = true;
        if(maximizing)
        {
            if(board.advantage == TileColor.White)
            {
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
        }
        else
        {

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
