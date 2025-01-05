using Avalonia.Media;

namespace Tetris.ViewModels
{
    public class BlockCell : ViewModelBase
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public double TopPosition { get; set; }
        public double LeftPosition { get; set; }


        private IImage _source;

        public IImage ImageSource
        {
            get => _source;
            set
            {
                _source = value;
                OnPropertyChanged();
            }
        }

        public BlockCell(IImage source)
        {
            _source = source;
        }

    }
}
