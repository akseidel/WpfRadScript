using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfRadScript
{
    internal static class Helpers
    {
        internal static void RunExplorerHere(string path)
        {
            Process.Start("Explorer.exe", path);

            // to do
            //public bool ExploreFile(string filePath)
            //{
            //    if (!System.IO.File.Exists(filePath))
            //    {
            //        return false;
            //    }
            //    //Clean up file path so it can be navigated OK
            //    filePath = System.IO.Path.GetFullPath(filePath);
            //    System.Diagonstics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
            //    return true;
            //}

        }

        internal static void WebBrowserToHere(string target)
        {
            try
            {
                Process.Start(target);
            }
            catch
                (
                 System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (Exception other)
            {
                MessageBox.Show(other.Message);
            }


        }

        internal static string MakeFileNameDateTimeRegexPattern(List<string> fnameExtList)
        {
            /// regex pattern matches 012345.01.pic at string end
            // pat_general_datetime = @"\d{6}\.\d{2}\.pic\Z";
            string pat_general_datetime = @"\d{6}\.\d{2}\.(";
            // string resStr = @"^.+\.(";
            foreach (string s in fnameExtList)
            {
                pat_general_datetime = pat_general_datetime + s + '|';
            }
            pat_general_datetime = pat_general_datetime.TrimEnd('|');
            pat_general_datetime = pat_general_datetime + @")\Z";
            return pat_general_datetime;
        }

        internal static string GiveMeBestPathOutOfThisPath(string proposedPath)
        {
            string[] words = proposedPath.Split('\\');
            string bestPath = String.Empty;
            foreach (string token in words)
            {
                string attempt = string.Empty;
                if (token.Equals(words[0]))
                {
                    attempt = token;
                }
                else
                {
                    attempt = String.Concat(bestPath, '\\', token);
                }
                if (Directory.Exists(attempt))
                {
                    bestPath = attempt;
                }
                else
                {
                    break;
                }
            }
            return bestPath;
        }

        internal static void SetTextToASelectedFolder(object sender, string msg, bool dotmode, string basePath)
        {
            TextBox tb = sender as TextBox;
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = tb.Text
            };
            if (dotmode)
            {
                // if the dotmode folder does not exist then we want to drop back no
                // further than necessary woith the default folder. 
                string proposedPath = CombineIntoPath(basePath, tb.Text);
                folderDialog.SelectedPath = GiveMeBestPathOutOfThisPath(proposedPath);
                if (!Directory.Exists(folderDialog.SelectedPath))
                {
                    folderDialog.SelectedPath = basePath; // for the time being
                }
            }
            folderDialog.Description = msg;
            var result = folderDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var folder = folderDialog.SelectedPath;
                    string fp = EnsurePathStringEndsInBackSlash(folder);
                    if (!dotmode)
                    {
                        tb.Text = fp;
                    }
                    else
                    {
                        string t = fp.ReplaceString(basePath, ".\\", StringComparison.CurrentCultureIgnoreCase);
                        tb.Text = t;
                    }
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:

                    break;
            }
        }

        internal static void SetTextToASelectedFile(object sender, string msg, bool fullpath)
        {
            TextBox tb = sender as TextBox;
            var filepickerDialog = new System.Windows.Forms.OpenFileDialog
            {
                Title = msg
            };

            var result = filepickerDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:

                    string thepick = filepickerDialog.FileName;

                    if (!fullpath)
                    {
                        thepick = Path.GetFileName(thepick);
                    }

                    tb.Text = thepick;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    break;
            }
        }

        internal static string CombineIntoPath(string partA_RootPath, string partB_PartialPathWithDot, string partC_OptionalFileName = "")
        {
            try
            {
                string fullPath = Path.Combine(partA_RootPath, partB_PartialPathWithDot);
                fullPath = fullPath.Replace(".\\", "") + partC_OptionalFileName;
                return fullPath;
            }
            catch (ArgumentException e)
            {
                string msg = "Part of this path name " + partB_PartialPathWithDot + partC_OptionalFileName + " is illegal. You need to correct it";
                msg = msg + "\n\n" + e.Message;
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, "The Path Has Illegal An Character");
                explain.ShowDialog();
            }
            return string.Empty;
        }

        internal static string EnsurePathStringEndsInBackSlash(string path)
        {
            if (!path.EndsWith("\\", StringComparison.CurrentCultureIgnoreCase))
            {
                path = path + "\\";
            }
            return path;
        }

        internal static void EndsInBackSlash(object sender)
        {
            TextBox tb = sender as TextBox;
            tb.Text = EnsurePathStringEndsInBackSlash(tb.Text);
            tb.CaretIndex = tb.Text.Length;
        }

        internal static bool IsPCombTargetAlreadyUsed(string fn, ListBox lb)
        {
            /// Don't add newlbi if there is already a matching existinglbi. There could be.
            var mtchLBI = from TextBlock item in lb.Items.Cast<TextBlock>()
                          where item.Text.ToString().EndsWith(fn, StringComparison.CurrentCultureIgnoreCase)
                          select item as TextBlock;
            TextBlock existingtbi = mtchLBI.FirstOrDefault();
            if (existingtbi == null) { return false; }
            //MessageBox.Show("target already used");
            return true;
        }

        // colors a filename textbox as to filename's existance
        // Returns false if file does not exist, true if it does
        internal static bool MarkPCombTargetForFile(string subpath, string basePath, TextBox theTextBox, ListBox lb, GroupBox gbox)
        {
            if (subpath == null) { return true; }
            if (theTextBox == null) { return true; }
            string fn = theTextBox.Text.Trim();

            subpath = CombineIntoPath(basePath, subpath);
            subpath = CombineIntoPath(subpath, theTextBox.Text);


            if (!IsPCombTargetAlreadyUsed(fn, lb))
            {
                gbox.Header = "Pcomb To This Folder, Using This Target File Name";
                theTextBox.Foreground = Brushes.Black;
                return false;
            }
            else
            {
                gbox.Header = "< Note: The pcomb command list is already creating this image >";
                theTextBox.Foreground = Brushes.Red;
                return true;
            }
        }

        // colors a filepath textbox as to filepath's existance
        // Also returns as bool for path existance. False = path does not exist
        internal static bool MarkTextBoxForPath(TextBox theTextBox, string basePath, bool dotMode = false)
        {
            // if (path == null) { return; }
            if (theTextBox == null) { return false; }
            string path = theTextBox.Text;
            if (dotMode) { path = Helpers.CombineIntoPath(basePath, path); }
            if (Directory.Exists(path))
            {
                theTextBox.Foreground = Brushes.Black;
                return true;
            }
            else
            {
                theTextBox.Foreground = Brushes.Red;
                return false;
            }
        }

        // colors a filename textbox as to filename's existance or allowed characters
        internal static void MarkTextBoxForFile(TextBox theTextBox, string basePath, string subpath)
        {
            if (subpath == null) { return; }
            if (theTextBox == null) { return; }
            subpath = CombineIntoPath(basePath, subpath);
            subpath = CombineIntoPath(subpath, theTextBox.Text);
            if (File.Exists(subpath))
            {
                theTextBox.Foreground = Brushes.Black;
            }
            else
            {
                theTextBox.Foreground = Brushes.Red;
            }
        }

        // colors a filename textbox as to filename's existance or allowed characters
        internal static void MarkLabelForFile(Label theLabel, string fullPath, bool sense)
        {
            if (fullPath == null) { return; }
            if (theLabel == null) { return; }
            if (!IsValidWindowsFileName(Path.GetFileName(fullPath)))
            {
                theLabel.Foreground = Brushes.Red;
            }
            if (File.Exists(fullPath))
            {
                if (sense)
                {
                    theLabel.Foreground = Brushes.Black;
                }
                else
                {
                    theLabel.Foreground = Brushes.Red;
                }
            }
            else
            {
                if (sense)
                {
                    theLabel.Foreground = Brushes.Red;
                }
                else
                {
                    theLabel.Foreground = Brushes.Black;
                }
            }
        }

        // colors a filename datagridcell as to filename's existance
        internal static bool MarkDataGridCellForFile(DataGridCell theCell, string fullPath, bool sense)
        {
            if (fullPath == null) { return false; }
            if (theCell == null) { return false; }

            if (File.Exists(fullPath))
            {
                if (sense)
                {
                    theCell.Foreground = Brushes.Black;
                    return true;
                }
                else
                {
                    theCell.Foreground = Brushes.Red;
                    return false;
                }
            }
            else
            {
                if (sense)
                {
                    theCell.Foreground = Brushes.Red;
                    return false;
                }
                else
                {
                    theCell.Foreground = Brushes.Black;
                    return true;
                }
            }
        }

        internal static Dictionary<string, bool> BuildCheckListDictionary(string thePath, List<string> regexsearchPat)
        {
            string pat_toapply = MakeFilesListRegexString(regexsearchPat);
            Dictionary<string, bool> thisCheckListDict = new Dictionary<string, bool>();
            if (!Directory.Exists(thePath)) { return thisCheckListDict; }
            try
            {
                IEnumerable<String> ResultsList = Directory.GetFiles(thePath).Select(Path.GetFileName).Where(file => Regex.IsMatch(file, pat_toapply, RegexOptions.IgnoreCase));
                foreach (string f in ResultsList)
                {
                    thisCheckListDict.Add(Path.GetFileName(f), false);
                }
            }
            catch (ArgumentException e)
            {
                string msg = "Not Good! It looks like the regular expression \"" + pat_toapply + "\" has an invalid syntax. ";
                msg = msg + " This is happening in the BuildCheckListDictionary function.";
                msg = msg + "\n\n" + e.Message;
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, "Up The Creek Without A Paddle");
                explain.ShowDialog();
            }
            return thisCheckListDict;
        }

        internal static string MakePcombRegexFiltetString(string strThis, List<string> extList)
        {
            string pat_toapply = strThis + @".*\.(";
            foreach (string s in extList)
            {
                pat_toapply = pat_toapply + s + '|';
            }
            pat_toapply = pat_toapply.TrimEnd('|');
            pat_toapply = pat_toapply + @")";
            return pat_toapply;
        }

        /// Given a list of file extensions, less any period, returns a regex expression
        /// that when used in context with file names, matches only those files that have
        /// the file extension text.
        internal static string MakeFilesListRegexString(List<string> extList)
        {
            string resStr = @"^.+\.(";
            foreach (string s in extList)
            {
                resStr = resStr + s + '|';
            }
            resStr = resStr.TrimEnd('|');
            resStr = resStr + ")$";
            return resStr;
        }

        internal static void GetPrefs(RadSession theRad, ObservableCollection<string> theRadPComboList, string _PrefData)
        {
            string _PrefPath = theRad.PrefsPath;
            string prefFile = _PrefPath + _PrefData;
            string line = string.Empty;

            if (!Directory.Exists(_PrefPath))
            {
                string msg = "Please set a valid directory to be the location where "
                    + "this application saves the settings file " + _PrefData + ". The "
                    + "current settings file directory " + _PrefPath + " is unknown.";
                string APath = _PrefPath;
                FormMsgWPF askUserForPath = new FormMsgWPF(APath, 0); // mode 0 means ask for path
                askUserForPath.SetMsg(msg, "What Me Worry?");
                askUserForPath.ShowDialog();
                _PrefPath = askUserForPath.ThePath;
                Properties.Settings.Default.PrefsPath = _PrefPath;
                Properties.Settings.Default.Save();
                theRad.PrefsPath = _PrefPath;
            }

            if (File.Exists(prefFile))
            {
                StreamReader thisPrefs = new StreamReader(prefFile);
                while ((line = thisPrefs.ReadLine()) != null)
                {
                    string hdr = string.Empty;
                    int rLen = 0;
                    string theR = string.Empty;
                    if (line.Length > 4) // line is long enough to decode
                    {
                        hdr = line.Substring(0, 3); // first three characters
                        rLen = line.Length - 3;
                        theR = line.Substring(3, rLen);
                        try
                        {
                            switch (hdr)
                            {
                                case "pp:":
                                    theRad.ProjectPath = theR;
                                    break;
                                case "vs:":
                                    theRad.VScnPath = theR;
                                    break;
                                case "bs:":
                                    theRad.BScnPath = theR;
                                    break;
                                case "mp:":
                                    theRad.MatPath = theR;
                                    break;
                                case "ir:":
                                    theRad.IrradPath = theR;
                                    break;
                                case "im:":
                                    theRad.ImagePath = theR;
                                    break;
                                case "ci:":
                                    theRad.PcombimagesourcePath = theR;
                                    break;
                                case "ct:":
                                    theRad.PcombimagetargetPath = theR;
                                    break;
                                case "ff:":
                                    theRad.RatiffimagePath = theR;
                                    break;
                                case "fi:":
                                    theRad.FinalimagePath = theR;
                                    break;
                                case "ap:":
                                    theRad.AmbPath = theR;
                                    break;
                                case "oc:":
                                    theRad.OctPath = theR;
                                    break;
                                case "ip:":
                                    theRad.IllumPath = theR;
                                    break;
                                case "if:":
                                    theRad.IllumFileName = theR;
                                    break;
                                case "sh:":
                                    theRad.SHour = Convert.ToInt32(theR);
                                    break;
                                case "eh:":
                                    theRad.EHour = Convert.ToInt32(theR);
                                    break;
                                case "hi:":
                                    theRad.HrInc = Convert.ToDouble(theR);
                                    break;
                                case "ms:":
                                    theRad.MStart = Convert.ToInt32(theR);
                                    break;
                                case "me:":
                                    theRad.MEnd = Convert.ToInt32(theR);
                                    break;
                                case "mi:":
                                    theRad.MInc = Convert.ToDouble(theR);
                                    break;
                                case "ri:":
                                    theRad.RadPath = theR;
                                    break;
                                case "ov:":
                                    theRad.OvertureOptsFileName = theR;
                                    break;
                                case "rp:":
                                    theRad.RenderOptsPath = theR;
                                    break;
                                case "ro:":
                                    theRad.RenderOptsFileName = theR;
                                    break;
                                case "mk:":
                                    theRad.MkillumOptsFileName = theR;
                                    break;
                                case "pi:":
                                    theRad.IncRpictI = Convert.ToBoolean(theR);
                                    break;
                                case "bf:":
                                    theRad.BatchFileName = theR;
                                    break;
                                case "bp:":
                                    theRad.PcombBatchFileName = theR;
                                    break;
                                case "ix:":
                                    theRad.ImgX = Convert.ToInt32(theR);
                                    break;
                                case "iy:":
                                    theRad.ImgY = Convert.ToInt32(theR);
                                    break;
                                case "ox:":
                                    theRad.OvImgX = Convert.ToInt32(theR);
                                    break;
                                case "oy:":
                                    theRad.OvImgY = Convert.ToInt32(theR);
                                    break;
                                case "sd:":
                                    theRad.SimDay = Convert.ToInt32(theR);
                                    break;
                                case "nt:":
                                    theRad.NightTime = Convert.ToBoolean(theR);
                                    break;
                                case "sr:":
                                    theRad.SetRadEnv = Convert.ToBoolean(theR);
                                    break;
                                case "uo:":
                                    theRad.UseOverture = Convert.ToBoolean(theR);
                                    break;
                                case "p0:":
                                    theRad.Pcombpause = Convert.ToBoolean(theR);
                                    break;
                                case "rx:":
                                    //theRad.Pcompfilters.Add(theR);
                                    theRadPComboList.Add(theR);
                                    break;
                                case "s0:":
                                    theRad.Scales[0] = Convert.ToDouble(theR);
                                    break;
                                case "s1:":
                                    theRad.Scales[1] = Convert.ToDouble(theR);
                                    break;
                                case "s2:":
                                    theRad.Scales[2] = Convert.ToDouble(theR);
                                    break;
                                case "s3:":
                                    theRad.Scales[3] = Convert.ToDouble(theR);
                                    break;
                                case "s4:":
                                    theRad.Scales[4] = Convert.ToDouble(theR);
                                    break;
                                case "s5:":
                                    theRad.Scales[5] = Convert.ToDouble(theR);
                                    break;
                                case "s6:":
                                    theRad.Scales[6] = Convert.ToDouble(theR);
                                    break;
                                case "s7:":
                                    theRad.Scales[7] = Convert.ToDouble(theR);
                                    break;
                                case "hv:":
                                    theRad.Hdrviewer = theR;
                                    break;
                                case "ed:":
                                    theRad.EditorPath = theR;
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception err)
                        {
                            string msg = "This is a problem with this line in the preferences file '" + prefFile + "'\n\n" + line + "\n\n" + err.ToString();
                            string msgT = "Error Reading Saved Preferences";
                            FormMsgWPF explain = new FormMsgWPF(null, 3);
                            explain.SetMsg(msg, msgT);
                            explain.ShowDialog();
                        }
                    }
                }
                thisPrefs.Close();
            }
        }

        internal static void SavePrefs(RadSession theRad, ObservableCollection<string> theRadPComboList, string _PrefData)
        {
            string _PrefPath = theRad.PrefsPath;
            if (!Directory.Exists(_PrefPath))
            {
                string msg = "Please set a valid directory to be the location where "
                    + "this application saves the settings file " + _PrefData + ". The "
                    + "current settings file directory " + _PrefPath + " is unknown.";
                string APath = _PrefPath;
                FormMsgWPF askUserForPath = new FormMsgWPF(APath, 0); // mode 0 means ask for path
                askUserForPath.SetMsg(msg, "Last Request - Closing Time");
                askUserForPath.ShowDialog();
                _PrefPath = askUserForPath.ThePath;
                askUserForPath = null;
                if (Directory.Exists(_PrefPath))
                {
                    Properties.Settings.Default.PrefsPath = _PrefPath;
                    Properties.Settings.Default.Save();
                }
                else
                {
                    msg = "Path " + _PrefPath + " is invalid.";
                    string msgT = "Settings Not Saved!";
                    FormMsgWPF explain = new FormMsgWPF(null, 3);
                    explain.SetMsg(msg, msgT);
                    explain.ShowDialog();
                    return;
                }
            }

            string prefFile = _PrefPath + _PrefData;
            try
            {
                StreamWriter thisPrefFile = new StreamWriter(prefFile);
                thisPrefFile.WriteLine("This is the Radscript preferences file. ");
                thisPrefFile.WriteLine("Remark >> ri: + theRad.RadPath - The Radiance programs install path");
                thisPrefFile.WriteLine("ri:" + theRad.RadPath);
                thisPrefFile.WriteLine("Remark >> pp:  theRad.ProjectPath - The project path");
                thisPrefFile.WriteLine("pp:" + theRad.ProjectPath);
                thisPrefFile.WriteLine("Remark >> vs:  theRad.VScnPath - The variable scenes path within the project");
                thisPrefFile.WriteLine("vs:" + theRad.VScnPath);
                thisPrefFile.WriteLine("Remark >> bs: theRad.BScnPath - The base scenes path within the project");
                thisPrefFile.WriteLine("bs:" + theRad.BScnPath);
                thisPrefFile.WriteLine("Remark >> mp: + theRad.MatPath - The materials path within the project");
                thisPrefFile.WriteLine("mp:" + theRad.MatPath);
                thisPrefFile.WriteLine("Remark >> rp: + theRad.RenderOptsPath - The render options path within the project");
                thisPrefFile.WriteLine("rp:" + theRad.RenderOptsPath);
                thisPrefFile.WriteLine("Remark >> ro: + theRad.RenderOptsFileName - The render options file name within the project");
                thisPrefFile.WriteLine("ro:" + theRad.RenderOptsFileName);
                thisPrefFile.WriteLine("Remark >> mk: + theRad.MkillumOptsFileName - The mkillum options file name within the project");
                thisPrefFile.WriteLine("mk:" + theRad.MkillumOptsFileName);
                thisPrefFile.WriteLine("Remark >> ov: + theRad.OvertureOptsFileName - The overture options file name within the project");
                thisPrefFile.WriteLine("ov:" + theRad.OvertureOptsFileName);
                thisPrefFile.WriteLine("Remark >> ir: + theRad.IrradPath - The irradiance path within the project");
                thisPrefFile.WriteLine("ir:" + theRad.IrradPath);
                thisPrefFile.WriteLine("Remark >> im: + theRad.ImagePath - The images path within the project");
                thisPrefFile.WriteLine("im:" + theRad.ImagePath);
                thisPrefFile.WriteLine("Remark >> ci: + theRad.PcombimagesourcePath - The pcomb images source path within the project");
                thisPrefFile.WriteLine("ci:" + theRad.PcombimagesourcePath);
                thisPrefFile.WriteLine("Remark >> ct: + theRad.PcombimagetargetPath - The pcomb images target path within the project");
                thisPrefFile.WriteLine("ct:" + theRad.PcombimagetargetPath);
                thisPrefFile.WriteLine("Remark >> ff: + theRad.RatiffimagePath - The ra_tiff images results path within the project");
                thisPrefFile.WriteLine("ff:" + theRad.RatiffimagePath);
                thisPrefFile.WriteLine("Remark >> fi: + theRad.FinalimagePath - The final images results path within the project");
                thisPrefFile.WriteLine("fi:" + theRad.FinalimagePath);
                thisPrefFile.WriteLine("Remark >> ap: + theRad.AmbPath - The amb path within the project");
                thisPrefFile.WriteLine("ap:" + theRad.AmbPath);

                thisPrefFile.WriteLine("Remark >> i_: + theRad.ImgX - The image x and y pixel size.");
                thisPrefFile.WriteLine("ix:" + theRad.ImgX.ToString());
                thisPrefFile.WriteLine("iy:" + theRad.ImgY.ToString());
                thisPrefFile.WriteLine("Remark >> o_: + theRad.OvImgX - The overture image x and y pixel size.");
                thisPrefFile.WriteLine("ox:" + theRad.OvImgX.ToString());
                thisPrefFile.WriteLine("oy:" + theRad.OvImgY.ToString());

                thisPrefFile.WriteLine("Remark >> ip: + theRad.IllumPath - The illum path within the project");
                thisPrefFile.WriteLine("ip:" + theRad.IllumPath);
                thisPrefFile.WriteLine("Remark >> if: + theRad.IllumFileName - The illum file name");
                thisPrefFile.WriteLine("if:" + theRad.IllumFileName);
                thisPrefFile.WriteLine("Remark >> op: + theRad.OctPath - The oct path within the project");
                thisPrefFile.WriteLine("op:" + theRad.OctPath);
                thisPrefFile.WriteLine("Remark >> sh: + theRad.SHour - simulation hour start");
                thisPrefFile.WriteLine("sh:" + theRad.SHour);
                thisPrefFile.WriteLine("Remark >> eh: + theRad.EHour - simulation hour end");
                thisPrefFile.WriteLine("eh:" + theRad.EHour);
                thisPrefFile.WriteLine("Remark >> hi: + theRad.HrInc - simulation hour increment");
                thisPrefFile.WriteLine("hi:" + theRad.HrInc);
                thisPrefFile.WriteLine("Remark >> ms: + theRad.MStart - simulation start month");
                thisPrefFile.WriteLine("ms:" + theRad.MStart);
                thisPrefFile.WriteLine("Remark >> me: + theRad.MEnd - simulation last month");
                thisPrefFile.WriteLine("me:" + theRad.MEnd);
                thisPrefFile.WriteLine("Remark >> mi: + theRad.MInc - simulation month increment");
                thisPrefFile.WriteLine("mi:" + theRad.MInc);
                thisPrefFile.WriteLine("Remark >> pi: + theRad.IncRpictI - flag to include rpicti line");
                thisPrefFile.WriteLine("pi:" + theRad.IncRpictI.ToString());
                thisPrefFile.WriteLine("Remark >> bf: + theRad.BatchFileName - rad script batch file name, will be written at project path");
                thisPrefFile.WriteLine("bf:" + theRad.BatchFileName);
                thisPrefFile.WriteLine("Remark >> bp: + theRad.PcombBatchFileName - pcomb batch file name ");
                thisPrefFile.WriteLine("bp:" + theRad.PcombBatchFileName);
                thisPrefFile.WriteLine("Remark >> ed: + theRad.EditorPath - code editor application");
                thisPrefFile.WriteLine("ed:" + theRad.EditorPath);

                thisPrefFile.WriteLine("Remark >> sd: + theRad.SimDay - day of the month for simulations");
                thisPrefFile.WriteLine("sd:" + theRad.SimDay.ToString());
                thisPrefFile.WriteLine("Remark >> nt: + theRad.NightTime - flag to be night time");
                thisPrefFile.WriteLine("nt:" + theRad.NightTime.ToString());
                thisPrefFile.WriteLine("Remark >> sr: + theRad.SetRadEnv - flag indicates to set Path and RAYPATH in the batch file");
                thisPrefFile.WriteLine("sr:" + theRad.SetRadEnv.ToString());
                thisPrefFile.WriteLine("Remark >> uo: + theRad.UseOverture - flag to use overtures");
                thisPrefFile.WriteLine("uo:" + theRad.UseOverture.ToString());
                thisPrefFile.WriteLine("Remark >> hv: + theRad.Hdrviewer - The application for viewing HDR image files");
                thisPrefFile.WriteLine("hv:" + theRad.Hdrviewer);
                thisPrefFile.WriteLine("Remark >> p0: + theRad.Pcombpause - flag pauses each pcomb process for observation");
                thisPrefFile.WriteLine("p0:" + theRad.Pcombpause.ToString());

                thisPrefFile.WriteLine("Remark >> rx: + rgl - regex filters for filtering the source file selections list at pcomb");
                foreach (string rgl in theRadPComboList)
                {
                    thisPrefFile.WriteLine("rx:" + rgl);
                }
                thisPrefFile.WriteLine("Remark >> s#: + # - The pcomb scales arguments");
                for (int i = 0; i < theRad.Scales.Length; i++)
                {
                    string val = theRad.Scales[i].ToString();
                    string prefcode = "s" + i.ToString() + ":";
                    thisPrefFile.WriteLine(prefcode + val);
                }
                thisPrefFile.Close();
            }
            catch (Exception e)
            {
                //MessageBox.Show("Error at SavePrefs: " + e.ToString());
                string msg = e.ToString();
                string msgT = "Error at SavePrefs";
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, msgT);
                explain.ShowDialog();
            }
        }

        internal static void AddToList(ref ObservableCollection<string> pcombfilters, string newpcombfilter)
        {
            if (!pcombfilters.Contains(newpcombfilter))
            {
                pcombfilters.Add(newpcombfilter);
                ObservableCollection<string> a = new ObservableCollection<string>(pcombfilters.OrderBy(i => i));
                pcombfilters = a;
            }
        }

        /// <summary>
        /// Gets rad header
        /// </summary>
        /// <param name="thisRad"></param>
        /// <param name="radfile"></param>
        /// <returns></returns>
        internal static string GetInfoThisRadFile(RadSession thisRad, string radfile)
        {
            bool bedebug = false;
            string theArgs = string.Empty;
            string appN = "getinfo ";
            string size = string.Empty;
            if (!thisRad.Getinfofull) { size = " -d "; }
            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            // add radiance install path to PATH the child process use
            string pathvar = Environment.GetEnvironmentVariable("PATH");
            psi.EnvironmentVariables["PATH"] = @thisRad.RadPath + @"bin;" + pathvar;
            string raypath = String.Concat(@thisRad.RadPath, "lib");
            psi.EnvironmentVariables["RAYPATH"] = @raypath;
            // Required when setting process EnvironmentVariables
            psi.UseShellExecute = false;
            // set the working directory
            psi.WorkingDirectory = @thisRad.ProjectPath;
            // Tactic is for CMD.EXE and the program name to be part of argument
            psi.FileName = "CMD.EXE";
            if (!bedebug)
            {
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                // no window
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                theArgs = String.Concat(@"/c ", appN, size, radfile);  // window closes
                                                                       // MessageBox.Show(theArgs);
            }
            else
            {
                psi.RedirectStandardError = false;
                psi.RedirectStandardOutput = false;
                // no window
                psi.CreateNoWindow = false;
                psi.WindowStyle = ProcessWindowStyle.Normal;
                theArgs = String.Concat(@"/k ", appN, size, radfile);  // window stays open
                                                                       // MessageBox.Show(theArgs);
            }
            // set the arguments
            psi.Arguments = theArgs;

            // Starts process
            string error = string.Empty;
            string output = string.Empty;
            p.StartInfo = psi;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected error stream.
            // p.WaitForExit();
            // Read the error stream first and then wait.
            if (!bedebug)
            {
                error = p.StandardError.ReadToEnd();
                output = p.StandardOutput.ReadToEnd();
            }
            p.WaitForExit();
            if (error.Equals(string.Empty))
            {
                return output.Trim();
            }
            else
            {
                // there was an error. 
                thisRad.StatusMsg = "getinfo reported this problem:\n" + error;
                return error;
            }

        }

        /// <summary>
        /// Instant pcomb. Returns true if pcomb did not error, false if there was.
        /// </summary>
        /// <param name="thisRad"></param>
        /// <param name="args"></param>
        /// <param name="target"></param>
        /// <param name="statMsg"></param>
        /// <returns></returns>
        internal static bool PCombThisLine(RadSession thisRad, string args, string target)
        {
            //bool bedebug = false;
            string theArgs = args;
            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            // add radiance install path to PATH the child process use
            string pathvar = Environment.GetEnvironmentVariable("PATH");
            psi.EnvironmentVariables["PATH"] = @thisRad.RadPath + @"bin;" + pathvar;
            string raypath = String.Concat(@thisRad.RadPath, "lib");
            psi.EnvironmentVariables["RAYPATH"] = @raypath;
            // Required when setting process EnvironmentVariables
            psi.UseShellExecute = false;
            // set the working directory
            psi.WorkingDirectory = @thisRad.ProjectPath;
            // Tactic is for CMD.EXE and the program name to be part of argument
            psi.FileName = "CMD.EXE";
            if (!thisRad.Pcombpause)
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
                // no window
                psi.CreateNoWindow = false;
                psi.WindowStyle = ProcessWindowStyle.Normal;
                theArgs = String.Concat(@"/k ", theArgs);  // window stays open
            }
            // set the arguments
            psi.Arguments = theArgs;
            #region Debug
            //// show window or not
            //if (!bedebug)
            //{
            //    psi.WindowStyle = ProcessWindowStyle.Hidden;
            //}
            //else
            //{
            //    psi.WindowStyle = ProcessWindowStyle.Normal;
            //} 
            #endregion
            // Starts process
            string error = string.Empty;
            p.StartInfo = psi;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected error stream.
            // p.WaitForExit();
            // Read the error stream first and then wait.
            if (!thisRad.Pcombpause)
            {
                error = p.StandardError.ReadToEnd();
            }
            p.WaitForExit();
            if (error.Equals(string.Empty))
            {
                // no error message generated
                if (File.Exists(thisRad.Hdrviewer))
                {
                    thisRad.StatusMsg = "PComb Done. Now starting viewer for\n" + target;
                    Process v = new Process();
                    ProcessStartInfo pvsi = new ProcessStartInfo();
                    pvsi.FileName = thisRad.Hdrviewer;
                    pvsi.Arguments = target;
                    v.StartInfo = pvsi;
                    v.Start();
                }
                else
                {
                    thisRad.StatusMsg = "Pcomb Done. But no viewer specified.\nYou should define a viewer.";
                    string msg = "The image might have been created but the HDR viewer application for viewing it has yet to be set!";
                    FormMsgWPF explain = new FormMsgWPF(null, 3);
                    explain.SetMsg(msg, "Is the HDR viewing application set?");
                    explain.ShowDialog();
                }
                return true;
            }
            else
            {
                // there was an error. 
                thisRad.StatusMsg = "pcomb reported this problem:\n" + error;
                return false;
            }
        }

        internal static void CheckInThisListBox(ListBox thisListBox, bool beChecked)
        {
            foreach (var item in thisListBox.Items)
            {
                if (item.GetType() == typeof(CheckBox))
                {
                    CheckBox cb = item as CheckBox;
                    cb.IsChecked = beChecked;
                }
            }
        }

        internal static void RunCodeEditorThisProject(RadSession theRad)
        {
            if (File.Exists(theRad.EditorPath))
            {
                string path = theRad.ProjectPath;
                Process.Start(theRad.EditorPath, path);
            }

        }

        internal static void CreateThisPath(string newPathToCreate)
        {
            try
            {
                Directory.CreateDirectory(@newPathToCreate);
            }
            catch (Exception er)
            {
                string ttl = "Create Directory Error";
                string msg = "Unable to create the paths in " + newPathToCreate + "\n\n" + er.Message;
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, ttl);
                explain.ShowDialog();
            }
        }

        // reports if text in textbox would be ok for a windows filename
        internal static void SniffTextBoxToBeAValidFileName(TextBox theTextBox)
        {
            if (theTextBox == null) { return; }
            string fn = theTextBox.Text;
            if (fn.Trim() == string.Empty) { return; }
            if (!IsValidWindowsFileName(fn))
            {
                string msg = "Windows will not allow \n\n" + fn + "\n\n to be a file name.";
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, "By The Way. Not A Valid Name");
                explain.ShowDialog();
            }
        }

        /// <summary>
        /// Only works properly on file names. Do not use for full path names.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static bool IsValidWindowsFileName(string expression)
        {
            // https://stackoverflow.com/questions/62771/how-do-i-check-if-a-given-string-is-a-legal-valid-file-name-under-windows
            string sPattern = @"^(?!^(PRN|AUX|CLOCK\$|NUL|CON|COM\d|LPT\d|\..*)(\..+)?$)[^\x00-\x1f\\?*:\"";|/]+$";
            return (Regex.IsMatch(expression, sPattern, RegexOptions.CultureInvariant));
        }

        /// <summary>
        /// Tries to find ghostscript via registry and typical install location.
        /// </summary>
        /// <returns></returns>
        internal static string FindGhostScript()
        {
            // 
            string registryRoot = @"SOFTWARE\\GPL Ghostscript";
            RegistryKey root;
            //     case "HKCU":
            //root = Registry.CurrentUser.OpenSubKey(registryRoot, false);
            //     case "HKLM":
            root = Registry.LocalMachine.OpenSubKey(registryRoot, false);

            if (root != null)
            {
                var subKeys = root.GetSubKeyNames();
                string gsExecPath = @"C:\Program Files\gs\gs" + subKeys.FirstOrDefault<string>().ToString() + @"\bin\gswin64.exe";
                //MessageBox.Show(gsExecPath);
                return gsExecPath;
            }
            else
            {
                return "Not Installed. Convert to PDF wil not be possible.";
            }
        }

        internal static string FindSomething(string something)
        {
            bool debug = false;
            string Sp = " ";
            String theArgs = string.Empty;
            Process p = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            // Required when setting process EnvironmentVariables
            psi.UseShellExecute = false;
            // set the working directory
            // Tactic is for CMD.EXE and programname to be part of argument
            psi.FileName = "CMD.EXE";
            if (!debug)
            {
                psi.RedirectStandardError = true;
                psi.RedirectStandardOutput = true;
                // no window
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                theArgs = String.Concat(@"/c ", "where" + Sp + something);  // window closes
            }
            else
            {
                psi.RedirectStandardError = false;
                psi.RedirectStandardOutput = false;
                psi.CreateNoWindow = false;
                psi.WindowStyle = ProcessWindowStyle.Normal;
                theArgs = String.Concat(@"/k ", "where" + Sp + something);  // window stays open
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
            string output = string.Empty;
            string error = string.Empty;
            p.StartInfo = psi;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected error stream.
            // p.WaitForExit();
            // Read the error stream first and then wait.
            if (!debug)
            {
                error = p.StandardError.ReadToEnd();
                output = p.StandardOutput.ReadToEnd();
            }
            p.WaitForExit();
            if (error.Equals(string.Empty))
            {
                return output;
            }
            else
            {
                //return error;
                return "Did not locate " + something;
            }
        }

        internal static bool RegistryValueExists(string _strHive_HKLM_HKCU, string _registryRoot, string _valueName)
        {
            RegistryKey root;
            switch (_strHive_HKLM_HKCU.ToUpper())
            {
                case "HKLM":
                    root = Registry.LocalMachine.OpenSubKey(_registryRoot, false);
                    break;
                case "HKCU":
                    root = Registry.CurrentUser.OpenSubKey(_registryRoot, false);
                    break;
                default:
                    throw new InvalidOperationException("parameter registryRoot must be either \"HKLM\" or \"HKCU\"");
            }

            return root.GetValue(_valueName) != null;
        }

        internal static void DeleteFileFromListBox(object sender, string rootPath, string contextSubPath)
        {
            ListBox lb = sender as ListBox;
            if (lb.SelectedItems.Count == 0) { return; }
            string msg = string.Empty;
            string ttl = "What? Delete This File?";
            if (lb.SelectedItems.Count > 1) { ttl = "What?, Delete These Files?"; }
            foreach (ListBoxItem lbi in lb.SelectedItems)
            {
                msg = msg + lbi.Content.ToString() + "\n";
            }
            FormMsgWPF explain = new FormMsgWPF(null, 2);
            explain.SetMsg(msg, ttl);
            explain.ShowDialog();
            if (explain.theResult == MessageBoxResult.OK)
            {
                foreach (ListBoxItem lbi in lb.SelectedItems)
                {
                    String fname = Helpers.CombineIntoPath(rootPath, contextSubPath, lbi.Content.ToString());
                    if (fname != string.Empty)
                    {
                        if (File.Exists(fname))
                        {
                            try
                            {
                                File.Delete(fname);
                            }
                            catch (Exception err)
                            {
                                msg = "Cannot delete.";
                                msg = msg + "\n\n" + err.Message;
                                explain = new FormMsgWPF(null, 3);
                                explain.SetMsg(msg, "IO Error");
                                explain.ShowDialog();
                            }
                        }
                    }
                }
            }
        }

        internal static void ExploreListBoxSelection(ListBox listBox, string listPath)
        {
            var selected = listBox.SelectedItem;
            if (selected == null)
            {
                Process.Start("explorer.exe", listPath);
                return;
            }

            if (selected.GetType() == typeof(CheckBox))
            {
                CheckBox cb = selected as CheckBox;
                TextBlock tb = cb.Content as TextBlock;
                if (tb != null)
                {
                    string fullPathName = listPath + tb.Text;
                    ExploreFile(fullPathName);
                    return;
                }
            }

            if (selected.GetType() == typeof(ListBoxItem))
            {
                ListBoxItem lbi = selected as ListBoxItem;
                if (lbi != null)
                {
                    string fullPathName = listPath + lbi.Content.ToString();
                    ExploreFile(fullPathName);
                    return;
                }
            }
        }

        internal static bool ExploreFile(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return false;
            }
            //Clean up file path so it can be navigated OK
            filePath = Path.GetFullPath(filePath);
            Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
            return true;
        }



    }

    internal static class StringExtensions
    {
        internal static string Left(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);
            return (value.Length <= maxLength
                    ? value
                    : value.Substring(0, maxLength)
                    );
        }

        internal static string ReplaceString(this string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }
    }

    internal static class BitmapExtensions
    {
        internal static BitmapImage GetBitmapImage(this Uri imageAbsolutePath, BitmapCacheOption bitmapCacheOption = BitmapCacheOption.Default)
        {
            BitmapImage bi = new BitmapImage();
            try
            {
                bi.BeginInit();
                bi.CacheOption = bitmapCacheOption;
                bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bi.UriSource = imageAbsolutePath;
                bi.EndInit();
                return bi;
            }
            catch (Exception e)
            {
                string msg = "Trouble viewing \"" + imageAbsolutePath + "\".";
                msg = msg + "\n\n" + e.Message;
                FormMsgWPF explain = new FormMsgWPF(null, 3);
                explain.SetMsg(msg, "Sorry Charlie");
                explain.ShowDialog();
                bi = null;
            }
            return bi;
        }
    }

    /// <summary>
    /// Used to convert system drawing colors to WPF brush
    /// </summary>
    internal static class ColorExt
    {
        internal static System.Windows.Media.Brush ToBrush(System.Drawing.Color color)
        {
            {
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
        }
    }
}
