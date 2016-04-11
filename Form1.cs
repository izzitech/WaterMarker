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
            UnZipOnFolder(writer8File);
            
            // 3 - Decompress report.


            // 4 - Add values and content.

            
            // 5 - Compress report on odt file.


        }

        // ***
        // This method should be moved to izzilib
        // ***

        public static void UnZipOnFolder(FileInfo inFile)
        {
            string appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string zipCommand = "e \"{0}\" o\"{1}\" -r";
            string zipCompressorFolder = "7-Zip";
            string zipCompressorFileName = "7z.exe";
            string zipError = "Unable to compress file: {0}";

            string zipCompressor = Path.Combine(appPath, zipCompressorFolder);
            string arguments = string.Format(zipCommand, inFile.FullName, inFile.Directory + inFile.Name.Split('.')[0]);

            Console.WriteLine(arguments);

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
