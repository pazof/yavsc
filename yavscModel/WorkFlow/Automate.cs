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
	public class Automate
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Yavsc.Model.WorkFlow.Automate"/> class.
		/// </summary>
		public Automate ()
		{

		}

		private List<IEquatable<long>> eventids = 
			new List<IEquatable<long>>();
		private List<IEquatable<long>> stateids = 
			new List<IEquatable<long>>();
		private IEquatable<long> startingstateid ;
		private List<IEquatable<long>> endingstateids = 
			new List<IEquatable<long>>();
		private Dictionary<IEquatable<long>,Dictionary<IEquatable<long>,IEquatable<long>>> transitions =
			new Dictionary<IEquatable<long>, Dictionary<IEquatable<long>, IEquatable<long>>>();

		/// <summary>
		/// Sets the state of the starting.
		/// </summary>
		/// <param name="startingStateId">Starting state identifier.</param>
		public void SetStartingState(IEquatable<long> startingStateId)
		{
			if (endingstateids.Contains (startingStateId))
				throw new InvalidOperationException ("endingstateids.Contains (startingStateId)");
			if (!stateids.Contains (startingStateId))
				stateids.Add (startingStateId);
			startingstateid = startingStateId;
		}

		/// <summary>
		/// Sets the state of the ending.
		/// </summary>
		/// <param name="endingStateId">Ending state identifier.</param>
		public void SetEndingState(IEquatable<long> endingStateId)
		{

			if (startingstateid==endingStateId)
				throw new InvalidOperationException ("startingstateid==endingStateId");
			if (!stateids.Contains (endingStateId))
				stateids.Add (endingStateId);
			if (!endingstateids.Contains (endingStateId))
				endingstateids.Add (endingStateId);
		}


		/// <summary>
		/// Adds the transition.
		/// </summary>
		/// <param name="startingStateId">Starting state identifier.</param>
		/// <param name="endingStateId">Ending state identifier.</param>
		/// <param name="eventId">Event identifier.</param>
		public void AddTransition(
			IEquatable<long> startingStateId, 
			IEquatable<long> endingStateId, 
			IEquatable<long> eventId)
		{
			if (!stateids.Contains (startingStateId))
				stateids.Add (startingStateId);
			if (!stateids.Contains (endingStateId))
				stateids.Add (endingStateId);

			Dictionary<IEquatable<long>,IEquatable<long>> currentNode = transitions [startingStateId];
			if (currentNode == null) {
				transitions.Add(
					startingStateId,
					currentNode = 
					new Dictionary<IEquatable<long>, IEquatable<long>> ());
			}
			currentNode [eventId] = endingStateId;
			if (!eventids.Contains (eventId))
				eventids.Add (eventId);
		}

		/// <summary>
		/// Gets or sets the state.
		/// </summary>
		/// <value>The state.</value>
		public IEquatable<long> State { get; set; }

		/// <summary>
		/// Move this instance according the specified eventId.
		/// </summary>
		/// <param name="eventId">Event identifier.</param>
		public void Move(IEquatable<long> eventId)
		{
			if (State == null)
				throw new InvalidOperationException ("Set the current state or reset the automate before");
			Dictionary<IEquatable<long>,IEquatable<long>> node = transitions [State];
			if (node == null) // no transition found
				return;
			IEquatable<long> nextState = node [eventId];
			if (nextState == null) // no transition found for this event
				return;
			State = nextState;
		}

		/// <summary>
		/// Reset the state of this automate to the starting state
		/// </summary>
		public void Reset()
		{
			State = startingstateid;
		}

		/// <summary>
		/// Clear all valid transitions from this automate,
		/// reset all of its properties
		/// </summary>
		public void FactoryReset() {
			State = null;
			startingstateid = null;
			endingstateids.Clear ();
			eventids.Clear ();
			transitions.Clear ();
		}

		/// <summary>
		/// Determines whether this instance is in final state.
		/// </summary>
		/// <returns><c>true</c> if this instance is in final state; otherwise, <c>false</c>.</returns>
		public bool IsInFinalState()
		{
			return endingstateids.Contains (State);
		}
	}

}

