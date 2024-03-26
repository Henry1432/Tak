using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strategy
{
    public static float agression; //a value between 0 and 1 to show whos on the defensive and whos on the offensive. used when compairing board states to decide what is actually better
                                   //if there is a move that lowers control but has a higher score the agression decides if its worth the risk


    public static float MiniMax(Board current, int depth, float alpha, float beta, bool maximizing)
    {
        //this will be the impementation of the minimax with alpha beta pruning. I feel that this is a better fit for my use case then MCTS
            //I do need to consider weather I am wrong about this and that is how I spent the majority of todays work session.

        if(depth == 0 || current.win != TileColor.None)
        {
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

            if(current.root == current)
            {
                //return for move format
            }

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

            if (current.root == current)
            {
                //return for move format
            }
            return minEval;
        }
    }
    public static float Score(Board board)
    {
        //use the saved information of the board to generate a numarical value that represents the value of the board

        return -1f;
    }

    public static float Compair(Board board1, Board board2)
    {
        //this is for compairing 2 boards. sometimes obvous, like if the scores are like "Do you want to give the enemy the victory or naw?" but other times the different board states are super similar and using the agression system is needed. 

        return -1;
    }
}
