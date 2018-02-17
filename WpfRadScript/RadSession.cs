
using System.ComponentModel;

namespace WpfRadScript
{
    public class RadSession : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        private string prefsPath;
        private string projectPath;
        private string batchFileName;
        private string pcombbatchFileName;
        private string radPath;
        private string skyFilePath;
        private string octPath;
        private string illumPath;
        private string illumFileName;
        private string mkillumOptsFileName;
        private string overtureOptsFileName;
        private string ambPath;
        private string imagePath;
        private string pcombimagesourcePath;
        private string pcombimagetargetPath;
        private string ratiffimagePath;
        private string finalimagePath;
        private string finalimageType;
        private string viewPath;
        private string renderOptsFileName;
        private string renderOptsPath;
        private string matPath;
        private string bScnPath;
        private string vScnPath;
        private bool incRpictI;
        private string skyFileName;
        private string skySwitch;
        private int sHour;
        private int eHour;
        private double hrInc;
        private int mStart;
        private int mEnd;
        private double mInc;
        private int simDay;
        private bool nightTime;
        private bool setRadEnv;
        private string irradPath;
        private string statusMsg;
        private int imgX;
        private int imgY;
        private int ovImgY;
        private int ovImgX;
        private bool useOverture;
        private double[] scales = new double[7];
        private string hdrviewer;
        private bool pcombh;
        private bool pcombpause;
        private bool ratiffpause;
        private bool getinfofull;
        private string editorPath;
        private string magickPath;
        private string ghostScriptPath;

        public string GhostScriptPath { get { return ghostScriptPath; } set { ghostScriptPath = value; OnPropertyChanged("GhostScriptPath"); } }
        public string MagickPath { get { return magickPath; } set { magickPath = value; OnPropertyChanged("MagickPath"); } }
        public string FinalimageType { get { return finalimageType; } set { finalimageType = value; OnPropertyChanged("FinalimageType"); } }
        public string FinalimagePath { get { return finalimagePath; } set { finalimagePath = value; OnPropertyChanged("FinalimagePath"); } }
        public bool Getinfofull { get { return getinfofull; } set { getinfofull = value; OnPropertyChanged("Getinfofull"); } }
        public string EditorPath { get { return editorPath; } set { editorPath = value; OnPropertyChanged("EditorPath"); } }
        public string PrefsPath { get { return prefsPath; } set { prefsPath = value; OnPropertyChanged("PrefsPath"); } }
        public bool Ratiffpause { get { return ratiffpause; } set { ratiffpause = value; OnPropertyChanged("Ratiffpause"); } }
        public bool Pcombpause { get { return pcombpause; } set { pcombpause = value; OnPropertyChanged("Pcombpause"); } }
        public bool Pcombh { get { return pcombh; } set { pcombh = value; OnPropertyChanged("Pcombh"); } }
        public string Hdrviewer { get { return hdrviewer; } set { hdrviewer = value; OnPropertyChanged("Hdrviewer"); } }
        public double[] Scales { get { return scales; } set { scales = value; OnPropertyChanged("Scales"); } }
        public string ImagePath { get { return imagePath; } set { imagePath = value; OnPropertyChanged("ImagePath"); } }
        public string PcombimagesourcePath { get { return pcombimagesourcePath; } set { pcombimagesourcePath = value; OnPropertyChanged("PcombimagesourcePath"); } }
        public string PcombimagetargetPath { get { return pcombimagetargetPath; } set { pcombimagetargetPath = value; OnPropertyChanged("PcombimagetargetPath"); } }
        public string RatiffimagePath { get { return ratiffimagePath; } set { ratiffimagePath = value; OnPropertyChanged("RatiffimagePath"); } }
        public string AmbPath { get { return ambPath; } set { ambPath = value; OnPropertyChanged("AmbPath"); } }
        public string OvertureOptsFileName { get { return overtureOptsFileName; } set { overtureOptsFileName = value; OnPropertyChanged("OvertureOptsFileName"); } }
        public string BatchFileName { get { return batchFileName; } set { batchFileName = value; OnPropertyChanged("BatchFileName"); } }
        public string PcombBatchFileName { get { return pcombbatchFileName; } set { pcombbatchFileName = value; OnPropertyChanged("PcombBatchFileName"); } }
        public string RadPath { get { return radPath; } set { radPath = value; OnPropertyChanged("RadPath"); } }
        public string ProjectPath { get { return projectPath; } set { projectPath = value; OnPropertyChanged("ProjectPath"); } }
        public string SkyFilePath { get { return skyFilePath; } set { skyFilePath = value; OnPropertyChanged("SkyFilePath"); } }
        public string OctPath { get { return octPath; } set { octPath = value; OnPropertyChanged("OctPath"); } }
        public string IllumPath { get { return illumPath; } set { illumPath = value; OnPropertyChanged("IllumPath"); } }
        public string IllumFileName { get { return illumFileName; } set { illumFileName = value; OnPropertyChanged("IllumFileName"); } }
        public string MkillumOptsFileName { get { return mkillumOptsFileName; } set { mkillumOptsFileName = value; OnPropertyChanged("MkillumOptsFileName"); } }
        public string ViewPath { get { return viewPath; } set { viewPath = value; OnPropertyChanged("ViewPath"); } }
        public string RenderOptsFileName { get { return renderOptsFileName; } set { renderOptsFileName = value; OnPropertyChanged("RenderOptsFileName"); } }
        public string RenderOptsPath { get { return renderOptsPath; } set { renderOptsPath = value; OnPropertyChanged("RenderOptsFileName"); } }
        public string MatPath { get { return matPath; } set { matPath = value; OnPropertyChanged("MatPath"); } }
        public string BScnPath { get { return bScnPath; } set { bScnPath = value; OnPropertyChanged("BScnPath"); } }
        public string VScnPath { get { return vScnPath; } set { vScnPath = value; OnPropertyChanged("VScnPath"); } }
        public bool IncRpictI { get { return incRpictI; } set { incRpictI = value; OnPropertyChanged("IncRpictI"); } }
        public string SkyFileName { get { return skyFileName; } set { skyFileName = value; OnPropertyChanged("SkyFileName"); } }
        public string SkySwitch { get { return skySwitch; } set { skySwitch = value; OnPropertyChanged("SkySwitch"); } }
        
