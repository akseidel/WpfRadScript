//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace WpfRadScript
//{
//    class ClassFromVBARadscript
//    {
//        // path where preference datafile resides
//        const string PrefPath = "U:\\";
//        // name of preference data file
//        const string PrefData = "RadScript.dat";
//        const string Sp = " ";
//        const string Dot = ".";
//        const string DotDot = "..";
//        const string PSep = "\\";
//        const string RadExt = ".rad";
//        const string SSSExt = ".sss";
//        const string App1WName = "Explorer";
//        const string App1FName = "Explorer.exe";
//        const string App2WName = "NotePad";
//        const string App2FName = "Notepad.exe";
//        string RayPath;
//        int mnth;
//        int runday;
//        double hr;
//        string Sky;
//        string RadPath;
//        //Dim fnBatchPath As String
//        string BatchFile;
//        string Mat;
//        string MatPath;
//        string BScnPath;
//        string VScnPath;
//        string IllumPath;
//        string IllumFile;
//        string SkyPath;
//        string SunFile;
//        string SkyFile;
//        //Dim ScnPath As String
//        string ScnBase;
//        string ScnOpt1;
//        string ScnOpt2;
//        string ScnOpt3;
//        string ViewPath;
//        string OctPath;
//        string OctBase;
//        string AmbPath;
//        string AmbFile;
//        string ImagePath;
//        string ImgSize1;
//        //ImgeSize2 is handled as a function call
//        string RadImage;
//        string IrradPath;
//        string IrradImage;
//        string FalseColorPath;
//        string FalseColorImage;
//        string FalseContPath;
//        string FalseContImage;
//        string PcondPath;
//        string RenderOptsPath;
//        string RenderOptsFile;
//        string MkillumOptsFile;
//        string OvertureOptsFile;
//        string View;
//        int SHour;
//        int EHour;
//        double HrInc;
//        int MStart;
//        int MEnd;
//        double MInc;

//        bool IncRpictI;

//        private long lngHwnd;
//        private void cbMkIllumOpt_Click()
//        {
//            if (!IsNull(ListBoxOptions))
//            {
               
//                tbox_mkillumoptfile.Content = ListBoxOptions;
//                MkillumOptsFile = tboxMkillumOptFile.Text;
//                Check4Files();
//            }
//        }

//        private void cbOvertureOpt_Click()
//        {
//            if (!IsNull(ListBoxOptions))
//            {
//                tboxOverOPtFile.Text = ListBoxOptions;
//                OvertureOptsFile = tboxOverOPtFile.Text;
//                Check4Files();
//            }
//        }

//        private void cbPowerDeskBS_Click()
//        {
//            int stat = 0;
//            stat = ShowApp1(AsFullPath(BScnPath));
//        }

//        private void cbSetRenderOpt_Click()
//        {
//            if (!IsNull(ListBoxOptions))
//            {
//                tboxRenderOptFile.Text = ListBoxOptions;
//                RenderOptsFile = this.tboxRenderOptFile.Text;
//                Check4Files();
//            }
//        }

//        private void cbViewBatchFile_Click()
//        {
//            string fname = null;
//            BatchFile = this.tboxBatchFName;
//            fname = fnBatchPath() + BatchFile;

//            if (PathExists(fnBatchPath() + BatchFile))
//            {
//                //        Shell "notepad.exe" & " " & fnBatchPath & BatchFile, vbNormalFocus
//            }
//            else
//            {
//                Interaction.MsgBox("No such file " + fnBatchPath() + BatchFile);
//            }
//        }

//        private void chkBoxIncludeRpicti_Click()
//        {
//            IncRpictI = chkBoxIncludeRpicti.Value;
//            RegenCmdLines();
//        }

//        private void lbOvertureOPt_Click()
//        {
//            ShowOptionsFile(ufScenePicker.tboxOverOPtFile);
//            //ShowOvertureOptFile
//        }

//        private void lboxSelViews_Change()
//        {
//            if (string.IsNullOrEmpty(lboxSelViews.Text))
//            {
//                tboxOtherViewer.Text = "";
//            }
//        }

//        private void tboxMkillumOptFile_AfterUpdate()
//        {
//            MkillumOptsFile = this.tboxMkillumOptFile.Text;
//            Check4Files();
//        }

//        private void tboxMkillumOptFile_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelMkillumOptFile();
//        }

//        private void tboxOptFileViewer_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            EditOptionsFile();
//        }

//        private void tboxOtherViewer_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            EditViewFile();
//        }

//        private void tboxOverOPtFile_AfterUpdate()
//        {
//            OvertureOptsFile = this.tboxOverOPtFile.Text;
//            Check4Files();
//        }

//        private void tboxOverOPtFile_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelOvertureOptFile();
//        }

//        private void tboxOverOPtFile_MouseDown(int Button, int Shift, float X, float Y)
//        {
//            ShowOptionsFile(ufScenePicker.tboxOverOPtFile);
//            //ShowOvertureOptFile
//        }

//        private void tboxRadInstPath_AfterUpdate()
//        {
//            string str = null;
//            str = this.tboxRadInstPath.Text;
//            this.tboxRadInstPath.Text = EnsureAsPath(str);
//            RadPath = this.tboxRadInstPath.Text;
//        }

//        private void tboxRenderOptPath_Change()
//        {
//            string str = null;
//            str = this.tboxRenderOptPath.Text;
//            this.tboxRenderOptPath.Text = EnsureAsPath(str);
//            RenderOptsPath = this.tboxRenderOptPath.Text;
//        }

//        private void tboxRenderOptPath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelRendOptsPath();
//        }

//        private void tboxViewSceneFile_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            EditSceneFile();
//        }

//        private void UserForm_Initialize()
//        {
//            MiscInits();
//            FillListBoxes();
//            Check4Files();
//            SetViewAsPicked();
//            RegenCmdLines();
//            // rengen must be last item
//            mpgCompo.Value = 0;
//        }

//        private void MiscInits()
//        {
//            // used to set defaults
//            RadPath = "C:\\Program Files\\Radiance\\";
//            OctPath = ".\\octree\\";
//            // has picker
//            IllumPath = ".\\illums\\";
//            // has picker
//            IllumFile = "illums.rad";
//            // has picker
//            MkillumOptsFile = "mkillum.opt";
//            //has picker
//            OvertureOptsFile = "overture.opt";
//            //has picker
//            ImgSize1 = "-x 400 -y 400";
//            // has setter
//            AmbPath = ".\\ambfile\\";
//            // has picker
//            ImagePath = ".\\images\\";
//            // has picker
//            IrradPath = ImagePath + "irradiance\\";
//            // has picker
//            ViewPath = ".\\views\\";
//            // has picker
//            View = "vne_iso";
//            //has setter
//            RenderOptsFile = "options.opt";
//            // has picker
//            RenderOptsPath = ".\\optfile\\";
//            // has picker
//            MatPath = ".\\material\\";
//            BScnPath = ".\\scene\\";
//            VScnPath = ".\\scene\\";
//            Prjpath = "r:\\proj\\05017a\\rad\\test\\";
//            IncRpictI = false;
//            BatchFile = "vba_batch.bat";
//            // used to reset to what has been saved
//            GetPrefs();
//            this.tboxRadInstPath = RadPath;
//            this.tboxBatchFName = BatchFile;
//            this.tboxPrjPath = Prjpath;
//            this.tboxMatPath = MatPath;
//            this.tboxBScnPath = BScnPath;
//            this.tboxVScnPath = VScnPath;
//            this.tboxView = View;
//            this.tboxIrradPath = IrradPath;
//            this.tboxImagePath = ImagePath;
//            this.tboxAmbPath = AmbPath;
//            this.tboxOctPath = OctPath;
//            this.tboxIllumPath = IllumPath;
//            this.tboxIllumFile = IllumFile;
//            this.tboxSHour = SHour;
//            this.tboxEHour = EHour;
//            this.tboxHrInc = HrInc;
//            this.tboxMStart = MStart;
//            this.tboxMEnd = MEnd;
//            this.tboxMInc = MInc;
//            this.tboxRenderOptPath = RenderOptsPath;
//            this.tboxOverOPtFile = OvertureOptsFile;
//            this.tboxMkillumOptFile = MkillumOptsFile;
//            this.tboxRenderOptFile = RenderOptsFile;
//            this.chkBoxIncludeRpicti = IncRpictI;
//        }

//        public string fnBatchPath()
//        {
//            return this.tboxPrjPath;
//        }

