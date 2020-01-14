using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.HighDefinition
{
    [GenerateHLSL]
    internal enum StencilBeforeTransparent
    {
        Clear = 0,

        RequiresDeferredLighting    = (1 << 1),
        SubsurfaceScattering        = (1 << 2),     //  SSS, Split Lighting
        TraceReflectionRay          = (1 << 3),     //  SSR or RTR
        Decals                      = (1 << 4),     //  Used for surfaces that receive decals
        ObjectMotionVector          = (1 << 5),     //  Animated object (for motion blur, SSR, SSAO, TAA)

        // User bits
        UserBit0 = (1 << 6),
        UserBit1 = (1 << 7),

        // Util value to encompass HDRP reserved bits
        HDRPReservedBits = 255 & ~(UserBit0 | UserBit1),
    }

    [GenerateHLSL]
    internal enum StencilAfterOpaque
    {
        Clear = 0,

        ExcludeFromTAA              = (1 << 1),    // Disable Temporal Antialiasing for certain objects
        DistortionVectors           = (1 << 2),    // Distortion pass - reset after distortion pass, shared with SMAA
        SMAA                        = (1 << 2),    // Subpixel Morphological Antialiasing
        TraceReflectionRay          = (1 << 3),    // SSR or RTR

        ReservedBits                = 0x38,        // Reserved for future usage

        // User bits
        UserBit0                    = (1 << 6),
        UserBit1                    = (1 << 7),

        // Util value to encompass HDRP reserved bits
        HDRPReservedBits            = 255 & ~(UserBit0 | UserBit1),
    }
}
