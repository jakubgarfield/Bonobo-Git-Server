using System;
using System.Drawing;
using System.Web;

namespace TSharp.Core.Mvc.MvcCaptcha
{
    /// <summary>
    /// 验证码图片生成服务
    /// <para>2010/8/12</para>
    /// 	<para>TANGJINGBO</para>
    /// 	<para>tangjingbo</para>
    /// </summary>
    [Serializable]
    public abstract class ICaptchaImageService
    {
        internal protected abstract string UniqueId { get; }
        internal protected abstract void ResetText();
        internal protected abstract string Text { get; set; }
        internal protected abstract Bitmap RenderImage();
    }
}