//        public bool Check4MatScn()
//        {
//            bool functionReturnValue = false;
//            int stat = 0;
//            string title = null;
//            if (string.IsNullOrEmpty(Mat) | string.IsNullOrEmpty(ScnBase))
//            {
//                if (string.IsNullOrEmpty(Mat) & string.IsNullOrEmpty(ScnBase))
//                {
//                    title = "Materials and Scene(s) Are Missing";
//                }
//                if (string.IsNullOrEmpty(Mat) & !string.IsNullOrEmpty(ScnBase))
//                {
//                    title = "Materials Are Missing";
//                }
//                if (!string.IsNullOrEmpty(Mat) & string.IsNullOrEmpty(ScnBase))
//                {
//                    title = "Scene(s) Are Missing";
//                }
//                stat = Interaction.MsgBox("Write batch file anyway?", Constants.vbQuestion + Constants.vbOKCancel, title);
//                if (stat == Constants.vbOK)
//                {
//                    functionReturnValue = true;
//                }
//                else
//                {
//                    functionReturnValue = false;
//                }
//            }
//            else
//            {
//                functionReturnValue = true;
//            }
//            return functionReturnValue;
//        }

//        public bool ConfirmWrite(int mode)
//        {
//            bool functionReturnValue = false;
//            string str = null;
//            string msg = null;
//            string msgT = null;
//            int stat = 0;
//            str = this.tboxPrjPath + this.tboxBatchFName;
//            switch (mode)
//            {
//                case 0:
//                    msg = "About to write batch file:" + Constants.vbCr + Constants.vbCr + str;
//                    if (FileExists(str))
//                    {
//                        msg = msg + Constants.vbCr + Constants.vbCr + "<<< An existing file will be overwritten. >>>";
//                        msgT = "Confirm OverWriting the Batch File";
//                    }
//                    else
//                    {
//                        msgT = "Confirm Writing the Batch File";
//                    }
//                    stat = Interaction.MsgBox(msg, Constants.vbQuestion + Constants.vbOKCancel, msgT);
//                    break;
//                case 1:
//                    if (!FileExists(str))
//                    {
//                        msg = "About to write batch file:" + Constants.vbCr + Constants.vbCr + str;
//                        msg = msg + Constants.vbCr + Constants.vbCr + "<<< There is not an existing file to add to. >>>";
//                        msgT = "Not Exactly What You Wanted";
//                    }
//                    else
//                    {
//                        msg = "About to add to batch file:" + Constants.vbCr + Constants.vbCr + str;
//                        msgT = "Confirm Adding to the Batch File";
//                    }
//                    stat = Interaction.MsgBox(msg, Constants.vbQuestion + Constants.vbOKCancel, msgT);
//                    break;
//            }
//            if (stat == Constants.vbOK)
//            {
//                functionReturnValue = true;
//            }
//            else
//            {
//                functionReturnValue = false;
//            }
//            return functionReturnValue;
//        }

//        //'batch file creation
//        //Function WriteBatchFile(mode As Integer) As Integer
//        //    Dim BatchArgs As String
//        //    Dim fname As String
//        //    Dim Lines As Integer
//        //    If Check4MatScn = False Then
//        //        WriteBatchFile = 0
//        //        Exit Function
//        //    End If
//        //    If ConfirmWrite(mode) = False Then
//        //        WriteBatchFile = 0
//        //        Exit Function
//        //    End If
//        //    BatchFile = Me.tboxBatchFName
//        //    fname = fnBatchPath & BatchFile
//        //    PrjDrive = Left(Prjpath, 2)
//        //    On Error GoTo Bad
//        //    Select Case mode
//        //    Case 0
//        //        Open fname For Output As #1
//        //    Case 1
//        //        Open fname For Append As #1
//        //    End Select
//        //    Print #1, "REM - Created by Radscript on " & DateTime.Now
//        //    Print #1, ""
//        //    Lines = Lines + 2
//        //    
//        //    If (chkboxRadInstPath.Value = True) Then
//        //        Print #1, "REM - Setting Radiance variables for this session"
//        //        Print #1, ""
//        //        Print #1, "PATH=" & RadPath & "bin\;%PATH%"
//        //        Print #1, "set RAYPATH=" & RadPath & "lib"
//        //        Print #1, ""
//        //        Lines = Lines + 4
//        //    End If
//        //   
//        //    ' for some reason we find peforming the cd prior to switching the drive
//        //    ' is more reliable
//        //    ' Perform the cd
//        //    'BatchArgs = "cd " & fnBatchPath
//        //    'Print #1, BatchArgs
//        //    ' switch the drive
//        //    'BatchArgs = PrjDrive
//        //    'Print #1, BatchArgs
//        //    
//        //    ' Trying this instead!!!
//        //    BatchArgs = "cd /D " & fnBatchPath
//        //    Print #1, BatchArgs
//        //    Lines = Lines + 1
//        //    
//        //    Print #1, vbCr
//        //    Lines = Lines + 2
//        //    
//        //    Dim mnth As Double
//        //    Dim hr As Double
//        //    'add foreach view
//        //    For mnth = Val(Me.tboxMStart.Value) To Val(Me.tboxMEnd.Value) Step Val(Me.tboxMInc.Value)
//        //        For hr = Val(Me.tboxSHour.Value) To Val(Me.tboxEHour.Value) Step Val(Me.tboxHrInc.Value)
//        //            Print #1, "REM - Month: " & mnth & " | Hour: " & hr
//        //            Lines = Lines + 1
//        //            'gensky
//        //            Print #1, GenSkyCommand(mnth, Me.tboxRunDay.Text, hr)
//        //            Lines = Lines + 1
//        //            'oconv
//        //            Print #1, OConvCommand(mnth, Me.tboxRunDay.Text, hr)
//        //            Lines = Lines + 1
//        //            'mkillum
//        //            Print #1, MkillumCommand(mnth, Me.tboxRunDay.Text, hr)
//        //            Lines = Lines + 1
//        //            'oconv
//        //            Print #1, ReOConvCommand(mnth, Me.tboxRunDay.Text, hr)
//        //            Lines = Lines + 1
//        //            'rpict
//        //            Print #1, RpictCommand(mnth, Me.tboxRunDay.Text, hr)
//        //            Lines = Lines + 1
//        //            'rpict -i
//        //            If IncRpictI Then
//        //                Print #1, RpictICommand(mnth, Me.tboxRunDay.Text, hr)
//        //            Else
//        //                Print #1, "REM - We are excluding rpict -i."
//        //                Print #1, " "
//        //            End If
//        //            Lines = Lines + 1
//        //        Next
//        //    Next
//        //    'Print #1, "pause"
//        //    Close #1
//        //    WriteBatchFile = Lines
//        //    Exit Function
//        //Bad:
//        //    MsgBox ("Error: " & Err.Description & " " & Err.Number)
//        //    On Error GoTo 0
//        //    WriteBatchFile = 0
//        //End Function

//        //Function strMonth(mnth As Variant) As String
//        //    strMonth = format(CStr(mnth), "00")
//        //End Function
//        //Function strDay(runday As Variant) As String
//        //    strDay = format(CStr(runday), "00")
//        //End Function
//        //Function strHr(hr As Variant) As String
//        //    strHr = format(CStr(hr), "00.00")
//        //End Function

//        //GenSkyCommand
//        //returns the full argument gensky command line whe passed month,runday,hr
//        //
//        //Function GenSkyCommand(mnth As Variant, runday As Variant, hr As Variant) As Variant
//        //    Select Case Me.chkNight
//        //    Case False
//        //        SkyPath = Me.tboxSkyPath.Text
//        //        Sky = Me.tboxSkySwitch.Text
//        //        SunFile = SkyPath & Sky & strMonth(mnth) & strDay(runday) & strHr(hr) & "sun.rad"
//        //        GenSkyCommand = "gensky" & Sp & strMonth(mnth) & Sp & strDay(runday) & Sp & strHr(hr) & Sp & Sky
//        //        GenSkyCommand = GenSkyCommand & Sp & ">" & Sp & SunFile
//        //    Case True
//        //        GenSkyCommand = ""
//        //    End Select
//        //End Function

//        //OConvCommand
//        //returns the full argument Oconv command line whe passed month,runday,hr
//        //
//        //Function OConvCommand(mnth As Variant, runday As Variant, hr As Variant) As Variant
//        //    Select Case Me.chkNight
//        //    Case False
//        //        SkyPath = Me.tboxSkyPath.Text
//        //        Sky = Me.tboxSkySwitch.Text
//        //        Mat = Me.lbMatARgStrg.Caption
//        //        ScnBase = Me.lbBScnArg.Caption
//        //        SkyFile = SkyPath & Me.tboxSkyFName
//        //        SunFile = SkyPath & Sky & strMonth(mnth) & strDay(runday) & strHr(hr) & "sun.rad"
//        //        OctBase = OctPath & Sky & strMonth(mnth) & strDay(runday) & strHr(hr) & "base.oct"
//        //        OConvCommand = "oconv" & Sp & Mat & Sp & ScnBase & Sp & SunFile & Sp & SkyFile
//        //        OConvCommand = OConvCommand & Sp & ">" & Sp & OctBase
//        //    Case True
//        //        OConvCommand = ""
//        //    End Select
//        //End Function

