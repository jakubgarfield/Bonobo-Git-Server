using System.Configuration;

namespace TSharp.Core.Mvc
{
    public class MvcCaptchaConfigSection : ConfigurationSection
    {
        [ConfigurationProperty("textLength", IsRequired = false, DefaultValue = 4)]
        public int TextLength
        {
            get
            {
                var length = (int) this["textLength"];
                return length < 3 ? 3 : length;
            }
        }

        [ConfigurationProperty("textChars", IsRequired = false, DefaultValue = "ACDEFGHJKLMNPQRSTUVWXYZ2346789")]
        public string TextChars
        {
            get
            {
                var chars = (string) this["textChars"];
                return chars.Length < 3 ? "ACDEFGHJKLMNPQRSTUVWXYZ2346789" : chars;
            }
        }

        [ConfigurationProperty("fontWarp", IsRequired = false, DefaultValue = Level.Medium)]
        public Level FontWarp
        {
            get { return (Level) this["fontWarp"]; }
        }

        [ConfigurationProperty("lineNoise", IsRequired = false, DefaultValue = Level.Low)]
        public Level LineNoise
        {
            get { return (Level) this["lineNoise"]; }
        }

        [ConfigurationProperty("backgroundNoise", IsRequired = false, DefaultValue = Level.Low)]
        public Level BackgroundNoise
        {
            get { return (Level) this["backgroundNoise"]; }
        }

        public static MvcCaptchaConfigSection GetConfig()
        {
            return ConfigurationManager.GetSection("mvcCaptchaGroup/mvcCaptchaOptions") as MvcCaptchaConfigSection;
        }
    }
}