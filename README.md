# WpfRadScript&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  ![](WpfRadScript/Site.ico)
A WPF front end for various common workflow tasks associated with using the **Radiance Synthetic Imaging System** ([http://radsite.lbl.gov/radiance/][a4d8a80e]) ([https://www.radiance-online.org/][d13f1279]). The **Radiance** system is what is used to make accurate lighting level predictions.

  [a4d8a80e]: http://radsite.lbl.gov/radiance/ "http://radsite.lbl.gov/radiance/"
  [d13f1279]: https://www.radiance-online.org/ "https://www.radiance-online.org/"

  This is a complex application for a very complex suite of **Radiance** utilities. No attempt here is made to explain this WpfRadScript program. Those familiar with running a **Radiance** system at a nuts and bolts level would recognize what is going on and they might find WpfRadScript useful for what is does.
  
  WpfRadScript was created for the lighting designer Mark de la Fuente. The images shown in this documentation are from one of his projects.

  ### Requirements
  - Radiance installed ([http://radsite.lbl.gov/radiance/][a4d8a80e]) and WpfRadScript told where to find the **Radiance** binaries.
  - The various project definition files needed to setup the lighting simulation.
  - ImageMagick and GhostScript installed. These are used for the final image creation steps in the workflow this application automates.

### User Interface Notes

- Wherever there is a text input field for a filename or path, then a mouse doubleclick in that field will open a filename or filepath browser.
- Wherever a file's contents are displayed, then a mouse doubleclick in the content will open that file in a simple text editor.
- Wherever a text input field is not correct for some reason, then that text field is shown in red color. This concept applies also to some of the argument text fields where an error in the argument would result in the command failing.
- Much of what **WpfRadScript** does is dispatch work to utility programs on backgroundworker threads. When this happens **WpfRadScript** tries to report the backgroundworker progress in the upper left corner message viewer.
- Bold text like **?gensky** opens an online manual for that command when mouse doubleclicked.  

---
#### General Tab
In this workflow Scenes, Materials, Views tab and Options are choreographed in a batch file to generate special simulated HDR images. The **General** tab is the main control point.

The various **Radiance** command lines that will be written to the batch file are shown.

![General](WpfRadScript/Images/General.PNG)

---
#### Scenes Tab
The **Scenes** tab is where the scenes argument is built. The arguments shown are transferred to the **General** tab.

![Scenes](WpfRadScript/Images/Scenes.PNG)

---
#### Materials Tab
The **Materials** tab is where the materials argument is built. The arguments shown are transferred to the **General** tab.

![Materials](WpfRadScript/Images/Materials.PNG)

---
#### Views Setting Tab
The **View Setting** tab is where the views are selected. The selections show up in the **General** tab command lines.

![View Setting](WpfRadScript/Images/ViewsSetting.PNG)

---
#### Options Tab
The **Options** tab is where the options files are selected.

![Options](WpfRadScript/Images/Options.PNG)

---
#### PComb Tab
The **Radiance** results for running the batch file are **Radiance** PIC image files. These are components to the final result. The **Radiance PComb** function combines **Radiance** PIC image files into another **Radiance** PIC file using parameters for each component that controls how it is combined into the final result. The **PComb** tab is where this happens.

The interface in this tab is a bit abstract. It needs some explanation. The **PComb** function combines **Radiance** PIC image files. Those files are listed on the left. There are regex controls on the list selection because there could be many obscure filenames to select.

The **PComb** function arguments are selected on the right side. A field shows the **PComb** function command line with its proposed arguments as it is assembled. The "\ /" button loads the **PComb** function command line into the execution chamber below. The **gears** button ![gears](WpfRadScript/3-gears-hi-s.png) on this tab dispatches the **PComb** function command lines.

![PComb](WpfRadScript/Images/PComb.PNG)

---
#### Pic to Tiff Tab
The final **Radiance** PIC image files, which are special files that have significant technical data use within **Radiance**, must be converted to standard image formats when intended for presentation viewing. The first conversion is from PIC to Tiff using the **Radiance ra_tiff** utility. The **Pic to Tiff** tab is where this happens.

Use the **Observe ra_tiff Process** checkbox to see what **ra_tiff** has to report about processes that fail.

![PicToTiff](WpfRadScript/Images/PicToTiff.PNG)

---
#### Tiff to Other Tab
Once the **Radiance** PIC images are converted to TIFF files they can then be converted to more useful image formats. The **Tiff to Other** tab is where this happens. At this step **ImageMagick** makes the conversion and can be used in its full capability via command arguments.

Use the **Observe Process** checkbox to see what **ImageMagick** has to report about processes that fail.

![TiffToOther](WpfRadScript/Images/TiffToOther.PNG)

---
#### Utilities Tab
The **Utilities** tab is where some **WpfRadScript** application settings are made and where standard Radiance related project folders can be created.

The HDRViewer setting is where the **Radiance PIC** file viewer is defined. The year 2000 hdrview.exe program provided in this repository in the HDRViewer folder can view a **Radiance PIC** file. Hdrview.exe may have been written by Haibin Huang while a student at University of California Berkeley. **WpfRadScript** passes the **Radiance PIC** file resulting from **PComb** to **Hdrview.exe** for viewing.

Two items on this tab are features not implemented. These are the Replace String Utility and the Editor Path setting.

![](WpfRadScript/Images/Utilities.PNG)
