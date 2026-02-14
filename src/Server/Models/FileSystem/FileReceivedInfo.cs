
//
//  FileRecievedInfo.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 - 2017 Paul Schneider
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.


using Yavsc.Abstract.FileSystem;

namespace Yavsc.Models.FileSystem
{
    public class FileReceivedInfo : IFileReceivedInfo
    {
        public FileReceivedInfo(string destDir, string fileName, bool quotaOffense=false) 
        {
            this.DestDir = destDir;
            this.FileName = fileName;
            this.QuotaOffense = quotaOffense;
        }
        public static FileReceivedInfo FromPath(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            return new FileReceivedInfo(
                fi.Directory.FullName,
                fi.Name
            );
        }

        public string DestDir { get; set; }
        
        public string FileName { get; set; }
        public bool Overridden { get; set; }

        public bool QuotaOffense { get; set; }
        public string FullName { get => Path.Combine(DestDir, FileName); }

    }
}
