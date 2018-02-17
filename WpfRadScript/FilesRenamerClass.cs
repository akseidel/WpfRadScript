using System.Collections.Generic;
using System.ComponentModel;

namespace WpfRadScript
{
    public class FilesRenamerClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private string pathPart;
        private List<RenamerFileName> theFiles;

        public string PathPart {
            get { return pathPart; }
            set { pathPart = value; OnPropertyChanged("PathPart"); }
        }
        public List<RenamerFileName> TheFiles {
            get { return theFiles; }
            set { theFiles = value; OnPropertyChanged("TheFiles"); }
        }
    }

    public class RenamerFileName : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        private string present_Name;
        private string changed_Name;
        public string Present_Name {
            get { return present_Name; }
            set { present_Name = value; OnPropertyChanged("Present_Name"); }
        }
        public string Changed_Name {
            get { return changed_Name; }
            set { changed_Name = value; OnPropertyChanged("Changed_Name"); }
        }
    }
}
