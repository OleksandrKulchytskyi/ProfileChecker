using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuance.Radiology.DNSProfileChecker.Infrastructure.Configuration
{
	public class Service : ConfigurationElement
	{
		public Service() { }

		[ConfigurationProperty("bind", DefaultValue = "", IsKey = true, IsRequired = true)]
		public string bind
		{
			get { return (string)this["bind"]; }
			set { this["bind"] = value; }
		}

		[ConfigurationProperty("to", DefaultValue = "", IsRequired = true)]
		public string to
		{
			get { return (string)this["to"]; }
			set { this["to"] = value; }
		}

		[ConfigurationProperty("singleton", DefaultValue = 0, IsRequired = true)]
		public int singleton
		{
			get { return (int)this["singleton"]; }
			set { this["singleton"] = value; }
		}
	}

	public class Services : ConfigurationElementCollection
	{
		public Service this[int index]
		{
			get
			{
				return base.BaseGet(index) as Service;
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

		public new Service this[string responseString]
		{
			get { return (Service)BaseGet(responseString); }
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
			return new Service();
		}

		protected override object GetElementKey(System.Configuration.ConfigurationElement element)
		{
			return ((Service)element).bind;
		}
	}

	public class ServiceProviderConfig : ConfigurationSection
	{
		public static ServiceProviderConfig GetConfig()
		{
			return (ServiceProviderConfig)System.Configuration.ConfigurationManager.GetSection("servicesProvider") ?? new ServiceProviderConfig();
		}

		[System.Configuration.ConfigurationProperty("services")]
		[ConfigurationCollection(typeof(Service), AddItemName = "service")]
		public Services Services
		{
			get
			{
				object loggers = this["services"];
				return loggers as Services;
			}
		}
	}
}
