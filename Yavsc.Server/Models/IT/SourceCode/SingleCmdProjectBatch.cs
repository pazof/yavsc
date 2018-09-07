using System;
using System.IO;

namespace Yavsc.Server.Models.IT.SourceCode
{
    public abstract class SingleCmdProjectBatch : Batch<Project>
    {
        protected string _repositoryRootPath;
        protected string _cmdPath ;
        public SingleCmdProjectBatch (string repoRoot, string cmdPath)
        {
            _cmdPath = cmdPath;
            _repositoryRootPath = repoRoot;
            WorkingDir = _repositoryRootPath;
            var fie = new DirectoryInfo(WorkingDir);
            if (!fie.Exists)
                throw new Exception ($"This directory doesn't exist: {WorkingDir},\nand cannot be used as a repository.");
        }

    }
}