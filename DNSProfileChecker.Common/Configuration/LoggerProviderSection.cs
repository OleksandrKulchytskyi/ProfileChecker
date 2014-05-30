using System.Configuration;

namespace DNSProfileChecker.Common.Configuration
{
	public class Logger : ConfigurationElement
	{
		public Logger() { }

		[ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
		public string name
		{
			get { return (string)this["name"]; }
			set { this["name"] = value; }
		}

		[ConfigurationProperty("type", DefaultValue = "", IsRequired = true)]
		public string type
		{
			get { return (string)this["type"]; }
			set { this["type"] = value; }
		}

		[ConfigurationProperty("description", DefaultValue = "", IsRequired = false)]
		public string description
		{
			get { return (string)this["description"]; }
			set { this["description"] = value; }
		}
	}

	public class Loggers : ConfigurationElementCollection
	{
		public Logger this[int index]
		{
			get
			{
				return base.BaseGet(index) as Logger;
			}
			set
			{
				if (base.BaseGet(index) != null)
				{
					base.BaseRemoveAt(index);
				}
				this.BaseAdd(index, value);
			}
		}

		public new Logger this[string responseString]
		{
			get { return (Logger)BaseGet(responseString); }
			set
			{
				if (BaseGet(responseString) != null)
				{
					BaseRemoveAt(BaseIndexOf(BaseGet(responseString)));
				}
				BaseAdd(value);
			}
		}

		protected override System.Configuration.ConfigurationElement CreateNewElement()
		{
			return new Logger();
		}

		protected override object GetElementKey(System.Configuration.ConfigurationElement element)
		{
			return ((Logger)element).name;
		}
	}

	public class LogProviderConfig : ConfigurationSection
	{
		public static LogProviderConfig GetConfig()
		{
			return (LogProviderConfig)System.Configuration.ConfigurationManager.GetSection("logProvider") ?? new LogProviderConfig();
		}

		[System.Configuration.ConfigurationProperty("loggers")]
		[ConfigurationCollection(typeof(Logger), AddItemName = "logger")]
		public Loggers Loggers
		{
			get
			{
				object loggers = this["loggers"];
				return loggers as Loggers;
			}
		}
	}
}
