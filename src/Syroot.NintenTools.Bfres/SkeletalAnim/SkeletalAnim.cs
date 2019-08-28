﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Syroot.NintenTools.Bfres.Core;
using System.ComponentModel;
using SharpYaml;
using SharpYaml.Serialization;

namespace Syroot.NintenTools.Bfres
{
    /// <summary>
    /// Represents an FSKA subfile in a <see cref="ResFile"/>, storing armature animations of <see cref="Bone"/>
    /// instances in a <see cref="Skeleton"/>.
    /// </summary>
    [DebuggerDisplay(nameof(SkeletalAnim) + " {" + nameof(Name) + "}")]
    public class SkeletalAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkeletalAnim"/> class.
        /// </summary>
        public SkeletalAnim()
        {
            Name = "";
            Path = "";
            FlagsAnimSettings = 0;
            FlagsScale = SkeletalAnimFlagsScale.Maya;
            FlagsRotate = SkeletalAnimFlagsRotate.EulerXYZ;
            FrameCount = 0;
            BakedSize = 0;
            BoneAnims = new List<BoneAnim>();
            BindSkeleton = new Skeleton();
            BindIndices = new ushort[0];
            UserData = new ResDict<UserData>();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSKA";

        private const uint _flagsMaskScale = 0b00000000_00000000_00000011_00000000;
        private const uint _flagsMaskRotate = 0b00000000_00000000_01110000_00000000;
        private const uint _flagsMaskAnimSettings = 0b00000000_00000000_00000000_00001111;

        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private uint _flags;

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in
        /// <see cref="ResDict{SkeletalAnim}"/> instances.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        public string Path { get; set; }


        /// <summary>
        /// Gets or sets the total number of frames this animation plays.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        [DisplayName("Frame Count")]
        public int FrameCount { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SkeletalAnimFlags"/> mode used to control looping and baked settings.
        /// </summary>
        [Browsable(false)]
        public SkeletalAnimFlags FlagsAnimSettings
        {
            get { return (SkeletalAnimFlags)(_flags & _flagsMaskAnimSettings); }
            set { _flags = _flags & ~_flagsMaskAnimSettings | (uint)value; }
        }

        [Browsable(true)]
        [Category("Animation")]
        public bool Loop
        {
            get
            {
                return FlagsAnimSettings.HasFlag(SkeletalAnimFlags.Looping);
            }
            set
            {
                if (value == true)
                    FlagsAnimSettings |= SkeletalAnimFlags.Looping;
                else
                    FlagsAnimSettings &= ~SkeletalAnimFlags.Looping;
            }
        }

        [Browsable(true)]
        [Category("Animation")]
        public bool Baked
        {
            get
            {
                return FlagsAnimSettings.HasFlag(SkeletalAnimFlags.BakedCurve);
            }
            set
            {
                if (value == true)
                    FlagsAnimSettings |= SkeletalAnimFlags.BakedCurve;
                else
                    FlagsAnimSettings &= ~SkeletalAnimFlags.BakedCurve;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SkeletalAnimFlagsScale"/> mode used to store scaling values.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        [DisplayName("Scaling")]
        public SkeletalAnimFlagsScale FlagsScale
        {
            get { return (SkeletalAnimFlagsScale)(_flags & _flagsMaskScale); }
            set { _flags = _flags & ~_flagsMaskScale | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SkeletalAnimFlagsRotate"/> mode used to store rotation values.
        /// </summary>
        [Browsable(true)]
        [Category("Animation")]
        [DisplayName("Rotation")]
        public SkeletalAnimFlagsRotate FlagsRotate
        {
            get { return (SkeletalAnimFlagsRotate)(_flags & _flagsMaskRotate); }
            set { _flags = _flags & ~_flagsMaskRotate | (uint)value; }
        }

        /// <summary>
        /// Gets or sets the number of bytes required to bake all <see cref="AnimCurve"/> instances of all
        /// <see cref="BoneAnims"/>.
        /// </summary>
        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Animation")]
        [DisplayName("Baked Size")]
        public uint BakedSize { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BoneAnim"/> instances creating the animation.
        /// </summary>
        [Browsable(false)]
        public List<BoneAnim> BoneAnims { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Skeleton"/> instance affected by this animation.
        /// </summary>
        [Browsable(false)]
        public Skeleton BindSkeleton { get; set; }

        /// <summary>
        /// Gets or sets the indices of the <see cref="Bone"/> instances in the <see cref="Skeleton.Bones"/> dictionary
        /// to bind for each animation. <see cref="UInt16.MaxValue"/> specifies no binding.
        /// </summary>
        [Browsable(false)]
        public ushort[] BindIndices { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        [Browsable(false)]
        public ResDict<UserData> UserData { get; set; }

        ushort numBoneAnim = 0;

        public void Import(string FileName, ResFile ResFile)
        {
            using (ResFileLoader loader = new ResFileLoader(this, ResFile, FileName))
            {
                loader.ImportSection();
            }
        }

        public void Export(string FileName, ResFile ResFile)
        {
            if (FileName.EndsWith("yaml") || FileName.EndsWith("yml"))
            {
                ExportYaml(FileName, ResFile);
            }
            else
            {
                using (ResFileSaver saver = new ResFileSaver(this, ResFile, FileName))
                {
                    saver.ExportSection();
                }
            }
        }

        private void ExportYaml(string FileName, ResFile ResFile)
        {
            var serializer = new Serializer();
            var text = serializer.Serialize(new
            {
                Name = this.Name,
                Path = this.Path,
                FrameCount = this.FrameCount,
                BindIndices = this.BindIndices,
                BakedSize = this.BakedSize,
                Baked = this.Baked,
                Loop = this.Loop,
                FlagsScale = this.FlagsScale,
                FlagsRotate = this.FlagsRotate,
            });
            Console.WriteLine(text);
        }

        private void ReplaceYaml(string FileName, ResFile ResFile)
        {
            var serializer = new Serializer();
            var obj = serializer.Deserialize(new FileStream(FileName,FileMode.Open));
            
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.ResFile.Version >= 0x02040000)
            {
                loader.CheckSignature(_signature);
                Name = loader.LoadString();
                Path = loader.LoadString();
                _flags = loader.ReadUInt32();

                if (loader.ResFile.Version >= 0x03040000)
                {
                    FrameCount = loader.ReadInt32();
                    numBoneAnim = loader.ReadUInt16();
                    ushort numUserData = loader.ReadUInt16();
                    int numCurve = loader.ReadInt32();
                    BakedSize = loader.ReadUInt32();
                }
                else
                {
                    FrameCount = loader.ReadUInt16();
                    numBoneAnim = loader.ReadUInt16();
                    ushort numUserData = loader.ReadUInt16();
                    ushort numCurve = loader.ReadUInt16();
                    BakedSize = loader.ReadUInt32();
                    loader.Seek(4); //padding
                }

                BoneAnims = loader.LoadList<BoneAnim>(numBoneAnim).ToList();
                BindSkeleton = loader.Load<Skeleton>();
                BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(numBoneAnim));
                UserData = loader.LoadDict<UserData>();
            }
            else
            {
                _flags = loader.ReadUInt32();
                FrameCount = loader.ReadUInt16();
                numBoneAnim = loader.ReadUInt16();
                ushort numUserData = loader.ReadUInt16();
                ushort numCurve = loader.ReadUInt16();
                Name = loader.LoadString();
                Path = loader.LoadString();
                BoneAnims = loader.LoadList<BoneAnim>(numBoneAnim).ToList();
                BindSkeleton = loader.Load<Skeleton>();
                BindIndices = loader.LoadCustom(() => loader.ReadUInt16s(numBoneAnim));
            }
        }

        internal long PosBindModelOffset;
        internal long PosBindIndicesOffset;
        internal long PosBoneAnimsOffset;
        internal long PosUserDataOffset;

        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            saver.SaveString(Name);
            saver.SaveString(Path);
            saver.Write(_flags);
            if (saver.ResFile.Version >= 0x03040000)
            {
                saver.Write(FrameCount);
                saver.Write((ushort)BoneAnims.Count);
                saver.Write((ushort)UserData.Count);
                saver.Write(BoneAnims.Sum((x) => x.Curves.Count));
                saver.Write(BakedSize);
            }
            else
            {
                saver.Write((ushort)FrameCount);
                saver.Write((ushort)BoneAnims.Count);
                saver.Write((ushort)UserData.Count);
                saver.Write((ushort)BoneAnims.Sum((x) => x.Curves.Count));
                saver.Write(BakedSize);
                saver.Seek(4);
            }

            PosBoneAnimsOffset = saver.SaveOffsetPos();
            PosBindModelOffset = saver.SaveOffsetPos();
            PosBindIndicesOffset = saver.SaveOffsetPos();
            PosUserDataOffset = saver.SaveOffsetPos();
        }
    }
    
    /// <summary>
    /// Represents flags specifying how animation data is stored or should be played.
    /// </summary>
    [Flags]
    public enum SkeletalAnimFlags : uint
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

    /// <summary>
    /// Represents the data format in which scaling values are stored.
    /// </summary>
    public enum SkeletalAnimFlagsScale : uint
    {
        /// <summary>
        /// No scaling.
        /// </summary>
        None,

        /// <summary>
        /// Default scaling.
        /// </summary>
        Standard = 1 << 8,

        /// <summary>
        /// Autodesk Maya scaling.
        /// </summary>
        Maya = 2 << 8,

        /// <summary>
        /// Autodesk Softimage scaling.
        /// </summary>
        Softimage = 3 << 8
    }

    /// <summary>
    /// Represents the data format in which rotation values are stored.
    /// </summary>
    public enum SkeletalAnimFlagsRotate : uint
    {
        /// <summary>
        /// Quaternion, 4 components.
        /// </summary>
        Quaternion,

        /// <summary>
        /// Euler XYZ, 3 components.
        /// </summary>
        EulerXYZ = 1 << 12
    }
}
 