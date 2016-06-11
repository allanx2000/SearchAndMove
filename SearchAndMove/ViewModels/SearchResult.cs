using Innouvous.Utils.MVVM;

namespace SearchAndMove.ViewModels
{
    public class SearchResult : ViewModel
    {
        private bool selected = false;
        public bool Selected
        {
            get { return HasResult && selected; }
            set
            {
                selected = value;
                RaisePropertyChanged();
            }
        }

        public string Path { get; private set; }

        public bool HasResult { get; private set; }

        public SearchResult(string path, bool isResult)
        {
            Path = path;
            HasResult = isResult;
        }
        
    }
}