using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MMDExtensions.Tools
{
    /// <summary>
    /// VMDParser
    /// </summary>
    public static class VMDParser
    {
        /// <summary>
        /// Return a VMD from given vmd file stream
        /// Rotation in rad
        /// </summary>
        /// <param name="stream">The vmd file stream</param>
        /// <returns></returns>
        public static VMD ParseVMD(Stream stream)
        {
            var reader = new BinaryReader(stream);

            /// Header
            string header = ReadString(reader, 30);
            var headerType = header.Contains("Vocaloid Motion Data 0002") ? VMDHeaderType.VocaloidMotionData0002 : VMDHeaderType.VocaloidMotionDatafile;

            /// Following the magic bytes, there is a fixed-length string which tells the name of the model that this VMD is compatible with.
            /// This string is 10 bytes in the old version of VMD, and 20 bytes in the new version. 
            string modelName = ReadString(reader, headerType == VMDHeaderType.VocaloidMotionDatafile ? 10 : 20);

            /// Note that the position coordinates are relative to the "bind pose", or the model's default pose. The position data of the bones in the PMD 
            /// model are relative to the world's origin, and the position data here is relative to that. So, for example, the bind pose of a bone is (1, 2, 3) 
            /// and the VMD gives (10, 25, 30). The final world position would be (11, 27, 33). 
            var offset = stream.Position;
            List<VMDBoneFrame> bones = new List<VMDBoneFrame>();
            List<VMDMorphFrame> morphs = new List<VMDMorphFrame>();
            List<VMDCameraFrame> cameras = new List<VMDCameraFrame>();

            byte getBoneByte()
            {
                var bt = reader.ReadByte();
                reader.ReadBytes(3);
                return bt;
            }

            byte getCameraByte()
            {
                var bt = reader.ReadByte();
                reader.ReadBytes(20);
                return bt;
            }

            var bcount = reader.ReadUInt32();
            /// Bone list, length 4-byte
            Enumerable.Range(0, Convert.ToInt32(bcount))
                .ToList()
                .ForEach(x => bones.Add(new VMDBoneFrame()
                {
                    BoneName = ReadString(reader, 15),
                    FrameNumber = reader.ReadUInt32(),
                    XPosition = reader.ReadSingle(),
                    YPosition = reader.ReadSingle(),
                    ZPosition = reader.ReadSingle(),
                    XRotation = reader.ReadSingle(),
                    YRotation = reader.ReadSingle(),
                    ZRotation = reader.ReadSingle(),
                    WRotation = reader.ReadSingle(),
                    XCurve = new VMDCurve()
                    {
                        AX = getBoneByte(),
                        AY = getBoneByte(),
                        BX = getBoneByte(),
                        BY = getBoneByte(),
                    },
                    YCurve = new VMDCurve()
                    {
                        AX = getBoneByte(),
                        AY = getBoneByte(),
                        BX = getBoneByte(),
                        BY = getBoneByte(),
                    },
                    ZCurve = new VMDCurve()
                    {
                        AX = getBoneByte(),
                        AY = getBoneByte(),
                        BX = getBoneByte(),
                        BY = getBoneByte(),
                    },
                    RCurve = new VMDCurve()
                    {
                        AX = getBoneByte(),
                        AY = getBoneByte(),
                        BX = getBoneByte(),
                        BY = getBoneByte(),
                    },
                }));

            /// Morph list, length 4-byte
            Enumerable.Range(0, Convert.ToInt32(reader.ReadUInt32()))
                .ToList()
                .ForEach(x => morphs.Add(new VMDMorphFrame()
                {
                    MorphName = ReadString(reader, 15),
                    FrameIndex = reader.ReadUInt32(),
                    Weight = reader.ReadSingle()
                }));

            var ccount = reader.ReadUInt32();
            /// Camera list, length 4-byte
            Enumerable.Range(0, Convert.ToInt32(ccount))
                .ToList()
                .ForEach(x => cameras.Add(new VMDCameraFrame()
                {
                    FrameIndex = reader.ReadUInt32(),
                    Distance = reader.ReadSingle(),
                    XPosition = reader.ReadSingle(),
                    YPosition = reader.ReadSingle(),
                    ZPosition = reader.ReadSingle(),
                    XRotation = reader.ReadSingle(),
                    YRotation = reader.ReadSingle(),
                    ZRotation = reader.ReadSingle(),
                    Curve = new VMDCurve() { 
                        AX=reader.ReadByte(),
                        AY=reader.ReadByte(),
                        BX=reader.ReadByte(),
                        BY=getCameraByte()
                    },
                    FOV = reader.ReadUInt32(),
                    Orthographic = reader.ReadBoolean()
                }));

            return new VMD()
            {
                FileName = modelName,
                Header = header,
                HeaderType = headerType,
                Bones = bones,
                Morphs = morphs,
                Cameras = cameras
            };
        }

        /// <summary>
        /// Return a string with given count
        /// </summary>
        /// <param name="reader">Binary Reader</param>
        /// <param name="count">Counts</param>
        /// <returns></returns>
        private static string ReadString(BinaryReader reader, int count)
        {
            /// ShiftJis encode
            return Encoding.GetEncoding("shift_jis").GetString(reader.ReadBytes(count)).Trim('\0');
        }
    }


    public class VMDBoneFrame
    {
        public string BoneName { get; set; }
        public uint FrameNumber { get; set; }
        public float XPosition { get; set; }
        public float YPosition { get; set; }
        public float ZPosition { get; set; }
        public float XRotation { get; set; }
        public float YRotation { get; set; }
        public float ZRotation { get; set; }
        public float WRotation { get; set; }
        public VMDCurve XCurve { get; set; }
        public VMDCurve YCurve { get; set; }
        public VMDCurve ZCurve { get; set; }
        public VMDCurve RCurve { get; set; }

    }

    public class VMDCurve
    {
        public uint AX { get; set; }
        public uint AY { get; set; }
        public uint BX { get; set; }
        public uint BY { get; set; }
    }


    public class VMDMorphFrame
    {
        public string MorphName { get; set; }
        public uint FrameIndex { get; set; }
        public float Weight { get; set; }
    }

    public class VMDCameraFrame
    {
        public uint FrameIndex { get; set; }
        public float Distance { get; set; }
        public float XPosition { get; set; }
        public float YPosition { get; set; }
        public float ZPosition { get; set; }
        public float XRotation { get; set; }
        public float YRotation { get; set; }
        public float ZRotation { get; set; }
        public VMDCurve Curve { get; set; }
        public uint FOV { get; set; }
        public bool Orthographic { get; set; }
    }

    public class VMD
    {
        public string FileName { get; set; }
        public string Header { get; set; }
        public VMDHeaderType HeaderType { get; set; }
        public List<VMDBoneFrame> Bones { get; set; }
        public List<VMDMorphFrame> Morphs { get; set; }
        public List<VMDCameraFrame> Cameras { get; set; }
    }

    public enum VMDHeaderType
    {
        VocaloidMotionDatafile,
        VocaloidMotionData0002,
    }
}