//        //MkillumCommand
//        //returns the full argument mkillum command line whe passed month,runday,hr
//        //
//        //Function MkillumCommand(mnth As Variant, runday As Variant, hr As Variant) As Variant
//        //    Select Case Me.chkNight
//        //    Case False
//        //        Dim MkillumFile As Variant
//        //        Dim FullIllumFile As String
//        //        Sky = Me.tboxSkySwitch.Text
//        //        SkyFile = SkyPath & Me.tboxSkyFName
//        //        FullIllumFile = IllumPath & IllumFile
//        //        MkillumFile = IllumPath & Sky & strMonth(mnth) & strDay(runday) & strHr(hr) & "mkillum.rad"
//        //        OctBase = OctPath & Sky & strMonth(mnth) & strDay(runday) & strHr(hr) & "base.oct"
//        //        MkillumCommand = "mkillum" & Sp & MkillumOptsFile & Sp & OctBase & Sp & "<" & Sp & FullIllumFile
//        //        MkillumCommand = MkillumCommand & Sp & ">" & Sp & MkillumFile
//        //    Case True
//        //        MkillumCommand = ""
//        //    End Select
//        //End Function
//        //ReOConvCommand
//        //returns the full argument Oconv 2nd pass command line whe passed month,runday,hr
//        //
//        //Function ReOConvCommand(mnth As Variant, runday As Variant, hr As Variant) As Variant
//        //    Dim MkillumFile As Variant
//        //    Dim TimeCode As String
//        //    SkyPath = Me.tboxSkyPath.Text
//        //    If Not (Me.chkNight.Value) Then
//        //        Sky = Me.tboxSkySwitch.Text
//        //    Else
//        //        Sky = "+nosky"
//        //    End If
//        //    Mat = Me.lbMatARgStrg.Caption
//        //    ScnBase = Me.lbBScnArg.Caption
//        //    MkillumFile = IllumPath & Sky & strMonth(mnth) & strDay(runday) & strHr(hr) & "mkillum.rad"
//        //    SkyFile = SkyPath & Me.tboxSkyFName
//        //    SunFile = SkyPath & Sky & strMonth(mnth) & strDay(runday) & strHr(hr) & "sun.rad"
//        //    If Not (Me.chkNight.Value) Then TimeCode = strMonth(mnth) & strDay(runday) & strHr(hr)
//        //    OctBase = OctPath & Sky & TimeCode & "base.oct"
//        //    ReOConvCommand = "oconv" & Sp & Mat & Sp & ScnBase & Sp
//        //    If Not (Me.chkNight.Value) Then ReOConvCommand = ReOConvCommand & SunFile & Sp & SkyFile & Sp & MkillumFile
//        //    ReOConvCommand = ReOConvCommand & Sp & ">" & Sp & OctBase
//        //End Function

//        //Rpict
//        //returns the full argument rpict command line whe passed month,runday,hr
//        //
//        //Function RpictCommand(mnth As Variant, runday As Variant, hr As Variant) As Variant
//        //    Dim X As Integer
//        //    Dim AmbFile As Variant
//        //    Dim TimeCode As String
//        //    Dim MkillumFile As Variant
//        //    If Not (Me.chkNight.Value) Then TimeCode = strMonth(mnth) & strDay(runday) & strHr(hr)
//        //    OctBase = OctPath & Sky & TimeCode & "base.oct"
//        //    AmbFile = AmbPath & Sky & TimeCode & ".amb"
//        //    With ufScenePicker.lboxSelViews
//        //        For X = 0 To .ListCount - 1
//        //            View = .List(X)
//        //            If Me.chkOvertureImage Then
//        //                RadImage = ImagePath & View & Sky & TimeCode & ".pic"
//        //                RpictCommand = RpictCommand & "rpict -vf" & Sp & ViewPath & View & Sp & "-af" & Sp & AmbFile & Sp & ImgSize2 & Sp & "-t 5"
//        //                RpictCommand = RpictCommand & Sp & "@" & RenderOptsPath & OvertureOptsFile & Sp & "-av 1 1 1" & Sp & OctBase & Sp & ">" & Sp & RadImage
//        //                RpictCommand = RpictCommand & vbCr & vbLf
//        //            End If
//        //            RadImage = ImagePath & View & Sky & TimeCode & ".pic"
//        //            RpictCommand = RpictCommand & "rpict -vf" & Sp & ViewPath & View & Sp & "-af" & Sp & AmbFile & Sp & ImgSize1 & Sp & "-t 5"
//        //            RpictCommand = RpictCommand & Sp & "@" & RenderOptsPath & RenderOptsFile & Sp & "-av 1 1 1" & Sp & OctBase & Sp & ">" & Sp & RadImage
//        //            RpictCommand = RpictCommand & vbCr & vbLf
//        //        Next
//        //    End With
//        //End Function
//        public string ImgSize2()
//        {
//            return "-x " + Convert.ToString(Conversion.Val(this.tboxImage2x.Value)) + " -y " + Convert.ToString(Conversion.Val(this.tboxImage2y.Value));
//        }

//        //Rpict-i
//        //returns the full argument rpict command line whe passed month,runday,hr
//        //
//        //Function RpictICommand(mnth As Variant, runday As Variant, hr As Variant) As Variant
//        //    Dim X As Integer
//        //    Dim AmbFile As Variant
//        //    Dim TimeCode As String
//        //    Dim MkillumFile As Variant
//        //    If Not (Me.chkNight.Value) Then TimeCode = strMonth(mnth) & strDay(runday) & strHr(hr)
//        //    OctBase = OctPath & Sky & TimeCode & "base.oct"
//        //    AmbFile = AmbPath & Sky & TimeCode & ".amb"
//        //    With ufScenePicker.lboxSelViews
//        //        For X = 0 To .ListCount - 1
//        //            View = .List(X)
//        //            IrradImage = IrradPath & View & Sky & TimeCode & "_irrad.pic"
//        //            RpictICommand = RpictICommand & "rpict -i -vf" & Sp & ViewPath & View & Sp & "-af" & Sp & AmbFile & Sp & ImgSize1 & Sp & "-t 5"
//        //            RpictICommand = RpictICommand & Sp & "@" & RenderOptsPath & RenderOptsFile & Sp & "-av 1 1 1" & Sp & OctBase & Sp & ">" & Sp & IrradImage
//        //            RpictICommand = RpictICommand & vbCr & vbLf
//        //        Next
//        //    End With
//        //End Function

//        //Private Sub RemoveFromSelListBox(toLbox As Variant)
//        //    Dim X As Integer
//        //    Dim ListItem As String
//        //    With toLbox
//        //        For X = 0 To .ListCount - 1
//        //            ListItem = .List(X)
//        //            If .Selected(X) = True Then
//        //                toLbox.RemoveItem (X)
//        //                Exit Sub
//        //            End If
//        //        Next
//        //    End With
//        //End Sub

//        //Private Sub cbMatDel_Click()
//        //    RemoveFromSelListBox ufScenePicker.lboxSelMats
//        //End Sub
//        //
//        //Private Sub cbMatUse_Click()
//        //    Move2SelListBox ufScenePicker.lboxMats, ufScenePicker.lboxSelMats
//        //End Sub

//        //Private Sub ApplySceneSSets()
//        //    Dim X As Integer
//        //    Dim i As Integer
//        //    Dim Found As Boolean
//        //    Dim ListItem As String
//        //    Dim SSFname As String
//        //    Dim Line As String
//        //    With ufScenePicker.lboxBScnSS
//        //        For X = 0 To .ListCount - 1
//        //            If .Selected(X) = True Then
//        //                ListItem = .List(X)
//        //                'MsgBox (ListItem)
//        //                ' open file and for each item
//        //                ' find in .lboxBaseScene and make selected
//        //                ' warn if not found in .lboxBaseScene
//        //                SSFname = AsFullPath(BScnPath) & ListItem
//        //                On Error GoTo BugOut
//        //                Open SSFname For Input As #1
//        //                Do While Not EOF(1)    ' Loop until end of file.
//        //                    Input #1, Line
//        //                    'MsgBox (Line)
//        //                    Found = False
//        //                    For i = 0 To ufScenePicker.lboxBaseScene.ListCount - 1
//        //                        If LCase(ufScenePicker.lboxBaseScene.List(i)) = LCase(Line) Then
//        //                            ufScenePicker.lboxBaseScene.Selected(i) = True
//        //                            Found = True
//        //                            Exit For
//        //                        End If
//        //                    Next
//        //                    If Not (Found) Then MsgBox ("The item " & Line & " in set " & ListItem & " is not available.")
//        //                Loop
//        //                Close #1    ' Close file.
//        //            End If
//        //        Next
//        //    End With
//        //    Exit Sub
//        //BugOut:
//        //    Close #1
//        //    MsgBox (Err.Description)
//        //    On Error GoTo 0
//        //End Sub

