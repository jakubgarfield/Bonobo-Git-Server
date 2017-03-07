using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;
using System.Web;
using TSharp.Core.Mvc.MvcCaptcha;

namespace TSharp.Core.Mvc
{
    /// <summary>
    ///     CAPTCHA Image
    /// </summary>
    /// <seealso href="http://www.codinghorror.com">Original By Jeff Atwood</seealso>
    [Serializable]
    internal class MvcCaptchaImage : ICaptchaImageService
    {
        #region Static

        private static readonly string[] RandomFontFamily =
            {
                "arial", "arial black", "comic sans ms", "courier new",
                "estrangelo edessa", "franklin gothic medium", "georgia",
                "lucida console", "lucida sans unicode", "mangal",
                "microsoft sans serif", "palatino linotype", "sylfaen",
                "tahoma", "times new roman", "trebuchet ms", "verdana"
            };

        private static readonly Color[] RandomColor =
            {
                Color.Red, Color.Green, Color.Blue, Color.Black, Color.Purple,
                Color.Orange
            };

        /// <summary>
        ///     Gets the cached captcha.
        /// </summary>
        public static ICaptchaImageService GetCachedCaptcha(string guid)
        {
            if (String.IsNullOrEmpty(guid))
                return null;
            var img = (ICaptchaImageService)HttpContext.Current.Session[guid];
            return img;
        }

        #endregion

        private readonly Random _rand;
        private string _UniqueId;

        #region Public Properties

        /// <summary>
        ///     Returns a GUID that uniquely identifies this Captcha
        /// </summary>
        /// <value>The unique id.</value>
        override internal protected string UniqueId
        {
            get { return _UniqueId; }
        }

        /// <summary>
        ///     Gets the randomly generated Captcha text.
        /// </summary>
        /// <value>The text.</value>
        override internal protected string Text { get; set; }

        public MvcCaptchaOptions CaptchaOptions { get; set; }

        #endregion

        internal MvcCaptchaImage()
            : this(new MvcCaptchaOptions())
        {
        }

        internal MvcCaptchaImage(MvcCaptchaOptions options)
        {
            CaptchaOptions = options;
            _UniqueId = Guid.NewGuid().ToString("N");
            _rand = new Random();
            //Text = GenerateRandomText();
        }

        override internal protected void ResetText()
        {
            Text = GenerateRandomText();
        }

        /// <summary>
        ///     Returns a random font family from the font whitelist
        /// </summary>
        private string GetRandomFontFamily()
        {
            return RandomFontFamily[_rand.Next(0, RandomFontFamily.Length)];
        }

        /// <summary>
        ///     generate random text for the CAPTCHA
        /// </summary>
        private string GenerateRandomText()
        {
            string txtChars = CaptchaOptions.TextChars;
            if (string.IsNullOrEmpty(txtChars))
                txtChars = "ACDEFGHJKLMNPQRSTUVWXYZ2346789";
            var sb = new StringBuilder(CaptchaOptions.TextLength);
            int maxLength = txtChars.Length;
            for (int n = 0; n <= CaptchaOptions.TextLength - 1; n++)
                sb.Append(txtChars.Substring(_rand.Next(maxLength), 1));

            return sb.ToString();
        }

        /// <summary>
        ///     Returns a random point within the specified x and y ranges
        /// </summary>
        private PointF RandomPoint(int xmin, int xmax, int ymin, int ymax)
        {
            return new PointF(_rand.Next(xmin, xmax), _rand.Next(ymin, ymax));
        }

        /// <summary>
        ///     Get Random color.
        /// </summary>
        private Color GetRandomColor()
        {
            return RandomColor[_rand.Next(0, RandomColor.Length)];
        }

        /// <summary>
        ///     Returns a random point within the specified rectangle
        /// </summary>
        private PointF RandomPoint(Rectangle rect)
        {
            return RandomPoint(rect.Left, rect.Width, rect.Top, rect.Bottom);
        }

        /// <summary>
        ///     Returns a GraphicsPath containing the specified string and font
        /// </summary>
        private static GraphicsPath TextPath(string s, Font f, Rectangle r)
        {
            var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
            var gp = new GraphicsPath();
            gp.AddString(s, f.FontFamily, (int)f.Style, f.Size, r, sf);
            return gp;
        }

        /// <summary>
        ///     Returns the CAPTCHA font in an appropriate size
        /// </summary>
        private Font GetFont()
        {
            float fsize;
            string fname = GetRandomFontFamily();

            switch (CaptchaOptions.FontWarp)
            {
                case Level.Low:
                    fsize = Convert.ToInt32(CaptchaOptions.Height * 0.8);
                    break;
                case Level.Medium:
                    fsize = Convert.ToInt32(CaptchaOptions.Height * 0.85);
                    break;
                case Level.High:
                    fsize = Convert.ToInt32(CaptchaOptions.Height * 0.9);
                    break;
                case Level.Extreme:
                    fsize = Convert.ToInt32(CaptchaOptions.Height * 0.95);
                    break;
                default:
                    fsize = Convert.ToInt32(CaptchaOptions.Height * 0.7);
                    break;
            }
            return new Font(fname, fsize, FontStyle.Bold);
        }

