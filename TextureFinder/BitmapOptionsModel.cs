using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing.Dds;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TextureFinder
{
    public enum FormatMode
    {
        DxgiFormat,
        XboxFormat,
        ImageFormat
    }

    public class BitmapOptionsModel : BindableBase
    {
        public IReadOnlyList<FormatMode> AvailableFormatModes => availableFormatModes;
        private readonly List<FormatMode> availableFormatModes = new List<FormatMode> { FormatMode.DxgiFormat, FormatMode.XboxFormat, FormatMode.ImageFormat };

        public IReadOnlyList<DxgiFormat> AvailableDxgiFormats => availableDxgiFormats;
        private readonly List<DxgiFormat> availableDxgiFormats = new List<DxgiFormat>
        {
            DxgiFormat.BC1_UNorm,
            DxgiFormat.BC2_UNorm,
            DxgiFormat.BC3_UNorm,
            DxgiFormat.BC4_UNorm,
            DxgiFormat.BC5_UNorm,
            DxgiFormat.BC5_SNorm,
            DxgiFormat.BC7_UNorm,
            DxgiFormat.B4G4R4A4_UNorm,
            DxgiFormat.B5G6R5_UNorm,
            DxgiFormat.B5G5R5A1_UNorm,
            DxgiFormat.B8G8R8A8_UNorm,
            DxgiFormat.P8
        };

        public IReadOnlyList<XboxFormat> AvailableXboxFormats => availableXboxFormats;
        private readonly List<XboxFormat> availableXboxFormats = new List<XboxFormat>
        {
            XboxFormat.A8,
            XboxFormat.AY8,
            XboxFormat.CTX1,
            XboxFormat.DXN,
            XboxFormat.DXN_mono_alpha,
            XboxFormat.DXN_SNorm,
            XboxFormat.DXT3a_scalar,
            XboxFormat.DXT3a_mono,
            XboxFormat.DXT3a_alpha,
            XboxFormat.DXT5a_scalar,
            XboxFormat.DXT5a_mono,
            XboxFormat.DXT5a_alpha,
            XboxFormat.Y8,
            XboxFormat.Y8A8,
        };

        //public IReadOnlyList<ImageFormat> AvailableImageFormats => availableImageFormats;
        //private List<ImageFormat> availableImageFormats = new List<ImageFormat>
        //{
        //    ImageFormat.Jpeg,
        //    ImageFormat.Png,
        //    ImageFormat.Tiff
        //};

        public Visibility DxgiVisibility => SelectedFormatMode == FormatMode.DxgiFormat ? Visibility.Visible : Visibility.Collapsed;
        public Visibility XboxVisibility => SelectedFormatMode == FormatMode.XboxFormat ? Visibility.Visible : Visibility.Collapsed;

        private double zoomLevel;
        public double ZoomLevel
        {
            get => zoomLevel;
            set => SetProperty(ref zoomLevel, value);
        }

        private FileStream fileStream;

        private string fileName;
        public string FileName
        {
            get => fileName == null ? null : Path.GetFileName(fileName);
            set
            {
                if (SetProperty(ref fileName, value))
                {
                    fileStream?.Dispose();
                    fileStream = File.OpenRead(value);
                    UpdateImageSource();
                    ZoomLevel = 1;
                }
            }
        }

        private FormatMode selectedFormatMode;
        public FormatMode SelectedFormatMode
        {
            get => selectedFormatMode;
            set
            {
                if (SetProperty(ref selectedFormatMode, value))
                {
                    RaisePropertyChanged(nameof(DxgiVisibility));
                    RaisePropertyChanged(nameof(XboxVisibility));
                    UpdateImageSource();
                }
            }
        }

        private DxgiFormat selectedDxgiFormat;
        public DxgiFormat SelectedDxgiFormat
        {
            get => selectedDxgiFormat;
            set
            {
                if (SetProperty(ref selectedDxgiFormat, value))
                    UpdateImageSource();
            }
        }

        private XboxFormat selectedXboxFormat;
        public XboxFormat SelectedXboxFormat
        {
            get => selectedXboxFormat;
            set
            {
                if (SetProperty(ref selectedXboxFormat, value))
                    UpdateImageSource();
            }
        }

        private int offsetStep = 1;
        public int OffsetStep
        {
            get => offsetStep;
            set
            {
                if (SetProperty(ref offsetStep, value))
                    UpdateImageSource();
            }
        }

        private int startAddress;
        public int StartAddress
        {
            get => startAddress;
            set
            {
                if (SetProperty(ref startAddress, value))
                    UpdateImageSource();
            }
        }

        private int offset;
        public int Offset
        {
            get => offset;
            set
            {
                if (SetProperty(ref offset, value))
                    UpdateImageSource();
            }
        }

        private int width;
        public int Width
        {
            get => width;
            set
            {
                if (SetProperty(ref width, value))
                    UpdateImageSource();
            }
        }

        private int height;
        public int Height
        {
            get => height;
            set
            {
                if (SetProperty(ref height, value))
                    UpdateImageSource();
            }
        }

        private bool deflate;
        public bool Deflate
        {
            get => deflate;
            set
            {
                if (SetProperty(ref deflate, value))
                    UpdateImageSource();
            }
        }

        private bool zlib;
        public bool Zlib
        {
            get => zlib;
            set
            {
                if (SetProperty(ref zlib, value))
                    UpdateImageSource();
            }
        }

        private ImageSource imageSource;
        public ImageSource ImageSource
        {
            get => imageSource;
            private set => SetProperty(ref imageSource, value);
        }

        private int MaxDataLength => Width * Height * 4 + 0x800;

        public BitmapOptionsModel()
        {
            SelectedFormatMode = AvailableFormatModes.First();
            SelectedDxgiFormat = AvailableDxgiFormats.First();
            SelectedXboxFormat = AvailableXboxFormats.First();
            Width = Height = 512;
        }

        private void UpdateImageSource()
        {
            if (fileStream == null)
                return;

            try
            {
                if (Deflate || Zlib)
                    ReadCompressed();
                else
                    ReadUncompressed();
            }
            catch { ImageSource = null; }

            void ReadUncompressed()
            {
                fileStream.Position = StartAddress + Offset;
                using (var br = new BinaryReader(fileStream, Encoding.UTF8, true))
                {
                    var data = br.ReadBytes((int)Math.Min(fileStream.Length - fileStream.Position, MaxDataLength));
                    LoadFromBytes(data);
                }
            }

            void ReadCompressed()
            {
                fileStream.Position = StartAddress;
                var ds = Deflate
                    ? (Stream)new DeflateStream(fileStream, CompressionMode.Decompress, true)
                    : new Ionic.Zlib.ZlibStream(fileStream, Ionic.Zlib.CompressionMode.Decompress, true);

                using (ds)
                using (var br = new BinaryReader(ds))
                {
                    var temp = new byte[0x8000];
                    var bytesToRead = Offset;
                    while (bytesToRead > 0)
                    {
                        var progress = br.Read(temp, 0, temp.Length);
                        if (progress == 0)
                            break;
                        bytesToRead -= progress;
                    }

                    var data = new byte[MaxDataLength];
                    bytesToRead = (int)Math.Min(fileStream.Length - fileStream.Position, data.Length);
                    br.Read(data, 0, bytesToRead);

                    LoadFromBytes(data);
                }
            }

            void LoadFromBytes(byte[] data)
            {
                if (SelectedFormatMode == FormatMode.ImageFormat)
                {
                    using (var ms = new MemoryStream(data))
                    using (var image = System.Drawing.Image.FromStream(ms))
                    using (var bitmap = new System.Drawing.Bitmap(image))
                    {
                        ImageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          bitmap.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
                    }
                }
                else
                {
                    var dds = SelectedFormatMode == FormatMode.DxgiFormat
                        ? new DdsImage(height, width, SelectedDxgiFormat, data)
                        : new DdsImage(height, width, SelectedXboxFormat, data);

                    ImageSource = dds.ToBitmapSource(new DdsOutputArgs { Options = DecompressOptions.Bgr24 });
                }
            }
        }
    }
}
