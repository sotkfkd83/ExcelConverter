using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExcelConvert
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        ConfigData m_config;
        ExcelToJsonMaker converter;

        private void Form1_Load(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize(string path = @".\config.conf")
        {
            WriteLog("Initialize");

            var loader = new ConfigLoader(path);
            m_config = loader.Config;

            if (string.IsNullOrWhiteSpace(m_config.SourcePath))
            {
                WriteLog($"{path} Config Problem");
                return;
            }

            txt_excel_path.Text = m_config.SourcePath;

            WriteLog($"{m_config.SourceExtentions} Source Extentions");
        }

        private void btn_search_Click(object sender, EventArgs e)
        {
            ShowFolderDialog();
        }

        private void ShowFolderDialog(string rootFolder = "")
        {
            if (rootFolder.Length <= 0)
            {
                rootFolder = Environment.CurrentDirectory;
            }

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = true;
            dialog.SelectedPath = rootFolder;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txt_excel_path.Text = dialog.SelectedPath;
            }
        }

        private void btn_convert_Click(object sender, EventArgs e)
        {
            var srcPath =  txt_excel_path.Text;
            if (string.IsNullOrWhiteSpace(srcPath))
            {
                return;
            }

            converter = new ExcelToJsonMaker(srcPath, m_config.SourceExtentions, delegate (string msg)
            {
                WriteLog(msg);
            });

            converter.Convert();
        }

        private void WriteLog(string msg)
        {
            txt_log.AppendText($"{DateTime.Now} {msg}{Environment.NewLine}");

            if (txt_log.Lines.Length > 100)
            {
                
            }
        }
    }
}
