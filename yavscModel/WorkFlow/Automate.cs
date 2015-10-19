//
//  Automate.cs
//
//  Author:
//       Paul Schneider <paulschneider@free.fr>
//
//  Copyright (c) 2015 Paul Schneider
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
using System.Collections.Generic;

namespace Yavsc.Model.WorkFlow
{
	/// <summary>
	/// Automate.
	/// </summary>
	public class Automate<TState,TLetter>
	{
		/// <summary>
		/// Initializes a new instance of the Automate class.
		/// </summary>
		 public Automate ()
		{
		}
		private Dictionary<TState,Dictionary<TLetter,TState>> transitions =
			new Dictionary<TState,Dictionary<TLetter,TState>>();
		private List<TLetter> letters = new List<TLetter>();
		private List<TState> states = new List<TState>();
		/// <summary>
		/// Gets or sets the state.
		/// </summary>
		/// <value>The state.</value>
		public TState State { get; set; }

		/// <summary>
		/// Gets or sets the starting state.
		/// </summary>
		/// <value>The state.</value>
		public TState StartingState { get; set; }

		/// <summary>
		/// Adds the transition.
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		/// <param name="letter">Letter.</param>
		public virtual void AddTransition(
			TState start, 
			TState end, 
			TLetter letter)
		{
			if (!states.Contains (start))
				states.Add (start);
			if (!states.Contains (end))
				states.Add (end);
			if (!letters.Contains (letter))
				letters.Add (letter);
			Dictionary<TLetter,TState> node = null;
			if (!transitions.ContainsKey(start))
				transitions.Add(
					start,
					node = 
					new Dictionary<TLetter,TState> ());
			else node = transitions [start];
			if (node.ContainsKey (letter))
				throw new NotImplementedException ("Automates indéterministes");
			node.Add(letter, end);
		}


		/// <summary>
		/// Move this instance according the specified eventId.
		/// </summary>
		/// <param name="eventId">Event identifier.</param>
		public void Aggregate(TLetter eventId)
		{
			if (State == null)
				throw new InvalidOperationException ("Set the current state or reset the automate before");
			Dictionary<TLetter,TState> node = transitions [State];
			if (node == null) // no transition found from this state
				// it is final.
				throw new FinalStateException();
			TState nextState = node [eventId];
			if (nextState == null) // no transition found for this event
				throw new InvalidLetterException(eventId);
			State = nextState;
		}

		/// <summary>
		/// Reset the state of this automate to the starting state
		/// </summary>
		public void Reset()
		{
			State = StartingState;
		}

		/// <summary>
		/// Clear all valid transitions from this automate,
		/// reset all of its properties
		/// </summary>
		public void FactoryReset() {
			State = default(TState);
			StartingState = default(TState);
			letters.Clear ();
			transitions.Clear ();
		}

		/// <summary>
		/// Determines whether this instance is in final state.
		/// </summary>
		/// <returns><c>true</c> if this instance is in final state; otherwise, <c>false</c>.</returns>
		public bool IsInFinalState()
		{
			return !transitions.ContainsKey(this.State);
		}
	}

}

