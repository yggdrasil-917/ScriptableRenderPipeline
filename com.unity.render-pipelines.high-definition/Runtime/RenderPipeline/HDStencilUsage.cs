using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.HighDefinition
{
    [GenerateHLSL]
    internal enum StencilUsage
    {
        Clear                           = 0,
        // --- Following bits are used before transparent rendering ---
        RequiresDeferredLighting        = (1 << 1),     
        SubsurfaceScattering            = (1 << 2),     //  SSS, Split Lighting
        TraceReflectionRay              = (1 << 3),     //  SSR or RTR
        Decal                           = (1 << 4),     //  Used for surfaces that receive decals
        ObjectMotionVector              = (1 << 5),     //  Animated object (for motion blur, SSR, SSAO, TAA)

        // --- Following bits are used during and after transparent rendering ---
        ExcludeFromTAA                  = (1 << 1),     //  Disable Temporal Antialiasing for certain objects
        DistortionVector                = (1 << 2),     //  Distortion pass - reset after distortion pass, shared with SMAA
        SMAA                            = (1 << 2),     //  Subpixel Morphological Antialiasing
        ReservedBits                    = 0x38,         //  Reserved for future usage

        // User bits
        UserBit0                        = (1 << 6),
        UserBit1                        = (1 << 7),

        // Util value to encompass HDRP reserved bits
        HDRPReservedBits                = 255 & ~(UserBit0 | UserBit1),
    }
}
