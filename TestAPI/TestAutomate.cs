//
//  MyClass.cs
//
//  Author:
//       Paul Schneider <paul@pschneider.fr>
//
//  Copyright (c) 2015 GNU GPL
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using Machine.Specifications;
using Yavsc.Model.WorkFlow;
using NUnit.Framework;

namespace TestAPI
{
	[Subject(typeof(Automate<int,int>),"simple automate")]
	public class CreatingAnAutomate
	{
		static Automate<int,int> Subject;
		/// <summary>
		/// The context.
		/// </summary>
		Establish context = () =>
		{
			// ... any mocking, stubbing, or other setup ...
			Subject = new Automate<int,int>();
		};

		Because of = () => Subject.AddTransition(0,1,'a');
		/// <summary>
		/// The should be in state 0.
		/// </summary>
		It should_be_in_state_0 = () => Subject.State.ShouldEqual(0);
		/// <summary>
		/// The should not be in state 1.
		/// </summary>
		It should_not_be_in_state_1 = () => Subject.State.ShouldNotEqual(1);
		/// <summary>
		/// The state of the should not indicate o as final.
		/// </summary>
		It should_not_indicate_O_as_final_state = () => Subject.IsInFinalState().ShouldNotEqual(true); 

		[Test]
		public void DoTheTest()
		{
			context.Invoke ();
			of.Invoke ();
			should_be_in_state_0.Invoke ();
			should_not_be_in_state_1.Invoke ();
			should_not_indicate_O_as_final_state.Invoke ();
		}

	}
}

