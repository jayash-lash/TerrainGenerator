#ifndef CUSTOM_LIT_INCLUDED
#define CUSTOM_LIT_INCLUDED

half _TextureScale;

half my_fmod(const float2 a, const float2 b)
{
    half2 c = frac(abs(a / b)) * abs(b);
    return c;
}

void CustomFragmentPass(Varyings input, out half4 outColor : SV_Target0
            #ifdef _WRITE_RENDERING_LAYERS
                , out float4 outRenderingLayers : SV_Target1
            #endif
            )
{
    float x = input.positionWS.x * _TextureScale;
    float y = (input.positionWS.y - 0) * _TextureScale; // change value to scroll texture offset
    float z = input.positionWS.z * _TextureScale;
    float isUp = abs(input.normalWS.y);
            
    float2 offset = float2(my_fmod(z + x * (1 -isUp),0.25), my_fmod(y + x * isUp, 0.25));
    input.uv += offset;
                
    LitPassFragment(input, outColor
#ifdef _WRITE_RENDERING_LAYERS
        , out outRenderingLayers
#endif
    );
}

#endif
