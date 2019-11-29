using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;

namespace sonar
{
    
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        /// 
        static mainForm form1;
        static SerialPort serialPort;
        static Thread readThread = new Thread(Read);
        public static bool _continue;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form1 = new mainForm();
            Application.Run(form1);
        }
        public static void StartSerial(string port)
        {
            if (serialPort != null)
                serialPort.Close();
            serialPort = new SerialPort();
            serialPort.PortName = port;
            serialPort.BaudRate = 250000;
            serialPort.ReadTimeout = 0;
            serialPort.WriteTimeout = 200;
            serialPort.Open();
            if (readThread != null)
                readThread.Abort();
            _continue = true;
            readThread = new Thread(Read);
            readThread.Start();

        }
        public static void startMeasure()
        {
            serialPort.WriteLine("+");
        }
        public static void StopSerial()
        {
            try
            {
                serialPort.WriteLine("-");
                _continue = false;
            }
            catch { }
            try
            {
                //readThread.Abort();
                serialPort.Close();
            }
            catch { };
            //readThread = null;
            
        }

        public static void saveGraph(Graphs graphs, string filename)
        {
            if (filename == "")
                filename = "noName";

            string Dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save");

            filename = Dir + "\\" + filename;

            //System.IO.Directory.CreateDirectory(@"\save\");

            FileStream fs = new FileStream(filename + ".graph", FileMode.Create);

            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                formatter.Serialize(fs, graphs);
            }
            catch { }
            finally
            {
                fs.Close();
            }
        }

       

        static void Read()
        {
            while (_continue)
            {
                try
                {
                    try
                    {
                        //form1.updateDistance("2=2");
                        string message = serialPort.ReadLine();
                        
                        if (message.Contains("force"))
                        {
                            form1.addChartData(message);
                        } else if (message.Contains("maxForce"))
                        {
                            form1.updateValues(message);
                        } else if (message.Contains("Ready"))
                        {
                            form1.stop();
                        }


                    }
                    catch { }
                    
                }
                catch (TimeoutException) { }
            }
        }

        public static void WriteToSerial(String data)
        {
            serialPort.WriteLine(data);
        }

    }
}