//        //Private Sub SaveSSS(fromLBox As ListBox, extType As String)
//        //    Dim X As Integer
//        //    Dim cnt As Integer
//        //    Dim ListItem As String
//        //    Dim SSFname As String
//        //    With fromLBox
//        //        cnt = .ListCount
//        //        If cnt = 0 Then Exit Sub
//        //        SSFname = InputBox("Save as what SSS name? ", "Save as " & extType & " Selection Set")
//        //        If SSFname = "" Then Exit Sub
//        //        SSFname = AsFullPath(BScnPath) & SSFname & extType
//        //        If FileExists(SSFname) Then
//        //            If MsgBox("Overwrite the existing " & SSFname, vbOKCancel) = vbCancel Then
//        //                Exit Sub
//        //            End If
//        //        End If
//        //        On Error GoTo skip
//        //        Open SSFname For Output As #1
//        //        For X = .ListCount - 1 To 0 Step -1   ' backwards
//        //            Print #1, fromLBox.List(X)
//        //        Next
//        //    End With
//        //    Close #1
//        //    Exit Sub
//        //skip:
//        //    Close #1
//        //    MsgBox ("We have an error." & Err.Description)
//        //    On Error GoTo 0
//        //End Sub

//        private void cbAppend_Click()
//        {
//            int stat = 0;
//            string str = null;
//            int Lines = 0;
//            str = this.tboxPrjPath + this.tboxBatchFName;
//            Lines = WriteBatchFile(1);
//            if (Lines > 0)
//            {
//                stat = Interaction.MsgBox("Added " + Lines + " command lines to file:" + Constants.vbCr + Constants.vbCr + str, Constants.vbInformation, "Write Status");
//            }
//            else
//            {
//                //MsgBox ("Nothing Added")
//            }
//        }

//        //Private Sub cbBScnClear_Click()
//        //    ClearAllListBox ufScenePicker.lboxBScnSS
//        //    ClearAllListBox ufScenePicker.lboxBaseScene
//        //End Sub

//        //Private Sub cbBSCnDn_Click()
//        //    Dim X As Integer
//        //    Dim item As String
//        //    With ufScenePicker.lboxSelBaseScene
//        //        For X = 0 To .ListCount - 1
//        //            If .Selected(X) And X <> .ListCount - 1 Then
//        //                item = .List(X)
//        //                .AddItem item, X + 2
//        //                .RemoveItem X
//        //                .Selected(X + 1) = True
//        //                Exit For
//        //            End If
//        //        Next
//        //    End With
//        //End Sub

//        //Private Sub cbBScnUp_Click()
//        //    Dim X As Integer
//        //    Dim item As String
//        //    With ufScenePicker.lboxSelBaseScene
//        //        For X = 0 To .ListCount - 1
//        //            If .Selected(X) And X <> 0 Then
//        //                item = .List(X)
//        //                .AddItem item, X - 1
//        //                .RemoveItem X + 1
//        //                .Selected(X - 1) = True
//        //                Exit For
//        //            End If
//        //        Next
//        //    End With
//        //End Sub

//        private void cbClearVarScn_Click()
//        {
//            //ClearAllListBox ufScenePicker.lboxVarScene
//        }

//        private void cbFileStringRepl_Click()
//        {
//            FormL = ufScenePicker.Left;
//            FormT = ufScenePicker.Top;
//            this.Hide();
//            ufEditor.Show();
//            this.Show();
//        }

//        private void cbMatClr_Click()
//        {
//            // ClearAllListBox ufScenePicker.lboxMats
//        }

//        private void cbMatUseAll_Click()
//        {
//            //    UseAllListBox ufScenePicker.lboxMats
//        }

//        private void PickSkyFile()
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxSkyPath;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = strDir + "  Select the SkyFile file you want to use";
//            strFilter = "Sky Files (*.rad)|*.rad";
//            fname = ShowOpen(strDir, strTitle, strFilter);
//            fname = ParseFileName(fname, Prjpath, strDir);
//            if (!string.IsNullOrEmpty(fname))
//            {
//                this.tboxSkyFName = fname;
//                this.tboxSkyPath = strDir;
//            }
//            this.Show();
//        }


//        private void SelIllumFile()
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxIllumPath;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = strDir + "  Select the IllumsFile file you want to use";
//            strFilter = "Illums Files (*.rad)|*.rad";
//            fname = ShowOpen(Prjpath + strDir, strTitle, strFilter);
//            fname = ParseFileName(fname, Prjpath, strDir);
//            if (!string.IsNullOrEmpty(fname))
//            {
//                this.tboxIllumFile = fname;
//                IllumFile = fname;
//                this.tboxIllumPath = strDir;
//            }
//            this.Show();
//        }

//        private void SelPrjPath()
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxPrjPath;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = "Select the project Radiance path.";
//            strFilter = "Anything (*.*)|*.*";
//            fname = EnsureAsPath(BrowseFolder(strTitle, Prjpath));
//            if (!string.IsNullOrEmpty(fname))
//            {
//                Prjpath = fname;
//                this.tboxPrjPath = Prjpath;
//            }
//            this.Show();
//        }
//        private void SelBScnPath()
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxBScnPath.Text;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = Prjpath + "  Select a the folder you want to use for the path.";
//            fname = EnsureAsPath(BrowseFolder(strTitle, Prjpath));
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxBScnPath = strDir;
//            BScnPath = this.tboxBScnPath.Text;
//            FillChoiceList(AsFullPath(BScnPath), ufScenePicker.lboxBaseScene, RadExt);
//            this.Show();
//        }
//        private void SelVScnPath()
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxBScnPath.Text;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = Prjpath + "  Select a folder to be the path you want to use";
//            fname = EnsureAsPath(BrowseFolder(strTitle, Prjpath));
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxVScnPath = strDir;
//            VScnPath = this.tboxVScnPath.Text;
//            FillChoiceList(AsFullPath(VScnPath), ufScenePicker.lboxVarScene, RadExt);
//            this.Show();
//        }

//        private void SelOctPath()
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxOctPath;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = Prjpath + "  Select the Oct folder you want to use for the path.";
//            //strFilter = "Any (*.*)|*.*"
//            fname = EnsureAsPath(BrowseFolder(strTitle, Prjpath));
//            //FName = ShowOpen(Prjpath, strTitle, strFilter)
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxOctPath = strDir;
//            OctPath = strDir;
//            this.Show();
//        }
//        private void SelRendOptsPath()
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxRenderOptPath;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = strDir + "  Select the render options folder you want to use for the path.";
//            strFilter = "Any (*.*)|*.*";
//            fname = EnsureAsPath(BrowseFolder(strTitle, Prjpath));
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxRenderOptPath = strDir;
//            RenderOptsPath = strDir;
//            FillChoiceList(AsFullPath(RenderOptsPath), ufScenePicker.ListBoxOptions, "");
//            this.Show();
//        }
//        private void SelAmbPath()
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxAmbPath;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = strDir + "  Select the folder you want for the Amb path.";
//            strFilter = "Any (*.*)|*.*";
//            fname = EnsureAsPath(BrowseFolder(strTitle, Prjpath));
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxAmbPath = strDir;
//            AmbPath = strDir;
//            this.Show();
//        }
//        private void SelImagePath()
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxImagePath;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = Prjpath + "  Select the Image path you want to use";
//            //strFilter = "Any (*.*)|*.*"
//            fname = EnsureAsPath(BrowseFolder(strTitle, Prjpath));
//            //FName = ShowOpen(Prjpath, strTitle, strFilter)
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxImagePath = strDir;
//            ImagePath = strDir;
//            this.Show();
//        }

//        private void SelViewPath()
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxVpath;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = Prjpath + "  Select the View path you want to use";
//            //strFilter = "Any (*.*)|*.*"
//            fname = EnsureAsPath(BrowseFolder(strTitle, Prjpath));
//            //FName = ShowOpen(Prjpath, strTitle, strFilter)
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxVpath = strDir;
//            ViewPath = strDir;
//            FillChoiceList(AsFullPath(ViewPath), ufScenePicker.lboxViews, "");
//            this.Show();
//        }

//        private void SelIrradPath()
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxIrradPath;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = Prjpath + "  Select the Irrad path you want to use";
//            //strFilter = "Any (*.*)|*.*"
//            fname = EnsureAsPath(BrowseFolder(strTitle, Prjpath));
//            //FName = ShowOpen(Prjpath, strTitle, strFilter)
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxIrradPath = strDir;
//            IrradPath = strDir;
//            this.Show();
//        }

