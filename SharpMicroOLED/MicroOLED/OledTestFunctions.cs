using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MicroOLED
{
    public class OledTestFunctions
    {
        public int TestLoopCount;

        Random rng = new Random();
        private MicroOLEDSerial mos;

        public OledTestFunctions(MicroOLEDSerial mos)
        {
            this.mos = mos;
            TestLoopCount = 10;
        }

        public OledTestFunctions(MicroOLEDSerial mos, int testLoopCount)
        {
            this.mos = mos;
            TestLoopCount = testLoopCount;
        }

        public void EraseScreenTest()
        {
            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                mos.SetBackgroundColor(rng.Next(0, 0xffff));
                Thread.Sleep(100);
                int i = 30;
                while (i-- > 0)
                {
                    mos.DrawCircle((byte)rng.Next(10, 86), (byte)rng.Next(10, 54), (byte)rng.Next(1, 10), rng.Next(0xffff));
                }

                i = 100;
                while (i-- > 0)
                {
                    mos.CopyAndPasteScreenBitmap((byte)rng.Next(10, 86), (byte)rng.Next(10, 54), (byte)rng.Next(10, 86), (byte)rng.Next(10, 54), (byte)rng.Next(1, 10), (byte)rng.Next(1, 10));
                }

                mos.EraseScreen();
                Thread.Sleep(300);
            }
        }

        public void SetFontSizeTest()
        {

            // TODO: get a real test done for setting the font size.
            char[] text = new char[] { 'H', 'd' };
            byte textLength = (byte)text.Length;
            byte horizontalTextSize = 1;
            byte vertialTextSize = 1;
            byte x = 10;
            byte y = 10;
            int buttonColour = 0xc0c0;
            int textColour = 0xffff;

            int buttonState = 0;
            int fontSize = 0;
            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                switch (fontSize)
                {
                    case 0:
                        mos.SetFontSize(MicroOLEDSerial.FONTSIZE.SIZE_5X7);
                        fontSize = 1;
                        break;

                    case 1:
                        mos.SetFontSize(MicroOLEDSerial.FONTSIZE.SIZE_8X12);
                        fontSize = 2;
                        break;

                    case 2:
                        mos.SetFontSize(MicroOLEDSerial.FONTSIZE.SIZE_8X8);
                        fontSize = 0;
                        break;
                }

                //if (buttonState == 0)
                //{
                //    mos.TextButton(MicroOLEDSerial.TEXTBUTTONSTATE.DOWN, x, y, buttonColour, MicroOLEDSerial.FONTSIZE.SIZE_5X7, textColour, horizontalTextSize, vertialTextSize, text, textLength);
                //    buttonState = 1;
                //}
                //else if (buttonState == 1)
                //{
                //    mos.TextButton(MicroOLEDSerial.TEXTBUTTONSTATE.UP, x, y, buttonColour, MicroOLEDSerial.FONTSIZE.SIZE_5X7, textColour, horizontalTextSize, vertialTextSize, text, textLength);
                //    buttonState = 0;
                //}


                Thread.Sleep(300);
            }
        }


        public void BitmapTest()
        {
            // TODO: refactor the AddUserBitmappedCharacter to include the characters position as a parameter to the function.
            byte[] bitmap = new byte[] { 0x18, 0x24, 0x42, 0x81, 0x81, 0x42, 0x24, 0x18 };
            byte bitmapLength = (byte)bitmap.Length;
            byte characterLocation = 0x01;
            mos.AddUserBitmappedCharacter(characterLocation, bitmap, bitmapLength);

            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                mos.DisplayUserBitmappedCharacter(characterLocation, (byte)rng.Next(1, 80), (byte)rng.Next(1, 50), rng.Next(0xffff));
                Thread.Sleep(100);
            }
        }

        public void RandomBackgroundTest()
        {
            mos.SetBackgroundColor(rng.Next(0, 0xffff));
        }

        public void ButtonUpDownTest()
        {
            Console.WriteLine("Entering ButtonUpDownTest()");
            char[] text = new char[] { 'H', 'e', 'l', 'l', 'o', ' ', 'w', 'o', 'r', 'l', 'd' };
            byte textLength = (byte)text.Length;
            byte horizontalTextSize = 1;
            byte vertialTextSize = 1;
            byte x = 10;
            byte y = 10;
            int buttonColour = 0xc0c0;
            int textColour = 0xffff;

            int buttonState = 0;
            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                if (buttonState == 0)
                {
                    mos.TextButton(MicroOLEDSerial.TEXTBUTTONSTATE.DOWN, x, y, buttonColour, MicroOLEDSerial.FONTSIZE.SIZE_5X7, textColour, horizontalTextSize, vertialTextSize, text, textLength);
                    buttonState = 1;
                }
                else if (buttonState == 1)
                {
                    mos.TextButton(MicroOLEDSerial.TEXTBUTTONSTATE.UP, x, y, buttonColour, MicroOLEDSerial.FONTSIZE.SIZE_5X7, textColour, horizontalTextSize, vertialTextSize, text, textLength);
                    buttonState = 0;
                }

                Thread.Sleep(100);
            }

            Console.WriteLine("Leaving ButtonUpDownTest()");
        }

        public void RandomCirclesTest()
        {
            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {

                mos.DrawCircle((byte)rng.Next(10, 86), (byte)rng.Next(10, 54), (byte)rng.Next(1, 10), rng.Next(0xffff));
                Thread.Sleep(1);
            }
        }

        public void CopyAndPasteTest()
        {
            int i = 30;
            while (i-- > 0)
            {
                mos.DrawCircle((byte)rng.Next(10, 86), (byte)rng.Next(10, 54), (byte)rng.Next(1, 10), rng.Next(0xffff));
            }

            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                mos.CopyAndPasteScreenBitmap((byte)rng.Next(10, 86), (byte)rng.Next(10, 54), (byte)rng.Next(10, 86), (byte)rng.Next(10, 54), (byte)rng.Next(1, 10), (byte)rng.Next(1, 10));
                Thread.Sleep(1);
            }

        }

        public void RandomPixelTest()
        {
            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                mos.PutPixel((byte)rng.Next(0, 96), (byte)rng.Next(0, 96), rng.Next(0xffffff));
                mos.PutPixel((byte)rng.Next(0, 96), (byte)rng.Next(0, 96), rng.Next(0xffffff));
                mos.PutPixel((byte)rng.Next(0, 96), (byte)rng.Next(0, 96), rng.Next(0xffffff));
                mos.PutPixel((byte)rng.Next(0, 96), (byte)rng.Next(0, 96), rng.Next(0xffffff));
                mos.PutPixel((byte)rng.Next(0, 96), (byte)rng.Next(0, 96), rng.Next(0xffffff));
            }
        }

        public void TriangleTest()
        {
            byte x1 = 80;
            byte x2 = 30;
            byte x3 = 50;

            byte y1 = 10;
            byte y2 = 30;
            byte y3 = 50;

            // while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                mos.DrawTriangle(x1, y1, x2, y2, x3, y3, rng.Next(0xffff));

                Thread.Sleep(300);
            }
        }

        public void PolygonTest()
        {
            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                byte numberOfVertices = (byte)rng.Next(1, 7);

                byte[][] vertArray = new byte[numberOfVertices][];

                for (int i = 0; i < numberOfVertices; i++)
                {
                    vertArray[i] = new byte[2];
                    vertArray[i][0] = (byte)rng.Next(1, 95); // x
                    vertArray[i][1] = (byte)rng.Next(1, 63); // y
                }

                mos.DrawPolygon(numberOfVertices, vertArray, rng.Next(0xffff));

                Thread.Sleep(300);

                mos.EraseScreen();
            }
        }

        public void DisplayImageTest()
        {

            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                byte x = (byte)rng.Next(1, 40);
                byte y = (byte)rng.Next(1, 40);

                byte width = (byte)rng.Next(6, 15);
                byte height = (byte)rng.Next(6, 15);

                MicroOLED.MicroOLEDSerial.COLOURMODE colorMode = (rng.Next(0, 1) == 0) ? MicroOLED.MicroOLEDSerial.COLOURMODE.MODE65K : MicroOLEDSerial.COLOURMODE.MODE256;
                //MicroOLED.MicroOLEDSerial.COLOURMODE colorMode = MicroOLEDSerial.COLOURMODE.MODE256;

                int[] pixels = new int[width * height];

                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = (int)rng.Next(0xffff);
                }

                mos.DisplayImage(x, y, width, height, colorMode, pixels, pixels.Length);

                Thread.Sleep(100);
            }
        }

        public void DisplayLineTest()
        {

            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                byte x1 = (byte)rng.Next(1, 15); ;
                byte x2 = (byte)rng.Next(30, 94);
                byte y1 = (byte)rng.Next(1, 15); ;
                byte y2 = (byte)rng.Next(30, 62);

                int colour = rng.Next(0xffff);

                mos.DrawLine(x1, y1, x2, y2, colour);

                Thread.Sleep(100);
            }
        }

        public void OpaqueTransparentTextTest()
        {
            // TODO: dont have a draw string function done yet?
        }

        public void SetPenSizeTest()
        {
            // while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                MicroOLEDSerial.PENSIZE pn = (rng.Next(0, 1) == 1) ? MicroOLEDSerial.PENSIZE.SOLID : MicroOLEDSerial.PENSIZE.WIREFRAME;
            }
        }

        public void ReadPixelTest()
        {
            // TODO: write this test, to tired right now todo it.
        }

        public void DrawRectangleTest()
        {
            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                byte x1 = (byte)rng.Next(1, 30);
                byte y1 = (byte)rng.Next(1, 30);
                byte x2 = (byte)rng.Next(x1, 95);
                byte y2 = (byte)rng.Next(y1, 60);
                int colour = rng.Next(0xffff);

                if (true)
                {
                    mos.SetPenSize(MicroOLEDSerial.PENSIZE.WIREFRAME);
                }
                else
                {
                    mos.SetPenSize(MicroOLEDSerial.PENSIZE.SOLID);
                }

                mos.DrawRectangle(x1, y1, x2, y2, colour);

            }
        }

        public void PlaceStringOfAsciiTextUnformattedTest()
        {
            int counter = 5;

            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                byte x = (byte)rng.Next(1, 30);
                byte y = (byte)rng.Next(1, 30);

                MicroOLEDSerial.FONTSIZE fn = MicroOLEDSerial.FONTSIZE.SIZE_5X7;

                switch (rng.Next(0, 2))
                {
                    case 0:
                        fn = MicroOLEDSerial.FONTSIZE.SIZE_5X7;
                        break;

                    case 1:
                        fn = MicroOLEDSerial.FONTSIZE.SIZE_8X12;
                        break;

                    case 2:
                        fn = MicroOLEDSerial.FONTSIZE.SIZE_8X8;
                        break;
                }

                char[] asciiString = new char[] { 'h', '3', '1', '1' };
                byte asciiStringLength = (byte)asciiString.Length;
                mos.PlaceStringOfAsciiTextUnformatted(x, y, fn, rng.Next(0xffff), (byte)rng.Next(1, 3), (byte)rng.Next(1, 3), asciiString, asciiStringLength);

                Thread.Sleep(50);

                counter--;

                if (true)
                {
                    counter = 5;
                    mos.EraseScreen();
                }

            }
        }

        public void PlaceTextCharacterUnformatted()
        {
            int i = 5;

            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                char c = (char)rng.Next(32, 127);
                byte width = (byte)rng.Next(1, 10);
                byte height = (byte)rng.Next(1, 10);
                int colour = rng.Next(0xffff);
                byte x = (byte)rng.Next(1, 60);
                byte y = (byte)rng.Next(1, 30);

                mos.SetFontSize(MicroOLEDSerial.FONTSIZE.SIZE_5X7);

                mos.PlaceTextCharacterUnformatted(c, x, y, colour, width, height);

                i--;
                if (i == 0)
                {
                    i = 5;
                    mos.EraseScreen();
                }
                Thread.Sleep(100);
            }
        }

        public void PlaceTextCharacterFormatted()
        {
            int i = 5;

            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                char c = (char)rng.Next(32, 127);
                byte column = (byte)rng.Next(0, 11);
                byte row = (byte)rng.Next(0, 4);
                int colour = rng.Next(0xffff);

                MicroOLEDSerial.TEXTATTRIBUTE tx = (rng.Next(0, 1) == 1) ? MicroOLEDSerial.TEXTATTRIBUTE.OPAQUE : MicroOLEDSerial.TEXTATTRIBUTE.TRANSPARENT;

                switch (rng.Next(0, 2))
                {
                    case 0:
                        mos.SetFontSize(MicroOLEDSerial.FONTSIZE.SIZE_5X7);

                        break;

                    case 1:
                        mos.SetFontSize(MicroOLEDSerial.FONTSIZE.SIZE_8X12);

                        break;

                    case 2:
                        mos.SetFontSize(MicroOLEDSerial.FONTSIZE.SIZE_8X8);

                        break;
                }

                mos.SetTextOpaqueTransparent(tx);


                mos.PlaceTextCharacterFormatted(c, column, row, colour);

                i--;
                if (i == 0)
                {
                    i = 5;
                    mos.EraseScreen();
                }
                Thread.Sleep(100);
            }
        }

        public void PlaceStringOfAsciiTextFormattedTest()
        {
            int counter = 5;

            //while (true)
            int testLoopCount = TestLoopCount;
            while (testLoopCount-- > 0)
            {
                byte column = (byte)rng.Next(0, 11);
                byte row = (byte)rng.Next(0, 4);

                MicroOLEDSerial.FONTSIZE fn = MicroOLEDSerial.FONTSIZE.SIZE_5X7;

                switch (rng.Next(0, 2))
                {
                    case 0:
                        fn = MicroOLEDSerial.FONTSIZE.SIZE_5X7;
                        break;

                    case 1:
                        fn = MicroOLEDSerial.FONTSIZE.SIZE_8X12;
                        break;

                    case 2:
                        fn = MicroOLEDSerial.FONTSIZE.SIZE_8X8;
                        break;
                }

                char[] asciiString = new char[] { 'h', '3', '1', '1' };
                byte asciiStringLength = (byte)asciiString.Length;
                mos.PlaceStringOfAsciiTextFormatted(column, row, fn, rng.Next(0xffff), asciiString, asciiStringLength);

                Thread.Sleep(50);

                counter--;

                if (true)
                {
                    counter = 5;
                    mos.EraseScreen();
                }

            }
        }

        public void DisplayControlTest()
        {
            int i = 10;

            int b = 0;
            while (i-- > 0)
            {
                // Turn the display on and off;
                if (b == 0)
                {
                    mos.DisplayControlFunctions(MicroOLEDSerial.DISPLAYCONTROLMODE.DISPLAY_OFF);
                    b = 1;
                }
                else
                {
                    mos.DisplayControlFunctions(MicroOLEDSerial.DISPLAYCONTROLMODE.DISPLAY_ON);
                    b = 0;
                }
            }

            Thread.Sleep(100);

            mos.SetBackgroundColor(0x0f0f);
            mos.DrawRectangle(10, 10, 30, 30, 0xf0e0f);

            i = 17;

            while (i-- > 0)
            {
                mos.DisplayControlFunctions(MicroOLEDSerial.DISPLAYCONTROLMODE.OLED_CONTRAST, (byte)i);
                Thread.Sleep(1000);
            }

            i = 5;
            b = 0;
            while (i-- > 0)
            {
                if (b == 0)
                {
                    mos.DisplayControlFunctions(MicroOLEDSerial.DISPLAYCONTROLMODE.OLED_POWER_DOWN);
                    b = 1;
                }
                else
                {
                    mos.DisplayControlFunctions(MicroOLEDSerial.DISPLAYCONTROLMODE.OLED_POWER_UP);
                    b = 0;
                }
            }
        }

        public void VersionDeviceInfoRequestTest()
        {
            mos.VersionDeviceInfoRequest(MicroOLEDSerial.VERSIONOUTPUT.DISPLAYANDSERIALPORT);
        }

        internal void InitialiseMicroSDMemoryCard()
        {
            mos.InitialiseMicroSDMemoryCard();
        }
    }

}
