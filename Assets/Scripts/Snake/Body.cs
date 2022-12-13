using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Yuuta.Core;

namespace Yuuta.Snake
{
    public class Body : MonoBehaviour
    {
        private enum State
        {
            Initial,
            WaitMove,
            CanMove
        }
        
        private readonly TimeSpan _startDelayDuration = TimeSpan.FromSeconds(0.1f);
        private Queue<Vector2> _nextPositionsQueue = new();
        private Option<Body> _nextBody;
        private State _state = State.Initial;

        public void SetCanMove()
            => _state = State.CanMove;

        public void SetNextBody(Body body)
            => _nextBody = body.SomeNotNull();

        public Option<Vector2> GetTopNextPosition()
            => _nextPositionsQueue.Count <= 0 ? Option<Vector2>.None() : _nextPositionsQueue.Peek().Some();
        
        public void PushPosition(Vector2 nextPosition)
        {
            _nextPositionsQueue.Enqueue(nextPosition);

            if (_state == State.Initial)
            {
                _state = State.WaitMove;
                Observable.Timer(_startDelayDuration)
                    .Subscribe(_ => _state = State.CanMove)
                    .AddTo(this);
            }

            if (_state == State.CanMove) _UpdatePosition();
        }

        private void _UpdatePosition()
        {
            if (_nextPositionsQueue.Count <= 0)
                return;
            
            var topNextPosition = _nextPositionsQueue.Dequeue();
            GetComponent<RectTransform>().anchoredPosition = topNextPosition;
                
            foreach (var nextBody in _nextBody)
                nextBody.PushPosition(topNextPosition);
        }
    }
}