//        private void SelRenderOptFile()
//        {
//            string fname = null;
//            string SubPath = null;
//            string tPath = null;
//            strDir = this.tboxRenderOptFile;
//            tPath = AsFullPath(RenderOptsPath + strDir);
//            if (!(FileExists(tPath)))
//                tPath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = tPath + "  Select the RenderOpt file you want to use";
//            strFilter = "Opt/Txt (*.opt;*.txt)|*.opt;*.txt";
//            //FName = EnsureAsPath(BrowseFolder(strTitle, Prjpath))
//            fname = ShowOpen(tPath, strTitle, strFilter);
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxRenderOptFile = fname;
//            RenderOptsFile = fname;
//            ShowOptionsFile(ufScenePicker.tboxRenderOptFile);
//            this.Show();
//        }
//        private void SelMkillumOptFile()
//        {
//            string fname = null;
//            string SubPath = null;
//            string tPath = null;
//            strDir = this.tboxMkillumOptFile;
//            tPath = AsFullPath(RenderOptsPath + strDir);
//            if (!(FileExists(tPath)))
//                tPath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = tPath + "  Select the MkillumOpt file you want to use";
//            strFilter = "Opt/Txt (*.opt;*.txt)|*.opt;*.txt";
//            //FName = EnsureAsPath(BrowseFolder(strTitle, Prjpath))
//            fname = ShowOpen(tPath, strTitle, strFilter);
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxMkillumOptFile = fname;
//            MkillumOptsFile = fname;
//            ShowOptionsFile(ufScenePicker.tboxMkillumOptFile);
//            this.Show();
//        }
//        private void SelOvertureOptFile()
//        {
//            string fname = null;
//            string SubPath = null;
//            string tPath = null;
//            strDir = this.tboxOverOPtFile;
//            tPath = AsFullPath(RenderOptsPath + strDir);
//            if (!(FileExists(tPath)))
//                tPath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = tPath + "  Select the Overture Options file you want to use";
//            strFilter = "Opt/Txt (*.opt;*.txt)|*.opt;*.txt";
//            //FName = EnsureAsPath(BrowseFolder(strTitle, Prjpath))
//            fname = ShowOpen(tPath, strTitle, strFilter);
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxOverOPtFile = fname;
//            OvertureOptsFile = fname;
//            ShowOptionsFile(ufScenePicker.tboxOverOPtFile);
//            this.Show();
//        }
//        private void SelViewFile()
//        {
//            string fname = null;
//            string SubPath = null;
//            string tPath = null;
//            strDir = this.tboxVpath;
//            tPath = AsFullPath(strDir);
//            if (!(FileExists(tPath)))
//                tPath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = tPath + "  Select the Views file you want to use";
//            strFilter = "Any (*.*)|*.*";
//            //FName = EnsureAsPath(BrowseFolder(strTitle, Prjpath))
//            fname = ShowOpen(tPath, strTitle, strFilter);
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxView = fname;
//            View = fname;
//            lbView_Click();
//            this.Show();
//        }

//        private void cbMkRadPaths_Click()
//        {
//            this.Hide();
//            ufMakePaths.Show();
//            this.Show();
//        }

//        private void cbMkSSS_Click()
//        {
//            SaveSSS(ufScenePicker.lboxSelBaseScene, SSSExt);
//            FillChoiceList(AsFullPath(BScnPath), ufScenePicker.lboxBScnSS, SSSExt);
//        }

//        private void cbPwrDeskMat_Click()
//        {
//            int stat = 0;
//            stat = ShowApp1(AsFullPath(MatPath));
//        }

//        private void cbShellBat_Click()
//        {
//            int stat = 0;
//            string fname = null;
//            // Dim vPID As Variant
//            BatchFile = this.tboxBatchFName;
//            fname = fnBatchPath() + BatchFile;
//            stat = Interaction.MsgBox(fname, Constants.vbQuestion + Constants.vbYesNo, "Shell This BatchFile?");
//            if (stat == Constants.vbYes)
//            {
//                vPID = Interaction.Shell("cmd /k " + "\"" + fname + "\"", Constants.vbNormalFocus);
//            }

//        }

//        private void cbUseAllBaseScn_Click()
//        {
//            //UseAllListBox ufScenePicker.lboxBaseScene
//        }

//        private void cbUseAllVarScn_Click()
//        {
//            //UseAllListBox ufScenePicker.lboxVarScene
//        }

//        private void cbViewsClrAll_Click()
//        {
//            //ClearAllListBox ufScenePicker.lboxViews
//        }

//        private void cbViewUseAll_Click()
//        {
//            //UseAllListBox ufScenePicker.lboxViews
//        }

//        private void cbWriteBatchFile_Click()
//        {
//            int stat = 0;
//            string str = null;
//            int Lines = 0;
//            mpgCompo.Value = 0;
//            str = this.tboxPrjPath + this.tboxBatchFName;
//            Lines = WriteBatchFile(0);
//            if (Lines > 0)
//            {
//                stat = Interaction.MsgBox("Wrote " + Lines + " command lines to file:" + Constants.vbCr + Constants.vbCr + str, Constants.vbInformation, "Write Status");
//            }
//            else
//            {
//                //MsgBox ("Nothing Written")
//            }
//        }

//        private void chkNight_AfterUpdate()
//        {
//            FlopTimeCode();
//            RegenCmdLines();
//        }

//        private void chkOvertureImage_Click()
//        {
//            ufScenePicker.tboxImage2x.Enabled = ufScenePicker.chkOvertureImage;
//            ufScenePicker.tboxImage2y.Enabled = ufScenePicker.chkOvertureImage;
//        }

//        private void CommandButtonBS_Click()
//        {
//            int stat = 0;
//            stat = ShowApp1(AsFullPath(BScnPath));
//        }

//        private void cbPowerDesk2_Click()
//        {
//            int stat = 0;
//            stat = ShowApp1(AsFullPath(VScnPath));
//        }

//        private void cbPowerDesk_Click()
//        {
//            int stat = 0;
//            stat = ShowApp1(Prjpath);
//        }

//        private void frHour_Click()
//        {
//            UpdateHrFrame();
//        }

//        private void frMonth_Click()
//        {
//            UpdateMonthFrame();
//        }

//        private void RegenCmdLines()
//        {
//            UpdateHrFrame();
//            UpdateMonthFrame();
//            this.spinHr.Max = Conversion.Val(this.tboxEHour.Value) / Conversion.Val(this.tboxHrInc.Value);
//            this.spinHr.Min = Conversion.Val(this.tboxSHour.Value) / Conversion.Val(this.tboxHrInc.Value);
//            this.spinMnth.Max = Conversion.Val(this.tboxMEnd.Value) / Conversion.Val(this.tboxMInc.Value);
//            this.spinMnth.Min = Conversion.Val(this.tboxMStart.Value) / Conversion.Val(this.tboxMInc.Value);
//            var _with1 = this;
//            _with1.tboxGenSkyCmd.Text = GenSkyCommand((_with1.spinMnth.Value * _with1.tboxMInc.Value), _with1.tboxRunDay.Text, (_with1.spinHr.Value * _with1.tboxHrInc.Value));
//            _with1.tboxOconvCommand.Text = OConvCommand((_with1.spinMnth.Value * _with1.tboxMInc.Value), _with1.tboxRunDay.Text, (_with1.spinHr.Value * _with1.tboxHrInc.Value));
//            _with1.tboxMkillum.Text = MkillumCommand((_with1.spinMnth.Value * _with1.tboxMInc.Value), _with1.tboxRunDay.Text, (_with1.spinHr.Value * _with1.tboxHrInc.Value));
//            _with1.tboxReOconv.Text = ReOConvCommand((_with1.spinMnth.Value * _with1.tboxMInc.Value), _with1.tboxRunDay.Text, (_with1.spinHr.Value * _with1.tboxHrInc.Value));
//            _with1.tboxrpict.Text = RpictCommand((_with1.spinMnth.Value * _with1.tboxMInc.Value), _with1.tboxRunDay.Text, (_with1.spinHr.Value * _with1.tboxHrInc.Value));
//            if (IncRpictI)
//            {
//                _with1.tboxRpictI.Text = RpictICommand((_with1.spinMnth.Value * _with1.tboxMInc.Value), _with1.tboxRunDay.Text, (_with1.spinHr.Value * _with1.tboxHrInc.Value));
//            }
//            else
//            {
//                _with1.tboxRpictI.Text = "";
//            }
//        }

//        private void lboxBaseScene_Change()
//        {
//            // The list order in lboxSelBaseScene needs to be saved and then restored
//            // after the move2sellistbox routine.
//            //Move2SelListBox ufScenePicker.lboxBaseScene, ufScenePicker.lboxSelBaseScene
//            GenBScnArgString();
//            if (this.lboxSelBaseScene.ListCount > 0)
//            {
//                this.cbMkSSS.Enabled = true;
//            }
//            else
//            {
//                this.cbMkSSS.Enabled = false;
//            }
//        }

//        private void lboxBScnSS_Change()
//        {
//            ApplySceneSSets();
//        }

//        private void lboxSelBaseScene_Change()
//        {
//            GenBScnArgString();
//            if (IsNull(lboxSelBaseScene))
//            {
//                ClearViewScene();
//            }
//        }
//        private void lboxSelBaseScene_AfterUpdate()
//        {
//            int stat = 0;
//            string str = null;
//            string fp = null;
//            fp = BScnPath + lboxSelBaseScene;
//            if (FileExists(AsFullPath(fp)))
//            {
//                str = AsFullPath(fp);
//                stat = ViewFile(ufScenePicker.tboxViewSceneFile, str);
//                if (!IsNull(lboxSelBaseScene))
//                {
//                    FrameSceneFile.Caption = lboxSelBaseScene;
//                }
//                else
//                {
//                    FrameSceneFile.Caption = "";
//                }
//            }
//            else
//            {
//                ClearViewScene();
//            }
//        }

