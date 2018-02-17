using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfRadScript
{
    // A multicast delegate (used for more than one method) for passing MainWindow
    // listbox refilling methods to FileRenamerWPF
    public delegate void Delegate_FillRenamedChoices();
   
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Currently not using this.
        #region Exposing the Window so that its parts can be used by other windows it calls.
        ///// Singleton external application class instance.
        //internal static MainWindow radMainWnd = null;
        ///// Provide access to singleton class instance.
        //public static MainWindow Instance {
        //    get { return radMainWnd; }
        //} 
        #endregion

        // Declare a Delegate which will return the position of the 
        // DragDropEventArgs and the MouseButtonEventArgs event object
        public delegate Point GetDragDropPosition(IInputElement theElement);

        private readonly BackgroundWorker bwPicToTiff = new BackgroundWorker();
        private readonly BackgroundWorker bwTiffViewer = new BackgroundWorker();
        private readonly BackgroundWorker bwMagick = new BackgroundWorker();

        int prevRowIndex = -1;
        bool inPCombReposition = false;

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);
        Brush ClrA = ColorExt.ToBrush(System.Drawing.Color.AliceBlue);
        Brush ClrB = ColorExt.ToBrush(System.Drawing.Color.Cornsilk);

        public RadSession ThisRad;
        public ObservableCollection<string> Pcombfilters = new ObservableCollection<string>();

        const string DefaultStatMsg = "Radscript Session";
        const string Sp = " ";
        const string Dot = ".";
        const string DotDot = "..";
        const string PSep = "\\";
        const string RadExt = ".rad";
        const string SSSExt = ".sss";
        const string App1WName = "Explorer";
        const string App1FName = "Explorer.exe";
        const string App2WName = "NotePad";
        const string App2FName = "Notepad.exe";

        //string ScnBase;
        //string ScnOpt1;
        //string ScnOpt2;
        //string ScnOpt3;
        string OctBase;
        //string ImgSize1;
        //ImgeSize2 is handled as a function call  is it??
        //string RadImageName;
        //string FalseColorPath;
        //string FalseColorImageName;
        //string FalseContPath;
        //string FalseContImageName;

        // path where preference datafile resides
        string PrefPath = "U:\\";
        // name of preference data file
        const string PrefData = "RadScript.dat";

        List<string> fileExtRenderOpts = new List<string> { "txt", "opt" };
        List<string> fileExtView = new List<string> { "vp" };
        List<string> fileExtMat = new List<string> { "rad" };
        List<string> fileExtScenes = new List<string> { "rad" };
        List<string> fileExtSceneSets = new List<string> { "sss" };
        List<string> fileNameExtensionPComb = new List<string> { "pic" };
        List<string> fileNameExtensionTif = new List<string> { "tif" };
        List<string> fileNameExtensionMagick = new List<string> { "png", "jpg", "pic", "bmp", "ico", "pdf" };

        string newPathToCreate = string.Empty;
        string lastCheckedPcomb = string.Empty;

        private Dictionary<string, bool> PCombFilesCheckDictionary = new Dictionary<string, bool>();

        private readonly BackgroundWorker bkFileLoadingWorker = new BackgroundWorker();

        private bool TryToShowFile = true;

        public MainWindow()
        {
            InitializeComponent();
            // Used to have a changed event to enable\disable pcomb to list button
            ((INotifyCollectionChanged)listbox_pcomb_selections.Items).CollectionChanged += PcombListBox_CollectionChanged;
        }
        
        private void PcombListBox_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (listbox_pcomb_selections == null) { return; }
            SetAddCombLineButtons();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MiscInits();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            RegenCmdLines();
        }

        private void MiscInits()
        {
            ThisRad = new RadSession();
            // ThisRadPCombFiltersList = new PCombComboFiltersList();
            DataContext = ThisRad;

            ThisRad.StatusMsg = DefaultStatMsg;
            ThisRad.BatchFileName = "vba_batch.bat";
            ThisRad.RadPath = "C:\\Program Files\\Radiance\\";
            ThisRad.ProjectPath = "R:\\16059.01_DELMAR_GARDENS\\radiance\\";
            //thisRad.ProjectPath = "C:\\Users\\aksei\\Desktop\\radiance\\";

            ThisRad.SkyFilePath = ".\\skies\\";
            ThisRad.SkyFileName = "sky.rad";
            ThisRad.SkySwitch = "+s";
            ThisRad.OctPath = ".\\octree\\";
            ThisRad.IllumPath = ".\\illums\\";
            ThisRad.IllumFileName = "illums_out.rad";
            ThisRad.MkillumOptsFileName = "mkillum_high.txt";
            ThisRad.OvertureOptsFileName = "options_overture.txt";
            ThisRad.AmbPath = ".\\ambfile\\";
            ThisRad.ImagePath = ".\\images\\";
            ThisRad.PcombimagesourcePath = ".\\images\\";
            ThisRad.PcombimagetargetPath = ".\\images\\";
            ThisRad.RatiffimagePath = ".\\images\\tiffs\\";
            ThisRad.FinalimagePath = ".\\images\\tiffs\\";
            ThisRad.FinalimageType = "png";
            ThisRad.IrradPath = ".\\images\\irradiance\\";
            ThisRad.ViewPath = ".\\views\\";
            ThisRad.RenderOptsFileName = "options_amb.txt";
            ThisRad.RenderOptsPath = ".\\optfile\\";
            ThisRad.MatPath = ".\\material\\";
            ThisRad.BScnPath = ".\\scene\\";
            ThisRad.VScnPath = ".\\scene\\";
            ThisRad.IncRpictI = false;
            ThisRad.NightTime = false;
            ThisRad.SetRadEnv = false;
            ThisRad.ImgX = 400;
            ThisRad.ImgY = 400;
            ThisRad.OvImgX = 50;
            ThisRad.OvImgY = 50;
            ThisRad.UseOverture = false;
            ThisRad.SHour = 8;
            ThisRad.EHour = 12;
            ThisRad.HrInc = 0.5;
            ThisRad.MStart = 8;
            ThisRad.MEnd = 12;
            ThisRad.MInc = 3;
            ThisRad.SimDay = 21;
            ThisRad.Scales = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
            //ThisRad.Pcombfilters = new List<string>();
            ThisRad.Pcombh = true;

            mat_arg.Text = string.Empty;
            scene_arg.Text = string.Empty;

            string pf = Properties.Settings.Default.PrefsPath;
            if (pf != null && pf != string.Empty) { PrefPath = pf; }
            ThisRad.PrefsPath = pf;

            Helpers.GetPrefs(ThisRad, Pcombfilters, PrefData);

            FillAllChoices();
            // On a vain attempt to have a fully populated checkdictionary
            AddCheckStateToDictionary(listbox_pcomb_choices, PCombFilesCheckDictionary);

            //LoadUpUIImages();

            SetNightTimeCheckEffects();

            BuildRadFoldersList();

            combo_pcomfilter.ItemsSource = Pcombfilters;

            // disable the addpcombline button
            SetAddCombLineButtons();
            SetPCombBatchButtons();

            SetUpBackgroundWorkers();

            ThisRad.MagickPath = Helpers.FindSomething("magick").Trim();

            ThisRad.GhostScriptPath = Helpers.FindGhostScript().Trim();

        }

        private void SetUpBackgroundWorkers()
        {
            bkFileLoadingWorker.DoWork += BkFileLoadingWorker_DoWork;
            bkFileLoadingWorker.RunWorkerCompleted += BkFileLoadingWorker_RunWorkerCompleted;

            bwPicToTiff.WorkerSupportsCancellation = true;
            bwPicToTiff.WorkerReportsProgress = true;
            bwPicToTiff.DoWork += new DoWorkEventHandler(BwPicToTiff_DoWork);
            bwPicToTiff.ProgressChanged += new ProgressChangedEventHandler(BwPicToTiff_ProgressChanged);
            bwPicToTiff.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BwPicToTiff_RunWorkerCompleted);

            bwTiffViewer.WorkerSupportsCancellation = true;
            bwTiffViewer.DoWork += new DoWorkEventHandler(BwTiffViewer_DoWork);
            bwTiffViewer.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BwTiffViewer_RunWorkerCompleted);


            bwMagick.WorkerSupportsCancellation = true;
            bwMagick.WorkerReportsProgress = true;
            bwMagick.DoWork += new DoWorkEventHandler(BwMagick_DoWork);
            bwMagick.ProgressChanged += new ProgressChangedEventHandler(BwMagick_ProgressChanged);
            bwMagick.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BwMagick_RunWorkerCompleted);
        }

        private void BwTiffViewer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            TiffViewArgs tva = e.Result as TiffViewArgs;
            
            string temp = string.Empty;
            if (e.Cancelled)
            {
                ThisRad.StatusMsg = "Canceled Reading!";
            }
            else if (e.Error != null)
            {
                ThisRad.StatusMsg = "Error. Details: " + (e.Error as Exception).ToString();
                Label theLabel = FindName(tva.TheTargetWPFLabelname) as Label;
                theLabel.Content = tva.TheRealName;
            }

            if (e.Result != null)
            {
                // It HAS TO BE FROZEN TO AVOID THREAD ISSUES!!
                Image theImage = FindName(tva.TheTargetWPFImagename) as Image;
                theImage.Dispatcher.BeginInvoke(new Action(() => theImage.Source = tva.ThisImageSource));
                Label theLabel = FindName(tva.TheTargetWPFLabelname) as Label;
                theLabel.Content = tva.TheRealName;
            }
        }
        
        /// <summary>
        /// Returns imagesource. It HAS TO BE FROZEN TO AVOID THREAD ISSUES
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BwTiffViewer_DoWork(object sender, DoWorkEventArgs e)
        {
            TiffViewArgs tva = e.Argument as TiffViewArgs;
            try
            {
                Uri imageUri = tva.ThisImageUri;
                BitmapSource bitmapSource = imageUri.GetBitmapImage(BitmapCacheOption.OnLoad);
                tva.ThisImageSource = bitmapSource.GetAsFrozen() as BitmapSource;
            }
            catch (Exception)  // trying to make sure a valid e.Result returns even when there is an error
            {
                tva.TheRealName = "There is something wrong with\n\n" + tva.TheRealName + "\n\nMaking a bitmap threw an error.";
            }
            e.Result = tva;
        }

        private void SetAddCombLineButtons()
        {
            but_addpcombline.IsEnabled = listbox_pcomb_selections.HasItems;
            but_pccomb.IsEnabled = listbox_pcomb_selections.HasItems;
        }

        private void SetPCombBatchButtons()
        {
            string fpn = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ImagePath, ThisRad.PcombBatchFileName);
            bool fpnexists = File.Exists(@fpn);
            but_viewpcombscript.IsEnabled = fpnexists;
            but_executepcombscript.IsEnabled = fpnexists;
            but_makepcombscript.IsEnabled = false;
            if (tbox_pcombscriptname.Text.Trim() == String.Empty)
            {
                return;
            }
            but_makepcombscript.IsEnabled = listbox_pcomblines.HasItems;
        }

        private void Bt_regen_MouseUp(object sender, MouseButtonEventArgs e)
        {
            FillAllChoices();
        }

        private void FillAllChoices()
        {
            FillMaterialsChoices();
            FillViewsChoices();
            FillAllTheSceneChoices();
            FillRenderOptionsChoices();
            RegenCmdLines();
            ValidateTextBoxValues();
            BuildRadFoldersList();
            FillPCombChoices();
            FillRATiffChoices();
            FillTiffsChoices();
            FillFinalTiffChoices();
            FillFinalImageChoices();
        }

        ///// <summary>
        ///// For some reason we need to reload the images used in the WPF.
        ///// </summary>
        //private void LoadUpUIImages()
        //{
        //    //img_RadWiki.Source = Imaging.CreateBitmapSourceFromHIcon(
        //    //                         System.Drawing.SystemIcons.Question.Handle,
        //    //                         Int32Rect.Empty,
        //    //                         BitmapSizeOptions.FromEmptyOptions());

        //    //string ExecutingAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        //    //bt_regen.Source = NewBitmapImage(System.Reflection.Assembly.GetExecutingAssembly(),
        //    //                                 ExecutingAssemblyName + ".reload-icon.png");

        //}

        private void ValidateTextBoxValues()
        {
            Helpers.MarkTextBoxForPath(tbox_octpath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tbox_ambpath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tbox_ambpath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tbox_imgpath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tbox_irradpath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tbox_viewpath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tbox_basescenepath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tbox_varscenepath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tbox_matpath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tbox_renderoptpath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tboxskyfilepath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tboxillumpath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForFile(tbox_renderoptfile, ThisRad.ProjectPath, ThisRad.RenderOptsPath);
            Helpers.MarkTextBoxForFile(tbox_mkillumoptfile, ThisRad.ProjectPath, ThisRad.RenderOptsPath);
            Helpers.MarkTextBoxForFile(tbox_overtureoptfile, ThisRad.ProjectPath, ThisRad.RenderOptsPath);
            Helpers.MarkTextBoxForFile(tboxskyfile, ThisRad.ProjectPath, ThisRad.SkyFilePath);
            Helpers.MarkTextBoxForFile(tboxillumfile, ThisRad.ProjectPath, ThisRad.IllumPath);
            Helpers.MarkTextBoxForPath(Tbox_pcombimagetargetpath, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tbox_picsourcefolder, ThisRad.ProjectPath, true);
            Helpers.MarkTextBoxForPath(tbox_tiffresultsfolder, ThisRad.ProjectPath, true);
        }

        private void FillViewsChoices()
        {
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ViewPath), listbox_views_choices, Helpers.MakeFilesListRegexString(fileExtView));
        }

        private void FillMaterialsChoices()
        {
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.MatPath), listbox_mat_choices, Helpers.MakeFilesListRegexString(fileExtMat));
        }

        private void FillRenderOptionsChoices()
        {
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.RenderOptsPath), listbox_optionsfile_choices, Helpers.MakeFilesListRegexString(fileExtRenderOpts), false);
        }

        private void FillAllTheSceneChoices()
        {
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.BScnPath), listboxSSS, Helpers.MakeFilesListRegexString(fileExtSceneSets));
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.BScnPath), listboxBaseSceneChoices, Helpers.MakeFilesListRegexString(fileExtScenes));
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.BScnPath), listboxVarSceneChoices, Helpers.MakeFilesListRegexString(fileExtScenes));
        }

        private void FillPCombChoices()
        {
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ImagePath), listbox_pcomb_choices, Helpers.MakeFilesListRegexString(fileNameExtensionPComb));
        }

        private void FillRATiffChoices()
        {
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.PcombimagesourcePath), listbox_pic_choices, Helpers.MakeFilesListRegexString(fileNameExtensionPComb));
        }

        private void FillTiffsChoices()
        {
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.RatiffimagePath), listbox_tiff_results, Helpers.MakeFilesListRegexString(fileNameExtensionTif), false);
        }

        private void FillFinalTiffChoices()
        {
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.RatiffimagePath), listbox_finaltiff_choices, Helpers.MakeFilesListRegexString(fileNameExtensionTif));
        }

        public void FillFinalImageChoices()
        {
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.FinalimagePath), listbox_magick_results, Helpers.MakeFilesListRegexString(fileNameExtensionMagick), false);
        }

        private string ImgSize1(RadSession thisRad)
        {
            return "-x " + thisRad.ImgX.ToString() + " -y " + thisRad.ImgY.ToString();
        }

        private string ImgSizeOv(RadSession thisRad)
        {
            return "-x " + thisRad.OvImgX.ToString() + " -y " + thisRad.OvImgY.ToString();
        }

        private void RegenCmdLines()
        {
            tbox_gensky.Text = GenSkyCommand(Convert.ToInt16(scrollMnth.Value), ThisRad.SimDay, scrollHr.Value, ThisRad);
            tbox_oconv.Text = OConvCommand(Convert.ToInt16(scrollMnth.Value), ThisRad.SimDay, scrollHr.Value, ThisRad);
            tbox_mkillum.Text = MkillumCommand(Convert.ToInt16(scrollMnth.Value), ThisRad.SimDay, scrollHr.Value, ThisRad);
            tbox_oconv2nd.Text = ReOconvCommand(Convert.ToInt16(scrollMnth.Value), ThisRad.SimDay, scrollHr.Value, ThisRad);
            tbox_rpict.Text = RpictCommand(Convert.ToInt16(scrollMnth.Value), ThisRad.SimDay, scrollHr.Value, ThisRad);
            if (ThisRad.IncRpictI)
            {
                tbox_rpicti.Text = RPictICommand(Convert.ToInt16(scrollMnth.Value), ThisRad.SimDay, scrollHr.Value, ThisRad);
            }
            else
            {
                tbox_rpicti.Clear();
            }

        }

        //GenSkyCommand
        //returns the full argument gensky command line whe passed month,runday,hr
        //
        private string GenSkyCommand(int mnth, int runday, double hr, RadSession thisrad)
        {
            string genskycommand = string.Empty;
            if (thisrad.NightTime)
            {
                return genskycommand;
            }
            else
            {
                string skypath = thisrad.SkyFilePath;
                string sky = thisrad.SkySwitch;
                string sunfile = skypath + sky + mnth.ToString("00") + runday.ToString("00") + hr.ToString("00.00") + "sun.rad";
                genskycommand = "gensky" + Sp + mnth.ToString("00") + Sp + runday.ToString("00") + Sp + hr.ToString("00.00") + Sp + sky;
                genskycommand = genskycommand + Sp + ">" + Sp + sunfile;
                return genskycommand;
            }
        }

        //OConvCommand
        //returns the full argument Oconv command line whe passed month,runday,hr
        private string OConvCommand(int mnth, int runday, double hr, RadSession thisrad)
        {
            string oConvCommand = string.Empty;
            if (thisrad.NightTime)
            {
                return oConvCommand;
            }
            else
            {
                string skypath = thisrad.SkyFilePath;
                string sky = thisrad.SkySwitch;
                string skyFile = skypath + thisrad.SkyFileName;
                string matArg = mat_arg.Text;
                string scnArg = scene_arg.Text;
                string sunfile = skypath + sky + mnth.ToString("00") + runday.ToString("00") + hr.ToString("00.00") + "sun.rad";
                OctBase = thisrad.OctPath + sky + mnth.ToString("00") + runday.ToString("00") + hr.ToString("00.00") + "base.oct";
                oConvCommand = "oconv" + Sp + matArg + Sp + scnArg + Sp + sunfile + Sp + skyFile;
                oConvCommand = oConvCommand + Sp + ">" + Sp + OctBase;
                return oConvCommand;
            }

        }

        //mkillumCommand
        //returns the full argument mkillumCommand command line whe passed month,runday,hr
        private string MkillumCommand(int mnth, int runday, double hr, RadSession thisrad)
        {
            string mkillumCommand = string.Empty;
            if (thisrad.NightTime)
            {
                return mkillumCommand;
            }
            else
            {
                string skypath = thisrad.SkyFilePath;
                string sky = thisrad.SkySwitch;
                string skyFile = skypath + thisrad.SkyFileName;
                string fullIllumFile = thisrad.IllumPath + thisrad.IllumFileName;
                string mkillumFile = thisrad.IllumPath + sky + mnth.ToString("00") + runday.ToString("00") + hr.ToString("00.00") + "mkillum.rad";

                OctBase = thisrad.OctPath + sky + mnth.ToString("00") + runday.ToString("00") + hr.ToString("00.00") + "base.oct";
                mkillumCommand = "mkillum" + Sp + thisrad.MkillumOptsFileName + Sp + OctBase + Sp + "<" + Sp + fullIllumFile;
                mkillumCommand = mkillumCommand + Sp + ">" + Sp + mkillumFile;
                return mkillumCommand;
            }
        }

        private string ReOconvCommand(int mnth, int runday, double hr, RadSession thisrad)
        {
            string reOconvCommand = string.Empty;
            string sky = string.Empty;
            string timecode = string.Empty;
            if (thisrad.NightTime)
            {
                sky = "+nosky";
            }
            else
            {
                sky = thisrad.SkySwitch;
                timecode = mnth.ToString("00") + runday.ToString("00") + hr.ToString("00.00");
            }

            string matArg = mat_arg.Text;
            string scnArg = scene_arg.Text;
            string skypath = thisrad.SkyFilePath;
            string skyFile = skypath + thisrad.SkyFileName;
            string fullIllumFile = thisrad.IllumPath + thisrad.IllumFileName;
            string mkillumFile = thisrad.IllumPath + sky + mnth.ToString("00") + runday.ToString("00") + hr.ToString("00.00") + "mkillum.rad";
            string sunfile = skypath + sky + mnth.ToString("00") + runday.ToString("00") + hr.ToString("00.00") + "sun.rad";

            OctBase = thisrad.OctPath + sky + timecode + "base.oct";
            reOconvCommand = "oconv" + Sp + matArg + Sp + scnArg + Sp;
            if (!thisrad.NightTime) { reOconvCommand = reOconvCommand + sunfile + Sp + skyFile + Sp + mkillumFile; }
            reOconvCommand = reOconvCommand + Sp + ">" + Sp + OctBase;
            return reOconvCommand;
        }

        private string RpictCommand(int mnth, int runday, double hr, RadSession thisrad)
        {
            string rpictCommand = string.Empty;
            string ambFile = string.Empty;
            string sky = thisrad.SkySwitch;
            string timecode = string.Empty;
            if (thisrad.NightTime)
            { sky = "+nosky"; }
            else
            {
                sky = thisrad.SkySwitch;
                timecode = mnth.ToString("00") + runday.ToString("00") + hr.ToString("00.00");
            }
            OctBase = thisrad.OctPath + sky + timecode + "base.oct";
            ambFile = thisrad.AmbPath + sky + timecode + ".amb";

            string radImage = string.Empty;
            foreach (ListBoxItem lbi in listbox_view_sel.Items)
            {
                string view = lbi.Content.ToString();
                radImage = thisrad.ImagePath + view + sky + timecode + ".pic";
                if (thisrad.UseOverture)
                {
                    rpictCommand = rpictCommand + "rpict -vf" + Sp + thisrad.ViewPath + view + Sp + "-af" + Sp + ambFile + Sp + ImgSizeOv(ThisRad) + Sp + "-t 5";
                    rpictCommand = rpictCommand + Sp + "@" + thisrad.RenderOptsPath + thisrad.OvertureOptsFileName + Sp + "-av 1 1 1" + Sp + OctBase + Sp + ">" + Sp + radImage;
                    rpictCommand = rpictCommand + System.Environment.NewLine;
                }

                rpictCommand = rpictCommand + "rpict -vf" + Sp + thisrad.ViewPath + view + Sp + "-af" + Sp + ambFile + Sp + ImgSize1(ThisRad) + Sp + "-t 5";
                rpictCommand = rpictCommand + Sp + "@" + thisrad.RenderOptsPath + thisrad.RenderOptsFileName + Sp + "-av 1 1 1" + Sp + OctBase + Sp + ">" + Sp + radImage;
                rpictCommand = rpictCommand + System.Environment.NewLine;
            }
            return rpictCommand;
        }

        private string RPictICommand(int mnth, int runday, double hr, RadSession thisrad)
        {
            string rpictICommand = string.Empty;
            string ambFile = string.Empty;
            string sky = thisrad.SkySwitch;
            string timecode = string.Empty;
            if (thisrad.NightTime)
            { sky = "+nosky"; }
            else
            {
                sky = thisrad.SkySwitch;
                timecode = mnth.ToString("00") + runday.ToString("00") + hr.ToString("00.00");
            }

            OctBase = thisrad.OctPath + sky + timecode + "base.oct";
            ambFile = thisrad.AmbPath + sky + timecode + ".amb";

            string irradImage = string.Empty;
            foreach (ListBoxItem lbi in listbox_view_sel.Items)
            {
                string view = lbi.Content.ToString();
                irradImage = thisrad.IrradPath + view + sky + timecode + "_irrad.pic";
                rpictICommand = rpictICommand + "rpict -i -vf" + Sp + thisrad.ViewPath + view + Sp + "-af" + Sp + ambFile + Sp + ImgSize1(ThisRad) + Sp + "-t 5";
                rpictICommand = rpictICommand + Sp + "@" + thisrad.RenderOptsPath + thisrad.RenderOptsFileName + Sp + "-av 1 1 1" + Sp + OctBase + Sp + ">" + Sp + irradImage;
                rpictICommand = rpictICommand + System.Environment.NewLine;
            }
            return rpictICommand;
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            // Watch out. Fatal error if not primary button!
            if (args.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.PrefsPath = ThisRad.PrefsPath;
            Properties.Settings.Default.Save();
            if (bwPicToTiff.IsBusy == true) { bwPicToTiff.Dispose(); }
            if (bwMagick.IsBusy == true) { bwMagick.Dispose(); }
            if (bkFileLoadingWorker.IsBusy == true) { bkFileLoadingWorker.Dispose(); }
            Helpers.SavePrefs(ThisRad, Pcombfilters, PrefData);
        }

        //private void TimeOut_Tick(object sender, EventArgs e)
        //{
        //    ResizeMode = ResizeMode.NoResize;
        //}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ScrollHr_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            RegenCmdLines();
        }

        private void ScrollMnth_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            RegenCmdLines();
        }

        private void Tbox_startH_KeyUp(object sender, KeyEventArgs e)
        {
            RegenCmdLines();
        }

        private void Tbox_endH_KeyUp(object sender, KeyEventArgs e)
        {
            RegenCmdLines();
        }

        private void Tbox_intervalH_KeyUp(object sender, KeyEventArgs e)
        {
            RegenCmdLines();
        }

        private void CheckBox_NightTime_Click(object sender, RoutedEventArgs e)
        {
            RegenCmdLines();
            SetNightTimeCheckEffects();
        }

        private void SetNightTimeCheckEffects()
        {
            bool isEnab = !ThisRad.NightTime;
            lb_startHr.IsEnabled = isEnab;
            lb_endH.IsEnabled = isEnab;
            lb_intervalH.IsEnabled = isEnab;
            tbox_startH.IsEnabled = isEnab;
            tbox_endH.IsEnabled = isEnab;
            tbox_intervalH.IsEnabled = isEnab;

            lb_startM.IsEnabled = isEnab;
            lb_endM.IsEnabled = isEnab;
            lb_intervalM.IsEnabled = isEnab;
            tbox_startM.IsEnabled = isEnab;
            tbox_endM.IsEnabled = isEnab;
            tbox_intervalM.IsEnabled = isEnab;

            lb_simdayM.IsEnabled = isEnab;
            tbox_simdayM.IsEnabled = isEnab;
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            RegenCmdLines();
        }

        private void Tboxskyfilepath_KeyUp(object sender, KeyEventArgs e)
        {
            RegenCmdLines();
        }

        private void Tboxillumpath_KeyUp(object sender, KeyEventArgs e)
        {
            RegenCmdLines();
        }

        private void Tboxskyfile_KeyUp(object sender, KeyEventArgs e)
        {
            RegenCmdLines();
        }

        private void Tboxillumfile_KeyUp(object sender, KeyEventArgs e)
        {
            RegenCmdLines();
        }

        private void Tboxskyswitch_KeyUp(object sender, KeyEventArgs e)
        {
            RegenCmdLines();
        }

        private void Tbox_simdayM_KeyUp(object sender, KeyEventArgs e)
        {
            RegenCmdLines();
        }

        private void Tbox_startM_KeyUp(object sender, KeyEventArgs e)
        {
            RegenCmdLines();
        }

        private void Tbox_endM_KeyUp(object sender, KeyEventArgs e)
        {
            RegenCmdLines();
        }

        private void FillChoiceList(string thePath, ListBox theListBox, string regexsearchPat, bool asCheckBox = true)
        {
            theListBox.Items.Clear();
            if (!Directory.Exists(thePath)) { return; }
            try
            {
                IEnumerable<String> ResultsList = Directory.GetFiles(thePath).Select(Path.GetFileName).Where(file => Regex.IsMatch(file, regexsearchPat, RegexOptions.IgnoreCase));
                foreach (string f in ResultsList)
                {
                    if (asCheckBox)
                    {
                        /// This is no odinary checkbox. It has a lable control as its
                        /// content because the out of the box chckbox content reacts to
                        /// underscores in the text as meaning "alt" and will not show the
                        /// underscore. Since the content is supposed to be filenames we
                        /// are using a textblock as the content. The file name is in turn the 
                        /// textblock's content and not the checkbox's content.
                        CheckBox chkF = new CheckBox
                        {
                            VerticalContentAlignment = VerticalAlignment.Center
                        };
                        chkF.Click += new RoutedEventHandler(ThisListItem_Clicked);
                        TextBlock chkTB = new TextBlock
                        {
                            Text = Path.GetFileName(f)
                        };
                        chkF.Content = chkTB;
                        // chkF.Content = Path.GetFileName(f);
                        theListBox.Items.Add(chkF);
                    }
                    else
                    {
                        ListBoxItem lbi = new ListBoxItem();
                        lbi.Selected += new RoutedEventHandler(ThisListItem_Clicked);
                        lbi.Content = Path.GetFileName(f);
                        theListBox.Items.Add(lbi);
                    }

                }
            }
            catch (ArgumentException e)
            {
                string msg = "Not Good! It looks like the regular expression \"" + regexsearchPat + "\" has an invalid syntax. ";
                ThisRad.StatusMsg = msg;
                //msg = msg + " This is happening in the FillChoiceList function on " + theListBox.Name;
                msg = msg + "\n\n" + e.Message;
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, "Up The Creek Without A Paddle");
                explain.ShowDialog();
            }
        }

        private void But_mat_clear_Click(object sender, RoutedEventArgs e)
        {
            ClearCheckedFromThisListBoxOfCheckBoxes(listbox_mat_choices);
            UpdateListBoxBWithCheckedItemsInListBoxA(listbox_mat_choices, listbox_mat_sel);
            UpDateMaterialsArgument();
        }

        private void UpDateSceneArgument()
        {
            string arg = string.Empty;
            foreach (ListBoxItem lbi in listboxBaseSceneToArg.Items)
            {
                arg = arg + ThisRad.BScnPath + lbi.Content.ToString() + Sp;
            }
            foreach (ListBoxItem lbi in listboxSelVarSceneToArg.Items)
            {
                arg = arg + ThisRad.BScnPath + lbi.Content.ToString() + Sp;
            }
            scene_arg.Text = arg;
            RegenCmdLines();
        }

        private void UpDateMaterialsArgument()
        {
            string arg = string.Empty;
            foreach (ListBoxItem lbi in listbox_mat_sel.Items)
            {
                arg = arg + ThisRad.MatPath + lbi.Content.ToString() + Sp;
            }
            mat_arg.Text = arg;
            RegenCmdLines();
        }

        private void But_mat_useall_Click(object sender, RoutedEventArgs e)
        {
            CheckAllFromThisListBoxOfCheckBoxes(listbox_mat_choices);
            UpdateListBoxBWithCheckedItemsInListBoxA(listbox_mat_choices, listbox_mat_sel);
            UpDateMaterialsArgument();
        }

        private void ClearCheckedFromThisListBoxOfCheckBoxes(ListBox thisListBox)
        {
            foreach (CheckBox chkI in thisListBox.Items)
            {
                chkI.IsChecked = false;
            }

        }

        private void CheckAllFromThisListBoxOfCheckBoxes(ListBox thisListBox)
        {
            foreach (CheckBox chkI in thisListBox.Items)
            {
                chkI.IsChecked = true;
            }

        }

        private void ThisListItem_Clicked(object sender, RoutedEventArgs e)
        {
            ThisRad.StatusMsg = DefaultStatMsg;
            ListBox lb = new ListBox();
            CheckBox cb = new CheckBox();
            ListBoxItem lbi = new ListBoxItem();
            if (sender.GetType() == typeof(CheckBox))
            {
                cb = sender as CheckBox;
                lb = cb.Parent as ListBox;
            }
            if (sender.GetType() == typeof(ListBoxItem))
            {
                lbi = sender as ListBoxItem;
                lb = lbi.Parent as ListBox;
            }
            switch (lb.Name)
            {
                case "listbox_mat_choices":
                    UpdateListBoxBWithCheckedItemsInListBoxA(listbox_mat_choices, listbox_mat_sel);
                    UpDateMaterialsArgument();
                    break;
                case "listboxSSS":
                    if ((bool)cb.IsChecked)
                    {
                        UpdateBasedOnSelectedSSS(cb);
                        SynchronizeCheckboxListChoices(listboxBaseSceneChoices, listboxVarSceneChoices, null, listboxSelVarSceneToArg);
                        UpDateSceneArgument();
                        RegenCmdLines();
                    }
                    break;
                case "listboxBaseSceneChoices":
                    UpdateListBoxBWithCheckedItemsInListBoxA(listboxBaseSceneChoices, listboxBaseSceneToArg);
                    SynchronizeCheckboxListChoices(listboxBaseSceneChoices, listboxVarSceneChoices, null, listboxSelVarSceneToArg);
                    UpDateSceneArgument();
                    RegenCmdLines();
                    break;
                case "listboxVarSceneChoices":
                    UpdateListBoxBWithCheckedItemsInListBoxA(listboxVarSceneChoices, listboxSelVarSceneToArg);
                    UpDateSceneArgument();
                    RegenCmdLines();
                    break;
                case "listbox_views_choices":
                    UpdateListBoxBWithCheckedItemsInListBoxA(listbox_views_choices, listbox_view_sel);
                    RegenCmdLines();
                    break;
                case "listbox_optionsfile_choices":
                    if (lbi != null)
                    {
                        ShowOptionFile(lbi);
                    }
                    break;
                case "listbox_pcomb_choices":
                    ProcessPCombSelection(cb);
                    break;
                case "listbox_finaltiff_choices":
                    DoFinalResultSourcePick(cb);
                    break;
                default:
                    break;
            }
        }

        private void AddCheckStateToDictionary(ListBox thisListBox, Dictionary<string, bool> thisChkDictionary)
        {
            foreach (var item in thisListBox.Items)
            {
                if (item.GetType() != typeof(CheckBox)) { continue; }
                CheckBox cbloop = item as CheckBox;
                String strKey = ((TextBlock)cbloop.Content).Text;
                if (thisChkDictionary.ContainsKey(strKey))
                {
                    thisChkDictionary[strKey] = (bool)cbloop.IsChecked;
                }
            }
        }

        private void SaveCheckBoxStateToDictionary(CheckBox thisChkBox, Dictionary<string, bool> thisChkDictionary)
        {
            if (thisChkBox.Content is TextBlock tb)
            {
                String strKey = tb.Text;
                if (thisChkDictionary.ContainsKey(strKey))
                {
                    thisChkDictionary[strKey] = (bool)thisChkBox.IsChecked;
                }
            }
        }

        private void UpdateListBoxToSavedCheckState(ListBox thisListBox, Dictionary<string, bool> thisChkDictionary)
        {
            foreach (var item in thisListBox.Items)
            {
                if (item.GetType() != typeof(CheckBox)) { continue; }
                CheckBox cbloop = item as CheckBox;
                String strKey = ((TextBlock)cbloop.Content).Text;
                if (thisChkDictionary.ContainsKey(strKey))
                {
                    cbloop.IsChecked = thisChkDictionary[strKey];
                }
            }
        }

        private List<string> ListofCheckedItemsInListBox(ListBox thisListBox)
        {
            List<string> _chked = new List<string>();
            foreach (var item in thisListBox.Items)
            {
                if (item.GetType() != typeof(CheckBox)) { continue; }
                CheckBox cbloop = item as CheckBox;
                if ((bool)cbloop.IsChecked)
                {
                    _chked.Add(((TextBlock)cbloop.Content).Text);
                }
            }
            return _chked;
        }

        private void CheckItemsInListBoxFromItemsInList(List<string> _checkList, ListBox thisListBox)
        {
            foreach (var item in thisListBox.Items)
            {
                if (item.GetType() != typeof(CheckBox)) { continue; }
                CheckBox cbloop = item as CheckBox;
                if (_checkList.Contains(((TextBlock)cbloop.Content).Text))
                {
                    cbloop.IsChecked = true;
                }
            }
        }

        private void AddToPCombFiltersIfOk()
        {
            string thisStr = combo_pcomfilter.Text.Trim();
            if (!Pcombfilters.Contains(thisStr) && !thisStr.Equals(string.Empty))
            {
                Pcombfilters.Add(thisStr);
                // combo_pcomfilter.ItemsSource = ThisRad.Pcompfilters.ToList();
            }
        }

        private void RegexFilterForPcombValue()
        {

            // There is a tricky difference between the Text and the Selected Item
            string pcombfilter = combo_pcomfilter.Text;
            if (combo_pcomfilter.SelectedIndex != -1)
            {
                pcombfilter = combo_pcomfilter.SelectedItem.ToString();
            }
            string pat_toapply = Helpers.MakePcombRegexFiltetString(pcombfilter, fileNameExtensionPComb);
            /// The choice list is regenerated from the files in the folder matching the pattern. All the previous items
            /// in the listbox are cleared out in the process and new ones put in. That means the previous check state in the 
            /// prior set of items needs to be updated from the checkdictionary.
            /// First save the current check state to the checkdictionary
            AddCheckStateToDictionary(listbox_pcomb_choices, PCombFilesCheckDictionary);
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ImagePath), listbox_pcomb_choices, pat_toapply);
            /// Set the checkstate for any new items that are in the dictionary as checked.
            UpdateListBoxToSavedCheckState(listbox_pcomb_choices, PCombFilesCheckDictionary);
            StatMsgForRegexFilter(pcombfilter);
        }

        private void But_applypcombregexfilter_Click(object sender, RoutedEventArgs e)
        {
            string newpcombfilter = combo_pcomfilter.Text;
            //AddCheckStateToDictionary(listbox_pcomb_choices,  PCombFilesCheckDictionary);
            RegexFilterForPcombValue();
            Helpers.AddToList(ref Pcombfilters, newpcombfilter);
            combo_pcomfilter.ItemsSource = Pcombfilters;
        }

        private void StatMsgForRegexFilter(string fltr)
        {
            string pat_toapply = Helpers.MakePcombRegexFiltetString(fltr, fileNameExtensionPComb);
            ThisRad.StatusMsg = "Image choice filtering matches names like:\n\'" + pat_toapply + "\'";
        }

        private void But_unapplypcombregexfilter_Click(object sender, RoutedEventArgs e)
        {
            AddCheckStateToDictionary(listbox_pcomb_choices, PCombFilesCheckDictionary);
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ImagePath), listbox_pcomb_choices, Helpers.MakeFilesListRegexString(fileNameExtensionPComb));
            UpdateListBoxToSavedCheckState(listbox_pcomb_choices, PCombFilesCheckDictionary);
            combo_pcomfilter.Text = string.Empty;
            StatMsgForRegexFilter(combo_pcomfilter.Text);
        }

        /// Note to file 9/9/17, Any event processed on the combo_pcomfilter list box seems to zero
        /// the checkstate dictionary!
        /// Do Not Use!!
        //private void Combo_pcomfilter_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    //AddToPCombFiltersIfOk();
        //}

        /// Do Not Use!!
        //private void Combo_pcomfilter_KeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Return)
        //    {
        //        string newpcombfilter = combo_pcomfilter.Text;
        //        RegexFilterForPcombValue();
        //        Helpers.AddToList(ref Pcombfilters, newpcombfilter);
        //        combo_pcomfilter.ItemsSource = Pcombfilters;
        //    }
        //}

        /// DO NOT ENGAGE THIS!
        //private void Combo_pcomfilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    //var selectedcombo = sender as ComboBox;
        //    //string selectedFilter = selectedcombo.SelectedItem as string;
        //    // MessageBox.Show(selectedFilter);
        //    /// DO NOT ENGAGE THIS!
        //    //AddCheckStateToDictionary(listbox_pcomb_choices,  PCombFilesCheckDictionary);
        //    //RegexFilterForPcombValue();
        //}

        // Do Not Use!!
        //private void Combo_pcombregexfilter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    string msg = Helpers.MakePcombRegexFiltetString(combo_pcomfilter.Text, fileNameExtensionPComb);
        //    FormMsgWPF explain = new FormMsgWPF(null, 3);
        //    explain.SetMsg(msg, "The Regular Expression");
        //    explain.ShowDialog();
        //}

        private void ShowDictionary()
        {
            List<string> thislist = new List<string>();
            foreach (KeyValuePair<string, bool> entry in PCombFilesCheckDictionary)
            {
                string str = entry.Key.ToString() + "   " + entry.Value.ToString();
                thislist.Add(str);
            }
            thislist.Sort();
            foreach (string s in thislist)
            {
                // console.writeLine(s);
            }
        }

        private void ProcessPCombSelection(CheckBox cb)
        {
            // First save the checkstate
            SaveCheckBoxStateToDictionary(cb, PCombFilesCheckDictionary);
            TextBlock tblk = cb.Content as TextBlock;
            string fn = tblk.Text;

            if ((bool)cb.IsChecked)
            {
                lastCheckedPcomb = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.PcombimagesourcePath, fn);
                Tblock_radheader.Text = Helpers.GetInfoThisRadFile(ThisRad, @lastCheckedPcomb);
            }
            else
            {
                Tblock_radheader.Text = String.Empty;
                lastCheckedPcomb = string.Empty;
            }

            string pat_selected_datetime = string.Empty;
            bool selIsDateTime = false;

            string pat_general_datetime = Helpers.MakeFileNameDateTimeRegexPattern(fileNameExtensionPComb);

            // console.writeLine("Processing checkbox click for " + fn);

            /// Is this selection a datetime? If so then extract its timestamp
            /// for use selecting any other same timestamp.
            Regex r = new Regex(pat_general_datetime, RegexOptions.IgnoreCase);
            Match m = r.Match(fn);
            if (m.Success) // yes, now extract the specific datetimestamp
            {
                if (fn.Length > 13)  // could have datetimestamp
                {
                    int startIndex = fn.Length - 13;
                    pat_selected_datetime = fn.Substring(startIndex, 13);
                    selIsDateTime = true;
                }
            }

            /// Start building listboxitems to put into the listbox of checked items that will be in the 
            /// pcomb command line.
            ListBoxItem newlbi = new ListBoxItem
            {
                Content = fn
            };
            /// Don't add newlbi if there is already a matching existinglbi. There could be.
            var mtchLBI = from ListBoxItem item in listbox_pcomb_selections.Items.Cast<ListBoxItem>()
                          where item.Content.ToString().Equals(fn, StringComparison.CurrentCultureIgnoreCase)
                          select item as ListBoxItem;
            ListBoxItem existinglbi = mtchLBI.FirstOrDefault();
            /// Handing the two conditions: cb being checked and cb being unchecked.
            /// If cb was checked and its not in the list, then add it.
            if ((bool)cb.IsChecked && existinglbi == null) { listbox_pcomb_selections.Items.Add(newlbi); }
            /// If cb was unchecked and it is in the list, so remove it .
            if (!(bool)cb.IsChecked && existinglbi != null) { listbox_pcomb_selections.Items.Remove(existinglbi); }
            /// At this point only the single checkbox has been added to or removed from the listbox. The same
            /// time checkboxes have yet to be considered.

            /// If sametime is desired and this selection qualified as having a datetime, then all the other
            /// same datetimes get checked.
            if ((bool)chk_sametime.IsChecked && selIsDateTime)
            {
                CheckByRegexThisListBoxOfCheckBoxes(listbox_pcomb_choices, pat_selected_datetime, (bool)cb.IsChecked);
            }
            // console.writeLine("Processing checkbox click for " + fn + " now adding to check dictionary.");
            AddCheckStateToDictionary(listbox_pcomb_choices, PCombFilesCheckDictionary);
            BuildThePCombLine();
        }

        private void BuildThePCombLine()
        {
            string targetFN = ThisRad.PcombimagetargetPath + textbox_pcom_target.Text;
            // \\ is allowed up to this point
            targetFN = targetFN.Replace("\\\\", "\\");

            Helpers.MarkPCombTargetForFile(ThisRad.PcombimagetargetPath, ThisRad.ProjectPath, textbox_pcom_target, listbox_pcomblines, groupboxTargetfile);
            string pcombline = "pcomb.exe" + Sp;
            if (ThisRad.Pcombh)
            {
                pcombline = pcombline + "-h" + Sp;
            }
            int pos = 0;
            foreach (var item in listbox_pcomb_selections.Items)
            {
                if (item.GetType() != typeof(ListBoxItem)) { continue; }
                ListBoxItem lvi = item as ListBoxItem;

                string lviT = lvi.Content.ToString();
                lviT = ThisRad.ImagePath + lviT;
                pos++;
                TextBox tbscale = FindName("tbox_scale" + pos.ToString()) as TextBox;
                string scale;
                if (tbscale != null)
                {
                    scale = tbscale.Text;
                }
                else
                {
                    scale = "1.0";
                }
                pcombline = pcombline + "-s" + Sp + scale + Sp + lviT + Sp;

            }
            pcombline = pcombline + ">" + Sp + targetFN;
            tbox_pcombcandidate.Text = pcombline.Trim();
        }

        private void But_applypcombregex_Click(object sender, RoutedEventArgs e)
        {
            string rg = (@tbox_pcombregex.Text).Trim();
            if (rg.Equals(string.Empty)) { return; }
            CheckByRegexThisListBoxOfCheckBoxes(listbox_pcomb_choices, rg, true);
            AddCheckStateToDictionary(listbox_pcomb_choices, PCombFilesCheckDictionary);
            BuildThePCombLine();
        }

        private void But_unapplypcombregex_Click(object sender, RoutedEventArgs e)
        {
            string rg = (@tbox_pcombregex.Text).Trim();
            if (rg.Equals(string.Empty)) { return; }
            CheckByRegexThisListBoxOfCheckBoxes(listbox_pcomb_choices, rg, false);
            AddCheckStateToDictionary(listbox_pcomb_choices, PCombFilesCheckDictionary);
            BuildThePCombLine();
        }

        private void CheckByRegexThisListBoxOfCheckBoxes(ListBox thisListBox, string pat_regex, bool isChecked)
        {
            Regex r = new Regex(pat_regex, RegexOptions.IgnoreCase);
            Match m;
            /// Iterate through the list matching for the specific
            /// selected datetime pattern so to set other checkboxes.
            foreach (var thisitem in thisListBox.Items)
            {
                if (thisitem.GetType() != typeof(CheckBox)) { continue; }
                CheckBox cbloop = thisitem as CheckBox;
                TextBlock cbTBloop = cbloop.Content as TextBlock;
                string itemTxt = cbTBloop.Text;
                m = r.Match(itemTxt);
                if (m.Success)
                {
                    cbloop.IsChecked = isChecked;
                    /// now deal with the selection listbox
                    ListBoxItem newlbi = new ListBoxItem
                    {
                        Content = itemTxt
                    };
                    /// Don't add nlbi if there is already a matching lbi. There could be.
                    var mtchLBI = from ListBoxItem item in listbox_pcomb_selections.Items.Cast<ListBoxItem>()
                                  where item.Content.ToString().Equals(itemTxt, StringComparison.CurrentCultureIgnoreCase)
                                  select item as ListBoxItem;
                    ListBoxItem existinglbi = mtchLBI.FirstOrDefault();
                    /// cb was checked and its not in the list, so add it.
                    if (existinglbi == null) { listbox_pcomb_selections.Items.Add(newlbi); }
                }
            }
        }

        private void SynchronizeCheckboxListChoices(ListBox listboxA, ListBox listboxB, ListBox listboxAA = null, ListBox listboxBB = null)
        {
            /// These checkboxes use Texblocks to hold the content text
            foreach (CheckBox chkIA in listboxA.Items)
            {
                TextBlock tblk = new TextBlock();
                tblk = chkIA.Content as TextBlock;
                string strIA = tblk.Text;
                CheckBox matchingCheckBoxInB = null;
                if (listboxB.Items.Count > 0)
                /// Set matchingCheckBox to be what matches in listboxB what was touched in 
                /// listboxA.
                {
                    foreach (CheckBox cb in listboxB.Items)
                    {
                        TextBlock tb = cb.Content as TextBlock;
                        if (tb.Text.Equals(strIA))
                        {
                            matchingCheckBoxInB = cb;
                            break;
                        }
                    }
                }

                if (chkIA.IsChecked == true)
                {
                    /// The item touched in listboxA was checked. Checked listboxA items
                    /// are to be excluded form listboxB. Also, if it were checked in listboxB
                    /// then its entry in the argument checked list should be removed.
                    /// 
                    if (matchingCheckBoxInB != null)
                    {
                        /// The match needs to be removed.
                        listboxB.Items.Remove(matchingCheckBoxInB);
                        /// The counterpart in the BB list needs to be removed.
                        var mtchLBI = from ListBoxItem item in listboxBB.Items.Cast<ListBoxItem>()
                                      where item.Content.ToString().Equals(strIA, StringComparison.CurrentCultureIgnoreCase)
                                      select item as ListBoxItem;
                        ListBoxItem lbi = mtchLBI.FirstOrDefault();
                        if (lbi != null) { listboxBB.Items.Remove(lbi); }
                    }
                }
                else
                {
                    /// The item touched in listboxA was unchecked. Unchecked listboxA items
                    /// are to be included in listboxB. 
                    /// 
                    /// Add it back in if there is not one, but add it in
                    /// alphabetical order. 
                    if (matchingCheckBoxInB == null)  // ie not present
                    {
                        CheckBox chkToAdd = new CheckBox
                        {
                            VerticalContentAlignment = VerticalAlignment.Center
                        };
                        chkToAdd.Click += new RoutedEventHandler(ThisListItem_Clicked);
                        TextBlock chkToAddTB = new TextBlock
                        {
                            Text = strIA
                        };
                        chkToAdd.Content = chkToAddTB;
                        bool wasAdded = false;
                        foreach (CheckBox cb in listboxB.Items)
                        {
                            if (strIA.CompareTo(((TextBlock)cb.Content).Text) == -1)
                            {
                                int insertIndex = listboxB.Items.IndexOf(cb);
                                listboxB.Items.Insert(insertIndex, chkToAdd);
                                wasAdded = true;
                                break;
                            }
                        }
                        if (!wasAdded) { listboxB.Items.Add(chkToAdd); }
                    }
                }
            }
        }

        private void ShowOptionFile(ListBoxItem lbi)
        {
            string fn = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.RenderOptsPath);
            fn = Helpers.CombineIntoPath(fn, lbi.Content.ToString());
            tbox_optionsfileview.Clear();
            groupBoxOptionsFile.Header = "Options File View";
            if (File.Exists(fn))
            {
                tbox_optionsfileview.Text = File.ReadAllText(fn, Encoding.ASCII);
                groupBoxOptionsFile.Header = "Options File View" + " ==> " + lbi.Content.ToString();
                tbox_optionsfileview.Tag = fn as object;
            }
        }

        private void UpdateBasedOnSelectedSSS(CheckBox cb)
        {
            TextBlock chkTB = new TextBlock();
            chkTB = cb.Content as TextBlock;
            string fn = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.BScnPath) + chkTB.Text;
            string line;
            if (File.Exists(fn))
            {
                StreamReader thisSSS = new StreamReader(fn);
                while ((line = thisSSS.ReadLine()) != null)
                {
                    foreach (CheckBox chbox in listboxBaseSceneChoices.Items)
                    {
                        string cbTX = ((TextBlock)chbox.Content).Text;
                        if (cbTX.Equals(line, StringComparison.CurrentCultureIgnoreCase))
                        {
                            chbox.IsChecked = true;
                            UpdateListBoxBWithCheckedItemsInListBoxA(listboxBaseSceneChoices, listboxBaseSceneToArg);
                            break;
                        }
                    }
                }
                thisSSS.Close();
            }
        }

        private void UpdateListBoxBWithCheckedItemsInListBoxA(ListBox listBox_A, ListBox listBox_B)
        {
            foreach (CheckBox chkIA in listBox_A.Items)
            {
                TextBlock chkTB = new TextBlock();
                chkTB = chkIA.Content as TextBlock;
                string strIA = chkTB.Text;
                {
                    var mtchLBI = from ListBoxItem item in listBox_B.Items.Cast<ListBoxItem>()
                                  where item.Content.ToString().Equals(strIA, StringComparison.CurrentCultureIgnoreCase)
                                  select item as ListBoxItem;
                    ListBoxItem lbi = mtchLBI.FirstOrDefault();
                    /// lbi is now either the matching item or null for no match
                    if (chkIA.IsChecked == true)
                    {
                        /// Add to listB if not in listB.
                        /// Otherwise do nothing.
                        if (lbi == null)
                        {
                            ListBoxItem newItem = new ListBoxItem
                            {
                                Content = strIA
                            };
                            listBox_B.Items.Add(newItem);
                        }
                    }
                    else /// Remove any corresponding lbi if any
                    {
                        if (lbi != null) { listBox_B.Items.Remove(lbi); }
                    }
                }
            }
        }

        private void Listbox_mat_sel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbox_matfileview.Clear();
            groupBoxMatFile.Header = "Materials File View";
            ListBox lb = sender as ListBox;
            if (lb.SelectedItems.Count > 0)
            {
                ListBoxItem lbi = lb.SelectedItems[0] as ListBoxItem;
                string fn = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.MatPath) + lbi.Content.ToString();
                if (File.Exists(fn))
                {
                    tbox_matfileview.Text = File.ReadAllText(fn, Encoding.ASCII);
                    groupBoxMatFile.Header = "Materials File View" + " ==> " + lbi.Content.ToString();
                    tbox_matfileview.Tag = fn as Object;
                }
            }
        }

        private void But_view_useall_Click(object sender, RoutedEventArgs e)
        {
            CheckAllFromThisListBoxOfCheckBoxes(listbox_views_choices);
            UpdateListBoxBWithCheckedItemsInListBoxA(listbox_views_choices, listbox_view_sel);
            RegenCmdLines();
        }

        private void But_view_clear_Click(object sender, RoutedEventArgs e)
        {
            ClearCheckedFromThisListBoxOfCheckBoxes(listbox_views_choices);
            UpdateListBoxBWithCheckedItemsInListBoxA(listbox_views_choices, listbox_view_sel);
            RegenCmdLines();
        }

        private void Listbox_view_sel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbox_viewsfileview.Clear();
            groupBoxViews.Header = "Views File View";
            ListBox lb = sender as ListBox;
            if (lb.SelectedItems.Count > 0)
            {
                ListBoxItem lbi = lb.SelectedItems[0] as ListBoxItem;
                string fn = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ViewPath) + lbi.Content.ToString();
                if (File.Exists(fn))
                {
                    tbox_viewsfileview.Tag = fn as Object;
                    tbox_viewsfileview.Text = File.ReadAllText(fn, Encoding.ASCII);
                    groupBoxViews.Header = "Views File View" + " ==> " + lbi.Content.ToString();
                }
            }
        }

        private void But_basescene_useall_Click(object sender, RoutedEventArgs e)
        {
            CheckAllFromThisListBoxOfCheckBoxes(listboxBaseSceneChoices);
            UpdateListBoxBWithCheckedItemsInListBoxA(listboxBaseSceneChoices, listboxBaseSceneToArg);
            SynchronizeCheckboxListChoices(listboxBaseSceneChoices, listboxVarSceneChoices, null, listboxSelVarSceneToArg);
            UpDateSceneArgument();
        }

        private void But_basescene_clear_Click(object sender, RoutedEventArgs e)
        {
            ClearCheckedFromThisListBoxOfCheckBoxes(listboxBaseSceneChoices);
            UpdateListBoxBWithCheckedItemsInListBoxA(listboxBaseSceneChoices, listboxBaseSceneToArg);
            SynchronizeCheckboxListChoices(listboxBaseSceneChoices, listboxVarSceneChoices, null, listboxSelVarSceneToArg);
            ClearCheckedFromThisListBoxOfCheckBoxes(listboxSSS);
            UpDateSceneArgument();
        }

        private void But_varscene_useall_Click(object sender, RoutedEventArgs e)
        {
            CheckAllFromThisListBoxOfCheckBoxes(listboxVarSceneChoices);
            UpdateListBoxBWithCheckedItemsInListBoxA(listboxVarSceneChoices, listboxSelVarSceneToArg);
            UpDateSceneArgument();
        }

        private void But_varscene_clear_Click(object sender, RoutedEventArgs e)
        {
            ClearCheckedFromThisListBoxOfCheckBoxes(listboxVarSceneChoices);
            UpdateListBoxBWithCheckedItemsInListBoxA(listboxVarSceneChoices, listboxSelVarSceneToArg);
            UpDateSceneArgument();
        }

        private void ListboxScenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tbox_scenefileview.Clear();
            groupBoxSceneFile.Header = "Scene File View";
            ListBox lb = sender as ListBox;
            if (lb.SelectedItems.Count > 0)
            {
                ListBoxItem lbi = lb.SelectedItems[0] as ListBoxItem;
                string fn = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.BScnPath) + lbi.Content.ToString();
                if (File.Exists(fn))
                {
                    BkFileloadingArgs thisArgs = new BkFileloadingArgs
                    {
                        FNAME = fn
                    };

                    if (!bkFileLoadingWorker.IsBusy && TryToShowFile)
                    {
                        groupBoxSceneFile.Header = "Scene File View" + " ==> " + "Reading ... " + fn;
                        bkFileLoadingWorker.RunWorkerAsync(thisArgs);
                    }

                    //groupBoxSceneFile.Header = "Scene File View" + " ==> " + lbi.Content.ToString();
                    //tbox_scenefileview.Tag = fn as Object;
                    //thisRad.StatusMsg = temp;
                }
            }
        }

        private void BkFileLoadingWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BkFileloadingArgs theArgs = e.Argument as BkFileloadingArgs;
            string fname = theArgs.FNAME;
            if (File.Exists(fname))
            {
                theArgs.FILECONTENTS = File.ReadAllText(fname, Encoding.ASCII);
            }
            e.Result = theArgs;
        }

        private void BkFileLoadingWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BkFileloadingArgs theReturningArgs = e.Result as BkFileloadingArgs;
            string temp = string.Empty;
            if (e.Cancelled)
            {
                ThisRad.StatusMsg = "Canceled Reading!";
            }
            else if (e.Error != null)
            {
                ThisRad.StatusMsg = "Error. Details: " + (e.Error as Exception).ToString();
            }

            if (e.Result != null)
            {
                string fn = theReturningArgs.FNAME;
                tbox_scenefileview.Text = theReturningArgs.FILECONTENTS;
                groupBoxSceneFile.Header = "Scene File View" + " ==> " + System.IO.Path.GetFileName(fn);
               // tbox_scenefileview.Tag = fn as Object;
            }
        }

        class BkFileloadingArgs
        {
            public string FNAME { get; set; }
            public string FILECONTENTS { get; set; }
        }

        private void TboxProjectPath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string msg = "Pick the project's radiance path. " +
                "This will also be the location where the batch file will be created.";
            Helpers.SetTextToASelectedFolder(sender, msg, false, ThisRad.ProjectPath);
            FillAllChoices();
        }

        private void TboxRadInstallPath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the Radiance install path.", false, ThisRad.ProjectPath);
        }

        private void MakeRadPaths_Click(object sender, RoutedEventArgs e)
        {
            // SHEESH!!!
            foreach (TabItem ti in MainTabControl.Items)
            {
                if (ti.Name.Equals("tabitem_Utilities"))
                {
                    Dispatcher.BeginInvoke((Action)(() => MainTabControl.SelectedItem = ti));
                }
            }
        }

        private void Tboxskyfilepath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the skyfile path.", true, ThisRad.ProjectPath);
        }

        private void Tboxillumpath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the illum path.", true, ThisRad.ProjectPath);
        }

        private void Tbox_basescenepath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the base scene path.", true, ThisRad.ProjectPath);
            FillAllTheSceneChoices();
        }

        private void Tbox_varscenepath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the variable scene path.", true, ThisRad.ProjectPath);
            FillAllTheSceneChoices();
        }

        private void Tbox_octpath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the oct file path.", true, ThisRad.ProjectPath);
        }

        private void Tbox_ambpath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the amb file path.", true, ThisRad.ProjectPath);
        }

        private void Tbox_imgpath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the images file path.", true, ThisRad.ProjectPath);
        }

        private void Tbox_irradpath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the irrad file path.", true, ThisRad.ProjectPath);
        }

        private void Tbox_viewpath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the view file path.", true, ThisRad.ProjectPath);
        }

        private void Tbox_renderoptpath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the renderopts path.", true, ThisRad.ProjectPath);
            FillRenderOptionsChoices();
        }

        private void Tbox_matpath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the materials path.", true, ThisRad.ProjectPath);
            FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.MatPath), listbox_mat_choices, @"^.+\.(rad)$");
        }

        private void Tbox_radroot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the base path from which the typical radiance folders will be created.", false, ThisRad.ProjectPath);
            tbox_radwhereroot.Text = ThisRad.ProjectPath;
        }

        private void But_opts_setrenderopts_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_optionsfile_choices.SelectedItem is ListBoxItem lbi)
            {
                ThisRad.RenderOptsFileName = lbi.Content.ToString();
                RegenCmdLines();
            }
        }

        private void But_opts_setmkillumopts_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_optionsfile_choices.SelectedItem is ListBoxItem lbi)
            {
                ThisRad.MkillumOptsFileName = lbi.Content.ToString();
                RegenCmdLines();
            }
        }

        private void But_opts_setovertureopts_Click(object sender, RoutedEventArgs e)
        {
            if (listbox_optionsfile_choices.SelectedItem is ListBoxItem lbi)
            {
                ThisRad.OvertureOptsFileName = lbi.Content.ToString();
                RegenCmdLines();
            }
        }

        private void WriteBatchFile_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => MainTabControl.SelectedIndex = 0));
            int lines = WriteRadianceBatchFile(false);
            ThisRad.StatusMsg = "Wrote " + lines + " lines to " + ThisRad.BatchFileName +
                System.Environment.NewLine + DateTime.Now.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString();
        }

        private void AppendBatchFile_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => MainTabControl.SelectedIndex = 0));
            int lines = WriteRadianceBatchFile(true);
            ThisRad.StatusMsg = "Wrote " + lines + " lines into " + ThisRad.BatchFileName +
               System.Environment.NewLine + DateTime.Now.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString();
        }

        private void CheckAllPaths()
        {
            FillAllChoices();
        }

        private void TboxProjectPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath);
            CheckAllPaths();
        }

        private void TboxRadInstallPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath);
        }

        private void Tboxskyfilepath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
        }

        private void Tboxillumpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
        }

        private void Tboxskyfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForFile(sender as TextBox, ThisRad.ProjectPath, ThisRad.SkyFilePath);
        }

        private void Tboxillumfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForFile(sender as TextBox, ThisRad.ProjectPath, ThisRad.IllumPath);
        }

        private void Tbox_basescenepath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
        }

        private void Tbox_varscenepath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
        }

        private void Tbox_matpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
        }

        private void Tbox_octpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
        }

        private void Tbox_ambpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
        }

        private void Tbox_imgpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
        }

        private void Tbox_irradpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
        }

        private void Tbox_viewpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
        }

        private void Tbox_renderoptpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
        }

        private void Tbox_renderoptfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForFile(sender as TextBox, ThisRad.ProjectPath, ThisRad.RenderOptsPath);
        }

        private void Tbox_mkillumoptfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForFile(sender as TextBox, ThisRad.ProjectPath, ThisRad.RenderOptsPath);
        }

        private void Tbox_overtureoptfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForFile(sender as TextBox, ThisRad.ProjectPath, ThisRad.RenderOptsPath);
        }

        private void Tbox_viewname_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForFile(sender as TextBox, ThisRad.ProjectPath, ThisRad.ViewPath);
        }

        private void An_options_tbox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = sender as TextBox;
            SelectThisListBoxItem(listbox_optionsfile_choices, tb.Text);
        }

        private void SelectThisListBoxItem(ListBox theListBox, string itemText)
        {
            foreach (ListBoxItem lbi in theListBox.Items)
            {
                if (lbi.Content.ToString().Equals(itemText, StringComparison.CurrentCultureIgnoreCase))
                {
                    lbi.IsSelected = true;
                }
                else
                {
                    lbi.IsSelected = false;
                }
            }
        }

        private void Lb_renderoptfile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectThisListBoxItem(listbox_optionsfile_choices, ThisRad.RenderOptsFileName);
        }

        private void Lb_mkillumoptfile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectThisListBoxItem(listbox_optionsfile_choices, ThisRad.MkillumOptsFileName);
        }

        private void Lb_overtureoptfile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectThisListBoxItem(listbox_optionsfile_choices, ThisRad.OvertureOptsFileName);
        }

        /// batch file creation, returns the number of lines written
        private int WriteRadianceBatchFile(bool modeAppend)
        {
            string BatchFileName = ThisRad.BatchFileName;
            string BatchArgs = string.Empty;
            string fname = ThisRad.ProjectPath + BatchFileName;
            int Lines = 0;
            string PrjDrive = ThisRad.ProjectPath.Left(1);


            if (!Directory.Exists(ThisRad.ProjectPath))
            {
                string msg = "The project path " + ThisRad.ProjectPath + " does not exist.";
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, "Unable To Write BatchFile");
                explain.ShowDialog();
                return 0;
            }


            if (Check4MatScrn() == false) { return 0; }
            if (ConfirmThisWrite(fname, modeAppend) == false) { return 0; }

            try
            {
                StreamWriter file = new StreamWriter(fname, modeAppend);
                file.WriteLine("REM - Created by Radscript on " + DateTime.Now.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString());
                file.WriteLine("");
                Lines = Lines + 2;
                if (ThisRad.SetRadEnv)
                {
                    file.WriteLine("REM - Setting Radiance variables for this session");
                    file.WriteLine("");
                    file.WriteLine("PATH=" + ThisRad.RadPath + "bin\\;%PATH%");
                    file.WriteLine("set RAYPATH=" + ThisRad.RadPath + "lib");
                    file.WriteLine("");
                    Lines = Lines + 5;
                }
                BatchArgs = "cd /D " + ThisRad.ProjectPath;
                file.WriteLine(BatchArgs);
                Lines = Lines + 1;

                /// For night mode there is no daylight, hence no sky and therefore
                /// hour, day, month etc. Have no play.
                int mnthS = ThisRad.NightTime ? 1 : ThisRad.MStart;
                int mnthE = ThisRad.NightTime ? 1 : ThisRad.MEnd;
                int hrS = ThisRad.NightTime ? 1 : ThisRad.SHour;
                int hrE = ThisRad.NightTime ? 1 : ThisRad.EHour;
                double hrInc = ThisRad.NightTime ? 1 : ThisRad.HrInc;

                for (int mnth = mnthS; mnth <= mnthE; mnth++)
                {
                    for (double hr = hrS; hr <= hrE; hr += hrInc)
                    {
                        string str = string.Empty;
                        file.WriteLine("");
                        file.WriteLine("REM - Month: " + mnth.ToString() + " | Hour: " + hr.ToString());
                        Lines = Lines + 2;
                        // gensky
                        str = GenSkyCommand(mnth, ThisRad.SimDay, hr, ThisRad);
                        if (str.Length > 0)
                        {
                            file.WriteLine(str);
                            Lines = Lines + 1;
                        }
                        // oconv
                        str = OConvCommand(mnth, ThisRad.SimDay, hr, ThisRad);
                        if (str.Length > 0)
                        {
                            file.WriteLine(str);
                            Lines = Lines + 1;
                        }
                        // mkillum
                        str = MkillumCommand(mnth, ThisRad.SimDay, hr, ThisRad);
                        if (str.Length > 0)
                        {
                            file.WriteLine(str);
                            Lines = Lines + 1;
                        }
                        // reoconv
                        str = ReOconvCommand(mnth, ThisRad.SimDay, hr, ThisRad);
                        if (str.Length > 0)
                        {
                            file.WriteLine(str);
                            Lines = Lines + 1;
                        }
                        // rpict
                        str = RpictCommand(mnth, ThisRad.SimDay, hr, ThisRad);
                        if (str.Length > 0)
                        {
                            file.WriteLine(str);
                            Lines = Lines + 1;
                        }
                        if (ThisRad.IncRpictI)
                        {
                            // rpict -i
                            str = RPictICommand(mnth, ThisRad.SimDay, hr, ThisRad);
                            if (str.Length > 0)
                            {
                                file.WriteLine(str);
                                Lines = Lines + 1;
                            }
                        }
                        else
                        {
                            file.WriteLine("REM - We are excluding rpict -i.");
                            Lines = Lines + 1;
                        }
                    }
                }
                file.WriteLine("pause");
                file.Close();
                return Lines;
            }
            catch (Exception e)
            {
                string msg = e.ToString();
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, "Error at WriteRadianceBatchFile");
                explain.ShowDialog();
                return 0;
            }
        }

        /// pcomb batch file creation, returns the number of lines written
        private int WritePCombBatchFile(bool modeAppend)
        {
            string PcombBatchFileName = ThisRad.PcombBatchFileName;
            string BatchArgs = string.Empty;
            string fname = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ImagePath, PcombBatchFileName);
            int Lines = 0;
            string PrjDrive = ThisRad.ProjectPath.Left(1);

            if (!Directory.Exists(ThisRad.ProjectPath))
            {
                string msg = "The project path " + ThisRad.ProjectPath + " does not exist.";
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, "Unable To Write BatchFile");
                explain.ShowDialog();
                return 0;
            }

            if (PcombBatchFileName.Trim() == "")
            {
                string msg = "A file name is needed.";
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, "Unable To Write BatchFile");
                explain.ShowDialog();
                return 0;
            }

            if (ConfirmThisWrite(fname, modeAppend) == false) { return 0; }

            try
            {
                StreamWriter file = new StreamWriter(fname, modeAppend);
                file.WriteLine("REM - Created by Radscript on " + DateTime.Now.ToLongDateString() + "  " + DateTime.Now.ToLongTimeString());
                file.WriteLine("");
                Lines = Lines + 2;

                file.WriteLine("REM - Setting Radiance variables for this session");
                file.WriteLine("");
                file.WriteLine("PATH=" + ThisRad.RadPath + "bin\\;%PATH%");
                file.WriteLine("set RAYPATH=" + ThisRad.RadPath + "lib");
                file.WriteLine("");
                Lines = Lines + 5;

                BatchArgs = "cd /D " + ThisRad.ProjectPath;
                file.WriteLine(BatchArgs);
                Lines = Lines + 1;

                foreach (TextBlock tb in listbox_pcomblines.Items)
                {
                    BatchArgs = tb.Text + System.Environment.NewLine;
                    file.WriteLine(BatchArgs);
                    Lines = Lines + 1;
                }

                if (ThisRad.Pcombpause) { file.WriteLine("pause"); }
                file.Close();
                return Lines;
            }
            catch (Exception e)
            {
                string msg = e.ToString();
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, "Error at WritePCombBatchFile");
                explain.ShowDialog();
                return 0;
            }
        }

        private bool Check4MatScrn()
        {
            string title = null;
            if (string.IsNullOrEmpty(mat_arg.Text) | string.IsNullOrEmpty(scene_arg.Text))
            {
                if (string.IsNullOrEmpty(mat_arg.Text) & string.IsNullOrEmpty(scene_arg.Text))
                {
                    title = "Materials and Scene(s) Are Missing";
                }
                if (string.IsNullOrEmpty(mat_arg.Text) & !string.IsNullOrEmpty(scene_arg.Text))
                {
                    title = "Materials Are Missing";
                }
                if (!string.IsNullOrEmpty(mat_arg.Text) & string.IsNullOrEmpty(scene_arg.Text))
                {
                    title = "Scene(s) Are Missing";
                }
                string msg = title;
                FormMsgWPF explain = new FormMsgWPF(null, 2);
                explain.SetMsg(msg, "Write batch file anyway?");
                explain.ShowDialog();
                if (explain.theResult == MessageBoxResult.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private bool ConfirmThisWrite(string batchfname, bool modeAppend)
        {
            string BatchFileName = ThisRad.BatchFileName;
            string msg = null;
            string msgT = null;
            switch (modeAppend)
            {
                case false:
                    msg = "About to write batch file:" + "\n\n" + batchfname;
                    if (File.Exists(batchfname))
                    {
                        msg = msg + "\n\n" + "<<< An existing file will be overwritten. >>>";
                        msgT = "Confirm OverWriting the Batch File";
                    }
                    else
                    {
                        msgT = "Confirm Writing the Batch File";
                    }
                    break;
                case true:
                    if (!File.Exists(batchfname))
                    {
                        msg = "About to write batch file:" + "\n\n" + batchfname;
                        msg = msg + "\n\n" + "<<< There is not an existing file to add to. >>>";
                        msgT = "Not Exactly What You Wanted";
                    }
                    else
                    {
                        msg = "About to add to batch file:" + "\n\n" + batchfname;
                        msgT = "Confirm Adding to the Batch File";
                    }

                    break;
            }
            FormMsgWPF explain = new FormMsgWPF(null, 2);
            explain.SetMsg(msg, msgT);
            explain.ShowDialog();
            if (explain.theResult == MessageBoxResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void EditViewFile(string fn)
        {
            if (File.Exists(fn))
            {
                Process.Start("NotePad", fn);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string fn = ThisRad.ProjectPath + ThisRad.BatchFileName;
            EditViewFile(fn);
        }

        private void Tbox_matfileview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Tag = tbox_matfileview.Tag;
            if (Tag == null) { return; }
            string fn = Tag.ToString();
            EditViewFile(fn);
        }

        private void Tbox_scenefileview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Tag = tbox_scenefileview.Tag;
            if (Tag == null) { return; }
            string fn = Tag.ToString();
            EditViewFile(fn);
        }

        private void Explorer_Click(object sender, RoutedEventArgs e)
        {
            string path = ThisRad.ProjectPath;
            Helpers.RunExplorerHere(path);
        }

        private void ExplBSP_Click(object sender, RoutedEventArgs e)
        {
            string path = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.BScnPath);
            Helpers.RunExplorerHere(path);
        }

        private void ExplVSP_Click(object sender, RoutedEventArgs e)
        {
            string path = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.VScnPath);
            Helpers.RunExplorerHere(path);
        }

        private void ExplMat_Click(object sender, RoutedEventArgs e)
        {
            string path = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.MatPath);
            Helpers.RunExplorerHere(path);
        }

        private void Tbox_viewsfileview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Tag = tbox_viewsfileview.Tag;
            if (Tag == null) { return; }
            string fn = Tag.ToString();
            EditViewFile(fn);
        }

        private void Tbox_optionsfileview_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Tag = tbox_optionsfileview.Tag;
            if (Tag == null) { return; }
            string fn = Tag.ToString();
            EditViewFile(fn);
        }

        private void But_save_sss_Click(object sender, RoutedEventArgs e)
        {
            string fn = txb_sss_name.Text;
            if (fn.Length > 0 && listboxBaseSceneToArg.Items.Count > 0)
            {
                string fpn = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.BScnPath) + fn;
                if (!fpn.EndsWith(SSSExt, StringComparison.CurrentCultureIgnoreCase))
                {
                    fpn = fpn + SSSExt;
                }
                try
                {
                    if (File.Exists(fpn))
                    {
                        string msg = "Overwrite existing SSS file " + System.IO.Path.GetFileName(fpn);
                        string msgT = "Confirm";
                        FormMsgWPF explain = new FormMsgWPF(null, 2);
                        explain.SetMsg(msg, msgT);
                        explain.ShowDialog();
                        if (explain.theResult == MessageBoxResult.No) { return; }
                    }
                    StreamWriter file = new StreamWriter(fpn);
                    foreach (ListBoxItem lbi in listboxBaseSceneToArg.Items)
                    {
                        file.WriteLine(lbi.Content.ToString());
                    }
                    file.Close();
                    FillChoiceList(Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.BScnPath), listboxSSS, Helpers.MakeFilesListRegexString(fileExtSceneSets));
                    txb_sss_name.Clear();
                }
                catch (Exception) { }
            }
        }

        private void But_basescene_up_Click(object sender, RoutedEventArgs e)
        {
            if (listboxBaseSceneToArg.SelectedItems.Count > 0 && listboxBaseSceneToArg.Items.Count > 1)
            {
                int ndx = listboxBaseSceneToArg.SelectedIndex;
                if (ndx > 0)
                {
                    TryToShowFile = false;
                    ListBoxItem theOneToSelected = (ListBoxItem)listboxBaseSceneToArg.Items[ndx];
                    ListBoxItem theOneToBeforeSelected = (ListBoxItem)listboxBaseSceneToArg.Items[ndx - 1];
                    string temp = theOneToSelected.Content.ToString();
                    theOneToSelected.Content = theOneToBeforeSelected.Content;
                    theOneToBeforeSelected.Content = temp;
                    theOneToBeforeSelected.IsSelected = true;
                    theOneToSelected.IsSelected = false;
                    listboxBaseSceneToArg.Focus();
                    TryToShowFile = true;
                }
            }
        }

        private void But_basescene_dwn_Click(object sender, RoutedEventArgs e)
        {
            if (listboxBaseSceneToArg.SelectedItems.Count > 0 && listboxBaseSceneToArg.Items.Count > 1)
            {
                int ndx = listboxBaseSceneToArg.SelectedIndex;
                if (ndx < listboxBaseSceneToArg.Items.Count - 1)
                {
                    TryToShowFile = false;
                    ListBoxItem theOneToSelected = (ListBoxItem)listboxBaseSceneToArg.Items[ndx];
                    ListBoxItem theOneToAfterSelected = (ListBoxItem)listboxBaseSceneToArg.Items[ndx + 1];
                    string temp = theOneToSelected.Content.ToString();
                    theOneToSelected.Content = theOneToAfterSelected.Content;
                    theOneToAfterSelected.Content = temp;
                    theOneToAfterSelected.IsSelected = true;
                    theOneToSelected.IsSelected = false;
                    listboxBaseSceneToArg.Focus();
                    TryToShowFile = true;
                }
            }
        }

        private void Button_ShellBatch_Click(object sender, RoutedEventArgs e)
        {
            string msg = ThisRad.BatchFileName;
            string msgT = "Shell This Batch?";
            FormMsgWPF explain = new FormMsgWPF(null, 2);
            explain.SetMsg(msg, msgT);
            explain.ShowDialog();
            if (explain.theResult == MessageBoxResult.No) { return; }
            string fpn = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.BatchFileName);
            Process batchProcess = new Process();
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + fpn);
            var process = Process.Start(processInfo);
        }

        private void But_regexhelp_Click(object sender, RoutedEventArgs e)
        {
            Helpers.WebBrowserToHere(@"http://regexstorm.net/reference");
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.WebBrowserToHere(@"http://radsite.lbl.gov/radiance/man_html/gensky.1.html");
        }

        private void Lb_oconv_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.WebBrowserToHere(@"http://radsite.lbl.gov/radiance/man_html/oconv.1.html");
        }

        private void Lb_ocon2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.WebBrowserToHere(@"http://radsite.lbl.gov/radiance/man_html/oconv.1.html");
        }

        private void Lb_mkillum_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.WebBrowserToHere(@"http://radsite.lbl.gov/radiance/man_html/mkillum.1.html");
        }

        private void Lb_rpict_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.WebBrowserToHere(@"http://radsite.lbl.gov/radiance/man_html/rpict.1.html");
        }

        private void Lb_rpicti_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.WebBrowserToHere(@"http://radsite.lbl.gov/radiance/man_html/rpict.1.html");
        }

        /// <summary>
        /// Load a new icon bitmap from embedded resources.
        /// For the BitmapImage, make sure you reference WindowsBase and Presentation Core
        /// and PresentationCore, and import the System.Windows.Media.Imaging namespace. 
        /// </summary>
        BitmapImage NewBitmapImage(System.Reflection.Assembly a, string imageName)
        {
            Stream s = a.GetManifestResourceStream(imageName);
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = s;
            img.EndInit();
            return img;
        }

        private void CreateRadFolders()
        {
            Grid gd = grid_radfolders;
            foreach (var item in gd.Children)
            {
                if (item.GetType() == typeof(CheckBox))
                {
                    CheckBox thisChkBox = item as CheckBox;
                    TextBlock thisTB = thisChkBox.Content as TextBlock;
                    string fldName = Helpers.CombineIntoPath(ThisRad.ProjectPath, thisTB.Text);
                    if (thisChkBox.IsEnabled && (bool)thisChkBox.IsChecked)
                    {
                        try
                        {
                            Directory.CreateDirectory(fldName);
                        }
                        catch (Exception e)
                        {
                            string msg = fldName + "\n\n" + e.Message;
                            FormMsgWPF explain = new FormMsgWPF(null, 3);
                            explain.SetMsg(msg, "Unable To Create Folder");
                            explain.ShowDialog();
                        }
                    }
                }
            }
        }

        private void UnCheckRadFolders()
        {
            Grid gd = grid_radfolders;
            foreach (var item in gd.Children)
            {
                if (item.GetType() == typeof(CheckBox))
                {
                    CheckBox thisChkBox = item as CheckBox;
                    if (thisChkBox.IsEnabled)
                    {
                        thisChkBox.IsChecked = false;
                    }
                }
            }
        }

        private void CheckForRadFolder()
        {
            Grid gd = grid_radfolders;
            foreach (var item in gd.Children)
            {
                if (item.GetType() == typeof(CheckBox))
                {
                    CheckBox thisChkBox = item as CheckBox;
                    if (thisChkBox.Content is TextBlock thisTB)
                    {
                        string fldName = Helpers.CombineIntoPath(ThisRad.ProjectPath, thisTB.Text);
                        if (Directory.Exists(fldName))
                        {
                            thisChkBox.IsChecked = true;
                            thisChkBox.IsEnabled = false;
                        }
                        else
                        {
                            thisChkBox.IsChecked = false;
                            thisChkBox.IsEnabled = true;
                        }
                    }
                }
            }
        }

        private void BuildRadFoldersList()
        {
            List<string> radsubfolders = new List<string> { @".\ambfile",
                                                            @".\calcs",
                                                            @".\files",
                                                            @".\illums",
                                                            @".\images",
                                                            @".\material",
                                                            @".\octree",
                                                            @".\optfile",
                                                            @".\scene",
                                                            @".\skies",
                                                            @".\views",
                                                            @".\date",
                                                            @".\glare",
                                                            @".\images\irradiance",
                                                            @".\images\pconds",
                                                            @".\images\tiffs",
                                                            @".\parameter_text"
            };

            Grid gd = grid_radfolders;
            int lc = radsubfolders.Count();
            int rr = (int)Math.Ceiling((double)lc / (double)6);
            for (int i = 0; i < gd.RowDefinitions.Count(); i++) { gd.RowDefinitions.RemoveAt(i); }

            for (int i = 1; i < rr; i++)
            {
                RowDefinition nrd = new RowDefinition
                {
                    MinHeight = 26
                };
                gd.RowDefinitions.Add(nrd);
            }

            int rw = gd.RowDefinitions.Count();
            int cc = gd.ColumnDefinitions.Count();
            int rindx = 1;
            int cindx = 0;
            foreach (string fldrN in radsubfolders)
            {
                CheckBox chkFldr = new CheckBox();
                TextBlock chkFldrTB = new TextBlock
                {
                    Text = fldrN
                };
                chkFldr.Content = chkFldrTB;
                chkFldr.IsChecked = true;
                Grid.SetRow(chkFldr, rindx);
                Grid.SetColumn(chkFldr, cindx);
                gd.Children.Add(chkFldr);
                cindx++;
                if (cindx == cc)
                {
                    cindx = 0;
                    rindx++;
                    if (rindx == rw) { rindx = 1; }
                }
            }
            CheckForRadFolder();
        }

        private void Mkrpaths_clear_Click(object sender, RoutedEventArgs e)
        {
            UnCheckRadFolders();
        }

        private void Mkrpaths_doit_Click(object sender, RoutedEventArgs e)
        {
            CreateRadFolders();
            CheckForRadFolder();
        }

        private void But_clearpcomb_Click(object sender, RoutedEventArgs e)
        {
            Tblock_radheader.Text = String.Empty;
            ClearCheckedFromThisListBoxOfCheckBoxes(listbox_pcomb_choices);
            foreach (string key in PCombFilesCheckDictionary.Keys.ToList())
            { PCombFilesCheckDictionary[key] = false; }
            listbox_pcomb_selections.Items.Clear();
            BuildThePCombLine();
        }

        /// <summary>
        /// Use this to capture when a tab is selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl thisTabControl = sender as TabControl;
            if (thisTabControl.SelectedItem is TabItem thisTab)
            {
                if (thisTab.Name == "tabitem_PComb")
                {
                    string fullPath = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ImagePath);
                    PCombFilesCheckDictionary = Helpers.BuildCheckListDictionary(fullPath, fileNameExtensionPComb);
                    // MessageBox.Show(thisTab.Name);
                }
            }

        }

        private void Tbox_anyscale_KeyUp(object sender, KeyEventArgs e)
        {
            BuildThePCombLine();
        }

        private void Bt_regen_pcomb_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string fullPath = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ImagePath);
            PCombFilesCheckDictionary = Helpers.BuildCheckListDictionary(fullPath, fileNameExtensionPComb);
        }

        private void ProcessTargetForStrangeness(object sender)
        {
            //TextBox tb = sender as TextBox;
            //string targetFN = tb.Text;
            //int pos = tb.SelectionStart - 1;
            //if (targetFN.Contains("/"))
            //{
            //    tb.Text = targetFN.Replace("/", "\\");
            //    tb.SelectionStart = pos;
            //    targetFN = tb.Text;
            //}
            //if (targetFN.Contains("\\\\"))
            //{
            //    tb.Text = targetFN.Replace("\\\\", "\\");
            //    tb.SelectionStart = pos;
            //    targetFN = tb.Text;
            //}
            //if (targetFN.Contains(".\\"))
            //{
            //    tb.Text = targetFN.Replace(".\\", "\\");
            //    tb.SelectionStart = pos;
            //}
            //targetFN = tb.Text;
            //pos = targetFN.LastIndexOf("\\");
            //if (pos > 0)
            //{
            //    string subpath = targetFN.Substring(0, pos);
            //    subpath = (ThisRad.PcombimagetargetPath + subpath).Replace("\\\\", "\\");
            //    ThisRad.StatusMsg = subpath;

            //    //newPathToCreate = Helpers.CombineIntoPath(ThisRad.ProjectPath, subpath) + "\\";
            //    //if (!Directory.Exists(newPathToCreate))
            //    //{
            //    //    button_makeimagesubpath.Visibility = Visibility.Visible;
            //    //    ThisRad.StatusMsg = "Will create this path:\n" + newPathToCreate;
            //    //}
            //    //else
            //    //{
            //    //    button_makeimagesubpath.Visibility = Visibility.Collapsed;
            //    //    ThisRad.StatusMsg = "Radiance Session Setup";
            //    //    newPathToCreate = string.Empty;
            //    //}

            //}
            //else
            //{
            //    button_makeimagesubpath.Visibility = Visibility.Collapsed;
            //    ThisRad.StatusMsg = "Radiance Session Setup";
            //    newPathToCreate = string.Empty;
            //}
        }

        private void Textbox_pcom_target_KeyUp(object sender, KeyEventArgs e)
        {
            ProcessTargetForStrangeness(sender);
            Helpers.SniffTextBoxToBeAValidFileName(sender as TextBox);
            BuildThePCombLine();
        }

        private void RemoveComboRegex(object sender, RoutedEventArgs e)
        {
            string toremove = combo_pcomfilter.Text.Trim();
            if (toremove.Equals(string.Empty)) { return; }
            string msg = "\'" + toremove + "\'" + " will be removed from the regular expression choices.";
            FormMsgWPF explain = new FormMsgWPF(null, 2);
            explain.SetMsg(msg, "Delete This Expression?");
            explain.ShowDialog();
            if (explain.theResult == MessageBoxResult.OK)
            {
                combo_pcomfilter.Text = string.Empty;
                AddCheckStateToDictionary(listbox_pcomb_choices, PCombFilesCheckDictionary);
                RegexFilterForPcombValue();
                //ThisRad.Pcombfilters.Remove(toremove);
                Pcombfilters.Remove(toremove);
                combo_pcomfilter.ItemsSource = Pcombfilters;
            }

        }

        private void Listbox_pcomb_selections_PreviewDragLeave(object sender, DragEventArgs e)
        {
            ListBox lb = sender as ListBox;
            int index = GetListBoxCurrentRowIndex(e.GetPosition, lb);
            if (index < 0)
            {
                myMoverPopup.IsOpen = false;
                inPCombReposition = false;
                return;
            }
            //string arrow = " => ";
            //ThisRad.StatusMsg = (prevRowIndex + 1).ToString() + " to " + (index+1).ToString();
            //string FrmTo = "\nPos " +(prevRowIndex + 1).ToString() + " to Pos " + (index + 1).ToString();
            //string t = popMoverText.Text;
            //int cutstart = t.LastIndexOf(arrow);
            //if (cutstart > -1) {
            //    int trim = t.Length - cutstart;
            //    string org = t.Remove(cutstart, trim);
            //    popMoverText.Text = org + arrow + FrmTo;
            //} else {
            //    popMoverText.Text = t + arrow + FrmTo;
            //}
        }

        void Listbox_pcomb_selections_Drop(object sender, DragEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (prevRowIndex < 0) { return; }
            //int index = lb.SelectedIndex;
            int index = GetListBoxCurrentRowIndex(e.GetPosition, lb);

            if (index < 0) { return; }//The current Rowindex is -1 (No selected)
            if (index == prevRowIndex) { return; } //If Drag-Drop Location are same

            ListBoxItem movedItem = lb.Items[prevRowIndex] as ListBoxItem;

            lb.Items.RemoveAt(prevRowIndex);
            lb.Items.Insert(index, movedItem);
            //lb.SelectedIndex = index;
            lb.SelectedItem = GetListBoxRowItem(index, lb);
            BuildThePCombLine();
        }

        void Listbox_pcomb_selections_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = sender as ListBox;
            //prevRowIndex = lb.SelectedIndex;
            prevRowIndex = GetListBoxCurrentRowIndex(e.GetPosition, lb);


            if (prevRowIndex < 0) { return; }
            lb.SelectedIndex = prevRowIndex;

            ListBoxItem selectedItem = lb.Items[prevRowIndex] as ListBoxItem;

            if (selectedItem == null) { return; }

            //Now Create a Drag Rectangle with Mouse Drag-Effect
            //Here you can select the Effect as per your choice

            DragDropEffects dragdropeffects = DragDropEffects.Move;
            popMoverText.Text = "Moving:  " + selectedItem.Content.ToString();
            myMoverPopup.IsOpen = true;
            inPCombReposition = true;

            if (DragDrop.DoDragDrop(lb, selectedItem, dragdropeffects) != DragDropEffects.None)
            {
                //Now This Item will be dropped at new location and so the new Selected Item
                lb.SelectedItem = selectedItem;
                myMoverPopup.IsOpen = false;
                inPCombReposition = false;
            }
        }

        private void Listbox_pcomb_selections_LostFocus(object sender, RoutedEventArgs e)
        {
            if (inPCombReposition && myMoverPopup.IsOpen)
            {
                myMoverPopup.IsOpen = false;
                inPCombReposition = false;
            }
        }

        private void Listbox_pcomb_selections_DragOver(object sender, DragEventArgs e)
        {
            if (inPCombReposition && myMoverPopup.IsOpen)
            {
                double x = e.GetPosition(this).X + this.Left;
                double y = e.GetPosition(this).Y + this.Top;
                myMoverPopup.Placement = PlacementMode.AbsolutePoint;
                myMoverPopup.HorizontalOffset = x;
                myMoverPopup.VerticalOffset = y;
            }
        }

        /// <summary>
        /// Method checks whether the mouse is on the required Target
        /// Input Parameter (1) "Visual" -> Used to provide Rendering support to WPF
        /// Input Paraneter (2) "User Defined Delegate" positioning for Operation
        /// </summary>
        /// <param name="theTarget"></param>
        /// <param name="pos"></param>
        /// <returns>The "Rect" Information for specific Position</returns>
        private bool IsTheMouseOnTargetRow(Visual theTarget, GetDragDropPosition pos)
        {
            if (theTarget == null) { return false; }
            Rect posBounds = VisualTreeHelper.GetDescendantBounds(theTarget);
            Point theMousePos = pos((IInputElement)theTarget);
            return posBounds.Contains(theMousePos);
        }

        /// <summary>
        /// Returns the selected ListBoxItem
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private ListBoxItem GetListBoxRowItem(int index, ListBox lb)
        {
            if (lb.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;
            return lb.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        }

        /// <summary>
        /// Returns the Index of the Current Row.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private int GetListBoxCurrentRowIndex(GetDragDropPosition pos, ListBox lb)
        {
            int curIndex = -1;
            for (int i = 0; i < lb.Items.Count; i++)
            {
                ListBoxItem itm = GetListBoxRowItem(i, lb);
                if (IsTheMouseOnTargetRow(itm, pos))
                {
                    curIndex = i;
                    break;
                }
            }
            return curIndex;
        }

        private void But_addpcombline_Click(object sender, RoutedEventArgs e)
        {
            string pcombline = tbox_pcombcandidate.Text;
            TextBlock ntblk = new TextBlock() { Text = pcombline };
            if (Helpers.MarkPCombTargetForFile(ThisRad.PcombimagetargetPath, ThisRad.ProjectPath, textbox_pcom_target, listbox_pcomblines, groupboxTargetfile))
            {
                string msg = "The other image '" + textbox_pcom_target.Text + "' will be overwritten by the one you are about to add.";
                string msgT = "Use This Target Name?";
                FormMsgWPF explain = new FormMsgWPF(null, 2);
                explain.SetMsg(msg, msgT);
                explain.ShowDialog();
                if (explain.theResult != MessageBoxResult.OK) { return; }
            }
            listbox_pcomblines.Items.Add(ntblk);
            SetPCombBatchButtons();
        }

        private void Listbox_pcomblines_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                ListBox lb = sender as ListBox;
                if (lb.SelectedItems.Count == 0) { return; }
                string msg = string.Empty;
                string ttl = "Delete This PComb Command Line?";
                if (lb.SelectedItems.Count > 1) { ttl = "Delete These PComb Command Lines?"; }
                foreach (TextBlock tbi in lb.SelectedItems)
                {
                    msg = msg + tbi.Text + "\n";
                }
                FormMsgWPF explain = new FormMsgWPF(null, 2);
                explain.SetMsg(msg, ttl);
                explain.ShowDialog();
                if (explain.theResult == MessageBoxResult.OK)
                {
                    for (int i = lb.SelectedItems.Count - 1; i > -1; i--)
                    {
                        lb.Items.Remove(lb.SelectedItems[i]);
                    }
                }
                Helpers.MarkPCombTargetForFile(ThisRad.PcombimagetargetPath, ThisRad.ProjectPath, textbox_pcom_target, listbox_pcomblines, groupboxTargetfile);
                SetPCombBatchButtons();
            }
        }

        private void Button_makeimagesubpath_Click(object sender, RoutedEventArgs e)
        {
            if (newPathToCreate != null)
            {
                Helpers.CreateThisPath(newPathToCreate);
                SniffOutPcombImageTargetPath(Tbox_pcombimagetargetpath);
                BuildThePCombLine();
            }
        }

        private void But_pccomb_Click(object sender, RoutedEventArgs e)
        {
            string args = tbox_pcombcandidate.Text;
            string target = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ImagePath) + textbox_pcom_target.Text;
            Helpers.PCombThisLine(ThisRad, args, target);
        }

        private void Tb_hdrviewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFile(sender, "Pick the HDR viewer application.", true);
        }

        private void Label_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            Helpers.WebBrowserToHere("http://radsite.lbl.gov/radiance/man_html/pcomb.1.html");
        }

        private void Tog_pcombh_Click(object sender, RoutedEventArgs e)
        {
            BuildThePCombLine();
        }

        private void Tog_pcombh_Unchecked(object sender, RoutedEventArgs e)
        {
            ToggleButton tog = sender as ToggleButton;
            tog.BorderBrush = ColorExt.ToBrush(System.Drawing.Color.Red);
        }

        private void Tog_pcombh_Checked(object sender, RoutedEventArgs e)
        {
            ToggleButton tog = sender as ToggleButton;
            tog.BorderBrush = ColorExt.ToBrush(System.Drawing.Color.DarkGray);
        }

        private void But_makepcombscript_Click(object sender, RoutedEventArgs e)
        {
            WritePCombBatchFile(false);
        }

        private void But_viewpcombscript_Click(object sender, RoutedEventArgs e)
        {
            string fn = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ImagePath, ThisRad.PcombBatchFileName);
            EditViewFile(fn);
        }

        private void But_executepcombscript_Click(object sender, RoutedEventArgs e)
        {
            if (ThisRad.PcombBatchFileName == null) { return; }
            string fpn = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.ImagePath, ThisRad.PcombBatchFileName);
            if (!File.Exists(@fpn)) { return; }
            string msg = ThisRad.PcombBatchFileName;
            string msgT = "Run This PComb Script?";
            FormMsgWPF explain = new FormMsgWPF(null, 2);
            explain.SetMsg(msg, msgT);
            explain.ShowDialog();
            if (explain.theResult == MessageBoxResult.No) { return; }
            Process batchProcess = new Process();
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + fpn);
            var process = Process.Start(processInfo);
        }

        private void Tbox_pcombscriptname_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetPCombBatchButtons();
            Helpers.SniffTextBoxToBeAValidFileName(sender as TextBox);
        }

        private void Tbox_picsourcefolder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the pic source file path.", true, ThisRad.ProjectPath);
            FillRATiffChoices();
        }

        private void Label_MouseDoubleClick_2(object sender, MouseButtonEventArgs e)
        {
            Helpers.WebBrowserToHere(@"http://radsite.lbl.gov/radiance/man_html/ra_tiff.1.html");
        }

        private void Lb_radiance_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.WebBrowserToHere(@"http://radsite.lbl.gov/radiance/whatis_comp.html");
        }

        private void Tbox_tiffresultsfolder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the pic to tiff results file path.", true, ThisRad.ProjectPath);
            SetupForTiffImages();
        }

        private void SetupForTiffImages()
        {
            ClearTiffImage();
            FillTiffsChoices();
        }

        private void ClearTiffImage()
        {
            img_tiffconversion.Source = null;
            lbl_tiffname.Content = String.Empty;
        }

        private void Listbox_tiff_results_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThisRad.StatusMsg = "";
            DoTiffResultPick(sender);
        }

        private void Listbox_tiff_results_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ThisRad.StatusMsg = "";
            DoTiffResultPick(sender);
        }

        private void ClearImageView(Image theimage, Label thelable)
        {
            theimage.Source = null;
            thelable.Content = String.Empty;
        }

        private void DoTiffResultPick(object sender)
        {
            Image thisImage = img_tiffconversion;
            Label thisLabel = lbl_tiffname;
            ListBox lb = sender as ListBox;
            ListBoxItem lbi = lb.SelectedItem as ListBoxItem;
            if (lbi != null)
            {
                ClearImageView(thisImage, thisLabel);
                string realName = lbi.Content.ToString().Replace("_", "__");
                String fname = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.RatiffimagePath, lbi.Content.ToString());

                lbl_tiffname.Content = realName;
                if (fname != string.Empty)
                {
                    thisLabel.Content = "Loading image ...";

                    TiffViewArgs TiffArgs = new TiffViewArgs();
                    TiffArgs.ThisImageUri = new Uri(fname);
                    TiffArgs.TheRealName = realName;
                    TiffArgs.ThisImageSource = null;
                    TiffArgs.TheTargetWPFImagename = thisImage.Name;
                    TiffArgs.TheTargetWPFLabelname = thisLabel.Name;

                    if (!bwTiffViewer.IsBusy)
                    {
                        thisImage.Source = null;
                        bwTiffViewer.RunWorkerAsync(TiffArgs);
                    }

                    //img_tiffconversion.Source = null;
                    //Uri _imageUri = new Uri(fname);
                    //img_tiffconversion.Source = _imageUri.GetBitmapImage(BitmapCacheOption.OnLoad);
                }
            }
        }

        private void DoFinalResultSourcePick(CheckBox cb)
        {
            Image thisImage = img_magicsource;
            Label thisLabel = lbl_magicksource;
            if (!(bool)cb.IsChecked)
            {
                ClearImageView(thisImage, thisLabel);
                return;
            }
            TextBlock tbi = cb.Content as TextBlock;
            if (tbi != null)
            {
                string realName = tbi.Text.Replace("_", "__");
                String fname = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.RatiffimagePath, tbi.Text);

                if (fname != string.Empty)
                {
                    thisLabel.Content = "Loading image ...";

                    TiffViewArgs TiffArgs = new TiffViewArgs();
                    TiffArgs.ThisImageUri = new Uri(fname);
                    TiffArgs.TheRealName = realName;
                    TiffArgs.ThisImageSource = null;
                    TiffArgs.TheTargetWPFImagename = thisImage.Name;
                    TiffArgs.TheTargetWPFLabelname = thisLabel.Name;

                    if (!bwTiffViewer.IsBusy)
                    {
                        thisImage.Source = null;
                        bwTiffViewer.RunWorkerAsync(TiffArgs);
                    }
                }
            }
        }

        private void DoFinalResultPick(object sender)
        {
            ListBox lb = sender as ListBox;
            ListBoxItem lbi = lb.SelectedItem as ListBoxItem;
            if (lbi != null)
            {
                String fname = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.FinalimagePath, lbi.Content.ToString());
                if (fname != string.Empty)
                {
                    img_magicconversion.Source = null;
                    lbl_magickname.Content = "Loading image ...";
                    Uri _imageUri = new Uri(fname);
                    img_magicconversion.Source = _imageUri.GetBitmapImage(BitmapCacheOption.OnLoad);
                    lbl_magickname.Content = lbi.Content.ToString().Replace("_", "__");
                }
            }
        }

        private void But_runratiff_Click(object sender, RoutedEventArgs e)
        {
            if (bwPicToTiff.IsBusy == true)
            {
                ThisRad.StatusMsg = "OK, cancel is pending. Hold your horses.";
                bwPicToTiff.CancelAsync();
                but_runratiff.Content = "Run Ra__TIFF";
                but_runratiff.ClearValue(BackgroundProperty);
                return;
            }
            RunRaToTiffSelections();
            DoTiffResultPick(listbox_tiff_results);
        }

        private void RunRaToTiffSelections()
        {
            PicsToTiffArgs thisPTTA = new PicsToTiffArgs();
            ListBox lb = listbox_pic_choices;
            List<string> itemsChecked = new List<string>();
            foreach (var vb in lb.Items)
            {
                if (vb.GetType() != typeof(CheckBox)) { continue; }
                CheckBox cb = vb as CheckBox;
                if (!(bool)cb.IsChecked) { continue; }
                TextBlock tbi = cb.Content as TextBlock;
                itemsChecked.Add(tbi.Text);
            }
            if (itemsChecked.Count == 0) { return; }

            thisPTTA.ItemsChecked = itemsChecked;
            thisPTTA.PathAdd = @ThisRad.RadPath + @"bin;";
            thisPTTA.RayPather = String.Concat(@ThisRad.RadPath, "lib");
            thisPTTA.Workdirectory = @ThisRad.ProjectPath;
            thisPTTA.PcombImageSourcePath = ThisRad.PcombimagesourcePath;
            thisPTTA.RaTiffImagePath = ThisRad.RatiffimagePath;
            thisPTTA.ProgramName = "ra_tiff";
            thisPTTA.SourceExt = ".pic";
            thisPTTA.TargetExt = ".tif";
            thisPTTA.RaTiffArg = tbox_ratiff_arg.Text;
            thisPTTA.RaTiffPause = ThisRad.Ratiffpause;

            if (bwPicToTiff.IsBusy != true)
            {
                bwPicToTiff.RunWorkerAsync(thisPTTA);
                but_runratiff.Content = "Cancel";
                but_runratiff.Background = Brushes.Red;
            }
        }

        private void BwPicToTiff_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            PicsToTiffArgs PTTA = e.Argument as PicsToTiffArgs;
            int cnt = 0;
            int tcnt = PTTA.ItemsChecked.Count;
            foreach (string item in PTTA.ItemsChecked)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    string fname = item;
                    cnt++;
                    string topline = "Working on TIFF image task " + cnt.ToString() + " of " + tcnt.ToString() + ".";
                    worker.ReportProgress(cnt, topline + "\n" + "Starting " + fname);
                    string theArgs = PTTA.ProgramName + Sp + PTTA.RaTiffArg;
                    string s_pic = PTTA.PcombImageSourcePath + fname;
                    // doing this in a way that always ends in .tif just in case source does not end in .pic
                    string t_pic = PTTA.RaTiffImagePath + fname.Replace(PTTA.SourceExt, "") + PTTA.TargetExt;
                    theArgs = String.Concat(theArgs, Sp, @s_pic, Sp, @t_pic);
                    //MessageBox.Show(theArgs);
                    //bool bedebug = false;
                    Process p = new Process();
                    ProcessStartInfo psi = new ProcessStartInfo();
                    // add radiance install path to PATH the child process use
                    string pathvar = Environment.GetEnvironmentVariable("PATH");
                    psi.EnvironmentVariables["PATH"] = PTTA.PathAdd + pathvar;
                    psi.EnvironmentVariables["RAYPATH"] = PTTA.RayPather;
                    // Required when setting process EnvironmentVariables
                    psi.UseShellExecute = false;
                    // set the working directory
                    psi.WorkingDirectory = PTTA.Workdirectory;
                    // Tactic is for CMD.EXE and programname to be part of argument
                    psi.FileName = "CMD.EXE";

                    if (!PTTA.RaTiffPause)
                    {
                        psi.RedirectStandardError = true;
                        // no window
                        psi.CreateNoWindow = true;
                        psi.WindowStyle = ProcessWindowStyle.Hidden;
                        theArgs = String.Concat(@"/c ", theArgs);  // window closes
                    }
                    else
                    {
                        psi.RedirectStandardError = false;
                        psi.CreateNoWindow = false;
                        psi.WindowStyle = ProcessWindowStyle.Normal;
                        theArgs = String.Concat(@"/k ", theArgs);  // window stays open
                    }

                    // set the arguments
                    psi.Arguments = theArgs;

                    // Starts process
                    string error = string.Empty;
                    p.StartInfo = psi;
                    p.Start();
                    // Do not wait for the child process to exit before
                    // reading to the end of its redirected error stream.
                    // p.WaitForExit();
                    // Read the error stream first and then wait.
                    if (!PTTA.RaTiffPause)
                    {
                        error = p.StandardError.ReadToEnd();
                    }
                    p.WaitForExit();
                    if (error.Equals(string.Empty))
                    {
                        worker.ReportProgress(cnt, topline + "\n" + "Created " + fname.Replace(PTTA.SourceExt, "") + PTTA.TargetExt);
                    }
                    else
                    {
                        worker.ReportProgress(cnt, "Error! " + error);
                        e.Result = error;
                    }
                }
            }
        }

        private void BwPicToTiff_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            but_runratiff.Content = "Run ra__tiff";
            but_runratiff.ClearValue(BackgroundProperty);
            if ((e.Cancelled == true))
            {
                ThisRad.StatusMsg = "Pic to Tiff Canceled!";
            }

            else if (!(e.Error == null))
            {
                ThisRad.StatusMsg = ("Error: " + e.Error.Message);
            }

            else
            {
                if (e.Result != null)
                {
                    ThisRad.StatusMsg = "Last error was:\n" + e.Result.ToString().Trim();
                }
                else
                {
                    ThisRad.StatusMsg = "Finshed creating the TIFF images.";
                }
            }
            FillTiffsChoices();
        }

        private void BwPicToTiff_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ThisRad.StatusMsg = e.UserState.ToString();
            FillTiffsChoices();
        }

        private void But_picsource_selectall_Click(object sender, RoutedEventArgs e)
        {
            Helpers.CheckInThisListBox(listbox_pic_choices, true);
        }

        private void But_picsource_selectnone_Click(object sender, RoutedEventArgs e)
        {
            Helpers.CheckInThisListBox(listbox_pic_choices, false);
        }

        private void But_explorer_result_Click(object sender, RoutedEventArgs e)
        {
            string path = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.RatiffimagePath);
            Helpers.ExploreListBoxSelection(listbox_tiff_results, path);
        }

        private void Tbox_tiffresultsfolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
            SetupForTiffImages();
        }

        private void Tbox_picsourcefolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
            FillRATiffChoices();
        }

        private void FolderTboxEndsInBackSlash(object sender, RoutedEventArgs e)
        {
            Helpers.EndsInBackSlash(sender as TextBox);
        }

        private void Listbox_tiff_results_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                Helpers.DeleteFileFromListBox(sender, ThisRad.ProjectPath, ThisRad.RatiffimagePath);
                SetupForTiffImages();
            }
        }

        private void Tb_prefspath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the default storage location for this application preference *.dat file.", true, ThisRad.PrefsPath);
        }

        private void Tb_prefspath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.PrefsPath, true);
        }

        /// <summary>
        /// Checks for vaild arguments
        /// </summary>
        /// <param name="sender"></param>
        private void ParseRaTIFFArgsForErrors(object sender)
        {
            TextBox tb = sender as TextBox;
            string arg = tb.Text + Sp;
            Lb_ratiff_arg_e.Foreground = Brushes.Black; // innocent until proven guilty
            Lb_ratiff_arg_b.Foreground = Brushes.Black; // innocent until proven guilty
            Lb_ratiff_arg_g.Foreground = Brushes.Black; // innocent until proven guilty
            Lb_ratiff_arg_zLlfw.Foreground = Brushes.Black; // innocent until proven guilty
            string flipmsg = "OK, go head and run that. See for yourself what happens.";
            flipmsg = flipmsg + "\nBTW, you might need to vists Taskmanger to terminate Ra_tiff.exe.";
            bool flipFlag = false;

            string e_arg_pat = @"(-e\s+[\+|\-]\d+)";
            string b_arg_pat = @"(-b\s+)";
            string g_arg_pat = @"(-g\s+\d+\s+)";
            string the_nots = @"[a,c,d,h-k,m-v,x,y,A-K,M-Z,&,_,!,@,#,$,%,^,*,(,),:,;,=,?,\\,/,|,<,>,',""]";
            string neg_gamma = @"(-g\s+-\d+)";

            // throw out all for any of the nots
            Regex badgamma = new Regex(neg_gamma, RegexOptions.None);
            Match mgamma = badgamma.Match(arg);
            if (mgamma.Success)
            {
                Lb_ratiff_arg_g.Foreground = Brushes.Red;
                ThisRad.StatusMsg = "Negative gamma will hang your machine!! Ra_Tiff runs\n";
                ThisRad.StatusMsg = ThisRad.StatusMsg + "very agressively and negative gamma runs forever.";
                but_runratiff.IsEnabled = false;
                return;
            }
            else
            {
                but_runratiff.IsEnabled = true;
            }

            if (arg.Contains("--")) // flunk the whole thing
            {
                Lb_ratiff_arg_e.Foreground = Brushes.Red;
                Lb_ratiff_arg_b.Foreground = Brushes.Red;
                Lb_ratiff_arg_g.Foreground = Brushes.Red;
                Lb_ratiff_arg_zLlfw.Foreground = Brushes.Red;
                ThisRad.StatusMsg = flipmsg;
                return;
            }

            // throw out all for any of the nots
            Regex nots = new Regex(the_nots, RegexOptions.None);
            Match mnots = nots.Match(arg);
            if (mnots.Success)
            {
                Lb_ratiff_arg_e.Foreground = Brushes.Red;
                Lb_ratiff_arg_b.Foreground = Brushes.Red;
                Lb_ratiff_arg_g.Foreground = Brushes.Red;
                Lb_ratiff_arg_zLlfw.Foreground = Brushes.Red;
                ThisRad.StatusMsg = flipmsg;
                return;
            }

            // Keeping track of how many of these list items are in the arg string. There
            // should be only one of them if any.
            List<string> zLlfw = new List<string> { "-z ", "-L ", "-l ", "-f ", "-w " };
            int score = 0;
            foreach (string pat in zLlfw)
            {
                Regex r = new Regex(" " + pat);
                Match m = r.Match(arg);
                if (m.Success) { score++; }
                r = new Regex(pat);
                m = r.Match(arg);
                if (m.Success) { score++; }

                // now check exceptions a la brute force
                r = new Regex("Z");
                m = r.Match(arg);
                if (m.Success) { score = score + 2; } // ie not allowed
                r = new Regex("F");
                m = r.Match(arg);
                if (m.Success) { score = score + 2; } // ie not allowed
                r = new Regex("W");
                m = r.Match(arg);
                if (m.Success) { score = score + 2; } // ie not allowed

            }
            if (score > 1)
            {
                Lb_ratiff_arg_zLlfw.Foreground = Brushes.Red;
                flipFlag = true;
            }

            // if arg contains an 'e' and does not match to e_arg_pat then the
            // e arg componant shall be considered bad.
            if (arg.Contains("e"))
            {
                Regex r = new Regex(e_arg_pat, RegexOptions.None);
                Match m = r.Match(arg);
                if (!m.Success) { Lb_ratiff_arg_e.Foreground = Brushes.Red; flipFlag = true; }
            }
            // major exception
            if (arg.Contains("E")) { Lb_ratiff_arg_e.Foreground = Brushes.Red; flipFlag = true; }

            if (arg.Contains("b"))
            {
                Regex r = new Regex(b_arg_pat, RegexOptions.None);
                Match m = r.Match(arg);
                if (!m.Success) { Lb_ratiff_arg_b.Foreground = Brushes.Red; flipFlag = true; }
            }
            // major exception
            if (arg.Contains("B")) { Lb_ratiff_arg_b.Foreground = Brushes.Red; flipFlag = true; }

            if (arg.Contains("g"))
            {
                Regex r = new Regex(g_arg_pat, RegexOptions.None);
                Match m = r.Match(arg);
                if (!m.Success) { Lb_ratiff_arg_g.Foreground = Brushes.Red; flipFlag = true; }
            }
            // major exception
            if (arg.Contains("G")) { Lb_ratiff_arg_g.Foreground = Brushes.Red; flipFlag = true; }

            if (flipFlag)
            {
                ThisRad.StatusMsg = flipmsg;
            }
            else
            {
                ThisRad.StatusMsg = DefaultStatMsg;
            }

        }

        private void Tbox_ratiff_arg_TextChanged(object sender, TextChangedEventArgs e)
        {
            ParseRaTIFFArgsForErrors(sender);
        }

        private void Tb_editorspath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFile(sender, "Pick the code Editor application.", true);
        }

        private void TextBox_ForFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForFile(sender as TextBox, string.Empty, string.Empty);
        }

        private void Tb_editorspath_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForFile(sender as TextBox, string.Empty, string.Empty);
        }

        private void Tb_hdrviewer_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForFile(sender as TextBox, string.Empty, string.Empty);
        }

        private void Button_UtilityEditorClick(object sender, RoutedEventArgs e)
        {
            Helpers.RunCodeEditorThisProject(ThisRad);
        }

        private void SniffOutPcombImageTargetPath(object sender)
        {
            TextBox tb = sender as TextBox;
            bool pathOK = Helpers.MarkTextBoxForPath(tb, ThisRad.ProjectPath, true);
            string subpath = tb.Text;
            newPathToCreate = Helpers.CombineIntoPath(ThisRad.ProjectPath, subpath) + "\\";
            if (!Directory.Exists(newPathToCreate))
            {
                button_makeimagesubpath.Visibility = Visibility.Visible;
                ThisRad.StatusMsg = "Will create this path:\n" + newPathToCreate;
            }
            else
            {
                button_makeimagesubpath.Visibility = Visibility.Collapsed;
                ThisRad.StatusMsg = "Radiance Session Setup";
                newPathToCreate = string.Empty;
            }
        }

        private void Tbox_pcombimagetargetpath_TextChanged(object sender, TextChangedEventArgs e)
        {
            SniffOutPcombImageTargetPath(sender);
            BuildThePCombLine();
        }

        private void Tbox_pcombimagetargetpath_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Select a target file path.", true, ThisRad.ProjectPath);
            BuildThePCombLine();
        }

        private void TboxBatchFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.SniffTextBoxToBeAValidFileName(sender as TextBox);
        }

        private void Tog_getinfofull_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton togB = sender as ToggleButton;
            if ((bool)togB.IsChecked)
            {
                togB.Content = "Size";
                togB.VerticalContentAlignment = VerticalAlignment.Top;
            }
            else
            {
                togB.Content = "Full";
                togB.VerticalContentAlignment = VerticalAlignment.Center;
            }
            if (lastCheckedPcomb.Equals(string.Empty)) { return; }
            Tblock_radheader.Text = Helpers.GetInfoThisRadFile(ThisRad, lastCheckedPcomb);
        }

        private void Tbox_tiffsourcefolder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the pic source file path.", true, ThisRad.ProjectPath);
            FillFinalTiffChoices();
        }

        private void Tbox_conversionresultsfolder_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.SetTextToASelectedFolder(sender, "Pick the final images target file path.", true, ThisRad.ProjectPath);
            FillFinalImageChoices();
        }

        private void Tbox_tiffsourcefolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
            FillFinalTiffChoices();
        }

        private void Tbox_conversionresultsfolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            Helpers.MarkTextBoxForPath(sender as TextBox, ThisRad.ProjectPath, true);
            FillFinalImageChoices();
        }

        private void Buttiffsource_selectall_Click(object sender, RoutedEventArgs e)
        {
            Helpers.CheckInThisListBox(listbox_finaltiff_choices, true);
        }

        private void Buttiffsource_selectnone_Click(object sender, RoutedEventArgs e)
        {
            Helpers.CheckInThisListBox(listbox_finaltiff_choices, false);
            ClearImageView(img_magicsource, lbl_magicksource);
        }

        private void But_explorer_magicresult_Click(object sender, RoutedEventArgs e)
        {
            string path = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.FinalimagePath);
            Helpers.ExploreListBoxSelection(listbox_magick_results, path);
        }

        private void Listbox_magick_results_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ThisRad.StatusMsg = "";
            DoFinalResultPick(sender);
        }

        private void Listbox_magick_results_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ThisRad.StatusMsg = "";
            DoFinalResultPick(sender);
        }

        private void But_runtifftoother_Click(object sender, RoutedEventArgs e)
        {
            if (bwMagick.IsBusy == true)
            {
                ThisRad.StatusMsg = "OK, cancel is pending. Hold your horses.";
                bwMagick.CancelAsync();
                But_runtifftoother.Content = "Convert";
                But_runtifftoother.ClearValue(BackgroundProperty);
                return;
            }
            RunMagicSelections();
            DoFinalResultPick(listbox_magick_results);
        }

        private void RunMagicSelections()
        {
            MagickConvertArgs thisMagick = new MagickConvertArgs();
            ListBox lb = listbox_finaltiff_choices;
            List<string> itemsChecked = new List<string>();
            foreach (var vb in lb.Items)
            {
                if (vb.GetType() != typeof(CheckBox)) { continue; }
                CheckBox cb = vb as CheckBox;
                if (!(bool)cb.IsChecked) { continue; }
                TextBlock tbi = cb.Content as TextBlock;
                itemsChecked.Add(tbi.Text);
            }
            if (itemsChecked.Count == 0) { return; }

            thisMagick.ItemsChecked = itemsChecked;
            thisMagick.Workdirectory = @ThisRad.ProjectPath;
            thisMagick.ImageSourcePath = ThisRad.RatiffimagePath;
            thisMagick.ConvertImagePath = ThisRad.FinalimagePath;
            thisMagick.ProgramName = "magick.exe convert";
            thisMagick.SourceExt = ".tif";
            thisMagick.TargetExt = "." + ThisRad.FinalimageType;
            thisMagick.MagickArg = tbox_imagemagick_arg.Text;
            thisMagick.MagickPause = (bool)chkbox_magicpause.IsChecked;

            if (bwMagick.IsBusy != true)
            {
                bwMagick.RunWorkerAsync(thisMagick);
                But_runtifftoother.Content = "Cancel";
                But_runtifftoother.Background = Brushes.Red;
            }
        }

        private void BwMagick_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            MagickConvertArgs thisMagick = e.Argument as MagickConvertArgs;
            int cnt = 0;
            int tcnt = thisMagick.ItemsChecked.Count;
            foreach (string item in thisMagick.ItemsChecked)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    string fname = item;
                    cnt++;
                    string topline = "Working on conversion image task " + cnt.ToString() + " of " + tcnt.ToString() + ".";
                    worker.ReportProgress(cnt, topline + "\n" + "Starting " + fname);
                    string theArgs = thisMagick.ProgramName + Sp + thisMagick.MagickArg;
                    string s_pic = thisMagick.ImageSourcePath + fname;
                    // doing this in a way that always ends in .tif just in case source does not end in .pic
                    string t_pic = thisMagick.ConvertImagePath + fname.Replace(thisMagick.SourceExt, "") + thisMagick.TargetExt;
                    theArgs = String.Concat(theArgs, Sp, @s_pic, Sp, @t_pic);
                    //MessageBox.Show(theArgs);
                    Process p = new Process();
                    ProcessStartInfo psi = new ProcessStartInfo();
                    // Required when setting process EnvironmentVariables
                    psi.UseShellExecute = false;
                    // set the working directory
                    psi.WorkingDirectory = thisMagick.Workdirectory;
                    // Tactic is for CMD.EXE and programname to be part of argument
                    psi.FileName = "CMD.EXE";

                    if (!thisMagick.MagickPause)
                    {
                        psi.RedirectStandardError = true;
                        // no window
                        psi.CreateNoWindow = true;
                        psi.WindowStyle = ProcessWindowStyle.Hidden;
                        theArgs = String.Concat(@"/c ", theArgs);  // window closes
                    }
                    else
                    {
                        psi.RedirectStandardError = false;
                        psi.CreateNoWindow = false;
                        psi.WindowStyle = ProcessWindowStyle.Normal;
                        theArgs = String.Concat(@"/k ", theArgs);  // window stays open
                    }

                    // set the arguments
                    psi.Arguments = theArgs;

                    //// show window or not
                    //if (!bedebug)
                    //{
                    //    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    //}
                    //else
                    //{
                    //    psi.WindowStyle = ProcessWindowStyle.Normal;
                    //}

                    // Starts process
                    string error = string.Empty;
                    p.StartInfo = psi;
                    p.Start();
                    // Do not wait for the child process to exit before
                    // reading to the end of its redirected error stream.
                    // p.WaitForExit();
                    // Read the error stream first and then wait.
                    if (!thisMagick.MagickPause)
                    {
                        error = p.StandardError.ReadToEnd();
                    }
                    p.WaitForExit();
                    if (error.Equals(string.Empty))
                    {
                        worker.ReportProgress(cnt, topline + "\n" + "Created " + fname.Replace(thisMagick.SourceExt, "") + thisMagick.TargetExt);
                    }
                    else
                    {
                        worker.ReportProgress(cnt, "Error! " + error);
                        e.Result = error;
                    }
                }
            }
        }

        private void BwMagick_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            But_runtifftoother.Content = "Convert";
            But_runtifftoother.ClearValue(BackgroundProperty);
            if ((e.Cancelled == true))
            {
                ThisRad.StatusMsg = "Tiff Conversion Canceled!";
            }

            else if (!(e.Error == null))
            {
                ThisRad.StatusMsg = ("Error: " + e.Error.Message);
            }

            else
            {
                if (e.Result != null)
                {
                    ThisRad.StatusMsg = "Last error was:\n" + e.Result.ToString().Trim();
                }
                else
                {
                    ThisRad.StatusMsg = "Finshed converting the TIFF images.";
                }
            }
            FillFinalImageChoices();
        }

        private void BwMagick_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ThisRad.StatusMsg = e.UserState.ToString();
            FillFinalImageChoices();
        }

        private void Lbl_imagemagick_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Helpers.WebBrowserToHere(@"https://www.imagemagick.org/script/convert.php");
        }

        private void Listbox_magick_results_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                Helpers.DeleteFileFromListBox(sender, ThisRad.ProjectPath, ThisRad.FinalimagePath);
                FillFinalImageChoices();
            }
        }

        private void Tbox_magicktype_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ThisRad.FinalimageType.Equals("pdf", StringComparison.CurrentCultureIgnoreCase))
            {
                if (!File.Exists(ThisRad.GhostScriptPath))
                {
                    MessageBox.Show("N");
                    string msg = "Sorry, GhostScript must be properly installed in order to create PDFs.";
                    ThisRad.StatusMsg = msg;
                    FormMsgWPF explain = new FormMsgWPF(null, 3);
                    explain.SetMsg(msg, "GhostScript Is Not Installed");
                    explain.ShowDialog();
                }
            }
        }

        private void Listbox_magick_results_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmMagickResultsListImageItem") as ContextMenu;
            cm.PlacementTarget = sender as ListBoxItem;
            cm.IsOpen = true;
        }

        private void MagickResultsMenuItemRename_Click(object sender, RoutedEventArgs e)
        {  
            RenameMagickResults();
        }

        private void MagickResultsMenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            ListBox lb = listbox_magick_results;
            Helpers.DeleteFileFromListBox(lb, ThisRad.ProjectPath, ThisRad.FinalimagePath);
            FillFinalImageChoices();
        }

        private void RaTiffResultsMenuItemRename_Click(object sender, RoutedEventArgs e)
        {
            RenamePicToTiffResults();
        }

        private void RaTiffResultsMenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            ListBox lb = listbox_tiff_results;
            Helpers.DeleteFileFromListBox(lb, ThisRad.ProjectPath, ThisRad.RatiffimagePath);
            FillTiffsChoices();
        }

        private void But_rename_magicresult_Click(object sender, RoutedEventArgs e)
        {
            RenameMagickResults();
        }

        private void But_rename_pictotiffresult_Click(object sender, RoutedEventArgs e)
        {
            RenamePicToTiffResults();
        }

        private void RenameMagickResults()
        {
            // ThisRad.ProjectPath, ThisRad.FinalimagePath
            FilesRenamerClass FRenamer = new FilesRenamerClass
            {
                PathPart = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.FinalimagePath),
                TheFiles = new List<RenamerFileName>()
            };
            ListBox lb = listbox_magick_results;
            var sels_lb = lb.SelectedItems;
            if (sels_lb != null && sels_lb.Count > 0)
            {
                List<string> fNames = new List<string>();
                foreach (var item in sels_lb)
                {
                    if (item.GetType() == typeof(ListBoxItem))
                    {
                        ListBoxItem lbi = item as ListBoxItem;
                        String thisFName = lbi.Content.ToString();
                        fNames.Add(thisFName);
                        RenamerFileName rfn = new RenamerFileName
                        {
                            Present_Name = thisFName,
                            Changed_Name = thisFName
                        };
                        FRenamer.TheFiles.Add(rfn);
                    }
                }
                Delegate_FillRenamedChoices delObj = new Delegate_FillRenamedChoices(FillFinalImageChoices);
                FileRenamerWPF FNR_WPF = new FileRenamerWPF(FRenamer, delObj);
                FNR_WPF.ShowDialog();
               // FillFinalImageChoices();
            }
        }

        private void RenamePicToTiffResults()
        {
            // ThisRad.ProjectPath, ThisRad.FinalimagePath
            FilesRenamerClass FRenamer = new FilesRenamerClass
            {
                PathPart = Helpers.CombineIntoPath(ThisRad.ProjectPath, ThisRad.RatiffimagePath),
                TheFiles = new List<RenamerFileName>()
            };
            ListBox lb = listbox_tiff_results;
            var sels_lb = lb.SelectedItems;
            if (sels_lb != null && sels_lb.Count > 0)
            {
                List<string> fNames = new List<string>();
                foreach (var item in sels_lb)
                {
                    if (item.GetType() == typeof(ListBoxItem))
                    {
                        ListBoxItem lbi = item as ListBoxItem;
                        String thisFName = lbi.Content.ToString();
                        fNames.Add(thisFName);
                        RenamerFileName rfn = new RenamerFileName
                        {
                            Present_Name = thisFName,
                            Changed_Name = thisFName
                        };
                        FRenamer.TheFiles.Add(rfn);
                    }
                }
                Delegate_FillRenamedChoices delObj = new Delegate_FillRenamedChoices(FillTiffsChoices);
                FileRenamerWPF FNR_WPF = new FileRenamerWPF(FRenamer, delObj);
                FNR_WPF.ShowDialog();
                //FillTiffsChoices();
            }
        }

        private void Listbox_tiff_results_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmRaTiffResultsListImageItem") as ContextMenu;
            cm.PlacementTarget = sender as ListBoxItem;
            cm.IsOpen = true;
        }
    } // end window

    public class PicsToTiffArgs
    {
        public string PathAdd { get; set; }
        public string RayPather { get; set; }
        public string Workdirectory { get; set; }
        public string PcombImageSourcePath { get; set; }
        public string RaTiffImagePath { get; set; }
        public string ProgramName { get; set; }
        public string SourceExt { get; set; }
        public string TargetExt { get; set; }
        public string RaTiffArg { get; set; }
        public List<string> ItemsChecked { get; set; }
        public bool RaTiffPause { get; set; }
    }

    public class MagickConvertArgs
    {
        public string PathAdd { get; set; }
        public string RayPather { get; set; }
        public string Workdirectory { get; set; }
        public string ImageSourcePath { get; set; }
        public string ConvertImagePath { get; set; }
        public string ProgramName { get; set; }
        public string SourceExt { get; set; }
        public string TargetExt { get; set; }
        public string MagickArg { get; set; }
        public List<string> ItemsChecked { get; set; }
        public bool MagickPause { get; set; }
    }

    public class TiffViewArgs
    {
        public Uri ThisImageUri { get; set; }
        public ImageSource ThisImageSource { get; set; }
        public string TheTargetWPFImagename { get; set; }
        public string TheTargetWPFLabelname { get; set; }
        public string TheRealName { get; set; }
    }
}

//<Image x:Name="bt_regen"  Width="{Binding borderBtnAdd.Width}" Height="{Binding borderBtnAdd.Height}" Source ="{StaticResource ReloadImage}"  MouseUp="bt_regen_MouseUp"  />

//<Border x:Name="borderBtnAdd" BorderThickness="1" BorderBrush="DarkGray" CornerRadius="360" 
//                     Height="40" Width="40" Margin="0,0,10,0" 
//                     VerticalAlignment="Center" HorizontalAlignment="Center"  >
//                    <Image x:Name="bt_regen"  Width="{Binding borderBtnAdd.Width}" Height="{Binding borderBtnAdd.Height}" Source ="{StaticResource ReloadImage}"  MouseUp="bt_regen_MouseUp"  />
//               </Border>