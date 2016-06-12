using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SearchAndMove.ViewModels
{
    class OptionsWindowViewModel : ViewModel
    {
        public bool RefreshRecentLists { get; private set; }

        private readonly Properties.Settings Settings = Properties.Settings.Default;

        public OptionsWindowViewModel()
        {
            RefreshRecentLists = false;
            OpenDestination = Settings.OpenDestinationFolder;
        }

        public ICommand ClearRecentQueries
        {
            get
            {
                return new CommandHelper(() =>
                {
                    Settings.RecentQueries = null;
                    Settings.Save();

                    RefreshRecentLists = true;
                    ShowMessage("Queries");
                });
            }
        }

        private void ShowMessage(string list)
        {
            MessageBoxFactory.ShowInfo(list + " were cleared.", "Recent History Cleared");
        }

        public ICommand ClearRecentDestinations
        {
            get
            {
                return new CommandHelper(() =>
                {
                    Settings.RecentPaths = null;
                    Settings.Save();

                    RefreshRecentLists = true;
                    ShowMessage("Destinations");
                });
            }
        }

        private bool openDestination;
        public bool OpenDestination
        {
            get
            {
                return openDestination;
            }
            set
            {
                openDestination = value;
                Settings.OpenDestinationFolder = value;
                RaisePropertyChanged();
            }
        }


    }
}
