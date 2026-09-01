// SPDX-FileCopyrightText: 2026 Febris
// SPDX-License-Identifier: AGPL-3.0-only
using System;
using System.Collections.Generic;
using System.Text;
using System.IO.MemoryMappedFiles;
using System.Management;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Febris.PCModuleManagerV3.Utilties
{
    public class UniqueIdentifier
    {
        private ILogger _log;
        private IConfiguration _config;

        public UniqueIdentifier(ILogger log)
        {
            _log = log;
        }

        public UniqueIdentifier(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        public UniqueIdentifier()
        {
        }

        public void SetHardwareLicense()
        {
            string hardwareId = GetHardwareLicense();
            if (hardwareId == string.Empty)
            {
                hardwareId = UniqueIdentifierSet();
                StoreUniqueIdentifier(hardwareId);
            }
        }

        internal void StoreUniqueIdentifier(string uniqueIdentifier)
        {
            byte[] Buffer = ASCIIEncoding.ASCII.GetBytes(uniqueIdentifier);
            MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen("uniqueIdentifier", 1000);
            MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor();
            accessor.Write(54, (ushort)Buffer.Length);
            accessor.WriteArray(54 + 2, Buffer, 0, Buffer.Length);
        }
        internal string GetStoredUniqueIdentifier()
        {
            try
            {
                MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("uniqueIdentifier");
                MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor();
                ushort Size = accessor.ReadUInt16(54);
                byte[] Buffer = new byte[Size];
                accessor.ReadArray(54 + 2, Buffer, 0, Buffer.Length);
                return ASCIIEncoding.ASCII.GetString(Buffer);
            }
            catch
            {
                string hardwareId = UniqueIdentifierSet();
                return hardwareId;
            }
        }

        private string UniqueIdentifierSet()
        {
            string hardwareID = string.Empty;
            ManagementObjectCollection mbsList = null;
            ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select * From Win32_processor");
            mbsList = mbs.Get();
            string id = "";
            foreach (ManagementObject mo in mbsList)
            {
                id = mo["ProcessorID"].ToString();
            }
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            ManagementObjectCollection moc = mos.Get();
            string motherBoard = "";
            foreach (ManagementObject mo in moc)
            {
                motherBoard = (string)mo["SerialNumber"];
            }
            hardwareID = id.ToString() + motherBoard.ToString();

            return hardwareID;
        }

        public string GetHardwareLicense()
        {
            string hardwareId = string.Empty;
            hardwareId = GetStoredUniqueIdentifier();
            return hardwareId;
        }


    }
}
