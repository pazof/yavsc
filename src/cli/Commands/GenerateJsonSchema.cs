using Microsoft.Extensions.CommandLineUtils;
using System.Threading.Tasks;
using NJsonSchema;
using System.IO;
using cli.Model;
using Yavsc.Abstract.IT;

namespace cli
{

    public class GenerateJsonSchema : ICommander
    {
        public CommandLineApplication Integrate(CommandLineApplication rootApp)
        {
            CommandArgument genargclass = null;
            CommandArgument genargjson = null;
            CommandOption genopthelp = null;
            var cmd = rootApp.Command("gen",
                   (target) =>
                   {
                       target.FullName = "Generete";
                       target.Description = "generates some objects ...";
                       genopthelp = target.HelpOption("-? | -h | --help");
                       genargclass = target.Argument(
                       "class",
                       "class name of generation to execute (actually, only 'jsonSchema') .",
                       multipleValues: false);
                       genargjson = target.Argument(
                       "json",
                       "Json file generated",
                       multipleValues: false);
                   }, false);
            cmd.OnExecute(
                 () => {
                    if (genargclass.Value == "jsonSchema") {
                        GenerateCiBuildSettingsSchema(genargjson.Value);
                    } else {
                        cmd.ShowHint();
                        return 1;
                    } 
                    return 0;
                }
            );
                   return cmd;
        }
        public static void GenerateCiBuildSettingsSchema(string outputFileName = "pauls-ci-schema.json")
        {
            var schema = JsonSchema.FromType(typeof(CiBuildSettings));
            var schemaData = schema.ToJson();

            FileInfo ofi = new FileInfo(outputFileName);
            var ostream = ofi.OpenWrite();
            var owritter = new StreamWriter(ostream);
            owritter.WriteLine(schemaData);
            owritter.Close();
            ostream.Close();
        }

    }

}
