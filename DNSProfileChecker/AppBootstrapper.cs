using Caliburn.Micro;
using Nuance.Radiology.DNSProfileChecker.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using Common = DNSProfileChecker.Common;

namespace Nuance.Radiology.DNSProfileChecker
{
	public class AppBootstrapper : BootstrapperBase
	{
		private readonly SimpleContainer container;

		public AppBootstrapper()
		{
			container = new SimpleContainer();
			Initialize();
		}

		protected override void Configure()
		{
			base.Configure();

			Common.IServiceProvider provider = new Infrastructure.Providers.AppConfigServiceProvider();
			Dictionary<Type, Tuple<Type, int>> types = provider.GetServices();
			foreach (Type abstr in types.Keys)
			{
				if (types[abstr].Item2 == 1)
					container.RegisterSingleton(abstr, null, types[abstr].Item1);
				container.RegisterPerRequest(abstr, null, types[abstr].Item1);
			}

			container.Singleton<Common.ILogger, Common.LoggerBridge>();
			container.Singleton<IWindowManager, WindowManager>();
			container.Singleton<IEventAggregator, EventAggregator>();
			container.PerRequest<Common.Interfaces.IShell, ShellViewModel>();

			string defNamespace = typeof(AppBootstrapper).Namespace;
			TypeMappingConfiguration config = new TypeMappingConfiguration
			{
				DefaultSubNamespaceForViews = defNamespace + ".Views",
				DefaultSubNamespaceForViewModels = defNamespace + ".ViewModels"
			};

			ViewLocator.ConfigureTypeMappings(config);
			ViewModelLocator.ConfigureTypeMappings(config);
			//new Common.LoggerBridge(container.GetInstance<Common.ILogProvider>()));
		}

		protected override object GetInstance(Type serviceType, string key)
		{
			object instance = container.GetInstance(serviceType, key);
			if (instance != null)
				return instance;

			throw new InvalidOperationException(string.Format("Could not locate any instances for type {0}", serviceType));
		}

		protected override IEnumerable<object> GetAllInstances(Type serviceType)
		{
			return container.GetAllInstances(serviceType);
		}

		protected override void BuildUp(object instance)
		{
			container.BuildUp(instance);
		}

		protected override void OnStartup(object sender, StartupEventArgs e)
		{
			DisplayRootViewFor<Common.Interfaces.IShell>();

			var logger = container.GetInstance<Common.ILogger>();
			if (logger != null)
				logger.LogData(Common.LogSeverity.Success, "Application bootstrapper has been successfully initialized.", null);
		}

		protected override void OnUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			Common.ILogger log = Caliburn.Micro.IoC.Get<Common.ILogger>();
			if (log != null)
				log.LogData(Common.LogSeverity.Fatal, e.Exception.Message, e.Exception);
		}
	}
}
