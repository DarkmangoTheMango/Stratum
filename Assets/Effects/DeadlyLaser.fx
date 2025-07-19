float4 uSourceRect;
float uTime;

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
    float3(0.8, 0.2, 0.0),
    float3(0.8, 0.2, 0.0),
    float3(1.0, 0.8, 0.0),
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
    coords.x *= 0.5;

    float4 texColor = tex2D(uImage0, coords - float2(uTime, 0));

    float luminance = texColor.r;
    
    float centerYFactor = 1.0 - abs(coords.y * 2.0 - 1.0);
    
    float boost = smoothstep(0.0, 1.2, centerYFactor) - smoothstep(0.25, 0.0, centerYFactor);
    luminance += boost;
    
    luminance = saturate(luminance);

    float3 gradientColor = GradientMap(luminance);
    
    float alpha = smoothstep(0.1, 0.2, centerYFactor);

    return float4(gradientColor * alpha, texColor.a * alpha);
}

Technique techique1
{
    pass DeadlyLaser
    {
        PixelShader = compile ps_2_0 Main();
    }
}