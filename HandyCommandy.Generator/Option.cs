namespace HandyCommandy.Generator
{
    abstract class Option
    {
        public Option(string name, string typeName, string description)
        {
            Name = name;
            TypeName = typeName;
            Description = description;
        }

        public string Name { get; }
        public string TypeName { get; }
        public string Description { get; }

        public static Option FromKey(string key, string description)
        {
            var indexOfFirstSpace = key.IndexOf(' ');
            if (indexOfFirstSpace == -1)
            {
                var name = key.TrimStart('-');
                return new BoolOption(name, description);
            }
            else
            {
                var name = key.Substring(0, indexOfFirstSpace).TrimStart('-');
                var value = key.Substring(indexOfFirstSpace + 1);
                if (value.StartsWith("["))
                {
                    return new OptionalStringOption(name, description);
                }
                else
                {
                    return new StringOption(name, description);
                }
            }
        }

        public class BoolOption : Option
        {
            public BoolOption(string name, string description) : base(name, "bool", description)
            {
            }
        }

        public class StringOption : Option
        {
            public StringOption(string name, string description) : base(name, "string", description)
            {
            }
        }

        public class OptionalStringOption : Option
        {
            public OptionalStringOption(string name, string description) : base(name, "string?", description)
            {
            }
        }
    }
}
