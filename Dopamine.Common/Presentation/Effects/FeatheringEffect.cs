using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Dopamine.Common.Presentation.Utils;

namespace Dopamine.Common.Presentation.Effects
{
    public class FeatheringEffect : ShaderEffect
    {
        #region Variables
        private static PixelShader shader = new PixelShader()
        {
            UriSource = UriUtils.MakePackUri<FeatheringEffect>("Presentation/Effects/FeatheringEffect.ps")
        };
        #endregion

        #region Properties
        public Brush InputBackground
        {
            get => (Brush)GetValue(InputBackgroundProperty);
            set => SetValue(InputBackgroundProperty, value);
        }

        public double FeatheringRadius
        {
            get => (double) GetValue(FeatheringRadiusProperty);
            set => SetValue(FeatheringRadiusProperty, value);
        }

        public double TexWidth
        {
            get => (double)GetValue(TexWidthProperty);
            set => SetValue(TexWidthProperty, value);
        }

        public double TexHeight
        {
            get => (double)GetValue(TexHeightProperty);
            set => SetValue(TexHeightProperty, value);
        }
        #endregion

        #region Dependency Properties
        public static DependencyProperty InputBackgroundProperty =
            RegisterPixelShaderSamplerProperty("InputBackground", typeof(FeatheringEffect), 0);

        public static DependencyProperty FeatheringRadiusProperty = DependencyProperty.Register("FeatheringRadius",
            typeof(double), typeof(FeatheringEffect), new UIPropertyMetadata(0d, PixelShaderConstantCallback(0)));

        public static DependencyProperty TexWidthProperty = DependencyProperty.Register("TexWidth", typeof(double),
            typeof(FeatheringEffect), new UIPropertyMetadata(100d, PixelShaderConstantCallback(1)));

        public static DependencyProperty TexHeightProperty = DependencyProperty.Register("TexHeight", typeof(double),
            typeof(FeatheringEffect), new UIPropertyMetadata(100d, PixelShaderConstantCallback(1)));
        #endregion

        #region Constructor
        public FeatheringEffect()
        {
            PixelShader = shader;
            UpdateShaderValue(InputBackgroundProperty);
            UpdateShaderValue(FeatheringRadiusProperty);
            UpdateShaderValue(TexWidthProperty);
            UpdateShaderValue(TexHeightProperty);
        }
        #endregion
    }
}