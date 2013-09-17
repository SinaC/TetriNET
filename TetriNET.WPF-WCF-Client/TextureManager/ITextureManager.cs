using System.Windows.Media;
using TetriNET.Common.GameDatas;

namespace TetriNET.WPF_WCF_Client.TextureManager
{
    public interface ITextureManager
    {
        Brush GetBigTetrimino(Tetriminos tetrimino);
        Brush GetSmallTetrimino(Tetriminos tetrimino);
        Brush GetBigSpecial(Specials special);
        Brush GetSmallSpecial(Specials special);
        Brush GetBigBackground();
        Brush GetSmallBackground();
    }
}
