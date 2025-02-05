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

    //the score is progressivly better the closer it is to a win, if it is doing good defence boost the score, if it is about to win, boost that score
    public static float Score(Board board, TileColor moveColor, TileColor agentColor)
    {
        current.quantifyBoard();
        board.quantifyBoard();

        board.winState(out int winDist, out int whitePathCount, out int blackPathCount, out TileColor winning, out bool hasWinner, out List<Vector2> wallPoints);

        float score = (((int)GenBoard.getSize() - 1) - winDist);
        score += board.CalculateGroupScore();

        if (board.neighborGroups.Count == 0)
        {
            score -= 10;
        }

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

        return score;
    }
}
