float uTime;

texture2D tex;

float4 uSourceRect;

sampler2D uImage0 = sampler_state
{
    Texture = <tex>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

static const float stops[4] = { 0.0, 0.25, 0.5, 0.75 };

static const float3 colors[4] = {
    float3(0.0, 0.0, 0.0),
    float3(0.0, 0.2, 0.8),
    float3(0.0, 0.8, 1.0),
    float3(1.0, 1.0, 1.0)
};

float3 GradientMap(float t)
{
    t = saturate(t);
    
    int i = 0;
    
    for (; i < 4 - 1; i++)
        if (t < stops[i + 1])
            break;
            
    float f = (t - stops[i]) / (stops[i + 1] - stops[i]);
    return lerp(colors[i], colors[i + 1], f);
}

float4 Main(float2 coords : TEXCOORD0) : COLOR0
{
    float2 centeredCoords = coords * 2.0 - 1.0;
    
    if (length(centeredCoords) > 1)
    	return float4(0, 0, 0, 0);
    	
    	coords *= 0.5;
    
    float4 color = tex2D(uImage0, coords + float2(uTime, 0));
    
    float pulse = (sin(uTime) + 2.0) / 4;
    
    float luminance = color.rgb;
    float3 gradient = GradientMap(luminance * pulse);
	
    return float4(gradient * pulse, color.a * pulse);
}

Technique technique1
{
    pass Shield
    {
        PixelShader = compile ps_2_0 Main();
    }
}