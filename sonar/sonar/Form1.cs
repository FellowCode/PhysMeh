using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;

using OfficeOpenXml;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace sonar
{
    public partial class mainForm : Form
    {
        static bool isStart = false;
        static string[] ports = SerialPort.GetPortNames();
        static float max1=-10000, max2=-10000, min=10000;
        bool speedSeted = false;
        int numSonars;
        long lastX=0;

        double keyLength, keyWidth, h;

        double maxForceValue = 0;

        short counter = 0;


        public Graph[] graphs;
        
        public mainForm()
        {
            InitializeComponent();
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            portNum.Items.AddRange(ports);

            chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;

            //chart1.ChartAreas[0].AxisY.ScaleView.Zoom(-1, 1);
            chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
            chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
            chart1.ChartAreas[0].AxisY.ScrollBar.IsPositionedInside = true;


            loadSettings();


            //AllocConsole();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private void startBtn_Click(object sender, EventArgs e)
        {
            isStart = !isStart;
            if (isStart)
            {
                lastX = 0;
                bool success = false;

                try
                {
                    if (!Program._continue)
                        Program.StartSerial(ports[portNum.SelectedIndex]);
                    Program.startMeasure();
                    success = true;
                }
                catch {
                    MessageBox.Show("Не удалось открыть порт", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                } finally {
                    if (success)
                    {
                        saveBtn.Enabled = false;
                        startBtn.Text = "Stop";
                        numSonars = Int32.Parse(numSonar.Text);

                        graphs = new Graph[numSonars];

                        var type = chart1.Series[0].ChartType;
                        chart1.Series.Clear();
                        for (int i = 0; i < numSonars; i++)
                        {
                            chart1.Series.Add("Sonar " + (i + 1).ToString());
                            chart1.Series[i].ChartType = type;
                        }
                        speedSeted = false;
                        timer1.Enabled = true;
                        
                        
                    }
                }

                try
                {
                    h = Int32.Parse(height.Text) / 1000f;
                    keyLength = Int32.Parse(keyLengthBox.Text) / 1000f;
                    keyWidth = Int32.Parse(keyWidthBox.Text) / 1000f;
                } catch
                {
                    MessageBox.Show("Не корректные данные клавиши", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                saveBtn.Enabled = true;

                //Program.StopSerial();
                Program.WriteToSerial("-");
                startBtn.Text = "Start";


                timer1.Enabled = false;
            }
        }

        public void stop()
        {
            isStart = false;
            startBtn.Invoke((MethodInvoker)(()=> startBtn.Text = "Start"));
            timer1.Enabled = false;
            saveBtn.Enabled = true;
        }

        public void setSonar(string data)
        {
            try
            {
                while (data.Length > 0)
                {
                    string str_index = data.Substring(0, data.IndexOf(":"));
                    int index = Int32.Parse(str_index);
                    data = data.Remove(0, str_index.Length + 1);

                    string str_x = data.Substring(0, data.IndexOf(":"));
                    long x = Int64.Parse(str_x);
                    data = data.Remove(0, str_x.Length + 1);

                    string str_y = data.Substring(0, data.IndexOf(";"));
                    float y = Int32.Parse(str_y) / 10f;
                    data = data.Remove(0, str_y.Length + 1);

                    if (index < numSonars)
                    {
                        counter++;
                        if (counter > 10)
                            counter = 0;
                        else
                        {
                            if (lastX > x && lastX != 0)
                                chart1.Invoke((MethodInvoker)(() => chart1.Series[index].Points.RemoveAt(chart1.Series[index].Points.Count - 1)));
                            chart1.Invoke((MethodInvoker)(() => chart1.Series[index].Points.AddXY(x, y)));
                            if (chart1.Series[index].Points.Count > 100)
                                chart1.Invoke((MethodInvoker)(() => chart1.Series[index].Points.RemoveAt(0)));

                            lastX = x;
                        }

                        if (graphs[index] == null)
                            graphs[index] = new Graph();

                        graphs[index].AddPoint(x, y);
                        //label1.Invoke((MethodInvoker)(() => label1.Text = graphs[index].Count().ToString()));
                    }
                }
            }
            catch { }

        }

        public void addChartData(String data)
        {
            String[] values = data.Split(';');
          
            int time = Int32.Parse(values[0].Split('=')[1]);
            
            double force = Int32.Parse(values[1].Split('=')[1])/100f;

            if (force > maxForceValue)
                maxForceValue = force;

            chart1.Invoke((MethodInvoker)(() => chart1.Series[0].Points.AddXY(time, force)));

            if (graphs[0] == null)
                graphs[0] = new Graph();

            graphs[0].AddPoint(time, force);

        }

        public void updateValues(String data)
        {
            Debug.WriteLine("updateValues:"+data);
            flexure.Invoke((MethodInvoker)(() => flexure.Text = data));
            String[] values = data.Replace("\n", "").Replace(" ", "").Split(';');
            String distance = values[0].Split('=')[1];
            
            String force = values[1].Split('=')[1];
             

            double dForce = Int32.Parse(force) / 100f;
            double dDist = Int32.Parse(distance) / 100f;
            flexure.Invoke((MethodInvoker)(() => flexure.Text = dDist.ToString()));
            maxForce.Invoke((MethodInvoker)(() => maxForce.Text = dForce.ToString()));
            calculateParams(dDist, dForce);
        }

        public void calculateParams(double dDist, double dForce)
        {
            double P = dForce / 100;
            double flexLimit = 6 * P * keyLength / keyWidth / Math.Pow(h, 2);
            double mYunga = 4 * P * Math.Pow(keyLength, 3) / keyWidth / Math.Pow(h, 3) / dDist;
            flexureLimit.Invoke((MethodInvoker)(() => flexureLimit.Text = String.Format("{0:0.00}", flexLimit)));
            moduleYunga.Invoke((MethodInvoker)(() => moduleYunga.Text = String.Format("{0:0.00}", mYunga)));
        }


        private void saveBtn_Click(object sender, EventArgs e)
        {
            Graphs graphics = new Graphs(graphs);
            saveExcel(fileNameBox.Text, "save");
            //Program.saveGraph(graphics, fileNameBox.Text);
            //label1.Text = graphs[0].Count().ToString() ;
        }

        void saveSettings()
        {

            Settings settings = new Settings((string)portNum.SelectedItem, distanceBox.Text, numSonar.Text, keyLengthBox.Text, keyWidthBox.Text, height.Text);

            //System.IO.Directory.CreateDirectory(@"\save\");

            FileStream fs = new FileStream("settings.cf", FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                formatter.Serialize(fs, settings);
            }
            catch { }
            finally
            {
                fs.Close();
            }
        }

        void saveFilename(string filename)
        {

            Filename fileName = new Filename(filename);


            //System.IO.Directory.CreateDirectory(@"\save\");

            FileStream fs = new FileStream("filename.cf", FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                formatter.Serialize(fs, fileName);
            }
            catch { }
            finally
            {
                fs.Close();
            }
        }

        void loadSettings()
        {
            Settings settings = null;

            FileStream fs = null;
            try
            {
                fs = new FileStream("settings.cf", FileMode.Open);

                BinaryFormatter formatter = new BinaryFormatter();

                settings = (Settings)formatter.Deserialize(fs);
                portNum.SelectedItem = settings.port;
                distanceBox.Text = settings.fastSteps;
                numSonar.Text = settings.numSonars;
                keyWidthBox.Text = settings.keyWidth;
                keyLengthBox.Text = settings.keyLength;
                height.Text = settings.height;
                //fileNameBox.Text = settings.filename;
            }
            catch { }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
            Filename fileName = null;

            fs = null;
            try
            {
                fs = new FileStream("filename.cf", FileMode.Open);

                BinaryFormatter formatter = new BinaryFormatter();

                fileName = (Filename)formatter.Deserialize(fs);
                fileNameBox.Text = fileName.filename;
            }
            catch { }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }


        public void saveExcel(string filename, string dir)
        {
            if (filename == "")
                filename = "base";


            if (dir != "")
            {
                string Dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir);

                if (!Directory.Exists(Dir))
                    Directory.CreateDirectory(Dir);

                filename = Dir + "\\" + filename;
            }
            ExcelPackage ExcelPkg = new ExcelPackage();
            ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Sheet1");

            wsSheet1.Cells[1, 1].Value = "Milliseconds";

            int numData = graphs[0].Count();

            for (int j=0; j< numData; j++)                     //write milliseconds
            {
                wsSheet1.Cells[j + 2, 1].Value = graphs[0].getPoint(j).GetX();
            }

            for (int i = 0; i < graphs.Length; i++)              //write millimeters for each sonar
            {
                wsSheet1.Cells[1, i + 2].Value = "Sonar " + (i + 1);

                for (int j=0; j<numData; j++)
                {
                    wsSheet1.Cells[j + 2, i + 2].Value = graphs[i].getPoint(j).GetY();
                }
            }

            wsSheet1.Protection.IsProtected = false;
            wsSheet1.Protection.AllowSelectLockedCells = false;

            /*if (File.Exists(filename + ".xlsx"))
                File.Delete(filename + ".xlsx");*/

            ExcelPkg.SaveAs(new FileInfo(filename + ".xlsx"));
        }

        private void settings_click(object sender, EventArgs e)
        {
            saveSettings();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (graphs[0] != null)
                    saveExcel("backup", "backup");
            }
            catch { }
        }


        private void fileNameBox_TextChanged(object sender, EventArgs e)
        {
            saveFilename(fileNameBox.Text);
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //saveSettings();
            if (isStart)
                Program.StopSerial();
        }


    }
}
