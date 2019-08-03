using System;
using System.Security.Principal;
using Yavsc.Models;

namespace Yavsc.Services {
    [Flags]
    public enum FileAccessRight {
        None = 0,

        Read = 1,
        Write = 2
    }

    public interface IFileSystemAuthManager {
        string NormalizePath (string path);

        /// <summary>
        /// A full path starts with a slash,
        /// continues with a user name,
        /// and returns true by the helper fonction : 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="normalizedFullPath"></param>
        /// <returns></returns>
        FileAccessRight GetFilePathAccess(IPrincipal user, string normalizedFullPath);

        void SetAccess (long circleId, string normalizedFullPath, FileAccessRight access);

    }

    public class FileSystemAuthManager : IFileSystemAuthManager
    {
        ApplicationDbContext _dbContext;

        public FileSystemAuthManager(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public FileAccessRight GetFilePathAccess(IPrincipal user, string normalizedFullPath)
        {
            throw new NotImplementedException();
        }

        public string NormalizePath(string path)
        {
            throw new NotImplementedException();
        }

        public void SetAccess(long circleId, string normalizedFullPath, FileAccessRight access)
        {
            throw new NotImplementedException();
        }
    }
}