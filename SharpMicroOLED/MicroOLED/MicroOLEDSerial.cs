using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;

namespace MicroOLED
{
   
    public class MicroOLEDSerial
    {
        // NOTES: we can change all the color references to shorts' instead of int's
        // Big question is do we want to do a return base result or throw exceptions like
        // true OOP...
        // We should create our own type for the byte parameter, something that exposes length and
        // overloads the .ToString() function. This would make debugging easy and turn the data payloads
        // for the commands into objects.
        // REMOVE OLD CODE AND CLEAN UP THE COMMENTS.

        #region Enumerations 
        #region Enumeration of OLED Commands
        /// <summary>
        /// Enumeration of commands available on the oled display.
        /// </summary>
        enum COMMANDS 
        {
            AddUserBitmappedCharacter = 0x41,
            SetBackgroundColor = 0x42,
            TextButton = 0x62,
            DrawCircle = 0x43,
            CopyAndPasteScreenBitmap = 0x63,
            DisplayUserBitmappedCharacter = 0x44,
            EraseScreen = 0x45,
            SetFontSize = 0x46,
            DrawTriangle = 0x47,
            DrawPolygon = 0x67,
            DisplayImage = 0x49,
            DrawLine = 0x4c,
            OpaqueTransparentText = 0x4f,
            PutPixel = 0x50,
            SetPenSize = 0x70,
            ReadPixel = 0x52,
            DrawRectangle = 0x72,
            PlaceStringOfAsciiTextUnformatted = 0x53,
            PlaceStringOfAsciiTextFormatted = 0x73,
            PlaceTextCharacterFormatted = 0x54,
            PlaceTextCharacterUnformatted = 0x74,
            DisplayControlFunctions = 0x59,
            VersionDeviceInfoRequest = 0x56,
            DetectBaudRate = 0x55,
            // Extended commands for specific displays.
            DisplayScrollControl = 0x24,
            CustomCommand = 0xffff
        }
        #endregion 

        #region Enumeration of Button States
        /// <summary>
        /// Enumeration of the text buttons state.
        /// </summary>
        public enum TEXTBUTTONSTATE : int
        {
            DOWN = 0,
            UP = 1
        }
        #endregion 

        #region Enumeration of Font Sizes
        /// <summary>
        /// Enumeration of built in font sizes.
        /// </summary>
        public enum FONTSIZE
        {
            SIZE_5X7 = 0,
            SIZE_8X8 = 1,
            SIZE_8X12 = 2
        }
        #endregion 

        #region Enumeration of Color Mode
        public enum COLOURMODE
        {
            MODE256 = 8,
            MODE65K = 16
        }
        #endregion 

        #region Enumeration of Text Attribute
        public enum TEXTATTRIBUTE
        {
            TRANSPARENT = 0,
            OPAQUE = 1,
        }
        #endregion 

        #region Enumeration of Pen Size
        public enum PENSIZE
        {
            SOLID = 0,
            WIREFRAME = 1
        }
        #endregion 

        #region Enumeration of Display Controls
        public enum DISPLAYCONTROLMODE
        {
            DISPLAY_ON = 0,
            DISPLAY_OFF = 1,
            OLED_CONTRAST = 2,
            OLED_POWER_UP = 3,
            OLED_POWER_DOWN = 4,
        }
        #endregion 

        #region Enumeration of Versioning
        public enum VERSIONOUTPUT
        {
            SERIALPORT = 0,
            DISPLAYANDSERIALPORT = 1
        }
        #endregion 

        #region Enumeration of Responses
        enum RESPONSE {
        OLED_ACK=0x06,  // Ok
        OLED_NAK=0x15 // Error
        }
        #endregion 
        #endregion 

        #region Private Fields
        /// <summary>
        /// Variable where the resulting byte from the oled display is stored.
        /// </summary>
        private byte[] resultByte = new byte[1];

        /// <summary>
        /// Variable where the reference to the serial device is.
        /// </summary>
        private SerialPort port;


        // we could get rid of the new here and just allocate the data byte array first
        // and assign each element. and send the length of the command via the serial port.
        private byte[] data = new byte[500];


        #endregion 

        #region Public Fields
        public SerialPort Port
        {
            get { return port; }
        }
        #endregion 

        #region Constructor 
        /// <summary>
        /// Constructs the OLED Serial object.
        /// </summary>
        /// <param name="portNumber">The COM port number of the oled device.</param>
        /// <param name="speed">Set the serial speed for communications, Supports 300 to 256K Baud</param>
        public MicroOLEDSerial(int portNumber, int speed)
        {
            // Init the debug information.
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Debug.AutoFlush = true;
            Debug.Indent();
            
            //Debug.Unindent();

            // TODO: refactor...
            port = new SerialPort(string.Format("COM{0}", portNumber), speed, Parity.None, 8, StopBits.One);
            port.Open();

            // Reset the oled display.
            port.DtrEnable = true;
            Debug.WriteLine(string.Format("DTR = {0}", port.DtrEnable));
            
            Debug.WriteLine("Thread.Sleep(200)");
            // TODO: Remove this hard sleep...
            Thread.Sleep(200);
        }

        ~MicroOLEDSerial()
        {
            Debug.WriteLine("~MicroOLEDSerial(): Destructor called.");
            Debug.Unindent();
        }
        #endregion 

        private void ResponseStatus(byte[] sentData, COMMANDS sentCommand, string functionName)
        {
            // TODO: we could use a non blocking type of read, like a call back when the display sends data back..
            resultByte[0] = (byte)RESPONSE.OLED_NAK;
            port.Read(resultByte, 0, 1);
            Debug.WriteLine(string.Format("ResponseStatus() : resultByte {0:x}", resultByte[0]));

            if (resultByte[0] == (byte)RESPONSE.OLED_NAK)
            {
                string errorResult = string.Format("OLED Did not respond back to command correctly. Function Name = {0}, Command Sent = {1}, Data Sent = {2}", functionName, sentCommand.ToString(), sentData.ToString());
                Debug.WriteLine(errorResult);
                throw new OledResponseException(errorResult);
            }

            Debug.WriteLine("ResponseStatus() : result unknown.");
        }

