using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfRadScript
{
    /// <summary>
    /// Interaction logic for FileRenamerWPF.xaml
    /// </summary>
    public partial class FileRenamerWPF : Window
    {
        Brush ClrA = ColorExt.ToBrush(System.Drawing.Color.AliceBlue);
        Brush ClrB = ColorExt.ToBrush(System.Drawing.Color.Cornsilk);

        public FilesRenamerClass LocfRenamer = new FilesRenamerClass();
        public Replacers TheReplacers = new Replacers();
        private Delegate_FillRenamedChoices _delObj;  // used to pass method to this window

        public FileRenamerWPF(FilesRenamerClass fRenamer, Delegate_FillRenamedChoices delObj)
        {
            InitializeComponent();
            _delObj = delObj;
            LocfRenamer = fRenamer;
            Top = Properties.Settings.Default.FormMSG_Top;
            Left = Properties.Settings.Default.FormMSG_Left;
            Tog_ToolTips.IsChecked = Properties.Settings.Default.RenamerTips;

            LB_path.Content = LocfRenamer.PathPart;
            tb_withthis.DataContext = TheReplacers;
            tb_replacethis.DataContext = TheReplacers;
            tb_addthis.DataContext = TheReplacers;
            dg_names.DataContext = LocfRenamer;
            Tog_CampOut.DataContext = TheReplacers;
            TheReplacers.ReplaceThis = string.Empty;
            TheReplacers.WithThis = string.Empty;
            TheReplacers.LastAddThis = string.Empty;
            TheReplacers.LastAddWasFront = false;
            TheReplacers.AddThis = string.Empty;
            TheReplacers.CampOut = false;
        }

        public void DragWindow(object sender, MouseButtonEventArgs args)
        {
            // Watch out. Fatal error if not primary button!
            if (args.LeftButton == MouseButtonState.Pressed) { DragMove(); }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.FormMSG_Top = Top;
            Properties.Settings.Default.FormMSG_Left = Left;
            Properties.Settings.Default.RenamerTips = (bool)Tog_ToolTips.IsChecked;
            Properties.Settings.Default.Save();
        }

        // returns false on cancel
        private bool PerformTheRenaming()
        {
            bool SomeIssues = ValidateRenamedList();
            if (SomeIssues)
            {
                string ttl = "By The Way ...";
                string msg = "There are issues with some of the new file names. ";
                msg = msg + "The new names having issues will not rename.\n\nContinue anyway?";
                FormMsgWPF explain = new FormMsgWPF(null, 2);
                explain.SetMsg(msg, ttl);
                explain.ShowDialog();
                if (explain.theResult != MessageBoxResult.OK) { return false; }
            }
            string basePath = LocfRenamer.PathPart;
            string strFrom = string.Empty;
            string strTo = string.Empty;
            foreach (RenamerFileName rfn in LocfRenamer.TheFiles)
            {
                if (!rfn.Present_Name.Equals(rfn.Changed_Name, StringComparison.CurrentCultureIgnoreCase))
                {
                    strFrom = basePath + rfn.Present_Name;
                    strTo = basePath + rfn.Changed_Name;
                    try
                    {
                        File.Move(strFrom, strTo);
                        if (TheReplacers.CampOut)
                        {
                            // We are camping out. The present name must reflect the change just
                            // made.
                            rfn.Present_Name = rfn.Changed_Name;
                        }
                    }
                    catch (Exception err)
                    {
                        string msg = "Something unexpected happened. You'll have to deal with this.";
                        msg = msg + "\n\n" + err.Message;
                        FormMsgWPF explain = new FormMsgWPF(null, 3);
                        explain.SetMsg(msg, "Unable To Rename A File");
                        explain.ShowDialog();
                    }
                }
            }
            return true;
        }

        private void But_rename_Click(object sender, RoutedEventArgs e)
        {
            if (PerformTheRenaming() && !TheReplacers.CampOut) { Close(); }
            ValidateRenamedList();
            _delObj();
        }

        // Here the datagrid itself is parsed because it is the datagrid cells that
        // need to be marked bad.
        // Returns true if there are issues.
        private bool ValidateRenamedList()
        {
            // OMG!!
            bool ThereAreIssues = false;
            bool ThereAreIllegals = false;
            int invalidCount = 0;
            int changedCount = 0;
            DataGrid dgr = dg_names;
            List<string> NotUniqueOnDiskList = new List<string>();
            List<string> NotUniqueInThisList = new List<string>();
            string basePath = LocfRenamer.PathPart;
            int numRows = LocfRenamer.TheFiles.Count();
            for (int i = 0; i < numRows; i++)
            {
                DataGridRow thisRow = WPFDataGridHelpers.GetRow(dgr, i);
                DataGridCell CellCur = dgr.GetCell(thisRow, 0);
                DataGridCell CellRen = dgr.GetCell(thisRow, 1);
                // But the cell reads "" if it is not first updated when this validate method is called from
                // a textchanged event!!!
                CellRen.UpdateLayout();
                if (CellRen != null)
                {
                    // readonly cells are TextBlocks!!
                    TextBlock tblkCur = CellCur.Content as TextBlock;
                    // But when not editing a textblock??
                    //TextBox tbxRen = CellRen.Content as TextBox;
                    TextBlock tblkRen = CellRen.Content as TextBlock;
                    if (tblkRen != null)
                    {
                        string name_grid_cell_Fnew = tblkRen.Text;
                        string name_grid_cell_Fcur = tblkCur.Text;
                        string msg = string.Empty;
                        string fullPath = basePath + name_grid_cell_Fnew;
                        if (!Helpers.IsValidWindowsFileName(name_grid_cell_Fnew))
                        {
                            // invalid characters
                            Dispatcher.BeginInvoke((Action)(() => CellRen.Foreground = Brushes.Red));
                            ThereAreIssues = true;
                            ThereAreIllegals = true;
                            invalidCount++;
                            continue;
                        }
                        else
                        {
                            if (!name_grid_cell_Fcur.Equals(name_grid_cell_Fnew, StringComparison.CurrentCultureIgnoreCase))
                            {
                                // new tlbk <> cur tlbk  There is a change, no matter what.
                                changedCount++;
                                Dispatcher.BeginInvoke((Action)(() => CellRen.Foreground = Brushes.DarkGreen));  // innocent until proven guilty
                                if (!Helpers.MarkDataGridCellForFile(CellRen, fullPath, false))
                                {
                                    // There is another file on disk like this one.
                                    NotUniqueOnDiskList.Add(name_grid_cell_Fnew);
                                    Dispatcher.BeginInvoke((Action)(() => CellRen.Foreground = Brushes.Red));
                                    ThereAreIssues = true;
                                    invalidCount++;
                                }
                                else
                                {   // No file issue, now look for duplicates in the bound list.
                                    // We are comparing this row to all entries
                                    // in the bound list looking for the same name used for other current names.
                                    foreach (RenamerFileName list_rfn in LocfRenamer.TheFiles)
                                    {
                                        if (list_rfn.Present_Name.Equals(name_grid_cell_Fcur, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            // This must be the same name. Do nothing. Go on to the next.
                                            continue; // We do not want to do any more comparisons.
                                        }
                                        if (list_rfn.Changed_Name.Equals(name_grid_cell_Fnew, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            // list changed = a new changed, must be a name already used.
                                            // In this code the same name can wind up multiple times in the list if this
                                            // is not checked first.
                                            if (!NotUniqueInThisList.Contains(name_grid_cell_Fnew))
                                            {
                                                NotUniqueInThisList.Add(name_grid_cell_Fnew);
                                            }
                                            // this item has an issue
                                            ThereAreIssues = true;
                                            Dispatcher.BeginInvoke((Action)(() => CellRen.Foreground = Brushes.Red));
                                            invalidCount++;

                                            // When the name is a duplicate within the list, the prior duplicates will have
                                            // been marked green as valid ne ones. We need to pass through the list again to remark it to red
                                            for (int j = 0; j < numRows; j++)
                                            {
                                                DataGridRow thisReRow = WPFDataGridHelpers.GetRow(dgr, j);
                                                DataGridCell CellReRen = dgr.GetCell(thisReRow, 1);
                                                TextBlock retblkRen = CellReRen.Content as TextBlock;
                                                if (retblkRen.Text.Equals(name_grid_cell_Fnew))
                                                {
                                                    Dispatcher.BeginInvoke((Action)(() => CellReRen.Foreground = Brushes.Red));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else  // no change at all
                            {
                                // item not changed
                                Dispatcher.BeginInvoke((Action)(() => CellRen.Foreground = Brushes.Black));
                            }
                        }
                        //// If after the change the two wind up being equal, like in the case of a reset, then make it black.
                        //if (name_grid_cell_Fcur.Equals(name_grid_cell_Fcur))
                        //{
                        //    Dispatcher.BeginInvoke((Action)(() => CellRen.Foreground = Brushes.Black));
                        //}
                    }
                }
            }
            if (ThereAreIssues)
            {
                string msgDetail = string.Empty;
                string moreDetail = string.Empty;
                if (ThereAreIllegals)
                {
                    msgDetail = "There are illegal characters.";
                }
                if (NotUniqueOnDiskList.Count > 0)
                {
                    moreDetail = "There are existing files elsewhere: " + string.Join(" , ", NotUniqueOnDiskList);
                    if (ThereAreIllegals)
                    {
                        msgDetail = msgDetail + " , " + moreDetail;
                    }
                    else
                    {
                        msgDetail = moreDetail;
                    }
                }
                if (NotUniqueInThisList.Count > 0)
                {
                    moreDetail = "These are duplicates names: " + string.Join(" , ", NotUniqueInThisList);

                    if (ThereAreIllegals || NotUniqueOnDiskList.Count > 0)
                    {
                        msgDetail = msgDetail + " , " + moreDetail;
                    }
                    else
                    {
                        msgDetail = moreDetail;
                    }
                }
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Lb_topmsg.Content = "There are naming issues.";
                    Lb_topmsg.Foreground = Brushes.Red;
                    TBLK_msg.Text = msgDetail;
                    TBLK_msg.Visibility = Visibility.Visible;
                    TBLK_msg.Foreground = Brushes.Red;
                }));

            }
            else
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Lb_topmsg.Content = string.Empty;
                    Lb_topmsg.Foreground = Brushes.Black;
                    TBLK_msg.Text = string.Empty;
                    TBLK_msg.Foreground = Brushes.Black;
                    TBLK_msg.Visibility = Visibility.Collapsed;
                }));
            }
            Dispatcher.BeginInvoke((Action)(() =>
            {
                But_rename.IsEnabled = (changedCount > invalidCount);
            }));

            return ThereAreIssues;
        }

        // Cell editing has ended but has yet to be commited. Therefore the binding source has
        // yet to be changed.
        private void Dg_names_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() => ValidateRenamedList()));
            return;
        }

        private void DoTheAutomatics()
        {
            // This should perform the replace but not on the add portion.
            // The non add portion is going to be the the current state less the
            // previous add operations. 
            string r = TheReplacers.ReplaceThis;
            string w = TheReplacers.WithThis;
            string lastaddthis = TheReplacers.LastAddThis;
            bool lend = TheReplacers.LastAddWasFront;
            string addthis = TheReplacers.AddThis;
            string alteredCoreName = string.Empty;
            string curChangedName = string.Empty;
            foreach (RenamerFileName rfn in LocfRenamer.TheFiles)
            {
                string cn = Path.GetFileNameWithoutExtension(rfn.Changed_Name);
                string os = Path.GetFileNameWithoutExtension(rfn.Present_Name);
                string ex = Path.GetExtension(rfn.Present_Name);
                // Using the length of the lastaddthis as an indicator an add had been applied,
                // first get the changed name's core name without the last add. The lend value
                // indicates to which end the lastaddthis had been applied.
                if (lastaddthis.Length > 0) // There must have been a last add.
                {
                    if (lend)
                    { // Last was at front.
                        cn = cn.Substring(lastaddthis.Length);
                    }
                    else
                    { // Last was at end.
                        cn = cn.Substring(0, (cn.Length - lastaddthis.Length));
                    }
                    TheReplacers.LastAddThis = string.Empty;
                }
                // cn is now the changed name's core name.

                // Substitution on the core must always use the present name. There is
                // no way to keep track if it operated on the changed name. So if there
                // is a substitution asked for, then the changed name is tossed out at this point.
                alteredCoreName = cn;
                if (!r.Equals(string.Empty))
                {
                    alteredCoreName = os.Replace(r, w);
                }
                // Now perform the add to the core. Any previous add had
                // been removed. Adds will not be cumulative. If desired then
                // the add must be entered in its cumulative form.
                if (addthis.Length > 0)
                {
                    if ((bool)rb_tofront.IsChecked)
                    {
                        alteredCoreName = addthis + alteredCoreName;
                        TheReplacers.LastAddWasFront = true;
                    }
                    else
                    {
                        alteredCoreName = alteredCoreName + addthis;
                        TheReplacers.LastAddWasFront = false;
                    }
                    TheReplacers.LastAddThis = TheReplacers.AddThis;
                }
                rfn.Changed_Name = alteredCoreName + ex;
            }
            ValidateRenamedList();
        }

        private void ProcessTheAutomatics(object sender, TextChangedEventArgs e)
        {
            DoTheAutomatics();
        }

        private void AutomaticsRadioButtonClick(object sender, RoutedEventArgs e)
        {
            DoTheAutomatics();
        }

        private void But_RemoveLastAdd_Click(object sender, RoutedEventArgs e)
        {
            // This is going to remove the last add and set the current to nothing and
            // set the last to nothing.
            string la = TheReplacers.LastAddThis;
            bool lend = TheReplacers.LastAddWasFront;
            if (la.Trim().Equals(string.Empty)) { return; }
            string alteredName = string.Empty;
            foreach (RenamerFileName rfn in LocfRenamer.TheFiles)
            {
                string cn = Path.GetFileNameWithoutExtension(rfn.Changed_Name);
                string ex = Path.GetExtension(rfn.Present_Name);
                if (la != string.Empty)
                {
                    if (lend) // Last add was to front.
                    {
                        if (cn.StartsWith(la))
                        {
                            alteredName = cn.Substring(la.Length);
                        }
                    }
                    else // Last add was to back.
                    {
                        if (cn.EndsWith(la))
                        {
                            alteredName = cn.Substring(0, (cn.Length - la.Length));
                        }
                    }
                }
                rfn.Changed_Name = alteredName + ex;
            }
            TheReplacers.LastAddThis = string.Empty;
            TheReplacers.AddThis = string.Empty;
            ValidateRenamedList();
        }

        private void But_ZapReplace_Click(object sender, RoutedEventArgs e)
        {
            // This patches an odd condition
            TheReplacers.ReplaceThis = "*********"; // A dummmy never match
            TheReplacers.ReplaceThis = string.Empty;
            TheReplacers.WithThis = string.Empty;
            ValidateRenamedList();
        }

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            // Using imnoke seems to dispatch the validator AFTER WPF renders the 
            // cells in the deafult way.
            Dispatcher.BeginInvoke((Action)(() => ValidateRenamedList()));
        }

        private void But_Reset_Click(object sender, RoutedEventArgs e)
        {
            BackToCurrent();
        }

        private void BackToCurrent()
        {
            TheReplacers.ReplaceThis = string.Empty;
            TheReplacers.WithThis = string.Empty;
            TheReplacers.AddThis = string.Empty;
            foreach (RenamerFileName rfn in LocfRenamer.TheFiles)
            {
                rfn.Changed_Name = rfn.Present_Name;
            }
            ValidateRenamedList();
        }

        private void SwallowIllegalCharacters(object sender, TextCompositionEventArgs e)
        {
            if (!Helpers.IsValidWindowsFileName(e.Text)) { e.Handled = true; }
        }
    }

    public class Replacers : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public string replaceThis;
        public string withThis;
        public string addThis;
        public string lastAddThis;
        public bool lastAddWasFront;
        public bool campOut;

        //public string GhostScriptPath { get { return ghostScriptPath; } set { ghostScriptPath = value; OnPropertyChanged("GhostScriptPath"); } }

        public string ReplaceThis { get { return replaceThis; } set { replaceThis = value; OnPropertyChanged("ReplaceThis"); } }
        public string WithThis { get { return withThis; } set { withThis = value; OnPropertyChanged("WithThis"); } }
        public string AddThis { get { return addThis; } set { addThis = value; OnPropertyChanged("AddThis"); } }
        public string LastAddThis { get { return lastAddThis; } set { lastAddThis = value; OnPropertyChanged("LastAddThis"); } }
        public bool LastAddWasFront { get { return lastAddWasFront; } set { lastAddWasFront = value; OnPropertyChanged("LastAddWasFront"); } }
        public bool CampOut { get { return campOut; } set { campOut = value; OnPropertyChanged("CampOut"); } }
    }
}