//        private void ClearViewScene()
//        {
//            ufScenePicker.tboxViewSceneFile.Text = "";
//            FrameSceneFile.Caption = "";
//            lboxSelBaseScene = Null;
//        }

//        private void GenBScnArgString()
//        {
//            int X = 0;
//            string strTemp = null;
//            string BScnPath = null;
//            BScnPath = ufScenePicker.tboxBScnPath.Text;
//            var _with2 = ufScenePicker.lboxSelBaseScene;
//            for (X = _with2.ListCount - 1; X >= 0; X += -1)
//            {
//                strTemp = BScnPath + _with2.List(X) + Sp + strTemp;
//            }
//            var _with3 = ufScenePicker.lboxVarSelScene;
//            for (X = _with3.ListCount - 1; X >= 0; X += -1)
//            {
//                strTemp = strTemp + Sp + VScnPath + _with3.List(X);
//            }
//            ufScenePicker.lbBScnArg = strTemp;
//        }
//        private void lboxMats_Change()
//        {
//            // Move2SelListBox ufScenePicker.lboxMats, ufScenePicker.lboxSelMats
//            GenMatArgString();
//            Proc4View();
//        }

//        private void Proc4View()
//        {
//            int stat = 0;
//            string str = null;
//            var _with4 = ufScenePicker.lboxMats;
//            if (AnyInListBoxSelected(ufScenePicker.lboxMats))
//            {
//                str = AsFullPath(MatPath) + _with4.List(_with4.ListIndex);
//                stat = ViewFile(ufScenePicker.tboxMatV, str);
//            }
//            else
//            {
//                ufScenePicker.tboxMatV.Text = "";
//            }
//        }

//        private void GenMatArgString()
//        {
//            int X = 0;
//            string strTemp = null;
//            string MatPath = null;
//            MatPath = ufScenePicker.tboxMatPath.Text;
//            var _with5 = ufScenePicker.lboxSelMats;
//            for (X = _with5.ListCount - 1; X >= 0; X += -1)
//            {
//                strTemp = MatPath + _with5.List(X) + Sp + strTemp;
//            }
//            ufScenePicker.lbMatARgStrg = strTemp;
//        }

//        private void lboxSelViews_Click()
//        {
//            int stat = 0;
//            string str = null;
//            var _with6 = ufScenePicker.lboxSelViews;
//            if (AnyInListBoxSelected(ufScenePicker.lboxSelViews))
//            {
//                str = AsFullPath(ViewPath) + _with6.List(_with6.ListIndex);
//                stat = ViewFile(ufScenePicker.tboxOtherViewer, str);
//            }
//            else
//            {
//                ufScenePicker.tboxOtherViewer = "";
//            }
//        }

//        private void lboxVarScene_Change()
//        {
//            // Move2SelListBox ufScenePicker.lboxVarScene, ufScenePicker.lboxVarSelScene
//            GenBScnArgString();
//        }

//        private void lboxViews_Change()
//        {
//            //Move2SelListBox ufScenePicker.lboxViews, ufScenePicker.lboxSelViews
//            if (lboxViews.Selected(lboxViews.ListIndex))
//            {
//                lboxSelViews = lboxViews.List((lboxViews.ListIndex));
//            }
//        }

//        private void ShowOptionsFile(TextBox thisTbox)
//        {
//            int stat = 0;
//            string str = null;
//            string fp = null;
//            fp = RenderOptsPath + thisTbox;
//            if (FileExists(AsFullPath(fp)))
//            {
//                str = AsFullPath(fp);
//                stat = ViewFile(ufScenePicker.tboxOptFileViewer, str);
//                FrameOptFileView.Caption = thisTbox;
//                ListBoxOptions = thisTbox;
//            }
//            else
//            {
//                ufScenePicker.tboxOptFileViewer.Text = "";
//                FrameOptFileView.Caption = "";
//                ListBoxOptions = Null;
//            }
//        }

//        private void tboxMkillumOptFile_MouseDown(int Button, int Shift, float X, float Y)
//        {
//            ShowOptionsFile(ufScenePicker.tboxMkillumOptFile);
//        }

//        private void lbMkillumOPt_Click()
//        {
//            ShowOptionsFile(ufScenePicker.tboxMkillumOptFile);
//        }

//        private void tboxRenderOptFile_MouseDown(int Button, int Shift, float X, float Y)
//        {
//            ShowOptionsFile(ufScenePicker.tboxRenderOptFile);
//        }
//        private void lbRenderOpt_Click()
//        {
//            ShowOptionsFile(ufScenePicker.tboxRenderOptFile);
//        }

//        private void ListBoxOptions_Change()
//        {
//            int stat = 0;
//            string str = null;
//            string fp = null;
//            fp = RenderOptsPath + ListBoxOptions;
//            if (FileExists(AsFullPath(fp)))
//            {
//                str = AsFullPath(fp);
//                stat = ViewFile(ufScenePicker.tboxOptFileViewer, str);
//                if (!IsNull(ListBoxOptions))
//                {
//                    FrameOptFileView.Caption = ListBoxOptions;
//                }
//                else
//                {
//                    FrameOptFileView.Caption = "";
//                }
//            }
//            else
//            {
//                ufScenePicker.tboxOptFileViewer.Text = "";
//                FrameOptFileView.Caption = "";
//                ListBoxOptions = Null;
//            }
//        }

//        private void lbView_Click()
//        {
//            int stat = 0;
//            string str = null;
//            if (FileExists(AsFullPath(this.tboxVpath.Text + this.tboxView.Text)))
//            {
//                str = AsFullPath(this.tboxVpath.Text + this.tboxView.Text);
//                stat = ViewFile(ufScenePicker.tboxOtherViewer, str);
//            }
//            else
//            {
//                ufScenePicker.tboxOtherViewer.Text = "";
//            }
//        }

//        private void mpgCompo_Change()
//        {
//            RegenCmdLines();
//            Check4Files();
//            FillChoiceList(AsFullPath(BScnPath), ufScenePicker.lboxBScnSS, SSSExt);

//            if ((mpgCompo.Pages.item(mpgCompo.Value).Name == "pgOptions"))
//            {
//                FillChoiceList(AsFullPath(RenderOptsPath), ufScenePicker.ListBoxOptions, "");
//                //    Dim s As Variant
//                foreach (object s_loopVariable in ListBoxOptions.List)
//                {
//                    s = s_loopVariable;
//                    if (s == RenderOptsFile)
//                    {
//                        ListBoxOptions = s;
//                        break; // TODO: might not be correct. Was : Exit For
//                    }
//                }
//            }
//        }

//        private void spinHr_SpinDown()
//        {
//            RegenCmdLines();
//        }

//        private void spinHr_SpinUp()
//        {
//            RegenCmdLines();
//        }

//        private void spinMnth_SpinDown()
//        {
//            RegenCmdLines();
//        }

//        private void spinMnth_SpinUp()
//        {
//            RegenCmdLines();
//        }

//        private void tboxAmbPath_AfterUpdate()
//        {
//            string str = null;
//            str = this.tboxAmbPath.Text;
//            this.tboxAmbPath.Text = EnsureAsPath(str);
//            AmbPath = this.tboxAmbPath.Text;
//        }

//        private void tboxAmbPath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelAmbPath();
//        }

//        private void tboxBatchFName_AfterUpdate()
//        {
//            Check4Files();
//        }

//        private void tboxBScnPath_AfterUpdate()
//        {
//            string str = null;
//            str = this.tboxBScnPath.Text;
//            this.tboxBScnPath.Text = EnsureAsPath(str);
//            BScnPath = this.tboxBScnPath.Text;
//            FillChoiceList(AsFullPath(BScnPath), ufScenePicker.lboxBaseScene, RadExt);
//            FillChoiceList(AsFullPath(BScnPath), ufScenePicker.lboxBScnSS, SSSExt);
//        }

//        private void tboxBScnPath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelBScnPath();
//        }

//        private void tboxEHour_Change()
//        {
//            UpdateHrFrame();
//        }

//        private void tboxHrInc_AfterUpdate()
//        {
//            UpdateHrFrame();
//        }

//        private void UpdateHrFrame()
//        {
//            int stat = 0;
//            int NHrs = 0;
//            if (Conversion.Val(this.tboxHrInc.Text) == 0 | Conversion.Val(this.tboxHrInc.Text) < 0)
//            {
//                stat = Interaction.MsgBox("Resetting to 0.25", Constants.vbExclamation, "Cannot permit zero or negative hours interval value.");
//                this.tboxHrInc.Value = 0.25;
//            }
//            NHrs = 1 + (Conversion.Val(this.tboxEHour.Value) - Conversion.Val(this.tboxSHour.Value)) / Conversion.Val(this.tboxHrInc.Value);
//            this.frHour.Caption = "Hour Of Day" + "      " + NHrs + " Passes/Day";
//        }

