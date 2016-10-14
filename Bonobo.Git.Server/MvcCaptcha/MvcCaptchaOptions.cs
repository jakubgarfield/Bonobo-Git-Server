using System;

namespace TSharp.Core.Mvc
{
    public class MvcCaptchaOptions
    {
        #region constructor

        public MvcCaptchaOptions()
        {
            FontWarp = Level.High;
            BackgroundNoise = Level.High;
            LineNoise = Level.High;
            ReloadLinkText = CaptchaResource.RefreshImage;
            Width = 160;
            Height = 40;
            TextLength = 4;
        }

        #endregion

        #region private fields

        private string _chars;
        private int _height;
        private int _length;
        private int _width;

        #endregion

        #region public properties

        private string _captchaImageContainerId;
        private string _inputBoxId;

        /// <summary>
        ///     验证字符长度（字符个数）
        /// </summary>
        public int TextLength
        {
            get { return _length; }
            set { _length = value < 3 ? 3 : value; }
        }

        /// <summary>
        ///     生成验证码用的字符
        /// </summary>
        public string TextChars
        {
            get { return _chars; }
            set
            {
                _chars = string.IsNullOrEmpty(value) || (value.Trim().Length < 3)
                    ? "ACDEFGHJKLMNPQRSTUVWXYZ2346789"
                    : value.Trim();
            }
        }

        /// <summary>
        ///     Font warp factor
        /// </summary>
        public Level FontWarp { get; set; }

        /// <summary>
        ///     Background Noise level
        /// </summary>
        public Level BackgroundNoise { get; set; }

        /// <summary>
        ///     线条杂色级别
        /// </summary>
        public Level LineNoise { get; set; }


        /// <summary>
        ///     Width of captcha image
        /// </summary>
        public int Width
        {
            get { return _width; }
            set { _width = value < TextLength*18 ? TextLength*18 : value; }
        }

        /// <summary>
        ///     Height of captcha image
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { _height = value < 32 ? 32 : value; }
        }

        /// <summary>
        ///     客户端验证码输入文本框的Id
        /// </summary>
        public string ValidationInputBoxId
        {
            get
            {
                if (DelayLoad && string.IsNullOrEmpty(_inputBoxId))
                    throw new ArgumentNullException("ValidationInputBoxId",
                        "设置DelayLoad为true时必须指定ValidationInputBoxId的值");
                return _inputBoxId;
            }
            set { _inputBoxId = value; }
        }

        public string CaptchaImageContainerId
        {
            get
            {
                if (DelayLoad && string.IsNullOrEmpty(_captchaImageContainerId))
                    throw new ArgumentNullException("CaptchaImageContainerId",
                        "设置DelayLoad为true时必须指定CaptchaImageContainerId的值");
                return _captchaImageContainerId;
            }
            set { _captchaImageContainerId = value; }
        }

        public string ReloadLinkText { get; set; }

        /// <summary>
        ///     是否延迟加载（验证文本框获得焦点时才生成并加载验证图片）
        /// </summary>
        public bool DelayLoad { get; set; }

        #endregion
    }
}