using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.HighDefinition
{
    class PowerOfTwoTextureAtlas : Texture2DAtlas
    {
        public int mipPadding;
        const float k_MipmapFactorApprox = 1.33f;

        public PowerOfTwoTextureAtlas(int size, int mipPadding, GraphicsFormat format, FilterMode filterMode = FilterMode.Point, string name = "", bool useMipMap = true)
            : base(size, size, format, filterMode, true, name, useMipMap)
        {
            this.mipPadding = mipPadding;

            // Check if size is a power of two
            if ((size & (size - 1)) != 0)
                Debug.Assert(false, "Power of two atlas was constructed with non power of two size: " + size);
        }

        int GetTexturePadding() => (int)Mathf.Pow(2, mipPadding) * 2;

        // branchless previous power of two: Hacker’s Delight, Second Edition page 66
        static int PreviousPowerOfTwo(int size)
        {
            if (size <= 0)
                return 0;

            size |= (size >> 1);
            size |= (size >> 2);
            size |= (size >> 4);
            size |= (size >> 8);
            size |= (size >> 16);
            return size - (size >> 1);
        }
        
        void Blit2DTexturePadding(CommandBuffer cmd, Vector4 scaleOffset, Texture texture, Vector4 sourceScaleOffset, bool blitMips = true)
        {
            int mipCount = GetTextureMipmapCount(texture.width, texture.height);
            int pixelPadding = GetTexturePadding();
            Vector2 textureSize = GetPowerOfTwoTextureSize(texture);
            bool bilinear = texture.filterMode != FilterMode.Point;

            if (!blitMips)
                mipCount = 1;

            using (new ProfilingScope(cmd, ProfilingSampler.Get(HDProfileId.BlitTextureInPotAtlas)))
            {
                for (int mipLevel = 0; mipLevel < mipCount; mipLevel++)
                {
                    cmd.SetRenderTarget(m_AtlasTexture, mipLevel);
                    HDUtils.BlitQuadWithPadding(cmd, texture, textureSize, sourceScaleOffset, scaleOffset, mipLevel, bilinear, pixelPadding);
                }
            }
        }

        public override void BlitTexture(CommandBuffer cmd, Vector4 scaleOffset, Texture texture, Vector4 sourceScaleOffset, bool blitMips = true)
        {
            // We handle ourself the 2D blit because cookies needs mipPadding for trilinear filtering
            if (Is2D(texture))
                Blit2DTexturePadding(cmd, scaleOffset, texture, sourceScaleOffset, blitMips);
        }

        void TextureSizeToPowerOfTwo(Texture texture, ref int width, ref int height)
        {
            // Change the width and height of the texture to be power of two
            width = Mathf.NextPowerOfTwo(width);
            height = Mathf.NextPowerOfTwo(height);
        }

        Vector2 GetPowerOfTwoTextureSize(Texture texture)
        {
            int width = texture.width, height = texture.height;
            
            TextureSizeToPowerOfTwo(texture, ref width, ref height);
            return new Vector2(width, height);
        }

        // Override the behavior when we add a texture so all non-pot textures are blitted to a pot target zone
        public override bool AllocateTexture(CommandBuffer cmd, ref Vector4 scaleOffset, Texture texture, int width, int height)
        {
            // This atlas only supports square textures
            if (height != width)
            {
                Debug.LogError("Can't place " + texture + " in the atlas " + m_AtlasTexture.name + ": Only squared texture are allowed in this atlas.");
                return false;
            }

            TextureSizeToPowerOfTwo(texture, ref height, ref width);

            return base.AllocateTexture(cmd, ref scaleOffset, texture, width, height);
        }

        public static long GetApproxCacheSizeInByte(int nbElement, int resolution, bool hasMipmap, GraphicsFormat format)
            => (long)(nbElement * resolution * resolution * (double)((hasMipmap ? k_MipmapFactorApprox : 1.0f) * HDUtils.GetFormatSizeInBytes(format)));

        public static int GetMaxCacheSizeForWeightInByte(int weight, bool hasMipmap, GraphicsFormat format)
        {
            // Compute the max size of a power of two atlas for a given size in byte (weight)
            float bytePerPixel = (float)HDUtils.GetFormatSizeInBytes(format) * (hasMipmap ? k_MipmapFactorApprox : 1.0f);
            var maxAtlasSquareSize = Mathf.Sqrt((float)weight / bytePerPixel);
            return PreviousPowerOfTwo((int)maxAtlasSquareSize);
        }
    }
}
