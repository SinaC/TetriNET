using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Board
{
    public class BoardWithWallKick : Board
    {
        public BoardWithWallKick(int width, int height) : base(width, height)
        {
        }

        //http://tetris.wikia.com/wiki/Wall_kick
        public override bool RotateClockwise(IPiece piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to rotate
            IPiece tempPiece = piece.Clone();
            tempPiece.RotateClockwise();
            if (!CheckNoConflict(tempPiece))
            {
                // Try to move right then rotate
                tempPiece.CopyFrom(piece);
                tempPiece.Translate(1, 0);
                tempPiece.RotateClockwise();
                if (!CheckNoConflict(tempPiece))
                {
                    // Try to move left then rotate
                    tempPiece.CopyFrom(piece);
                    tempPiece.Translate(-1, 0);
                    tempPiece.RotateClockwise();
                    if (!CheckNoConflict(tempPiece))
                        return false;
                    else
                        piece.Translate(-1, 0);
                }
                else
                    piece.Translate(1, 0);
            }
            // Perform rotation (wall kick translation has been done before if needed)
            piece.RotateClockwise();
            return true;
        }

        //http://tetris.wikia.com/wiki/Wall_kick
        public override bool RotateCounterClockwise(IPiece piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to rotate
            IPiece tempPiece = piece.Clone();
            tempPiece.RotateCounterClockwise();
            if (!CheckNoConflict(tempPiece))
            {
                // Try to move right then rotate
                tempPiece.CopyFrom(piece);
                tempPiece.Translate(1, 0);
                tempPiece.RotateCounterClockwise();
                if (!CheckNoConflict(tempPiece))
                {
                    // Try to move left then rotate
                    tempPiece.CopyFrom(piece);
                    tempPiece.Translate(-1, 0);
                    tempPiece.RotateCounterClockwise();
                    if (!CheckNoConflict(tempPiece))
                        return false;
                    else
                        piece.Translate(-1, 0);
                }
                else
                    piece.Translate(1, 0);
            }
            // Perform rotation (wall kick translation has been done before if needed)
            piece.RotateCounterClockwise();
            return true;
        }
    }
}