//        private void UpdateMonthFrame()
//        {
//            int stat = 0;
//            int NMnths = 0;
//            if (Conversion.Val(this.tboxMInc.Value) == 0 | Conversion.Val(this.tboxMInc.Value) < 0)
//            {
//                stat = Interaction.MsgBox("Resetting to 3", Constants.vbExclamation, "Cannot permit zero or negative month interval value.");
//                this.tboxMInc.Value = 3;
//            }
//            NMnths = 1 + (Conversion.Val(this.tboxMEnd.Value) - Conversion.Val(this.tboxMStart.Value)) / Conversion.Val(this.tboxMInc.Value);
//            this.frMonth.Caption = "Simulation Months" + "      " + NMnths + " Month Passes";
//        }

//        private void tboxHrInc_Change()
//        {
//            if (Conversion.Val(this.tboxHrInc.Text) != 0)
//                RegenCmdLines();
//        }

//        private void tboxIllumFile_AfterUpdate()
//        {
//            RegenCmdLines();
//        }

//        private void tboxIllumFile_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelIllumFile();
//        }

//        private void tboxIllumPath_AfterUpdate()
//        {
//            RegenCmdLines();
//        }

//        private void tboxIllumPath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelIllumFile();
//        }

//        private void tboxImagePath_AfterUpdate()
//        {
//            string str = null;
//            str = this.tboxImagePath.Text;
//            this.tboxImagePath.Text = EnsureAsPath(str);
//            ImagePath = this.tboxImagePath.Text;
//        }

//        private void tboxImagePath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelImagePath();
//        }

//        private void tboxImageX_AfterUpdate()
//        {
//            ImgSize1 = "-x " + Convert.ToString(Conversion.Val(this.tboxImageX.Value)) + " -y " + Convert.ToString(Conversion.Val(this.tboxImageY.Value));
//        }

//        private void tboxImageY_AfterUpdate()
//        {
//            ImgSize1 = "-x " + Convert.ToString(Conversion.Val(this.tboxImageX.Value)) + " -y " + Convert.ToString(Conversion.Val(this.tboxImageY.Value));
//        }

//        private void tboxIrradPath_AfterUpdate()
//        {
//            string str = null;
//            str = this.tboxIrradPath.Text;
//            this.tboxIrradPath.Text = EnsureAsPath(str);
//            IrradPath = this.tboxIrradPath.Text;
//        }

//        private void tboxIrradPath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelIrradPath();
//        }

//        private void tboxMatPath_AfterUpdate()
//        {
//            string str = null;
//            str = this.tboxMatPath.Text;
//            this.tboxMatPath.Text = EnsureAsPath(str);
//            MatPath = this.tboxMatPath.Text;
//            FillChoiceList(AsFullPath(MatPath), ufScenePicker.lboxMats, RadExt);
//        }

//        private void tboxMatPath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            string fname = null;
//            string SubPath = null;
//            strDir = this.tboxMatPath.Text;
//            Prjpath = this.tboxPrjPath;
//            this.Hide();
//            strTitle = Prjpath + "  Select a Mat file at the path you want to use";
//            strFilter = "Any Files (*.*)|*.*";
//            fname = EnsureAsPath(BrowseFolder(strTitle, Prjpath));
//            fname = ParseFileName(fname, Prjpath, strDir);
//            this.tboxMatPath = strDir;
//            MatPath = this.tboxMatPath.Text;
//            FillChoiceList(AsFullPath(MatPath), ufScenePicker.lboxMats, RadExt);
//            this.Show();
//        }

//        private void tboxMatV_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            EditMaterialsFile();
//        }

//        private void tboxMEnd_AfterUpdate()
//        {
//            UpdateMonthFrame();
//        }

//        private void tboxMInc_AfterUpdate()
//        {
//            UpdateMonthFrame();
//        }

//        private void tboxMStart_AfterUpdate()
//        {
//            UpdateMonthFrame();
//        }

//        private void tboxMStart_Change()
//        {
//            RegenCmdLines();
//            UpdateMonthFrame();
//        }

//        private void tboxOctPath_AfterUpdate()
//        {
//            string str = null;
//            str = this.tboxOctPath.Text;
//            this.tboxOctPath.Text = EnsureAsPath(str);
//            OctPath = this.tboxOctPath.Text;
//        }

//        private void tboxOctPath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelOctPath();
//        }

//        private void tboxPrjPath_AfterUpdate()
//        {
//            string str = null;
//            str = this.tboxPrjPath.Text;
//            this.tboxPrjPath.Text = EnsureAsPath(str);
//            Prjpath = this.tboxPrjPath.Text;
//            FillListBoxes();
//        }

//        private void tboxPrjPath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelPrjPath();
//        }

//        private void tboxRenderOptFile_AfterUpdate()
//        {
//            RenderOptsFile = this.tboxRenderOptFile.Text;
//            Check4Files();
//        }

//        private void Check4Files()
//        {
//            // check for renderopt file
//            if (FileExists(AsFullPath(RenderOptsPath + this.tboxRenderOptFile.Text)))
//            {
//                this.lbRenderOpt.ForeColor = 0x80000008;
//            }
//            else
//            {
//                this.lbRenderOpt.ForeColor = 0xffL;
//            }
//            // check for mkillumopt file
//            if (FileExists(AsFullPath(RenderOptsPath + this.tboxMkillumOptFile.Text)))
//            {
//                this.lbMkillumOPt.ForeColor = 0x80000008;
//            }
//            else
//            {
//                this.lbMkillumOPt.ForeColor = 0xffL;
//            }
//            // check for overture file
//            if (FileExists(AsFullPath(RenderOptsPath + this.tboxOverOPtFile.Text)))
//            {
//                this.lbOvertureOPt.ForeColor = 0x80000008;
//            }
//            else
//            {
//                this.lbOvertureOPt.ForeColor = 0xffL;
//            }
//            // check for view file
//            if (FileExists(AsFullPath(this.tboxVpath + this.tboxView)))
//            {
//                this.lbView.ForeColor = 0x80000008;
//            }
//            else
//            {
//                this.lbView.ForeColor = 0xffL;
//            }
//            // check for existing bat file
//            if (FileExists(AsFullPath(this.tboxBatchFName.Text)))
//            {
//                this.cbAppend.Enabled = true;
//            }
//            else
//            {
//                this.cbAppend.Enabled = false;
//            }

//        }


//        private void tboxRenderOptFile_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelRenderOptFile();
//        }

//        private void tboxRunDay_Change()
//        {
//            RegenCmdLines();
//        }

//        private void tboxSHour_AfterUpdate()
//        {
//            RegenCmdLines();
//            UpdateHrFrame();
//        }

//        private void tboxSkyFName_Change()
//        {
//            RegenCmdLines();
//        }

//        private void tboxSkyFName_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            PickSkyFile();
//        }

//        private void tboxSkyPath_Change()
//        {
//            RegenCmdLines();
//        }

//        private void tboxSkyPath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            PickSkyFile();
//        }

//        private void tboxSkySwitch_Change()
//        {
//            RegenCmdLines();
//        }

//        private void tboxView_AfterUpdate()
//        {
//            View = this.tboxView.Text;
//            Check4Files();
//            SetViewAsPicked();
//            this.tboxView.SetFocus();
//        }

//        private void tboxView_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelViewFile();
//        }
//        private void SetViewAsPicked()
//        {
//            int X = 0;
//            // refresh list (the point is to clear selections)
//            FillChoiceList(AsFullPath(ViewPath), ufScenePicker.lboxViews, "");
//            var _with7 = ufScenePicker.lboxViews;
//            for (X = 0; X <= _with7.ListCount - 1; X++)
//            {
//                if (_with7.List(X) == ufScenePicker.tboxView.Text)
//                {
//                    _with7.Selected(X) = true;
//                    break; // TODO: might not be correct. Was : Exit For
//                }
//            }
//        }

//        private void tboxVpath_AfterUpdate()
//        {
//            string str = null;
//            str = this.tboxVpath.Text;
//            this.tboxVpath.Text = EnsureAsPath(str);
//            ViewPath = this.tboxVpath.Text;
//            FillChoiceList(AsFullPath(ViewPath), ufScenePicker.lboxViews, "");
//        }

//        private void tboxVpath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelViewPath();
//        }

//        private void tboxVScnPath_AfterUpdate()
//        {
//            string str = null;
//            str = this.tboxVScnPath.Text;
//            this.tboxVScnPath.Text = EnsureAsPath(str);
//            VScnPath = this.tboxVScnPath.Text;
//            FillChoiceList(AsFullPath(VScnPath), ufScenePicker.lboxVarScene, RadExt);
//        }

//        private void tboxVScnPath_DblClick(MSForms.ReturnBoolean Cancel)
//        {
//            SelVScnPath();
//        }

