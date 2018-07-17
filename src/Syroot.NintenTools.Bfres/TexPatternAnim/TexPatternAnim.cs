﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Syroot.NintenTools.Bfres.Core;

namespace Syroot.NintenTools.Bfres
{
    /// <summary>
    /// Represents an FTXP subfile in a <see cref="ResFile"/>, storing texture material pattern animations.
    /// </summary>
    [DebuggerDisplay(nameof(TexPatternAnim) + " {" + nameof(Name) + "}")]
    public class TexPatternAnim : IResData
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FTXP";
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{TexPatternAnim}"/> instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets flags controlling how animation data is stored or how the animation should be played.
        /// </summary>
        public TexPatternAnimFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the total number of frames this animation plays.
        /// </summary>
        public int FrameCount { get; set; }

        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="AnimCurve"/> instances of all
        /// <see cref="TexPatternMatAnims"/>.
        /// </summary>
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Model"/> instance affected by this animation.
        /// </summary>
        public Model BindModel { get; set; }

        /// <summary>
        /// Gets or sets the indices of the <see cref="Material"/> instances in the <see cref="Model.Materials"/>
        /// dictionary to bind for each animation. <see cref="UInt16.MaxValue"/> specifies no binding.
        /// </summary>
        public ushort[] BindIndices { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TexPatternAnim"/> instances creating the animation.
        /// </summary>
        public IList<TexPatternMatAnim> TexPatternMatAnims { get; set; }
        

        /// <summary>
        /// Gets or sets the <see cref="TextureRef"/> instances pointing to <see cref="Texture"/> instances
        /// participating in the animation.
        /// </summary>
        public ResDict<TextureRef> TextureRefs { get; set; }

        /// <summary>
        /// Note used for older bfres files
        /// Gets or sets the <see cref="TextureRef"/> instances pointing to <see cref="Texture"/> instances
        /// participating in the animation.
        /// </summary>
        public IList<string> TextureNames { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public ResDict<UserData> UserData { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            loader.CheckSignature(_signature);
            Name = loader.LoadString();
            Path = loader.LoadString();
            Flags = loader.ReadEnum<TexPatternAnimFlags>(true);
            ushort numUserData = loader.ReadUInt16();
            FrameCount = loader.ReadInt32();
            ushort numTextureRef = loader.ReadUInt16();
            ushort numMatAnim = loader.ReadUInt16();
            int numPatAnim = loader.ReadInt32();
            int numCurve = loader.ReadInt32();
            BakedSize = loader.ReadUInt32();
            BindModel = loader.Load<Model>();
            BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(numMatAnim));
            TexPatternMatAnims = loader.LoadList<TexPatternMatAnim>(numMatAnim);
            if (loader.ResFile.Version >= 0x03040000)
                TextureRefs = loader.LoadDict<TextureRef>();
            else
                TextureNames = loader.LoadStrings(numPatAnim);
            UserData = loader.LoadDict<UserData>();
        }
        
        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            saver.SaveString(Name);
            saver.SaveString(Path);
            saver.Write(Flags, true);
            saver.Write((ushort)UserData.Count);
            saver.Write(FrameCount);
            saver.Write((ushort)TextureRefs.Count);
            saver.Write((ushort)TexPatternMatAnims.Count);
            saver.Write(TexPatternMatAnims.Sum((x) => x.PatternAnimInfos.Count));
            saver.Write(TexPatternMatAnims.Sum((x) => x.Curves.Count));
            saver.Write(BakedSize);
            saver.Save(BindModel);
            saver.SaveCustom(BindIndices, () => saver.Write(BindIndices));
            saver.SaveList(TexPatternMatAnims);
            if (saver.ResFile.Version >= 0x03040000)
                saver.SaveDict(TextureRefs);
            else
                saver.SaveStrings(TextureNames);
            saver.SaveDict(UserData);
        }
    }

    /// <summary>
    /// Represents flags specifying how animation data is stored or should be played.
    /// </summary>
    [Flags]
    public enum TexPatternAnimFlags : ushort
    {
        /// <summary>
        /// The stored curve data has been baked.
        /// </summary>
        BakedCurve = 1 << 0,

        /// <summary>
        /// The animation repeats from the start after the last frame has been played.
        /// </summary>
        Looping = 1 << 2
    }
}