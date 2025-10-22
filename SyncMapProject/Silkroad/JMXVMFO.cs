using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SyncMapProject.Silkroad
{
    public class JMXVMFO
    {
        public string Signature => "JMXVMFO 1000";
        public short MapWidth { get; set; }
        public short MapHeight { get; set; }
        public short Unk01 { get; set; }
        public short Unk02 { get; set; }
        public short Unk03 { get; set; }
        public short Unk04 { get; set; }
        public List<bool> EnabledRegions { get; } = new List<bool>(); // width x height

        #region Constructor
        public JMXVMFO() { }
        #endregion

        #region Public Methods
        public bool IsRegionEnabled(byte X, byte Y) => EnabledRegions[Y * 256 + X];
        public bool IsRegionEnabled(short RegionId) => IsRegionEnabled((byte)(RegionId & 0xFF), (byte)(RegionId >> 8));
        public void SetRegion(byte X, byte Y, bool Enabled) => EnabledRegions[Y * 256 + X] = Enabled;
        public void SetRegion(short RegionId, bool Enabled) => SetRegion((byte)(RegionId & 0xFF), (byte)(RegionId >> 8), Enabled);

        public bool Load(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    // Check file header
                    if (new string(br.ReadChars(Signature.Length)) != Signature)
                        return false;

                    MapWidth = br.ReadInt16();
                    MapHeight = br.ReadInt16();
                    Unk01 = br.ReadInt16();
                    Unk02 = br.ReadInt16();
                    Unk03 = br.ReadInt16();
                    Unk04 = br.ReadInt16();

                    EnabledRegions.Clear();
                    var regionLimit = (256 * 256) / 8; // 256x256 encoded into bytes
                    for (int r = 0; r < regionLimit; r++)
                    {
                        var regions = br.ReadByte();
                        // Extract booleans from each byte
                        for (int i = 0; i < 8; i++)
                        {
                            var isEnabled = ((regions >> (7 - i)) & 1) == 1;
                            EnabledRegions.Add(isEnabled);
                        }
                    }

                    // Success
                    return true;
                }
            }
            catch { }
            return false;
        }
        public void Save(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                // Add file header
                bw.Write(Signature.ToArray());

                bw.Write(MapWidth);
                bw.Write(MapHeight);
                bw.Write(Unk01);
                bw.Write(Unk02);
                bw.Write(Unk03);
                bw.Write(Unk04);

                var regionLimit = (256 * 256) / 8; // 256x256 encoded into bytes
                for (int r = 0; r < regionLimit; r++)
                {
                    byte regions = 0;
                    // Set booleans into byte
                    for (int i = 0; i < 8; i++)
                    {
                        // Set each boolean
                        if (EnabledRegions[r * 8 + i])
                        {
                            regions |= (byte)(1 << (7 - i));
                        }
                    }
                    bw.Write(regions);
                }
            }
        }
        #endregion
    }
}
