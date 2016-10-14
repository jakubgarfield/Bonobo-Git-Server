using System;
using System.Drawing;

namespace TSharp.Core.Mvc.MvcCaptcha
{
    /// <summary>
    ///     验证码图片生成服务
    ///     <para>2010/8/12</para>
    ///     <para>TANGJINGBO</para>
    ///     <para>tangjingbo</para>
    /// </summary>
    [Serializable]
    public abstract class ICaptchaImageService
    {
        protected internal abstract string UniqueId { get; }
        protected internal abstract string Text { get; set; }
        protected internal abstract void ResetText();
        protected internal abstract Bitmap RenderImage();
    }
}