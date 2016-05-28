using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace WaterMarker
{
    public partial class Form1 : Form
    {
        // Steps to go
        // -----------
        // 1 - Search for old report.
        // 2 - Convert .doc to odt.
        // 3 - Decompress report.
        // 4 - Add values and content.
        // 5 - Compress report on odt file.
        // 6 - Print report.
        //
        string globalReportPath;

        public Form1()
        {
            InitializeComponent();
        }

        private void SearchReport(string UNC, int AccessionNumber)
        {
            string reportPath = izzilib.CDM.Ambulatorio.AccessionNumberToPath(UNC, AccessionNumber, null);
            if (File.Exists(reportPath))
            {
                cbFound.Checked = true;
                globalReportPath = reportPath;
            }
            else
            {
                cbFound.Checked = false;
                globalReportPath = "";
            }
        }

        private void InsertWaterMarkToDocFile(FileInfo file)
        {
            // 2 - Convert .doc to odt.
            // soffice --headless --convert-to odt --outdir documents/ *.doc <- Useless!!! Generates an XML Only file.
            // soffice --headless --convert-to writer8 --outdir documents/ *.doc <- Use this one, take care about fucking .writer8 extension...
            Console.WriteLine("Calling LibreOffice...");
            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            izzilib.Interact.LaunchInShell(Properties.Settings.Default.UNC, @"C:\Program Files (x86)\LibreOffice 4\program\soffice.exe", "--headless --convert-to writer8 --outdir \"" + documentsFolder + "\" " + file.FullName);
            Console.WriteLine("File: " + file.FullName + " converted.");
            Console.WriteLine("Saved on: " + documentsFolder);

            FileInfo writer8File = new FileInfo(Path.Combine(documentsFolder, file.Name.Split('.')[0] + ".writer8"));
            Console.WriteLine(writer8File.FullName);

            // 3 - Decompress report.
            // We have to wait cause if it begins before file has copied, it throws an ugly error.
            while (!File.Exists(writer8File.FullName)) { Thread.Sleep(200); };
            UnZipOnFolder(writer8File);
            
            // 4 - Add values and content.


            // 5 - Compress report on odt file.


        }

        public void ReplaceValuesOnFile(string file)
        {
            ReplaceValuesOnFile(file, file);
        }

        public void ReplaceValuesOnFile(string fileIn, string fileOut)
        {
            StringBuilder fileContent = new StringBuilder();

            using (StreamReader sr = new StreamReader(fileIn))
            {
                fileContent.Append(sr.ReadToEnd());
            }
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WARN] Error trying to replace values on file: {0}", ex.ToString());
            }


            using (StreamWriter sw = new StreamWriter(fileOut))
            {
                sw.Write(fileContent);
            }
        }

        public void ReplaceValuesOnFiles(FileInfo[] files)
        {
            for (int i = 0; i < files.Length; i++)
            {
                ReplaceValuesOnFile(files[i].Name);
            }
        }


        // ***
        // This method should be moved to izzilib
        // ***
        public static void UnZipOnFolder(FileInfo inFile)
        {
            string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string zipCommand = "e \"{0}\" -o\"{1}\" * -r";
            string zipCompressorFolder = "7-Zip";
            string zipCompressorFileName = "7z.exe";
            string zipError = "Unable to compress file: {0}";

            string zipCompressor = Path.Combine(appPath, zipCompressorFolder);
            string arguments = string.Format(zipCommand, inFile.FullName, inFile.Directory + "\\" + inFile.Name.Split('.')[0] + "\\");

            Console.WriteLine(arguments);

            try
            {
                System.Diagnostics.Process zipCompressorProcess = izzilib.Interact.LaunchInShell(zipCompressor, zipCompressorFileName, arguments);

                while (!zipCompressorProcess.HasExited) { }
                if (zipCompressorProcess.ExitCode == 0)
                {
                    // Directory.Delete(inFolder);
                }
                else
                {
                    throw new Exception(string.Format(zipError, inFile.Name));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // ***
        // This method should be moved to izzilib
        // ***
        public bool BuildODT(DirectoryInfo tempDirectory, FileInfo outFile, bool deleteSourceFiles)
        {
            Utilities.Files fileUtil = new Utilities.Files();

            if (!fileUtil.IsFileLocked(outFile.FullName))
            {
                try
                {
                    if (outFile.Exists)
                    {
                        Console.WriteLine("[INFO] Trying to backup old report.");
                        outFile.CopyTo(outFile.FullName + ".bak", true);
                        outFile.Delete();
                    }
                }
                catch (IOException copyError)
                {
                    Console.WriteLine("[ERROR] Could not make backup file. Aborting.\nError:{0}", copyError.Message);
                    return false;
                }

                try
                {
                    Utilities.Zippit.MakeZip(tempDirectory.FullName, outFile.FullName);
                    Console.WriteLine("[INFO] Created report: {0}", outFile.FullName);
                }
                catch (Exception ex)
                {
                    // Win32Exception - An error occurred when opening the associated file. 
                    // ObjectDisposedException - The process object has already been disposed. 
                    // FileNotFoundException - The PATH environment variable has a string containing quotes.

                    Console.WriteLine("[ERROR] ODT could not be created. Aborting.\nError:{0}", ex.Message);
                }

                if (deleteSourceFiles)
                {
                    try
                    {
                        Directory.Delete(tempDirectory.FullName, true);
                        Console.WriteLine("[INFO] Temporal files deleted.");
                    }
                    catch (IOException deleteError)
                    {
                        Console.WriteLine("[WARN] Could not delete temporal folder.\nError:{0}", deleteError.Message);
                        return true;
                    }
                }

                return true;
            }
            else
            {
                Console.WriteLine("[WARN] Destination file '{0}' locked by another application", outFile.Name);
                Console.WriteLine();
                return false;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            int AccessionNumber;
            if (lblAccessionNumber.Text.Length == 8)
            {
                lblAccessionNumber.BackColor = Color.LightGreen;
                if (int.TryParse(lblAccessionNumber.Text, out AccessionNumber))
                {
                    SearchReport(Properties.Settings.Default.UNC, AccessionNumber);
                }
            }
            else
            {
                lblAccessionNumber.BackColor = Color.Pink;
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            Console.WriteLine(globalReportPath);
            InsertWaterMarkToDocFile(new FileInfo(globalReportPath));
        }

        private void lblAccessionNumber_TextChanged(object sender, EventArgs e)
        {
            switch (lblAccessionNumber.Text.Length)
            {
                case 0:
                    lblAccessionNumber.BackColor = Color.White;
                    break;
                case 8:
                    lblAccessionNumber.BackColor = Color.LightGreen;
                    break;
                default:
                    lblAccessionNumber.BackColor = Color.Pink;
                    break;
            }
        }
    }
}
