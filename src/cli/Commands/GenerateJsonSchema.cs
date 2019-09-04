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
                       "Json file to generate a schema for.",
                       multipleValues: false);
                   }, false);
            cmd.OnExecute(
                async () => {
                    if (genargjson.Value == "jsonSchema") {
                        if (genargjson.Value == null)
                        await GenerateCiBuildSettingsSchema();
                        else 
                        await GenerateCiBuildSettingsSchema(genargjson.Value);
                    } else {
                        cmd.ShowHint();
                        return 1;
                    } 
                    return 0;
                }
            );
                   return cmd;
        }
        public static async Task GenerateCiBuildSettingsSchema(string outputFileName = "pauls-ci-schema.json")
        {
            var schema = await JsonSchema4.FromTypeAsync<CiBuildSettings>();
            var schemaData = schema.ToJson();

            FileInfo ofi = new FileInfo(outputFileName);
            var ostream = ofi.OpenWrite();
            var owritter = new StreamWriter(ostream);
            owritter.WriteLine(schemaData);
            owritter.Close();
            ostream.Close();
            /*  var errors = schema.Validate("{...}");

            foreach (var error in errors)
                Console.WriteLine(error.Path + ": " + error.Kind);

            schema = await JsonSchema4.FromJsonAsync(schemaData); */
        }

    }

}