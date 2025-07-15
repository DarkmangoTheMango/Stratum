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

// hardcoded stuff

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
    float3(0.1, 0.0, 0.0),
    float3(0.8, 0.2, 0.0),
    float3(1.0, 0.8, 0.0),
    float3(1.0, 1.0, 1.0)
};

// shitty gradient map

float3 GradientMap(float t)
{
    t = saturate(t);
    
    if (t <= stops[0])
        return colors[0];
    if (t >= stops[NUM_STOPS - 1])
        return colors[NUM_STOPS - 1];
    
    // idk what this does
    
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

// my brain hurt

float4 Main(float2 coords : TEXCOORD0) : COLOR0
{
    float2 centeredUV = coords * 2.0 - 1.0;
    
    float radius = length(centeredUV); // inside da circle
    float edging = smoothstep(1.0, 0.95, radius); // outside da circle
    
    // sphere math things
    
    float z = sqrt(1.2 - radius * radius);
    float3 sphereNormal = float3(centeredUV, z);
    
    float2 sphereUV;
    sphereUV.x = atan2(sphereNormal.x, sphereNormal.z) / 3.141;
    sphereUV.y = acos(sphereNormal.y) / 3.141;
    
    sphereUV.x += uTime * 0.2;
    
    // fucked up warping code
    
    float2 warpSampleUV = sphereUV + uTime * 0.1;
    float2 warp = tex2D(uImage0, warpSampleUV).rg - 0.5;
    warp *= 0.15; // warping intensity

    float2 finalUV = sphereUV + warp;
    float4 texColor = tex2D(uImage0, finalUV);
    
    // color stuff
    
    float luminance = texColor.x;
    float3 gradientColor = GradientMap(luminance);
    
    float ringGlow = smoothstep(0.7, 1.0, radius);
    float3 ringColor = colors[1];
    
    float ringGlow2 = smoothstep(0.9, 1.0, radius);
    float3 ringColor2 = colors[2];
    
    gradientColor += ringColor * ringGlow;
    gradientColor += ringColor2 * ringGlow2;
    
    return float4(gradientColor * edging, texColor.a * edging);
}

Technique technique1
{
    pass BurningStarShader
    {
        PixelShader = compile ps_3_0 Main();
    }
}