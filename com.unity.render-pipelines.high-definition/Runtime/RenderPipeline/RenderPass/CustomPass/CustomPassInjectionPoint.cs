using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Rendering.HighDefinition
{
    /// <summary>
    /// List all the injection points available for HDRP
    /// </summary>
    [GenerateHLSL]
    public enum CustomPassInjectionPoint
    {
        // Important: don't touch the value of the injection points for the serialization.
        // Ordered by injection point in the frame for the enum popup in the UI.

        /// <summary>Just after the depth clear, you can write to the depth buffer so Z-Tested opaque objects won't be rendered.</summary>
        BeforeRendering             = 0,
        /// <summary>At this point you can modify the normal, roughness and depth buffer, it will be taken in account in the lighting and the depth pyramid.</summary>
        AfterOpaqueDepthAndNormal   = 5,
        /// <summary>At this point to render any transparent objects that you want to be in the refraction (they, will end up in the color pyramid we use for refraction when drawing transparent objects)</summary>
        BeforePreRefraction         = 4,
        /// <summary>At this point you can sample the color pyramid we generated for rough transparent refraction</summary>
        BeforeTransparent           = 1,
        /// <summary>Before the post process and custom post processes are rendered</summary>
        BeforePostProcess           = 2,
        /// <summary>After the post processes. the depth is jittered so you can't draw depth tested objects without having artifacts</summary>
        AfterPostProcess            = 3,
    }
}