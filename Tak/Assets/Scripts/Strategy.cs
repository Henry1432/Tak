using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strategy
{
    public static float agression; //a value between 0 and 1 to show whos on the defensive and whos on the offensive. used when compairing board states to decide what is actually better
                                   //if there is a move that lowers control but has a higher score the agression decides if its worth the risk


    public static void MiniMax(Board current, int depth, float alpha, float beta, bool maximizing)
    {
        //this will be the impementation of the minimax with alpha beta pruning. I feel that this is a better fit for my use case then MCTS
            //I do need to consider weather I am wrong about this and that is how I spent the majority of todays work session.
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
