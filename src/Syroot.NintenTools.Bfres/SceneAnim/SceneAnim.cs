﻿using System;
using System.Diagnostics;
using Syroot.NintenTools.Bfres.Core;
using System.IO;

namespace Syroot.NintenTools.Bfres
{
    /// <summary>
    /// Represents an FSCN subfile in a <see cref="ResFile"/>, storing scene animations controlling camera, light and
    /// fog settings.
    /// </summary>
    [DebuggerDisplay(nameof(SceneAnim) + " {" + nameof(Name) + "}")]
    public class SceneAnim : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneAnim"/> class.
        /// </summary>
        public SceneAnim()
        {
            Name = "";
            Path = "";

            CameraAnims = new ResDict<CameraAnim>();
            LightAnims = new ResDict<LightAnim>();
            FogAnims = new ResDict<FogAnim>();
            UserData = new ResDict<UserData>();
        }

        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const string _signature = "FSCN";
        
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the name with which the instance can be referenced uniquely in <see cref="ResDict{SceneAnim}"/>
        /// instances.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the path of the file which originally supplied the data of this instance.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CameraAnim"/> instances.
        /// </summary>
        public ResDict<CameraAnim> CameraAnims { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LightAnim"/> instances.
        /// </summary>
        public ResDict<LightAnim> LightAnims { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="FogAnim"/> instances.
        /// </summary>
        public ResDict<FogAnim> FogAnims { get; set; }

        /// <summary>
        /// Gets or sets customly attached <see cref="UserData"/> instances.
        /// </summary>
        public ResDict<UserData> UserData { get; set; }

        public void Import(string FileName, ResFile ResFile)
        {
            using (ResFileLoader loader = new ResFileLoader(this, ResFile, FileName))
            {
                loader.ImportSection();
            }
        }

        public void Export(string FileName, ResFile ResFile)
        {
            using (ResFileSaver saver = new ResFileSaver(this, ResFile, FileName))
            {
                saver.ExportSection();
            }
        }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            if (loader.ResFile.Version >= 0x02040000)
            {
                loader.CheckSignature(_signature);
                Name = loader.LoadString();
                Path = loader.LoadString();
                ushort numUserData = loader.ReadUInt16();
                ushort numCameraAnim = loader.ReadUInt16();
                ushort numLightAnim = loader.ReadUInt16();
                ushort numFogAnim = loader.ReadUInt16();
                CameraAnims = loader.LoadDict<CameraAnim>();
                LightAnims = loader.LoadDict<LightAnim>();
                FogAnims = loader.LoadDict<FogAnim>();
                UserData = loader.LoadDict<UserData>();
            }
            else
            {

            }
        }

        internal long PosCameraAnimArrayOffset;
        internal long PosLightAnimArrayOffset;
        internal long PosFogAnimArrayOffset;
        internal long PosUserDataOffset;

        void IResData.Save(ResFileSaver saver)
        {
            saver.WriteSignature(_signature);
            saver.SaveString(Name);
            saver.SaveString(Path);
            saver.Write((ushort)UserData.Count);
            saver.Write((ushort)CameraAnims.Count);
            saver.Write((ushort)LightAnims.Count);
            saver.Write((ushort)FogAnims.Count);
            PosCameraAnimArrayOffset = saver.SaveOffsetPos();
            PosLightAnimArrayOffset = saver.SaveOffsetPos();
            PosFogAnimArrayOffset = saver.SaveOffsetPos();
            PosUserDataOffset = saver.SaveOffsetPos();
        }
    }
}