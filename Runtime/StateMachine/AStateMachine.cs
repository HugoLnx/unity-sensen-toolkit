using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public abstract class AStateMachine<TState, TStateId, TMessage> : MonoBehaviour
    where TState : AState<TStateId>
    where TStateId : struct, System.Enum
    where TMessage : struct, System.Enum
    {
        protected abstract TStateId InitialStateId { get; }
        protected Dictionary<TStateId, StateNode<TState, TStateId, TMessage>> Nodes = new();
        protected StateNode<TState, TStateId, TMessage> CurrentNode;
        private TStateId[] _allStateIds;
        public event Action<TStateId> OnStateChange;

#if UNITY_EDITOR
        [Header("FSM Debug")]
        [field: SerializeField, ReadOnly] protected string CurrentStateName { get; private set; }
#endif

        protected virtual void Awake()
        {
            TState[] states = GetComponentsInChildren<TState>();
            foreach (TState state in states)
            {
                Nodes[state.Id] = new StateNode<TState, TStateId, TMessage>(state);
                state.enabled = false;
            }
            _allStateIds = Enum.GetValues(typeof(TStateId)) as TStateId[];
            AssertInitializedCorrectly();
        }

        protected virtual void OnEnable()
        {
            EnterNode(Nodes[InitialStateId]);
#if UNITY_EDITOR
            StartCoroutine(DebuggerLoop());
#endif
        }

        [Conditional("UNITY_EDITOR")]
        private void AssertInitializedCorrectly()
        {
            HashSet<TStateId> missingStateIds = new(_allStateIds);
            missingStateIds.ExceptWith(Nodes.Keys);

            if (missingStateIds.Count > 0)
            {
                string missingStateIdsString = string.Join(", ", missingStateIds);
                UnityEngine.Debug.LogError($"Missing state nodes for states: {missingStateIdsString}");
            }
        }

        public void Send(TMessage message)
        {
            if (!CurrentNode.NextStates.ContainsKey(message)) return;
            StateNode<TState, TStateId, TMessage> nextNode = CurrentNode.NextStates[message];
            ExitNode(CurrentNode);
            EnterNode(nextNode);
        }

        public bool IsStateActive(TStateId stateId)
        {
            TStateId? currentStateId = CurrentNode?.State?.Id;
            if (currentStateId == null) return false;
            if (currentStateId.Value.Equals(stateId)) return true;
            HashSet<TStateId> currentGroupIds = CurrentNode.State.GroupIds;
            if (currentGroupIds == null) return false;
            return currentGroupIds.Contains(stateId);
        }

        private void ExitNode(StateNode<TState, TStateId, TMessage> node)
        {
            TState state = node.State;
            state.OnStateExitInternal();
        }

        private void EnterNode(StateNode<TState, TStateId, TMessage> node)
        {
            TState nextState = node.State;

            HashSet<TStateId> currentGroupIds = CurrentNode?.State?.GroupIds;
            HashSet<TStateId> nextGroupIds = nextState.GroupIds;
            if (currentGroupIds != null)
            {
                HashSet<TStateId> exitingGroupIds = new(currentGroupIds);
                if (nextGroupIds != null) exitingGroupIds.ExceptWith(nextGroupIds);
                foreach (TStateId groupId in exitingGroupIds)
                {
                    StateNode<TState, TStateId, TMessage> groupNode = Nodes[groupId];
                    groupNode.State.OnStateExitInternal();
                }
            }

            if (nextGroupIds != null)
            {
                HashSet<TStateId> enteringGroupIds = new(nextGroupIds);
                if (currentGroupIds != null) enteringGroupIds.ExceptWith(currentGroupIds);
                foreach (TStateId groupId in enteringGroupIds)
                {
                    StateNode<TState, TStateId, TMessage> groupNode = Nodes[groupId];
                    groupNode.State.OnStateEnterInternal();
                }
            }
            CurrentNode = node;
            nextState.OnStateEnterInternal();
            OnStateChange?.Invoke(nextState.Id);
        }

        protected TransitionAdder<TState, TStateId, TMessage> AddTransitionFrom(params TStateId[] from)
        {
            return new(this, from);
        }

        protected TransitionAdder<TState, TStateId, TMessage> AddTransitionFromAny(params TStateId[] except)
        {
            HashSet<TStateId> fromSet = new(_allStateIds);
            fromSet.ExceptWith(except);
            return new(this, fromSet.ToArray());
        }

        internal void AddTransition(TStateId from, TMessage message, TStateId to)
        {
            Nodes[from].AddTransition(message, Nodes[to]);
        }

        private IEnumerator DebuggerLoop()
        {
            WaitForSeconds wait = new(0.35f);
            while (true)
            {
                CurrentStateName = CurrentNode.State.Id.ToString();
                yield return wait;
            }
        }
    }
}
