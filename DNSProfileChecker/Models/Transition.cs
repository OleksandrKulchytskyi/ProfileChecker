﻿using Caliburn.Micro;
using Nuance.Radiology.DNSProfileChecker.ViewModels;
using System;
using System.Collections.Generic;

namespace Nuance.Radiology.DNSProfileChecker.Models
{
	public enum StateTransition
	{
		Cancel,
		SourceSelector,
		SourceSelectorFinished,
		ProfileFiltering,
		ProfileFilteringFinished,
		ProfileOptimization,
		ProfileOptimizationFinished
	}

	public class TransitionMap : Dictionary<Type, Dictionary<StateTransition, Type>>
	{
		private static TransitionMap _instance;

		private TransitionMap()
		{
		}

		public static TransitionMap GetInstance()
		{
			if (_instance == null)
			{
				_instance = new TransitionMap();
			}
			return _instance;
		}

		public static void Add<TIdentity, TResponse>(StateTransition transition)
			where TIdentity : IScreen
			where TResponse : IScreen
		{
			var instance = GetInstance();

			if (!instance.ContainsKey(typeof(TIdentity)))
			{
				instance.Add(typeof(TIdentity), new Dictionary<StateTransition, Type>() { { transition, typeof(TResponse) } });
			}
			else
			{
				instance[typeof(TIdentity)].Add(transition, typeof(TResponse));
			}
		}

		public static Type GetNextScreenType(BaseViewModel screenThatClosed)
		{
			var instance = GetInstance();
			var identity = screenThatClosed.GetType();
			var transition = screenThatClosed.NextTransition;

			if (!instance.ContainsKey(identity))
				throw new InvalidOperationException(string.Format("There are no states transitions defined for state {0}", identity.ToString()));

			if (!instance[identity].ContainsKey(transition))
				throw new InvalidOperationException(string.Format("There is response setup for transition {0} from screen {1}", transition.ToString(), identity.ToString()));

			return instance[identity][transition];
		}
	}
}