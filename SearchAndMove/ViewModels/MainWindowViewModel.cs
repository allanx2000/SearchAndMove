using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Specialized;

namespace SearchAndMove.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        #region Recents

        private ObservableCollection<string> recentQueries;
        private ObservableCollection<string> recentDestinations;

        public ObservableCollection<string> RecentQueries { get { return recentQueries; } }
        public ObservableCollection<string> RecentDestinations { get { return recentQueries; } }

        private readonly Properties.Settings Settings = Properties.Settings.Default;


        private void LoadRecents()
        {        
            recentQueries = new ObservableCollection<string>();
            LoadList(recentQueries, Settings.RecentQueries);
            
            recentDestinations = new ObservableCollection<string>();
            LoadList(recentDestinations, Settings.RecentPaths);
        }

        private void SaveRecentDestinations()
        {
            Settings.RecentPaths = ToStringCollection(RecentDestinations);
            Settings.Save();
        }

        private void SaveRecentQueries()
        {
            Settings.RecentQueries = ToStringCollection(recentQueries);
            Settings.Save();
        }

        private StringCollection ToStringCollection(IList<string> list)
        {
            StringCollection collection = new StringCollection();

            foreach (var i in list)
                collection.Add(i);

            return collection;
        }

        private void LoadList(ObservableCollection<string> output, StringCollection input)
        {
            if (input != null)
            {
                foreach (var s in input)
                {
                    output.Add(s);
                }
            }
        }

        #endregion

        #region Search Paths
        private ObservableCollection<string> paths = new ObservableCollection<string>();
        public ObservableCollection<string> Paths
        {
            get
            {
                return paths;
            }
        }

        private string selectedPath;
        public string SelectedPath
        {
            get { return selectedPath; }
            set
            {
                selectedPath = value;
            }
        }

        public ICommand AddPathCommand
        {
            get
            {
                return new CommandHelper(AddPath);
            }
        }

        private void AddPath()
        {
            var sfd = DialogsUtility.CreateFolderBrowser();
            sfd.ShowDialog();

            if (string.IsNullOrEmpty(sfd.SelectedPath))
                return;

            AddPath(sfd.SelectedPath);
        }

        public void AddPath(string path)
        {
            if (Paths.Contains(path))
                return;

            Paths.Add(path);
        }

        public ICommand RemovePathCommand
        {
            get { return new CommandHelper(RemovePath); }
        }

        private void RemovePath()
        {
            if (SelectedPath == null)
                return;

            Paths.Remove(SelectedPath);

            ClearResults();
        }

        public ICommand ClearPathsCommand
        {
            get { return new CommandHelper(ClearPaths); }
        }

        private void ClearPaths()
        {
            Paths.Clear();
            ClearResults();
        }

        #endregion

        #region Query

        private string query;
        public string Query
        {
            get { return query; }
            set
            {
                query = value;
                RaisePropertyChanged("Query");
            }
        }

        private bool recursive = true;
        public bool IsRecursive
        {
            get { return recursive; }
            set
            {
                recursive = value;
                RaisePropertyChanged("IsRecursive");
            }
        }

        public ICommand SearchCommand
        {
            get { return new CommandHelper(Search); }
        }

        private string lastQuery;
 
        private void Search()
        {
            try
            {
                Results.Clear();

                foreach (string path in Paths)
                {
                    List<SearchResult> results = Search(path, IsRecursive, Query);

                    if (results.Count == 0)
                        Results.Add(new SearchResult(path, false));
                    else
                    {
                        foreach (var r in results)
                        {
                            Results.Add(r);
                        }

                        if (Results.Count > 10000)
                            throw new Exception("Too many results!");
                    }
                }

                lastQuery = Query;

                RecentListUtil.Upsert(RecentQueries, Query);
                SaveRecentQueries();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

        private List<SearchResult> Search(string path, bool isRecursive, string query)
        {
            List<SearchResult> results = new List<SearchResult>();

            string[] files = query == null ? Directory.GetFiles(path)
                : Directory.GetFiles(path, query);

            foreach (var f in files)
            {
                results.Add(new SearchResult(f, true));
            }

            if (isRecursive)
            {
                foreach (var d in Directory.GetDirectories(path))
                {
                    results.AddRange(Search(d, isRecursive, query));
                }
            }

            return results;
        }

        #endregion

        #region Results

        private ObservableCollection<SearchResult> results = new ObservableCollection<SearchResult>();
        public ObservableCollection<SearchResult> Results
        {
            get { return results; }
        }

        private void ClearResults()
        {
            Results.Clear();
        }

        public ICommand DeselectAllCommand
        {
            get
            {
                return new CommandHelper(() => ToggleSelect(false));
            }
        }

        public ICommand SelectAllCommand
        {
            get
            {
                return new CommandHelper(() => ToggleSelect(true));
            }
        }

        private void ToggleSelect(bool selected)
        {
            foreach (var i in Results)
            {
                i.Selected = selected;
            }
        }

        private string destination;
        public string Destination
        {
            get { return destination; }
            set
            {
                destination = value;
                RaisePropertyChanged();
            }
        }
        public ICommand BrowseDestinationCommand
        {
            get
            {
                return new CommandHelper(BrowseDestination);
            }
        }

        private void BrowseDestination()
        {
            var sfd = DialogsUtility.CreateFolderBrowser();
            sfd.ShowDialog();

            if (!string.IsNullOrEmpty(sfd.SelectedPath))
                Destination = sfd.SelectedPath;
        }

        public bool DoCopy = false;

        public ICommand MoveCommand
        {
            get { return new CommandHelper(Move); }
        }

        private void Move()
        {
            try
            {
                if (string.IsNullOrEmpty(Destination))
                    throw new Exception("The destination cannot be empty.");
                else if (!Directory.Exists(Destination))
                    throw new Exception("The destination does not exist.");

                var selected = Results.Where(x => x.Selected);

                int ctr = 0;

                foreach (var s in selected)
                {
                    FileInfo f = new FileInfo(s.Path);
                    string fullPath = Path.Combine(Destination, f.Name);

                    if (DoCopy)
                        f.CopyTo(fullPath);
                    else
                        f.MoveTo(fullPath);

                    ctr++;
                }

                ClearResults();

                RecentListUtil.Upsert(RecentDestinations, Destination);
                SaveRecentDestinations();

                MessageBoxFactory.ShowInfo(ctr + " files were moved to " + Destination, "Moved Successfully");
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e);
            }
        }

        #endregion
    }
}
