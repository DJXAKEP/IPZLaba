﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан инструментальным средством
//     В случае повторного создания кода изменения, внесенные в этот файл, будут потеряны.
// </auto-generated>
//------------------------------------------------------------------------------
namespace IPZLaba.Server
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using Coffee = System.Collections.Generic.KeyValuePair<string, decimal>;
	using CoffeeList = System.Collections.Generic.Dictionary<string, decimal>;

	public class Program
	{
		private decimal moneyBalance { get; set; }

		private CoffeeList coffeeList { get; set; }

		private Dictionary<decimal, int> billsList;

		public virtual decimal getMoneyValue()
		{
			return moneyBalance;
		}

		public virtual CoffeeList getCoffeList()
		{
			return coffeeList;
		}

		public virtual void getLastError()
		{
			throw new System.NotImplementedException();
		}

		public static void Main()
		{
			//throw new System.NotImplementedException();
			Console.WriteLine("Hello, World!!!");
		}

	}
}