        /// <summary>
        ///     Renders the CAPTCHA image
        /// </summary>
        override internal protected Bitmap RenderImage()
        {
            var bmp = new Bitmap(CaptchaOptions.Width, CaptchaOptions.Height, PixelFormat.Format24bppRgb);

            using (Graphics gr = Graphics.FromImage(bmp))
            {
                gr.SmoothingMode = SmoothingMode.AntiAlias;
                gr.Clear(Color.White);

                int charOffset = 0;
                double charWidth = CaptchaOptions.Width / CaptchaOptions.TextLength;
                Rectangle rectChar;

                foreach (char c in Text)
                {
                    // establish font and draw area 
                    using (Font fnt = GetFont())
                    {
                        using (Brush fontBrush = new SolidBrush(GetRandomColor()))
                        {
                            rectChar = new Rectangle(Convert.ToInt32(charOffset * charWidth), 0,
                                                     Convert.ToInt32(charWidth), CaptchaOptions.Height);

                            // warp the character 
                            GraphicsPath gp = TextPath(c.ToString(), fnt, rectChar);
                            WarpText(gp, rectChar);
                            // draw the character 
                            gr.FillPath(fontBrush, gp);
                            charOffset += 1;
                        }
                    }
                }

                var rect = new Rectangle(new Point(0, 0), bmp.Size);
                AddNoise(gr, rect);
                AddLine(gr, rect);
            }
            return bmp;
        }

        /// <summary>
        ///     Warp the provided text GraphicsPath by a variable amount
        /// </summary>
        /// <param name="textPath">The text path.</param>
        /// <param name="rect">The rect.</param>
        private void WarpText(GraphicsPath textPath, Rectangle rect)
        {
            float warpDivisor;
            float rangeModifier;

            switch (CaptchaOptions.FontWarp)
            {
                case Level.Low:
                    warpDivisor = 6F;
                    rangeModifier = 1F;
                    break;
                case Level.Medium:
                    warpDivisor = 5F;
                    rangeModifier = 1.3F;
                    break;
                case Level.High:
                    warpDivisor = 4.5F;
                    rangeModifier = 1.4F;
                    break;
                case Level.Extreme:
                    warpDivisor = 4F;
                    rangeModifier = 1.5F;
                    break;
                default:
                    return;
            }

            var rectF = new RectangleF(Convert.ToSingle(rect.Left), 0, Convert.ToSingle(rect.Width), rect.Height);

            int hrange = Convert.ToInt32(rect.Height / warpDivisor);
            int wrange = Convert.ToInt32(rect.Width / warpDivisor);
            int left = rect.Left - Convert.ToInt32(wrange * rangeModifier);
            int top = rect.Top - Convert.ToInt32(hrange * rangeModifier);
            int width = rect.Left + rect.Width + Convert.ToInt32(wrange * rangeModifier);
            int height = rect.Top + rect.Height + Convert.ToInt32(hrange * rangeModifier);

            if (left < 0)
                left = 0;
            if (top < 0)
                top = 0;
            if (width > CaptchaOptions.Width)
                width = CaptchaOptions.Width;
            if (height > CaptchaOptions.Height)
                height = CaptchaOptions.Height;

            PointF leftTop = RandomPoint(left, left + wrange, top, top + hrange);
            PointF rightTop = RandomPoint(width - wrange, width, top, top + hrange);
            PointF leftBottom = RandomPoint(left, left + wrange, height - hrange, height);
            PointF rightBottom = RandomPoint(width - wrange, width, height - hrange, height);

            var points = new[] { leftTop, rightTop, leftBottom, rightBottom };
            var m = new Matrix();
            m.Translate(0, 0);
            textPath.Warp(points, rectF, m, WarpMode.Perspective, 0);
        }


        /// <summary>
        ///     Add a variable level of graphic noise to the image
        /// </summary>
        private void AddNoise(Graphics g, Rectangle rect)
        {
            int density;
            int size;

            switch (CaptchaOptions.BackgroundNoise)
            {
                case Level.None:
                    goto default;
                case Level.Low:
                    density = 30;
                    size = 40;
                    break;
                case Level.Medium:
                    density = 18;
                    size = 40;
                    break;
                case Level.High:
                    density = 16;
                    size = 39;
                    break;
                case Level.Extreme:
                    density = 12;
                    size = 38;
                    break;
                default:
                    return;
            }
            var br = new SolidBrush(GetRandomColor());
            int max = Convert.ToInt32(Math.Max(rect.Width, rect.Height) / size);
            for (int i = 0; i <= Convert.ToInt32((rect.Width * rect.Height) / density); i++)
                g.FillEllipse(br, _rand.Next(rect.Width), _rand.Next(rect.Height), _rand.Next(max), _rand.Next(max));
            br.Dispose();
        }

        /// <summary>
        ///     Add variable level of curved lines to the image
        /// </summary>
        private void AddLine(Graphics g, Rectangle rect)
        {
            int length;
            float width;
            int linecount;

            switch (CaptchaOptions.LineNoise)
            {
                case Level.None:
                    goto default;
                case Level.Low:
                    length = 4;
                    width = Convert.ToSingle(CaptchaOptions.Height / 31.25);
                    linecount = 1;
                    break;
                case Level.Medium:
                    length = 5;
                    width = Convert.ToSingle(CaptchaOptions.Height / 27.7777);
                    linecount = 1;
                    break;
                case Level.High:
                    length = 3;
                    width = Convert.ToSingle(CaptchaOptions.Height / 25);
                    linecount = 2;
                    break;
                case Level.Extreme:
                    length = 3;
                    width = Convert.ToSingle(CaptchaOptions.Height / 22.7272);
                    linecount = 3;
                    break;
                default:
                    return;
            }

            var pf = new PointF[length + 1];
            using (var p = new Pen(GetRandomColor(), width))
            {
                for (int l = 1; l <= linecount; l++)
                {
                    for (int i = 0; i <= length; i++)
                        pf[i] = RandomPoint(rect);

                    g.DrawCurve(p, pf, 1.75F);
                }
            }
        }
    }
}