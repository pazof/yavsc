using Yavsc.Server.Models.IT.SourceCode;

public class Builder {


    public void Clone()
    {
        var firstProject = _dbContext.Project.Include(p=>p.Repository).FirstOrDefault();
            Assert.NotNull (firstProject);
            var di = new DirectoryInfo(_serverFixture.SiteSetup.GitRepository);
            if (!di.Exists) di.Create();


            var clone = new GitClone(_serverFixture.SiteSetup.GitRepository);
            clone.Launch(firstProject);
            gitRepo = di.FullName;
            
    }
    
}