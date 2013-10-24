using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace MicroOLED
{
    class Program
    {
        //public SerialPort port;
     
        static void Main(string[] args)
        {
            MicroOLEDSerial mos = new MicroOLEDSerial(4, 9600);
            mos.Init();
            mos.Clear();

            Random rng = new Random();
            
            OledTestFunctions testfunc = new OledTestFunctions(mos, 50);

            //mos.SendCustomCommand(new byte[] { 64, 73, 0, 0, 96, 64, 16, 0, 16, 0 }, 10);

            testfunc.ButtonUpDownTest();
            testfunc.RandomCirclesTest();
            testfunc.CopyAndPasteTest();
            //testfunc.BitmapTest();
            testfunc.EraseScreenTest();
            testfunc.SetFontSizeTest();
            testfunc.TriangleTest();
            testfunc.PolygonTest();
            //testfunc.DisplayImageTest();
            //testfunc.DisplayLineTest();
            testfunc.RandomPixelTest();
            testfunc.DrawRectangleTest();
            testfunc.PlaceStringOfAsciiTextUnformattedTest();
            testfunc.PlaceStringOfAsciiTextFormattedTest();
            testfunc.PlaceTextCharacterFormatted();

            testfunc.PlaceTextCharacterUnformatted();
            testfunc.DisplayControlTest();

            testfunc.VersionDeviceInfoRequestTest();

            //testfunc.InitialiseMicroSDMemoryCard();
            while (true)
            {
                
            }
        }

      
    }
}
