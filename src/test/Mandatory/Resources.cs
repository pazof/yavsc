
namespace test {
    public class ResxResources {
        const string resPath = "Resources/Test.TestResources.resx";

        public void HaveAResxLoader()
        {
            System.Resources.ResourceReader loader = new System.Resources.ResourceReader(resPath);
            // IDictionary
            var etor = loader.GetEnumerator();
            while (etor.Current !=null)
            {
                byte[] data;
                string stringdata;
                string resName = etor.Key.ToString();

                loader.GetResourceData(resName, out stringdata, out data);

            }
            
        }
    }
}