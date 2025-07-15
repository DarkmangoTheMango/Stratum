float uTime;

float alpha;

float4 uSourceRect;

texture2D tex;

sampler2D uImage0 = sampler_state
{
    Texture = <tex>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

static const int NUM_STOPS = 4;

static const float stops[NUM_STOPS] =
{
    0.0,
    0.25,
    0.5,
    0.75
};

static const float3 colors[NUM_STOPS] =
{
    float3(0.0, 0.0, 0.1),
    float3(0.0, 0.2, 0.8),
    float3(0.0, 0.8, 1.0),
    float3(1.0, 1.0, 1.0)
};

float3 GradientMap(float t)
{
    t = saturate(t);
    
    if (t <= stops[0])
        return colors[0];
    if (t >= stops[NUM_STOPS - 1])
        return colors[NUM_STOPS - 1];
    
    for (int i = 0; i < NUM_STOPS - 1; i++)
    {
        if (t >= stops[i] && t < stops[i + 1])
        {
            float f = (t - stops[i]) / (stops[i + 1] - stops[i]);
            return lerp(colors[i], colors[i + 1], f);
        }
    }
    
    return colors[NUM_STOPS - 1];
}

float4 Main(float2 coords : TEXCOORD0) : COLOR0
{
    float2 scroll1 = float2(uTime * 0.2, 0.0);
    float2 scroll2 = float2(uTime * 0.1, 0.0);
    
    float2 uv1 = frac(coords * 0.5 + scroll1);
    float2 uv2 = frac((1.0 - coords) + scroll2);

    float4 texture1 = tex2D(uImage0, uv1);
    float4 texture2 = tex2D(uImage0, uv2);

    float4 combined = texture1 * texture2;

    float luminance = dot(combined.rgb, float3(0.299, 0.587, 0.114));
    float3 gradientColor = GradientMap(luminance);

    float2 center = float2(0.5, 0.5);
    float dist = distance(coords, center);
    float vignette = smoothstep(0.7, 0.0, dist);

    float wave = 0.5f + 0.5f * cos(uTime + coords.y * 5);
    float3 finalColor = gradientColor * vignette * float3(1.0 * wave, 0.0, 1.0);
    
    return float4(finalColor * alpha, combined.a * alpha);
}

Technique technique1
{
    pass CelestialSky
    {
        PixelShader = compile ps_3_0 Main();
    }
}