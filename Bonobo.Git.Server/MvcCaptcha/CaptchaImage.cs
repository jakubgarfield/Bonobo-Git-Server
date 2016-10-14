using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using TSharp.Core.Mvc;
using TSharp.Core.Mvc.MvcCaptcha;

namespace TSharp.Core.Web
{
    /// <summary>
    ///     FROM:http://www.codeproject.com/KB/aspnet/CaptchaImage.aspx
    ///     Summary description for CaptchaImage.
    /// </summary>
    public class CaptchaImage : ICaptchaImageService, IDisposable
    {
        private readonly MvcCaptchaOptions _captchaOptions;
        private readonly string _UniqueId;
        // Public properties (all read-only).

        // Internal properties.
        private readonly Random random = new Random();
        private string familyName;
        private string text;

        public CaptchaImage()
            : this(new MvcCaptchaOptions())
        {
        }

        public CaptchaImage(MvcCaptchaOptions otion)
            : this(otion, "Consolas")
        {
        }

        public CaptchaImage(MvcCaptchaOptions otion, string familyName)
        {
            _UniqueId = Guid.NewGuid().ToString("N");
            _captchaOptions = otion;
            text = GenerateRandomText();
            SetDimensions(otion.Width, otion.Height);
            SetFamilyName(familyName);
        }

        /// <summary>
        ///     Gets the text.
        /// </summary>
        /// <value>The text.</value>
        protected internal override string Text
        {
            get { return text; }
            set { }
        }

        /// <summary>
        ///     Gets the image.
        /// </summary>
        /// <value>The image.</value>
        public Bitmap Image
        {
            get { return InnerGenerateImage(); }
        }

        protected internal override string UniqueId
        {
            get { return _UniqueId; }
        }

        #region IDisposable Members

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        #endregion

        /// <summary>
        ///     generate random text for the CAPTCHA
        /// </summary>
        private string GenerateRandomText()
        {
            var txtChars = _captchaOptions.TextChars;
            if (string.IsNullOrEmpty(txtChars))
                txtChars = "ACDEFGHJKLMNPQRSTUVWXYZ2346789";
            var sb = new StringBuilder(_captchaOptions.TextLength);
            var maxLength = txtChars.Length;
            for (var n = 0; n <= _captchaOptions.TextLength - 1; n++)
                sb.Append(txtChars.Substring(random.Next(maxLength), 1));

            return sb.ToString();
        }

        // ====================================================================
        // This member overrides Object.Finalize.
        // ====================================================================

        // ====================================================================
        // Releases all resources used by this object.
        // ====================================================================

        /// <summary>
        ///     Releases unmanaged resources and performs other cleanup operations before the
        ///     <see cref="CaptchaImage" /> is reclaimed by garbage collection.
        /// </summary>
        ~CaptchaImage()
        {
            Dispose(false);
        }

        // ====================================================================
        // Custom Dispose method to clean up unmanaged resources.
        // ====================================================================
        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            //if (disposing)
            // Dispose of the bitmap.
            // this.image.Dispose();
        }

        // ====================================================================
        // Sets the image width and height.
        // ====================================================================
        private void SetDimensions(int width, int height)
        {
            // Check the width and height.
            if (width <= 0)
                throw new ArgumentOutOfRangeException("width", width,
                    "Argument out of range, must be greater than zero.");
            if (height <= 0)
                throw new ArgumentOutOfRangeException("height", height,
                    "Argument out of range, must be greater than zero.");
            _captchaOptions.Width = width;
            _captchaOptions.Height = height;
        }

        // ====================================================================
        // Sets the font used for the image text.
        // ====================================================================
        private void SetFamilyName(string familyName)
        {
            // If the named font is not installed, default to a system font.
            try
            {
                var font = new Font(this.familyName, 13F);
                this.familyName = familyName;
                font.Dispose();
            }
            catch (Exception)
            {
                this.familyName = FontFamily.GenericSerif.Name;
            }
        }

        // ====================================================================
        // Creates the bitmap image.
        // ====================================================================
        private Bitmap InnerGenerateImage()
        {
            // Create a new 32-bit bitmap image.
            var bitmap = new Bitmap(_captchaOptions.Width, _captchaOptions.Height, PixelFormat.Format32bppArgb);

            // Create a graphics object for drawing.
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, _captchaOptions.Width, _captchaOptions.Height);

                // Fill in the background.
                using (var hatchBrush = new HatchBrush(HatchStyle.SmallConfetti, Color.LightGray, Color.White))
                {
                    g.FillRectangle(hatchBrush, rect);
                }

                // Draw the text.
                using (var hatchBrush = new HatchBrush(HatchStyle.LargeConfetti, Color.LightGray, Color.DarkGray))
                {
                    // Set up the text format.
                    var format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    // Create a path using the text and warp it randomly.
                    var path = new GraphicsPath();

                    {
                        // Set up the text font.
                        Font font;
                        SizeF size;
                        float fontSize = rect.Height + 1;
                        // Adjust the font size until the text fits within the image.
                        do
                        {
                            fontSize--;
                            var fontTmp = new Font(familyName, fontSize, FontStyle.Bold);
                            size = g.MeasureString(text, fontTmp);
                            if (size.Width > rect.Width)
                            {
                                font = fontTmp;
                                break;
                            }
                            fontTmp.Dispose();
                        } while (true);

                        path.AddString(text, font.FontFamily, (int) font.Style, font.Size, rect, format);
                        font.Dispose();
                    }
                    using (var matrix = new Matrix())
                    {
                        matrix.Translate(0F, 0F);

                        var v = 4F;
                        PointF[] points =
                        {
                            new PointF(random.Next(rect.Width)/v, random.Next(rect.Height)/v),
                            new PointF(rect.Width - random.Next(rect.Width)/v, random.Next(rect.Height)/v),
                            new PointF(random.Next(rect.Width)/v, rect.Height - random.Next(rect.Height)/v),
                            new PointF(rect.Width - random.Next(rect.Width)/v,
                                rect.Height - random.Next(rect.Height)/v)
                        };
                        path.Warp(points, rect, matrix, WarpMode.Perspective, 0F);
                        points = null;
                    }

                    g.FillPath(hatchBrush, path);

                    // Add some random noise.
                    var m = Math.Max(rect.Width, rect.Height);
                    for (var i = 0; i < (int) (rect.Width*rect.Height/30F); i++)
                    {
                        var x = random.Next(rect.Width);
                        var y = random.Next(rect.Height);
                        var w = random.Next(m/50);
                        var h = random.Next(m/50);
                        g.FillEllipse(hatchBrush, x, y, w, h);
                    }
                }
                g.Dispose();
            }
            // Set the image.
            return bitmap;
        }

        protected internal override void ResetText()
        {
            text = GenerateRandomText();
        }

        protected internal override Bitmap RenderImage()
        {
            return InnerGenerateImage();
        }
    }
}