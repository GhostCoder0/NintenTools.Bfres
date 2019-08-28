﻿using Syroot.NintenTools.Bfres.Core;

namespace Syroot.NintenTools.Bfres
{
    public class ShaderAssign : IResData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderAssign"/> class.
        /// </summary>
        public ShaderAssign()
        {
            ShaderArchiveName = "";
            ShadingModelName = "";
            Revision = 0;
            AttribAssigns = new ResDict<ResString>();
            SamplerAssigns = new ResDict<ResString>();
            ShaderOptions = new ResDict<ResString>();
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        public string ShaderArchiveName { get; set; }

        public string ShadingModelName { get; set; }

        public uint Revision { get; set; }

        public ResDict<ResString> AttribAssigns { get; set; }

        public ResDict<ResString> SamplerAssigns { get; set; }

        public ResDict<ResString> ShaderOptions { get; set; }

        // ---- METHODS ------------------------------------------------------------------------------------------------

        void IResData.Load(ResFileLoader loader)
        {
            ShaderArchiveName = loader.LoadString();
            ShadingModelName = loader.LoadString();
            Revision = loader.ReadUInt32();
            byte numAttribAssign = loader.ReadByte();
            byte numSamplerAssign = loader.ReadByte();
            ushort numShaderOption = loader.ReadUInt16();
            AttribAssigns = loader.LoadDict<ResString>();
            SamplerAssigns = loader.LoadDict<ResString>();
            ShaderOptions = loader.LoadDict<ResString>();
        }

        internal long PosAttribAssigns;
        internal long PosSamplerAssigns;
        internal long PosShaderOptions;

        void IResData.Save(ResFileSaver saver)
        {
            saver.SaveString(ShaderArchiveName);
            saver.SaveString(ShadingModelName);
            saver.Write(Revision);
            saver.Write((byte)AttribAssigns.Count);
            saver.Write((byte)SamplerAssigns.Count);
            saver.Write((ushort)ShaderOptions.Count);
            PosAttribAssigns = saver.SaveOffsetPos();
            PosSamplerAssigns = saver.SaveOffsetPos();
            PosShaderOptions = saver.SaveOffsetPos();
        }
    }
}