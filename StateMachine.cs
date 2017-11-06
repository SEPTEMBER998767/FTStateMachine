﻿using System.Collections.Generic;
using System.Diagnostics;
using FTStateMachine.Interfaces;
using FTStateMachine.Triggers;

namespace FTStateMachine
{
    public class StateMachine<TStateToken> : IStateMachine<TStateToken>
    {
        private Dictionary<TStateToken, State<TStateToken>> States { get; }
        private State<TStateToken> StartingState { get; set; }
        private State<TStateToken> CurrentState { get; set; }

        public StateMachine()
        {
            States = new Dictionary<TStateToken, State<TStateToken>>();
        }

        public IState<TStateToken> Configure(TStateToken stateToken)
        {
            State<TStateToken> state;

            if (States.TryGetValue(stateToken, out state))
            {
                return state;
            }

            state = new State<TStateToken>(stateToken);
            States[stateToken] = state;
            if (States.Count == 1)
            {
                StartingState = state;
            }
            return state;
        }

        public void Start()
        {
            GoToStartingState();
        }

        public void Dispatch(object trigger)
        {
            if (CurrentState == null)
            {
                return;
            }

            var triggerResult = CurrentState.OnTriggerDispatch(trigger);
            var transitionedToNewState = GoToState(triggerResult.StateToTransitionTo);
            if (transitionedToNewState && triggerResult.ForwardTrigger)
            {
                Dispatch(trigger);
            }
        }

        private bool GoToState(TStateToken stateToken)
        {
            if (CurrentState != null && CurrentState.Token.Equals(stateToken))
            {
                return false;
            }

            if (States.TryGetValue(stateToken, out State<TStateToken> newState))
            {
                Dispatch(new StateExitedTrigger());
                CurrentState = newState;
                Dispatch(new StateEnteredTrigger());

                #if DEBUG
                Debug.WriteLine($" - {typeof(TStateToken).Name}: {CurrentState.Token}");
                #endif

                return true;
            }
            return false;
        }

        public void GoToStartingState()
        {
            GoToState(StartingState.Token);
        }
    }
}
