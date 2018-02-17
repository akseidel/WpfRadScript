using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace WpfRadScript
{
    public class PCombComboFiltersList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private    string pcombfilter = string.Empty;
        private ObservableCollection<string> pcombfilters = new ObservableCollection<string>();
       
        public PCombComboFiltersList()
        {
            
        }

        public ObservableCollection<string> Pcombfilters {
            get { return pcombfilters; }
            set {
                Pcombfilters = value;
                OnPropertyChanged("Pcombfilters");
            }
        }
        
        public string Pcombfilter {
            get { return pcombfilter; }
            set {
                if (pcombfilter != value)
                {
                    pcombfilter = value;
                    OnPropertyChanged("Pcombfilter");

                    if (!pcombfilters.Contains(value))
                    {
                        pcombfilters.Add(value);
                        pcombfilters = new ObservableCollection<string>(pcombfilters.OrderBy(i => i));
                        OnPropertyChanged("Pcombfilters");
                    }
                }
            }
        }
        
        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

    }
}
