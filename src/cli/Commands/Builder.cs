using System.IO;
using Yavsc.Server.Models.IT;
using Yavsc.Server.Models.IT.SourceCode;

public class Builder {
    string _gitRepository;
    private Project _projectInfo;

    public Builder()
    {

    }


    public void Clone()
    {
        var di = new DirectoryInfo(_gitRepository);
        if (!di.Exists) di.Create();

        var clone = new GitClone(_gitRepository);
        clone.Launch(_projectInfo);
    }
    
}