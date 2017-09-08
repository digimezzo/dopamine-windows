//ps_3_0

sampler2D implicitInputBackground : register(s0);
float implicitfeather : register(c0);
float implicitwidth : register(c1);
float implicitheight : register(c2);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 colorSample = tex2D(implicitInputBackground, uv);
    float width = uv[0] * implicitwidth;
    float feather = implicitfeather;
    if (width < feather)
    {
        colorSample *= width / feather;
    }
    if (width > implicitwidth - feather)
    {
        colorSample *= (implicitwidth - width) / feather;
    }

    return colorSample;
}

