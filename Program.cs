using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using QRCoder;

namespace QRcode
{


    public class UI
    {
        private readonly string[] args;
        private string url;
        private string outputFile = "qrcode.png";

        public UI(string[] args)
        {
            this.args = args ?? throw new ArgumentNullException(nameof(args));
            this.url = string.Empty;
        }

        public string Url
        {
            get => this.url;
            set => this.url = value ?? string.Empty;
        }

        public int PixelSize { get; private set; } = 20;
        public string OutputFile
        {
            get => outputFile;
            set => outputFile = EnsureValidImageExtension(value);
        }

        private void PrintHeader()
        {
            Console.WriteLine("QR Code Generator");
            Console.WriteLine("=------=+=------=");
        }

        private void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  -h, --help:    Display this help text");
            Console.WriteLine("  -u, --url:     Set URL for QR code");
            Console.WriteLine("  -s, --size:    Set pixel size (default: 20)");
            Console.WriteLine("  -o, --output:  Set output filename (default: 'qrcode.png' in current directory)");
        }

        public static void PrintRedText(string text)
        {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.White;
        }
        public static void PrintGreenText(string text)
        {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.White;
        }

        public bool Parse()
        {
            PrintHeader();

            if (args.Length == 0)
            {
                PrintHelp();
                return false;
            }

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-h":
                    case "--help":
                        PrintHelp();
                        return false;

                    case "-u":
                    case "--url":
                        if (i + 1 < args.Length)
                        {
                            Url = args[++i];
                            PrintGreenText($"URL set to: {Url}");
                        }
                        else
                        {
                            PrintRedText("Error: URL parameter is missing");
                            return false;
                        }
                        break;

                    case "-s":
                    case "--size":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int size))
                        {
                            PixelSize = Math.Max(10, Math.Min(size, 100));
                            PrintGreenText($"Pixel size set to: {PixelSize}");
                        }
                        else
                        {
                            PrintRedText("Error: Invalid size parameter");
                            return false;
                        }
                        break;

                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length)
                        {
                            string originalName = args[++i];
                            OutputFile = originalName; 

                            if (OutputFile != originalName)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Note: Changed output filename to {OutputFile} to ensure valid image format");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else
                            {
                                PrintGreenText($"Output file set to: {OutputFile}");
                            }
                        }
                        else
                        {
                            PrintRedText("Error: Output filename is missing");
                            return false;
                        }
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(Url))
            {
                PrintRedText("Error: URL is required");
                return false;
            }

            return true;
        }

        private string EnsureValidImageExtension(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return "qrcode.png";

            string extension = Path.GetExtension(filename);
            bool isValidExtension = extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                                   extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase);

            if (!isValidExtension)
            {
               
                return Path.ChangeExtension(filename, ".png");
            }

            return filename; 
        }


    }

    public class Program
    {

        public static void PrintRedText(string text)
        {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.White;
        }
        public static void PrintGreenText(string text)
        {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.White;
        }

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            try
            {
                var ui = new UI(args);
                if (!ui.Parse())
                {
                    return;
                }

                GenerateQRCode(ui.Url, ui.PixelSize, ui.OutputFile);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                PrintRedText($"Error: {ex.Message}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static void GenerateQRCode(string url, int pixelSize, string outputFile)
        {
            Console.WriteLine("Generating QR code...");

            QRCoder.QRCodeGenerator qrGenerator = null;
            QRCoder.QRCode qrCode = null;
            Bitmap qrImage = null;

            try
            {
                qrGenerator = new QRCoder.QRCodeGenerator();
                var qrData = qrGenerator.CreateQrCode(url, QRCoder.QRCodeGenerator.ECCLevel.Q);
                qrCode = new QRCoder.QRCode(qrData);
                qrImage = qrCode.GetGraphic(pixelSize);

                string directory = Path.GetDirectoryName(outputFile);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                qrImage.Save(outputFile, ImageFormat.Png);
                Console.ForegroundColor = ConsoleColor.Green;
                PrintGreenText($"QR Code successfully saved as {outputFile}\nSaved to {Path.GetFullPath(outputFile)}");
                PrintGreenText($"The QR code contains the URL: {url}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            finally
            {
                if (qrImage != null)
                    qrImage.Dispose();
                if (qrCode != null)
                    qrCode.Dispose();
                if (qrGenerator != null)
                    qrGenerator.Dispose();
            }
        }
    }
}