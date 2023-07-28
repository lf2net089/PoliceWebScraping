using System.Xml.Serialization;

namespace PoliceWebScraping
{
    public class SelectableOption
    {
        [XmlAttribute]
        public string Key { get; set; }

        [XmlArrayItem("Value")]
        public List<string> Values { get; set; }

        public SelectableOption()
        {
            Values = new List<string>();
        }
    }

    public class SelectableOptions
    {
        [XmlElement("Option")]
        public List<SelectableOption> Options { get; set; }

        public SelectableOptions()
        {
            Options = new List<SelectableOption>();
        }
    }

    public static class XmlHelper
    {
        public static void SaveSelectableOptions(string filePath, Dictionary<string, List<string>> options)
        {
            var selectableOptions = new SelectableOptions();

            foreach (var kvp in options)
            {
                var option = new SelectableOption
                {
                    Key = kvp.Key,
                    Values = kvp.Value
                };
                selectableOptions.Options.Add(option);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(SelectableOptions));

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, selectableOptions);
            }
        }

        public static Dictionary<string, List<string>> LoadSelectableOptions(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SelectableOptions));

            using (StreamReader reader = new StreamReader(filePath))
            {
                var selectableOptions = (SelectableOptions)serializer.Deserialize(reader);
                var options = new Dictionary<string, List<string>>();

                foreach (var option in selectableOptions.Options)
                {
                    options[option.Key] = option.Values;
                }

                return options;
            }
        }
        public static List<string> GetValuesForKey(string keyName, Dictionary<string, List<string>> optionsDictionary)
        {
            if (optionsDictionary.TryGetValue(keyName, out List<string> values))
            {
                return values;
            }

            return null; // Key not found in the dictionary
        }

    }
}