        public int SHour {
            get { return sHour; }
            set { sHour = value; OnPropertyChanged("SHour"); }
        }
        public int EHour {
            get { return eHour; }
            set { eHour = value; OnPropertyChanged("EHour"); }
        }
        public double HrInc {
            get { return hrInc; }
            set {
                hrInc = value;
                OnPropertyChanged("HrInc");
            }
        }
        public int MStart {
            get { return mStart; }
            set { mStart = value; OnPropertyChanged("MStart"); }
        }
        public int MEnd {
            get { return mEnd; }
            set { mEnd = value; OnPropertyChanged("MEnd"); }
        }
        public double MInc {
            get { return mInc; }
            set { mInc = value; OnPropertyChanged("MInc"); }
        }
        public int SimDay { get { return simDay; } set { simDay = value; OnPropertyChanged("SimDay"); } }
        public bool NightTime { get { return nightTime; } set { nightTime = value; OnPropertyChanged("NightTime"); } }
        public bool SetRadEnv { get { return setRadEnv; } set { setRadEnv = value; OnPropertyChanged("SetRadEnv"); } }
        public string IrradPath { get { return irradPath; } set { irradPath = value; OnPropertyChanged("IrradPath"); } }
        public string StatusMsg { get { return statusMsg; } set { statusMsg = value; OnPropertyChanged("StatusMsg"); } }
        public int ImgX { get { return imgX; } set { imgX = value; OnPropertyChanged("ImgX"); } }
        public int ImgY { get { return imgY; } set { imgY = value; OnPropertyChanged("ImgY"); } }
        public int OvImgY { get { return ovImgY; } set { ovImgY = value; OnPropertyChanged("OvImgX"); } }
        public int OvImgX { get { return ovImgX; } set { ovImgX = value; OnPropertyChanged("OvImgX"); } }
        public bool UseOverture { get { return useOverture; } set { useOverture = value; OnPropertyChanged("UseOverture"); } }

        //private string xxx;
        //public string XXX { get { return xxx; } set { xxx = value; } }
    }

}

