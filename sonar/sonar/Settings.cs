using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sonar
{
    [Serializable]
    public class Settings
    {
        public string fastSteps;
        public string port;
        public string numSonars;

        public string keyWidth;
        public string keyLength;
        public string height;

        public Settings(string port, string distance, string numSonars, string keyLength, string keyWidth, string height)
        {
            this.port = port;
            this.fastSteps = distance;
            this.numSonars = numSonars;
            this.keyLength = keyLength;
            this.keyWidth = keyWidth;
            this.height = height;
        }

        public Settings()
        {

        }

    }
    [Serializable]
    public class Filename
    {
        public string filename;



        public Filename(string filename)
        {
            this.filename = filename;
        }
    }
}
