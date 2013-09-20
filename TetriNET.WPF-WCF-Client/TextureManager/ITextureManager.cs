using System.Windows.Media;
using TetriNET.Common.DataContracts;

namespace TetriNET.WPF_WCF_Client.TextureManager
{
    public interface ITextureManager
    {
        Brush GetBigPiece(Pieces piece);
        Brush GetSmallPiece(Pieces piece);
        Brush GetBigSpecial(Specials special);
        Brush GetSmallSpecial(Specials special);
        Brush GetBigBackground();
        Brush GetSmallBackground();
    }
}
