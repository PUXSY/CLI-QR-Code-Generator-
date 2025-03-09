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
            get => url;
            set => url = value ?? string.Empty;
        }

        private void PrintHeader()
        {
            Console.WriteLine("QR Code Generator");
            Console.WriteLine("=================");
        }

        private void PrintHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  -h, --help     : Display this help text");
            Console.WriteLine("  -u, --url URL  : Set URL for QR code");
            Console.WriteLine("  -s, --size SIZE: Set pixel size (default: 20)");
            Console.WriteLine("  -o, --output FILE: Set output filename (default: qrcode.png)");
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
                            Console.WriteLine($"URL set to: {Url}");
                        }
                        else
                        {
                            Console.WriteLine("Error: URL parameter is missing");
                            return false;
                        }
                        break;

                    case "-s":
                    case "--size":
                        if (i + 1 < args.Length && int.TryParse(args[++i], out int size))
                        {
                            PixelSize = Math.Max(10, Math.Min(size, 100));
                            Console.WriteLine($"Pixel size set to: {PixelSize}");
                        }
                        else
                        {
                            Console.WriteLine("Error: Invalid size parameter");
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
                                Console.WriteLine($"Note: Changed output filename to {OutputFile} to ensure valid image format");
                            }
                            else
                            {
                                Console.WriteLine($"Output file set to: {OutputFile}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error: Output filename is missing");
                            return false;
                        }
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(Url))
            {
                Console.WriteLine("Error: URL is required");
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

        public int PixelSize { get; private set; } = 20;
        public string OutputFile
        {
            get => outputFile;
            set => outputFile = EnsureValidImageExtension(value);
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
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
                Console.WriteLine($"Error: {ex.Message}");
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

                Console.WriteLine($"QR Code successfully saved as {outputFile}\nSaved to {Path.GetFullPath(outputFile)}");
                Console.WriteLine($"The QR code contains the URL: {url}");
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