//        private void FlopTimeCode()
//        {
//            switch (ufScenePicker.chkNight)
//            {
//                case true:
//                    SHour = ufScenePicker.tboxSHour;
//                    EHour = ufScenePicker.tboxEHour;
//                    HrInc = ufScenePicker.tboxHrInc;
//                    MStart = ufScenePicker.tboxMStart;
//                    MEnd = ufScenePicker.tboxMEnd;
//                    MInc = ufScenePicker.tboxMInc;
//                    ufScenePicker.tboxSHour = 1;
//                    ufScenePicker.tboxEHour = 1;
//                    ufScenePicker.tboxHrInc = 1;
//                    ufScenePicker.tboxMStart = 1;
//                    ufScenePicker.tboxMEnd = 1;
//                    ufScenePicker.tboxMInc = 1;
//                    break;
//                case false:
//                    ufScenePicker.tboxSHour = SHour;
//                    ufScenePicker.tboxEHour = EHour;
//                    ufScenePicker.tboxHrInc = HrInc;
//                    ufScenePicker.tboxMStart = MStart;
//                    ufScenePicker.tboxMEnd = MEnd;
//                    ufScenePicker.tboxMInc = MInc;
//                    break;
//            }
//        }

//        private void FillListBoxes()
//        {
//            FillChoiceList(AsFullPath(MatPath), ufScenePicker.lboxMats, RadExt);
//            FillChoiceList(AsFullPath(BScnPath), ufScenePicker.lboxBaseScene, RadExt);
//            FillChoiceList(AsFullPath(BScnPath), ufScenePicker.lboxBScnSS, SSSExt);
//            FillChoiceList(AsFullPath(VScnPath), ufScenePicker.lboxVarScene, RadExt);
//            FillChoiceList(AsFullPath(ViewPath), ufScenePicker.lboxViews, "");
//        }

//        private void cbQuit_Click()
//        {
//            // Unload Me
//        }

//        //Sub GetPrefs()
//        //    Dim MacroPref As String
//        //    Dim X As Integer
//        //    Dim Line As String
//        //    Dim stat As Integer
//        //    Dim TL As Integer
//        //    Dim TT As Integer
//        //    On Error GoTo skip
//        //    MacroPref = PrefPath & PrefData
//        //  '  Open MacroPref For Input As #1
//        //    Do While Not EOF(1)    ' Loop until end of file.
//        //        Input #1, Line
//        //        Select Case Left(Line, 3)
//        //        Case "pp:"
//        //            Prjpath = Right(Line, Len(Line) - 3)
//        //        Case "vs:"
//        //            VScnPath = Right(Line, Len(Line) - 3)
//        //        Case "bs:"
//        //            BScnPath = Right(Line, Len(Line) - 3)
//        //        Case "mp:"
//        //            MatPath = Right(Line, Len(Line) - 3)
//        //        Case "vw:"
//        //            View = Right(Line, Len(Line) - 3)
//        //        Case "ir:"
//        //            IrradPath = Right(Line, Len(Line) - 3)
//        //        Case "im:"
//        //            ImagePath = Right(Line, Len(Line) - 3)
//        //        Case "ap:"
//        //            AmbPath = Right(Line, Len(Line) - 3)
//        //        Case "oc:"
//        //            OctPath = Right(Line, Len(Line) - 3)
//        //        Case "ip:"
//        //            IllumPath = Right(Line, Len(Line) - 3)
//        //        Case "if:"
//        //            IllumFile = Right(Line, Len(Line) - 3)
//        //        Case "sh:"
//        //            SHour = Right(Line, Len(Line) - 3)
//        //        Case "eh:"
//        //            EHour = Right(Line, Len(Line) - 3)
//        //        Case "hi:"
//        //            HrInc = Right(Line, Len(Line) - 3)
//        //        Case "ms:"
//        //            MStart = Right(Line, Len(Line) - 3)
//        //        Case "me:"
//        //            MEnd = Right(Line, Len(Line) - 3)
//        //        Case "mi:"
//        //            MInc = Right(Line, Len(Line) - 3)
//        //        Case "tl:"
//        //            TL = Right(Line, Len(Line) - 3)
//        //        Case "tt:"
//        //            TT = Right(Line, Len(Line) - 3)
//        //        Case "ri:"
//        //            RadPath = Right(Line, Len(Line) - 3)
//        //        Case "ov:"
//        //            OvertureOptsFile = Right(Line, Len(Line) - 3)
//        //        Case "ro:"
//        //            RenderOptsFile = Right(Line, Len(Line) - 3)
//        //        Case "mk:"
//        //            MkillumOptsFile = Right(Line, Len(Line) - 3)
//        //        Case "pi:"
//        //            IncRpictI = Right(Line, Len(Line) - 3)
//        //        Case "bf:"
//        //            BatchFile = Right(Line, Len(Line) - 3)
//        //        End Select
//        //    Loop
//        //    'MsgBox (TT & "   " & TL)
//        //    If (TL <> 0 And TT <> 0) Then
//        //        Me.StartUpPosition = 0
//        //        Me.Left = TL
//        //        Me.Top = TT
//        //    Else
//        //        Me.StartUpPosition = 1
//        //    End If
//        //    Close #1    ' Close file.
//        //    Exit Sub
//        //skip:
//        //    On Error GoTo 0
//        //End Sub
//        //Private Sub SavePrefs()
//        //    Dim MacroPref As String
//        //    Dim X As Integer
//        //    On Error GoTo skip
//        //    'MacroPref = Preferences.Files.TempFilePath & PrefData
//        //    MacroPref = PrefPath & PrefData
//        //    Close #1
//        //    Open MacroPref For Output As #1
//        //    Print #1, "This is the Radscript.dvb preferences file. "
//        //    On Error GoTo skip
//        //    Print #1, "pp:"; Prjpath
//        //    Print #1, "vs:"; VScnPath
//        //    Print #1, "bs:"; BScnPath
//        //    Print #1, "mp:"; MatPath
//        //    Print #1, "rp:"; RenderOptsPath
//        //    Print #1, "ro:"; RenderOptsFile
//        //    Print #1, "mk:"; MkillumOptsFile
//        //    Print #1, "ov:"; OvertureOptsFile
//        //    Print #1, "vw:"; View
//        //    Print #1, "ir:"; IrradPath
//        //    Print #1, "im:"; ImagePath
//        //    Print #1, "ap:"; AmbPath
//        //    Print #1, "ri:"; RadPath
//        //    ' Print #1, "iz:"; imgsize
//        //    Print #1, "ip:"; IllumPath
//        //    Print #1, "if:"; IllumFile
//        //    Print #1, "op:"; OctPath
//        //    Print #1, "sh:"; ufScenePicker.tboxSHour
//        //    Print #1, "eh:"; ufScenePicker.tboxEHour
//        //    Print #1, "hi:"; ufScenePicker.tboxHrInc
//        //    Print #1, "ms:"; ufScenePicker.tboxMStart
//        //    Print #1, "me:"; ufScenePicker.tboxMEnd
//        //    Print #1, "mi:"; ufScenePicker.tboxMInc
//        //    Print #1, "tl:"; ufScenePicker.Left
//        //    Print #1, "tt:"; ufScenePicker.Top
//        //    Print #1, "pi:"; IncRpictI
//        //    Print #1, "bf:"; ufScenePicker.tboxBatchFName
//        //skip:
//        //    Close #1
//        //    On Error GoTo 0
//        //End Sub

//        public void EditMaterialsFile()
//        {
//            int stat = 0;
//            string str = null;
//            if (AnyInListBoxSelected(ufScenePicker.lboxMats))
//            {
//                str = AsFullPath(MatPath) + ufScenePicker.lboxMats.List(ufScenePicker.lboxMats.ListIndex);
//                EditAnyFile(str);
//            }
//        }

//        public void EditViewFile()
//        {
//            int stat = 0;
//            string str = null;
//            if (AnyInListBoxSelected(ufScenePicker.lboxSelViews))
//            {
//                str = AsFullPath(ViewPath) + ufScenePicker.lboxSelViews.List(ufScenePicker.lboxSelViews.ListIndex);
//                EditAnyFile(str);
//            }
//        }

//        public void EditSceneFile()
//        {
//            int stat = 0;
//            string str = null;
//            if (AnyInListBoxSelected(ufScenePicker.lboxSelBaseScene))
//            {
//                str = AsFullPath(BScnPath) + ufScenePicker.lboxSelBaseScene.List(ufScenePicker.lboxSelBaseScene.ListIndex);
//                EditAnyFile(str);
//            }
//        }
//        public void EditOptionsFile()
//        {
//            int stat = 0;
//            string str = null;
//            if (AnyInListBoxSelected(ufScenePicker.ListBoxOptions))
//            {
//                str = AsFullPath(RenderOptsPath) + ufScenePicker.ListBoxOptions.List(ufScenePicker.ListBoxOptions.ListIndex);
//                EditAnyFile(str);
//            }
//        }

//        private void UserForm_QueryClose(int Cancel, int CloseMode)
//        {
//            if (ufScenePicker.chkNight)
//            {
//                ufScenePicker.chkNight = false;
//                FlopTimeCode();
//                // done to save older setting
//            }
//            SavePrefs();
//        }
//    }
//}
