using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Strategy
{
    public static float agression = 0.5f; //a value between 0 and 1 to show whos on the defensive and whos on the offensive. used when compairing board states to decide what is actually better
                                   //if there is a move that lowers control but has a higher score the agression decides if its worth the risk
    public const float ADVANTAGE = 10f;
    public const int DEPTH = 3;
    public static Moves GetNextMove(Agent agent)
    {
        Board current = new Board();
        Board.getCurrentBoard(current);

        fillTree(current, DEPTH, agent.agentColor);

        float moveScore = MiniMax(current, DEPTH, Mathf.Infinity, Mathf.Infinity, agent.agentColor == TileColor.White);
        Board lostBoardBackup = current.children[0];

        foreach(Board child  in current.children)
        {
            if(agent.agentColor == TileColor.White ? child.SaveScore > lostBoardBackup.SaveScore : child.SaveScore < lostBoardBackup.SaveScore)
            {
                lostBoardBackup = child;
            }
            if(child.SaveScore == moveScore)
            {
                Debug.Log("found");
                return child.saveMove;
            }
        }

        //shouldnt run
        Debug.Log("backup");
        return lostBoardBackup.saveMove;
    }

    private static void fillTree(Board board, int depth, TileColor currentTurn)
    {
        TileColor turn = currentTurn == TileColor.White ? TileColor.Black : TileColor.White;

        if(depth > 0 && board.children.Count == 0)
        {
            fillTarget(board, currentTurn);
        }
        if(depth > 0)
        {
            foreach(Board child in board.children)
            {
                fillTree(child, depth - 1, turn);
            }
        }
    }

    private static void fillTarget(Board board, TileColor turn)
    {
        List<Moves> moves = Agent.getMoves(board, turn);

        foreach (Moves move in moves)
        {
            Board.getNewBoard(board, move);
        }
    }

    public static float MiniMax(Board current, int depth, float alpha, float beta, bool maximizing)
    {
        //this will be the impementation of the minimax with alpha beta pruning. I feel that this is a better fit for my use case then MCTS
            //I do need to consider weather I am wrong about this and that is how I spent the majority of todays work session.

        if(depth == 0 || current.win != TileColor.None)
        {
            //this is ran at the base of the tree
            return Score(current);
        }

        if(maximizing)
        {
            float maxEval = -Mathf.Infinity;
            foreach(Board child in current.children)
            {
                float eval = MiniMax(child, depth - 1, alpha, beta, false);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                if(beta <= alpha)
                {
                    break;
                }
            }

            current.SaveScore = maxEval;
            //this might need to be different at the root, not taking in depth or alpha or beta, just what player is going. it generates the tree stuff and runs the minimax and returns the chosen move
            return maxEval;
        }
        else
        {
            float minEval = Mathf.Infinity;
            foreach (Board child in current.children)
            {
                float eval = MiniMax(child, depth - 1, alpha, beta, true);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if(beta <= alpha)
                {
                    break;
                }
            }

            current.SaveScore = maxEval;
            return minEval;
        }  
    }
    public static float Score(Board board)
    {
        board.quantifyBoard();

        float score;

        if(board.advantage == TileColor.None)
        {
            score = ADVANTAGE/2f;
        }
        else if(board.advantage == TileColor.White)
        {
            score = ADVANTAGE;
        }
        else
        {
            score = 0;
        }

        agression *= 1f - (0.5f - Mathf.Clamp(board.coverage, 0.3f, 0.7f));
        Mathf.Clamp(agression, 0.1f, 1f);
        if(board.proximity < 3)
        {
            agression *= 1.3f;
        }

        score *= ((1f-(0.5f-board.totalControl)) * Mathf.Clamp((1f-(0.5f-agression)), 0.8f, 1.5f));

        return score;
    }
}