        /// <summary>
        /// Print the debug information of the calling function and its paramters..
        /// </summary>
        /// <param name="sentData"></param>
        /// <param name="sentCommand"></param>
        /// <param name="functionName"></param>
        /// <returns></returns>
        private string PrintCommandInformation(byte[] sentData, COMMANDS sentCommand, string functionName)
        {
            return string.Format("Calling : Function Name = {0}, Command Sent = {1}, Data Sent = {2}", functionName, sentCommand.ToString(), sentData.ToString());
        }

        private string PrintCommandInformation(byte[] sentData, COMMANDS sentCommand, string functionName, string extraInformation)
        {
            return string.Format("Calling : Function Name = {0}, Command Sent = {1}, Data Sent = {2}, Extra Information = {3}", functionName, sentCommand.ToString(), sentData.ToString(), extraInformation);
        }

        /// <summary>
        /// Initilize the oled device communications speed.
        /// </summary>
        public void Init()
        {
            Thread.Sleep(5000);
            data[0] = (byte)COMMANDS.DetectBaudRate;
            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.DetectBaudRate, "Init()"));
            port.Write(data , 0, 1);
            ResponseStatus(data, COMMANDS.DetectBaudRate, "Init()");
        }

        /// <summary>
        /// Helper function for getting a single int for Red,Green,Blue.
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <returns></returns>
        public int GetRGB(int red, int green, int blue)
        {
            // TODO: this could be a static function.

            //int outR = ((red * 31) / 255);
            //int outG = ((green * 63) / 255);
            //int outB = ((blue * 31) / 255);

            return (((red * 31) / 255) << 11) | (((green * 63) / 255) << 5) | ((blue * 31) / 255);
        }

        /// <summary>
        /// Clear the oled display device.
        /// </summary>
        public void Clear()
        {
            data[0] = (byte)COMMANDS.EraseScreen;
            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.EraseScreen, "Clear()"));
            port.Write(data, 0, 1);
            Thread.Sleep(20);
            ResponseStatus(data, COMMANDS.EraseScreen, "Clear()");
        }

        #region DUPLICATE FUNCTIONS, FACTOR INTO Micro OLED API or Remove
        public void DrawRectangle(byte x, byte y, byte width, byte height, byte filled, int color)
        {
            data[0] = (byte)COMMANDS.DrawRectangle;
            data[1] = x;
            data[2] = y;
            data[3] = (byte)(x+width);
            data[4] = (byte)(y+height);
            data[5] = (byte)(color >> 8);
            data[6] = (byte)(color & 0xff);

            if (filled == 1)
            {
                data[7] = 0x01;
            }
            else
            {
                data[7] = 0x00;
            }

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.DrawRectangle, "DrawRectangle(byte x, byte y, byte width, byte height, byte filled, int color)",
                string.Format("x = {0}, y = {1}, width = {2}, height = {3}, filled = {4}, color = {5}", x, y, width, height, filled, color)));

            port.Write(data, 0, 8);

            ResponseStatus(data, COMMANDS.DrawRectangle, "DrawRectangle(byte x, byte y, byte width, byte height, byte filled, int color)");
        }

        public void DrawText(byte column, byte row, byte font_size, string mytext, int color)
        {
            // TODO: refactor.

            data[0] = (byte)COMMANDS.PlaceStringOfAsciiTextFormatted;

            int newCol = 13 - mytext.Length / 2;

            data[1] = column;
            data[2] = row;
            data[3] = font_size;
            data[4] = (byte)(color >> 8);
            data[5] = (byte)(color & 0xff);

            for (int i = 0; i < mytext.Length; i++)
            {
                data[i + 6] = Encoding.ASCII.GetBytes(new char[] { mytext[i] })[0];
            }

            data[mytext.Length + 6] = 0x00;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.PlaceStringOfAsciiTextFormatted, "DrawText(byte column, byte row, byte font_size, string mytext, int color)", 
                string.Format("column = (0), row = {1}, font_size = {2}, mytext = {3}, color = {4}", column, row, font_size, mytext, color)));

            port.Write(data, 0, mytext.Length + 6);
            
            ResponseStatus(data, COMMANDS.PlaceStringOfAsciiTextFormatted, "DrawText(byte column, byte row, byte font_size, string mytext, int color)");
        }

        public void DrawSingleChar(byte column, byte row, byte font_size, char MyChar, int color)
        {
            // TODO: finish this function, no write happens here.
            throw new Exception("TODO: finish this function");

            data[0] = (byte)COMMANDS.PlaceTextCharacterFormatted;
            data[1] = Encoding.ASCII.GetBytes(new char[] { MyChar })[0];
            data[2] = column;
            data[3] = row;
            data[4] = (byte)(color >> 8);
            data[5] = (byte)(color & 0xff);

            ResponseStatus(data, COMMANDS.PlaceTextCharacterFormatted, "DrawSingleChar(byte column, byte row, byte font_size, char MyChar, int color)");
        }
        #endregion 

        public void SendCustomCommand(byte[] customCommand, int length)
        {
            // TODO: clean up..

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.CustomCommand, "SendCustomCommand(byte[] customCommand, int length)"));

            port.Write(customCommand, 0, length);

            ResponseStatus(data, COMMANDS.CustomCommand, "SendCustomCommand(byte[] customCommand, int length)");
        }

        #region Micro OLED API

        //General Command Set                           [Live] [Object] [Memory]
        //(A) Add User Bitmapped Character                  v
        //(B) Set Background Colour                         v        v        v
        //(b) Place Text button                             v        v        v
        //(C) Draw Circle                                   v        v        v
        //(c) Block copy and Paste (bitmap copy)            v
        //(D) Display User Bitmapped Character              v
        //(E) Erase Screen                                  v        v        v
        //(F) Font Size                                     v        v        v
        //(G) Draw TrianGle                                 v        v        v
        //(g) Draw Polygon                                  v        v        v
        //(I) Display Image                                 v
        //(L) Draw Line                                     v        v        v
        //(O) Opaque or Transparent Text                    v        v        v
        //(P) Put Pixel                                     v
        //(p) Set pen Size                                  v        v        v
        //(R) Read Pixel                                    v
        //(r) Draw rectangle                                v        v        v
        //(S) Place String of ASCII Text (unformatted)      v        v        v
        //(s) Place string of ASCII Text (formatted)        v        v        v
        //(T) Place Text Character (formatted)              v        v        v
        //(t) Place text Character (unformatted)            v        v        v
        //(V) Version/Device Info Request                   v
        //(Y) OLED DisplaY Control functions                v        v        v




        /// <summary>
        /// This command will add a user defined bitmapped character into the
        /// internal memory. <seealso cref="2.2.1 Add User Bitmapped Character (A)"/>
        /// 
        /// NOTE: This documenation needs to be redone.
        /// </summary>
        /// <param name="Aascii">
        /// byte array with Syntax : char#, data1, data2, …….., dataN
        /// 
        /// char# : bitmap character number to add to memory:
        /// 0 to 31 (00h to 1Fh), 32 characters of 8x8 format.
        /// 
        /// data1 to dataN : number of data bytes that make up the composition and format of
        /// the bitmapped character. The 8x8 bitmap composition is 1 byte wide (8bits)
        /// by 8 bytes deep which makes N = 1x8 = 8.
        /// 
        /// </param>
        /// <param name="length">The number of bytes in the Aascii byte array.</param>
        /// <example>
        /// byte[] bitmap = new byte[] { 0x41, 0x01, 0x18, 0x24, 0x42, 0x81, 0x81, 0x42, 0x24, 0x18 };
        /// int bitmapLength = bitmap.Length;
        /// AddUserBitmappedCharacter(bitmap, bitmapLength);
        /// </example>
        /// <exception cref=""></exception>
        public void AddUserBitmappedCharacter(byte characterNumber, byte[] Aascii, byte length)
        {

            data[0] = (byte)COMMANDS.AddUserBitmappedCharacter;
            data[1] = characterNumber;
            for (int i = 0; i < length; i++)
            {
                data[2+i] = Aascii[i];
            }

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.AddUserBitmappedCharacter, "AddUserBitmappedCharacter(byte characterNumber, byte[] Aascii, byte length)", 
                string.Format("characterNumber = {0}, Aascii = {1}, length = {2}", characterNumber, Aascii, length)));

            port.Write(data, 0, length+2);

            ResponseStatus(data, COMMANDS.AddUserBitmappedCharacter, "AddUserBitmappedCharacter(byte characterNumber, byte[] Aascii, byte length)");
        }

        /// <summary>
        /// This command sets the current background colour. Once this command
        /// is sent, only the background colour will change. Any other object on the screen with a
        /// different colour value will not be affected.
        /// </summary>
        /// <param name="color">
        /// 
        /// colour(msb:lsb) : pixel colour value: 2 bytes (16 bits) msb:lsb
        /// 65,536 colours to choose from
        /// Black = 0000hex, 0dec
        /// White = FFFFhex, 65,535dec, (11111)(111111)(11111)bin, 5-6-5 (RGB)
        /// 
        /// <seealso cref="MicroOLED.MicroOLEDSerial.GetRGB(int R, int G, int B)"/>
        /// </param>
        /// <example >
        /// int white = 0xffff;
        /// SetBackgroundColor(white);
        /// </example>
        public void SetBackgroundColor(int colour)
        {
            data[0] = (byte)COMMANDS.SetBackgroundColor;
            data[1] = (byte)(colour >> 8);
            data[2] = (byte)(colour & 0xff);

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.SetBackgroundColor, "SetBackgroundColor(int colour)", 
                string.Format("colour = {0}", colour)));

            port.Write(data, 0, 3);

            ResponseStatus(data, COMMANDS.SetBackgroundColor, "SetBackgroundColor(int colour)");
        }

        /// <summary>
        /// This command will place a Text button similar to the ones used in a PC
        /// Windows environment. (x, y) refers to the top left corner of the button and the size of
        /// the button is automatically calculated and drawn on the screen with the text relatively
        /// justified inside the button box. The button can be displayed in an UP (button not
        /// pressed) or DOWN (button pressed) position by specifying the appropriate value in the
        /// state byte. Separate button and text colours provide many variations in appearance
        /// and format.
        /// </summary>
        /// 
        /// <param name="state">
        /// Specifies whether the displayed button is drawn as UP (not pressed) or
        /// DOWN (pressed). 0 = Button Down (pressed)
        /// 1 = Button Up (not pressed)
        /// </param>
        /// 
        /// <param name="x">
        /// top left horizontal start position of the button
        /// </param>
        /// 
        /// <param name="y">
        /// top left vertical start position of the button
        /// </param>
        /// 
        /// <param name="buttonColour">
        /// (msb:lsb) : 2 byte button colour value
        /// </param>
        /// 
        /// <param name="font">
        /// 0 = 5x7 font, 1 = 8x8 font, 2 = 8x12 font. This has precedence and does not
        /// affect the Font command.
        /// </param>
        /// 
        /// <param name="textColor">
        /// (msb:lsb) : 2 byte text colour value
        /// </param>
        /// 
        /// <param name="textWidth">
        /// horizontal size of the character, effects the width of the button.
        /// </param>
        /// 
        /// <param name="textHight">
        /// vertical size of the character, effects the height of the button.
        /// </param>
        /// 
        /// <param name="text">
        /// character array of ASCII characters (limit the string to line width)
        /// </param>
        /// 
        /// <param name="textLength">
        /// Length of the character text array.
        /// </param>
        /// <example>
        /// char[] text = new char[] { 'H', 'e', 'l', 'l', 'o', ' ', 'w', 'o', 'r', 'l', 'd' };
        /// byte textLength = (byte)text.Length;
        /// byte horizontalTextSize = 1;
        /// byte vertialTextSize = 1;
        /// byte x = 10;
        /// byte y = 10;
        /// int buttonColour = 0xc0c0;
        /// int textColour = 0xffff;
        ///
        /// int buttonState = 0;
        /// while (true)
        /// {
        ///   if (buttonState == 0)
        ///   {
        ///     mos.TextButton(MicroOLEDSerial.TEXTBUTTONSTATE.DOWN, x, y, buttonColour, MicroOLEDSerial.FONTSIZE.SIZE_5X7, textColour, horizontalTextSize, vertialTextSize, text, textLength);
        ///     buttonState = 1;
        ///   }
        ///   else if (buttonState == 1)
        ///   {
        ///     mos.TextButton(MicroOLEDSerial.TEXTBUTTONSTATE.UP, x, y, buttonColour, MicroOLEDSerial.FONTSIZE.SIZE_5X7, textColour, horizontalTextSize, vertialTextSize, text, textLength);
        ///     buttonState = 0;
        ///   }
        ///
        ///   Thread.Sleep(100);
        /// }
        /// </example>
        public void TextButton(TEXTBUTTONSTATE state, byte x, byte y, int buttonColour, FONTSIZE font, int textColor, byte textWidth, byte textHight, char[] text, byte textLength)
        {
            // TODO: fix textHight variable spelling error.
            data[0] = (byte)COMMANDS.TextButton;
            data[1] = (byte)state;
            data[2] = x;
            data[3] = y;
            data[4] = (byte)(buttonColour >> 8);
            data[5] = (byte)(buttonColour & 0xff);
            data[6] = (byte)(font);
            data[7] = (byte)(textColor >> 8);
            data[8] = (byte)(textColor & 0xff);
            data[9] = textWidth;
            data[10] = textHight;
            for (int i=0; i<textLength; i++)
            {
                data[11+i] = (byte)text[i];
            }

            data[11+textLength] = 0;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.TextButton, "TextButton(TEXTBUTTONSTATE state, byte x, byte y, int buttonColour, FONTSIZE font, int textColor, byte textWidth, byte textHight, char[] text, byte textLength)", 
                string.Format("state = {0}, x = {1}, y = {2}, buttonColour = {3}, font = {4}, textColor = {5}, textWidth = {6}, textHeight = {7}, text = {8}, textLength = {9}", 
                state, x, y, buttonColour, font, textColor, textWidth, textHight, text, textLength)));

            port.Write(data, 0, 12 + textLength);

            ResponseStatus(data, COMMANDS.TextButton, "TextButton(TEXTBUTTONSTATE state, byte x, byte y, int buttonColour, FONTSIZE font, int textColor, byte textWidth, byte textHight, char[] text, byte textLength)");
        }

        /// <summary>
        /// This command will draw a coloured circle centred at (x, y) with a
        /// radius determined by the value of rad. The circle can be either solid or wire frame
        /// (empty) depending on the value of the Pen Size (see Set Pen Size command). When
        /// Pen Size = 0 circle is solid, Pen Size = 1 circle is wire frame.
        /// 
        /// NOTE: add reference to Pen Size.
        /// </summary>
        /// <param name="x">circle centre horizontal position. 0dec to 95dec (00hex to 5Fhex). (These are values for the 96x64 OLED)</param>
        /// <param name="y">circle centre vertical position. 0dec to 63dec (00hex to 37Fhex). (These are values for the 96x64 OLED)</param>
        /// <param name="radius">radius size of the circle. 0dec to 63dec (00hex to 3Fhex). (These are values for the 96x64 OLED)</param>
        /// <param name="color">(msb:lsb) : 2 byte circle colour value</param>
        /// <example>
        /// byte x = 20;
        /// byte y = 30;
        /// byte radius = 10;
        /// int colour = 0xfec9;
        /// 
        /// DrawCircle(x, y, radius, colour);
        /// </example>
        public void DrawCircle(byte x, byte y, byte radius, int color)
        {
            // TODO: if we set the constructor to know about the size of the display, then we can do some basic checks so the commands
            // do not go out of range, like the radis is 10 when x is 94, and similar checks along those lines. The display is not fond
            // of those kinds of mistakes.
            data[0] = (byte)COMMANDS.DrawCircle;
            data[1] = x;
            data[2] = y;
            data[3] = radius;
            data[4] = (byte)(color >> 8);
            data[5] = (byte)(color & 0xff);

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.DrawCircle, "DrawCircle(byte x, byte y, byte radius, int color)", 
                string.Format("x = {0}, y = {1}, radius = {2}, color = {3}", x, y, radius, color)));

            port.Write(data, 0, 6);

            ResponseStatus(data, COMMANDS.DrawCircle, "DrawCircle(byte x, byte y, byte radius, int color)");
        }

        /// <summary>
        /// This command copies an area of a bitmap block of specified size. The
        /// start location of the block to be copied is represented by xs, ys (top left corner) and
        /// the size of the area to be copied is represented by width and height parameters.
        /// The start location of where the block is to be pasted (destination) is represented by
        /// xd, yd (top left corner).
        /// This is a very powerful feature for animating objects, smooth scrolling, implementing a
        /// windowing system or copying patterns across the screen to make borders or tiles.
        /// </summary>
        /// <param name="xs">top left horizontal start position of block to be copied (source)</param>
        /// <param name="ys">top left vertical start position of block to be copied (source)</param>
        /// <param name="xd">top left horizontal start position of where copied block is to be pasted
        /// (destination)</param>
        /// <param name="yd">top left vertical start position of where the copied block is to be pasted
        /// (destination)</param>
        /// <param name="width">width of block to be copied (source)</param>
        /// <param name="height">height of block to be copied (source)</param>
        /// <example>
        /// byte xSource = 10;
        /// byte ySource = 10;
        /// byte width = 20;
        /// byte height = 10;
        /// 
        /// byte xDestination = 40;
        /// byte yDestination = 40;
        /// 
        /// CopyAndPasteScreenBitmap(xSource, ySource, xDestination, yDestination, width, height);
        /// </example>
        public void CopyAndPasteScreenBitmap(byte xs, byte ys, byte xd, byte yd, byte width, byte height)
        {
            data[0] = (byte)COMMANDS.CopyAndPasteScreenBitmap;

            data[1] = xs;
            data[2] = ys;
            data[3] = xd;
            data[4] = yd;
            data[5] = width;
            data[6] = height;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.CopyAndPasteScreenBitmap, "CopyAndPasteScreenBitmap(byte xs, byte ys, byte xd, byte yd, byte width, byte height)", 
                string.Format("xs = {0}, ys = {1}, xd = {2}, yd = {3}, width = {4}, height = {5}", xs, ys, xd, yd, width, height)));

            port.Write(data, 0, 7);

            ResponseStatus(data, COMMANDS.CopyAndPasteScreenBitmap, "CopyAndPasteScreenBitmap(byte xs, byte ys, byte xd, byte yd, byte width, byte height)");
        }

        /// <summary>
        /// This command displays the previously defined user bitmapped
        /// character at location (x, y) on the screen. User defined bitmaps allow drawing &
        /// displaying unlimited graphic patterns quickly & effectively.
        /// </summary>
        /// <param name="characterNumber">which user defined character number to display from the selected group.
        /// 0dec to 31dec (00hex to 1Fhex), of 8x8 format.</param>
        /// <param name="x">horizontal display position of the character. 0dec to 95dec (00hex to 5Fhex). (These are values for the 96x64 OLED)</param>
        /// <param name="y">vertical display position of the character. 0dec to 63dec (00hex to 3Fhex). (These are values for the 96x64 OLED)</param>
        /// <param name="colour">(msb:lsb) : 2 byte bitmap colour value</param>
        public void DisplayUserBitmappedCharacter(byte characterNumber, byte x, byte y, int colour)
        {
            data[0] = (byte)COMMANDS.DisplayUserBitmappedCharacter;

            data[1] = characterNumber;
            data[2] = x;
            data[3] = y;
            data[4] = (byte)(colour >> 8); 
            data[5] = (byte)(colour & 0xff);

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.DisplayUserBitmappedCharacter, "DisplayUserBitmappedCharacter(byte characterNumber, byte x, byte y, int colour)", 
                 string.Format("characterNumber = {0}, x = {1}, y = {2}, colour = {3}", characterNumber, x, y, colour)));

            port.Write(data, 0, 6);

            ResponseStatus(data, COMMANDS.DisplayUserBitmappedCharacter, "DisplayUserBitmappedCharacter(byte characterNumber, byte x, byte y, int colour)");
        }

        /// <summary>
        /// This command clears the entire screen using the current background
        /// colour.
        /// </summary>
        public void EraseScreen()
        {
            data[0] = (byte)COMMANDS.EraseScreen;
            
            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.EraseScreen, "EraseScreen()", string.Format("")));
            
            port.Write(data, 0, 1);

            ResponseStatus(data, COMMANDS.EraseScreen, "EraseScreen()");
        }

        /// <summary>
        /// This command will change the size of the font according to the value
        /// set by size. Changes take place after the command is sent. Any character on the
        /// screen with the old font size will remain as it was.
        /// </summary>
        /// <param name="fontSize"></param>
        public void SetFontSize(FONTSIZE fontSize)
        {
            data[0] = (byte)COMMANDS.SetFontSize;
            data[1] = (byte)fontSize;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.SetFontSize, "SetFontSize(FONTSIZE fontSize)", string.Format("fontSize = {0}", fontSize)));
            
            port.Write(data, 0, 2);
            
            ResponseStatus(data, COMMANDS.SetFontSize , "SetFontSize(FONTSIZE fontSize)");
        }

        /// <summary>
        /// This command draws a Solid/Empty triangle. The vertices must be
        /// specified in an anti-clock wise manner, i.e.
        /// x2 < x1, x3 > x2, y2 > y1, y3 > y1.
        /// A solid or a wire frame triangle is determined by the value of the Pen Size setting, i.e.
        /// 0 = solid, 1 = wire frame.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        /// <param name="colour"></param>
        public void DrawTriangle(byte x1, byte y1, byte x2, byte y2, byte x3, byte y3, int colour)
        {
            data[0] = (byte)COMMANDS.DrawTriangle;

            data[1] = x1;
            data[2] = y1;
            data[3] = x2;
            data[4] = y2;
            data[5] = x3;
            data[6] = y3;
            data[7] = (byte)(colour >> 8);
            data[8] = (byte)(colour & 0xff);

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.DrawTriangle, "DrawTriangle(byte x1, byte y1, byte x2, byte y2, byte x3, byte y3, int colour)", 
                string.Format("x1 = {0}, y1 = {1}, x2 = {2}, y2 = {3}, x3 = {4}, y3 = {5}", x1, y1, x2, y2, x3, y3)));

            port.Write(data, 0, 9);
            
            ResponseStatus(data, COMMANDS.DrawTriangle , "DrawTriangle(byte x1, byte y1, byte x2, byte y2, byte x3, byte y3, int colour)");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfVertices"></param>
        /// <param name="vertices"></param>
        /// <param name="colour"></param>
        public void DrawPolygon(byte numberOfVertices, byte[][] vertices, int colour)
        {
            data[0] = (byte)COMMANDS.DrawPolygon;

            data[1] = numberOfVertices;

            for (int i = 0; i < numberOfVertices; i++)
            {
                data[2 + i] = vertices[i][0];
                data[3 + i] = vertices[i][1];
            }

            data[3 + numberOfVertices] = (byte)(colour >> 8);
            data[4 + numberOfVertices] = (byte)(colour & 0xff);

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.DrawPolygon, "DrawPolygon(byte numberOfVertices, byte[][] vertices, int colour)", 
                string.Format("numberOfVertices = {0}, vertices = {1}, colour = {2}", numberOfVertices, vertices, colour)));

            port.Write(data, 0, 4 + 2*numberOfVertices);
            
            ResponseStatus(data, COMMANDS.DrawPolygon, "DrawPolygon(byte numberOfVertices, byte[][] vertices, int colour)");
        }
        
        public void DisplayImage(byte x, byte y, byte width, byte height, COLOURMODE colourMode, int[] pixels, int pixelsLength)
        {
            data[0] = (byte)COMMANDS.DisplayImage;

            data[1] = x;
            data[2] = y;
            data[3] = width;
            data[4] = height;
            data[5] = (byte)colourMode;

            for (int i = 0; i < pixelsLength; i++)
            {
                if (COLOURMODE.MODE256 == colourMode)
                {
                    data[6 + i] = (byte)(pixels[i] >> 8);
                    data[7 + i] = (byte)(pixels[i] & 0xff);
                }
                else
                {
                    data[6 + i] = (byte)pixels[i];
                }
            }

            int serialWriteLength = (COLOURMODE.MODE65K == colourMode) ? 6 + 2*pixelsLength : 6 + pixelsLength;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.DisplayImage, "DisplayImage(byte x, byte y, byte width, byte height, COLOURMODE colourMode, int[] pixels, int pixelsLength)", 
                string.Format("x = {0}, y = {1}, width = {2}, height = {3}, colourMode = {4}, pixels = {5}, pixelsLength = {6}", x, y, width, height, colourMode, pixels, pixelsLength)));

            port.Write(data, 0, serialWriteLength);

            ResponseStatus(data, COMMANDS.DisplayImage , "DisplayImage(byte x, byte y, byte width, byte height, COLOURMODE colourMode, int[] pixels, int pixelsLength)");
        }

        public void DrawLine(byte x1, byte y1, byte x2, byte y2, int colour)
        {
            data[0] = (byte)COMMANDS.DrawLine;
            data[1] = x1;
            data[2] = y1;
            data[3] = x2;
            data[4] = y2;
            data[5] = (byte)(colour >> 8);
            data[6] = (byte)(colour & 0xff);

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.DrawLine, "DrawLine(byte x1, byte y1, byte x2, byte y2, int colour)", 
                string.Format("x1 = {0}, y1 = {1}, x2 = {2}, y2 = {3}", x1, y1, x2, y2, colour)));

            port.Write(data, 0, 7);

            ResponseStatus(data, COMMANDS.DrawLine, "DrawLine(byte x1, byte y1, byte x2, byte y2, int colour)");
        }

        public void SetTextOpaqueTransparent(TEXTATTRIBUTE attrib)
        {
            data[0] = (byte)COMMANDS.OpaqueTransparentText;
            data[1] = (byte)attrib;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.OpaqueTransparentText, "SetTextOpaqueTransparent(TEXTATTRIBUTE attrib)", 
                string.Format("attrib = {0}", attrib)));

            port.Write(data, 0, 2);

            ResponseStatus(data, COMMANDS.OpaqueTransparentText, "SetTextOpaqueTransparent(TEXTATTRIBUTE attrib)");
        }

        public void PutPixel(byte x, byte y, int colour)
        {
            data[0] = (byte)COMMANDS.PutPixel;

            data[1] = x;
            data[2] = y;
            data[3] = (byte)(colour >> 8);
            data[4] = (byte)(colour & 0xff);

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.PutPixel, "PutPixel(byte x, byte y, int colour)", 
                string.Format("x = {0}, y = {1}, colour = {2}", x, y, colour)));
            
            port.Write(data, 0, 5);

            ResponseStatus(data, COMMANDS.PutPixel, "PutPixel(byte x, byte y, int colour)");
        }

        public void SetPenSize(PENSIZE pensize)
        {
            data[0] = (byte)COMMANDS.SetPenSize;
            data[1] = (byte)pensize;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.SetPenSize, "SetPenSize(PENSIZE pensize)", 
                string.Format("pensize = {0}", pensize)));

            port.Write(data, 0, 2);

            ResponseStatus(data, COMMANDS.SetPenSize, "SetPenSize(PENSIZE pensize)");
        }

        public byte[] ReadPixel(byte x, byte y)
        {
            data[0] = (byte)COMMANDS.ReadPixel;
            data[1] = x;
            data[2] = y;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.ReadPixel, "ReadPixel(byte x, byte y)", 
                string.Format("x = {0}, y = {1}", x, y)));

            port.Write(data, 0, 3);

            byte[] colour = new byte[2];

            port.Read(colour, 0, 2);

            return colour;
        }

        public void DrawRectangle(byte x1, byte y1, byte x2, byte y2, int colour)
        {
            data[0] = (byte)COMMANDS.DrawRectangle;

            data[1] = x1;
            data[2] = y1;
            data[3] = x2;
            data[4] = y2;

            data[5] = (byte)(colour >> 8);
            data[6] = (byte)(colour & 0xff);

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.DrawRectangle, "DrawRectangle(byte x1, byte y1, byte x2, byte y2, int colour)", 
                string.Format("x1 = {0}, y1 = {1}, x2 = {3}, y2 = {4}, colour = {5}", x1, y1, x2, y2, colour)));

            port.Write(data, 0, 7);

            ResponseStatus(data, COMMANDS.DrawRectangle, "DrawRectangle(byte x1, byte y1, byte x2, byte y2, int colour)");
        }

        public void PlaceStringOfAsciiTextUnformatted(byte x, byte y, FONTSIZE font, int colour, byte width, byte height, char[] asciiString, byte asciiStringLength)
        {
            data[0] = (byte)COMMANDS.PlaceStringOfAsciiTextUnformatted;
            data[1] = x;
            data[2] = y;
            data[3] = (byte)font;
            data[4] = (byte)(colour >> 8);
            data[5] = (byte)(colour & 0xff);
            data[6] = width;
            data[7] = height;

            for (int i = 0; i < asciiStringLength; i++)
            {
                data[8 + i] = (byte)asciiString[i];
            }

            data[8 + asciiStringLength] = 0;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.PlaceStringOfAsciiTextUnformatted, "PlaceStringOfAsciiTextUnformatted(byte x, byte y, FONTSIZE font, int colour, byte width, byte height, char[] asciiString, byte asciiStringLength)", 
                string.Format("x = {0}, y = {1}, font = {2}, colour = {3}, width = {4}, height = {5}, asciiString = {6}, asciiStringLength = {7}",
                x, y, font, colour, width, height, asciiString, asciiStringLength)));

            port.Write(data, 0, 9 + asciiStringLength);
        }

        public void PlaceStringOfAsciiTextFormatted(byte column, byte row, FONTSIZE font, int colour, char[] asciiString, byte asciiStringLength)
        {
            data[0] = (byte)COMMANDS.PlaceStringOfAsciiTextFormatted;
            data[1] = column;
            data[2] = row;
            data[3] = (byte)font;
            data[4] = (byte)(colour >> 8);
            data[5] = (byte)(colour & 0xff);

            for (int i = 0; i < asciiStringLength; i++)
            {
                data[6 + i] = (byte)asciiString[i];
            }

            data[6 + asciiStringLength] = 0;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.PlaceStringOfAsciiTextFormatted, "PlaceStringOfAsciiTextFormatted(byte column, byte row, FONTSIZE font, int colour, char[] asciiString, byte asciiStringLength)", 
                string.Format("column = {0}, row = {1}, font = {2}, colour = {3}, asciiString = {4}, asciiStringLength = {5}", column, row, font, colour, asciiString, asciiStringLength)));

            port.Write(data, 0, 7 + asciiStringLength);

            ResponseStatus(data, COMMANDS.PlaceStringOfAsciiTextFormatted, "PlaceStringOfAsciiTextFormatted(byte column, byte row, FONTSIZE font, int colour, char[] asciiString, byte asciiStringLength)");

        }

        /// <summary>
        /// This command will place a coloured ASCII character (from the ASCII
        /// chart) on the screen at a location specified by (column, row). The position of the
        /// character on the screen is determined by the predefined horizontal and vertical
        /// positions available, namely 0 to 15 columns by 0 to 7 rows.
        /// </summary>
        /// <param name="character">inbuilt standard ASCII character, 32dec to 127dec (20hex to 7Fhex)</param>
        /// <param name="column">
        /// horizontal position of character, see range below:
        /// 0 - 15 for 5x7 font, 0 - 11 for 8x8 and 8x12 font.
        /// </param>
        /// <param name="row">vertical position of character:
        /// 0 - 7 for 5x7 and 8x8 font, 0 – 4 for 8x12 font.</param>
        /// <param name="colour">colour(msb:lsb) : 2 byte colour value of the character.</param>
        public void PlaceTextCharacterFormatted(char character, byte column, byte row, int colour)
        {
            data[0] = (byte)COMMANDS.PlaceTextCharacterFormatted;

            data[1] = (byte)character;
            data[2] = column;
            data[3] = row;
            data[4] = (byte)(colour >> 8);
            data[5] = (byte)(colour & 0xff);

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.PlaceTextCharacterFormatted, "PlaceTextCharacterFormatted(char character, byte column, byte row, int colour)",
                string.Format("character = {0}, column = {1}, row = {2}, colour = {3}", character, column, row, colour)));

            port.Write(data, 0, 6);

            ResponseStatus(data, COMMANDS.PlaceTextCharacterFormatted , "PlaceTextCharacterFormatted(char character, byte column, byte row, int colour)");

        }

        /// <summary>
        /// This command will place a coloured built in ASCII character anywhere
        /// on the screen at a location specified by (x, y). Unlike the ‘T’ command, this option
        /// allows text of any size (determined by width and height) to be placed at any
        /// position. The font of the character is determined by the ‘Font Size’ command.
        /// </summary>
        /// <param name="character">inbuilt standard ASCII character, 32dec to 127dec (20hex to 7Fhex)</param>
        /// <param name="x">the horizontal position of character (in pixel units).</param>
        /// <param name="y">the vertical position of character (in pixel units).</param>
        /// <param name="colour">colour(msb:lsb) : 2 byte colour value of the character.</param>
        /// <param name="width">horizontal size of the character, n x normal size</param>
        /// <param name="height">vertical size of the character, m x normal size</param>
        public void PlaceTextCharacterUnformatted(char character, byte x, byte y, int colour, byte width, byte height)
        {
            data[0] = (byte)COMMANDS.PlaceTextCharacterUnformatted;

            data[1] = (byte)character;
            data[2] = x;
            data[3] = y;
            data[4] = (byte)(colour >> 8);
            data[5] = (byte)(colour & 0xff);
            data[6] = width;
            data[7] = height;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.PlaceTextCharacterUnformatted, "PlaceTextCharacterUnformatted(char character, byte x, byte y, int colour, byte width, byte height)",
                string.Format("character = {0}, x = {1}, y = {2}, colour = {4}, width = {5}, height = {6}", character, x, y, colour, width, height)));

            port.Write(data, 0, 8);

            ResponseStatus(data, COMMANDS.PlaceTextCharacterUnformatted, "PlaceTextCharacterUnformatted(char character, byte x, byte y, int colour, byte width, byte height)");

        }

        public void DisplayControlFunctions(DISPLAYCONTROLMODE command)
        {
            DisplayControlFunctions(command, 0xff);
        }

        public void DisplayControlFunctions(DISPLAYCONTROLMODE command, byte value)
        {
            byte mode = 0;
            data[0] = (byte)COMMANDS.DisplayControlFunctions;

            switch (command)
            {
                case DISPLAYCONTROLMODE.DISPLAY_ON:
                    mode = 0x01;
                    value = 0x01;
                    break;

                case DISPLAYCONTROLMODE.DISPLAY_OFF:
                    mode = 0x01;
                    value = 0x00;
                    break;

                case DISPLAYCONTROLMODE.OLED_CONTRAST:
                    mode = 0x02;
                    if ((value == 0xff) || !(value >= 0 || value <= 15))
                    {
                        // This is happens if the oled contrast is not properly set.
                        // we default to half.
                        value = 0x07;
                    }
                    break;

                case DISPLAYCONTROLMODE.OLED_POWER_UP:
                    mode = 0x03;
                    value = 0x01;
                    break;

                case DISPLAYCONTROLMODE.OLED_POWER_DOWN:
                    mode = 0x03;
                    value = 0x01;
                    break;
            }

            data[1] = mode;
            data[2] = value;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.DisplayControlFunctions, "DisplayControlFunctions(DISPLAYCONTROLMODE command, byte value)", 
                string.Format("command = {0}, value = {1}", command, value)));

            port.Write(data, 0, 3);

            ResponseStatus(data, COMMANDS.DisplayControlFunctions, "DisplayControlFunctions(DISPLAYCONTROLMODE command, byte value)");
        }

        public byte[] VersionDeviceInfoRequest(VERSIONOUTPUT command)
        {
            data[0] = (byte)COMMANDS.VersionDeviceInfoRequest;

            data[1] = (byte)command;

            Debug.WriteLine(PrintCommandInformation(data, COMMANDS.VersionDeviceInfoRequest, "VersionDeviceInfoRequest(VERSIONOUTPUT command)", 
                string.Format("command = {0}", command)));

            port.Write(data, 0, 2);
           
            byte[] response = new byte[10];

            port.Read(response, 0, 5);
            return response;
        }
        #endregion

        #region Micro OLED API Display Specific Command Set

        public enum SCROLLCONTROLREGISTER
        {
            SCROLLENABLEDISABLE=0x00,
            SCROLLSPEED = 0x02,
        }

        public enum SCROLLCONTROLREGISTERDATA
        {
            DISABLE = 0,
            ENABLE = 1,
            FAST = 2,
            NORMAL = 3,
            SLOW = 4
        }


        public void OLED_DisplayScrollControl(char[] asciiString, int asciiStringLength )
        {
            // TODO: finish this..
            data[0] = (byte)COMMANDS.DisplayScrollControl;

            data[1] = (byte)COMMANDS.PlaceStringOfAsciiTextUnformatted;

            for (int i = 0; i < asciiStringLength; i++)
            {
                data[2 + i] = (byte)asciiString[i];
            }

            //data[2+asciiStringLength] = 
        }

        public void OLED_DimScreenArea()
        {

        }
        #endregion

        #region Micro OLED Extended Command Set


        // Extended Command Set                         [Live] [Object] [Memory]
        //(@i) initialise uSD Memory Card                   √
        //(@R) Read Sector                                  √
        //(@W) Write Sector                                 √
        //(@r) read Byte                                    √
        //(@w) write Byte                                   √
        //(@A) Set Address                                  √
        //(@C) Copy Screen to Memory Card                   √
        //(@I) Display Image/Icon from Memory Card          √        √        √
        //(@O) Display Object from Memory Card              √
        //(@P) Run Program from Memory Card                 √
        //(07hex) Delay (in milliseconds)                                     √
        //(08hex) Set Counter                                                 √
        //(09hex) Decrement Counter                                           √
        //(0Ahex) Jump to Address if Counter not Zero                         √
        //(0Bhex) Jump to Address                                             √
        //(0Chex) Exit Program from Memory Card                      √        √

        enum EXTENDEDCOMMANDS
        {
            // This is prefixed for in the data payload for all the extended commands.
            ExtCmd = 0x40,

            InitialiseMemoryCard = 0x69,
            ReadSectorDataFromMemoryCard = 0x52,
            WriteSectorDataToMemoryCard = 0x57,
            ReadByteDataFromMemoryCard = 0x72,
            WriteByteDataToMemoryCard = 0x77,
            SetMemoryAddress = 0x41,
            CopyScreenToMemoryCard = 0x43,
            DisplayImageFromMemoryCard = 0x49,
            DisplayObjectFromMemoryCard = 0x4f,
            RunProgramFromMemoryCard = 0x50,



        }

        enum EXTENDEDCOMMANDSMEMORYCARDONLY
        {

            // These commands does not use the extended command prefix.
            Delay = 0x07,
            SetCounterOrDecrementCounter = 0x08,
            JumpToAddressIfCounterNotZero = 0x0a,
            JumpToAddress = 0x0b,
            ExitProgramFromMemoryCard = 0x0c,
        }


        public void InitialiseMicroSDMemoryCard()
        {
            data[0] = (byte)EXTENDEDCOMMANDS.ExtCmd;
            data[1] = (byte)EXTENDEDCOMMANDS.InitialiseMemoryCard;
            
            // TODO: we need to extend the printcommandinformation to take extendedcommands as a parameters.
            //Debug.WriteLine(PrintCommandInformation(data, EXTENDEDCOMMANDS.InitialiseMemoryCard, "InitialiseMicroSDMemoryCard()", ""));

            port.Write(data, 0, 2);

            // TODO: we need a different type of response status here, some of these commands only return NAK cause they will be executed
            // on the memory card side.

            resultByte[0] = (byte)RESPONSE.OLED_NAK;

            port.Read(resultByte, 0, 1);

            Debug.WriteLine(string.Format("resultByte = {0}", resultByte[0] == (byte)RESPONSE.OLED_NAK ? RESPONSE.OLED_NAK : RESPONSE.OLED_ACK)); 
        }

        public void ReadSector()
        {


        }

        public void WriteSector()
        {

        }

        public void ReadByte()
        {

        }

        public void WriteByte()
        {

        }

        public void SetAddress()
        {
        }

        public void CopyScreenToMemoryCard()
        {

        }

        public void DisplayImageOrIconFromMemoryCard()
        {

        }

        public void DisplayObjectFromMemoryCard()
        {

        }

        public void RunProgramFromMemoryCard()
        {
        }

        public void ExitProgramFromMemoryCard()
        {

        }
        #endregion 
    }
}
