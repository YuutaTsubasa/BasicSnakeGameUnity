using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Yuuta.Core;

namespace Yuuta.Snake
{
    public class Body : MonoBehaviour
    {
        [SerializeField] private int _maxQueueSize = 20;

        private Queue<Vector2> _nextPositionsQueue = new Queue<Vector2>();
        private Option<Body> _nextBody;
        private bool _isFirst = false;

        public void SetFirst()
            => _isFirst = true;
        
        public void SetNextBody(Body body)
            => _nextBody = body.SomeNotNull();

        public Option<Vector2> GetTopNextPosition()
            => _nextPositionsQueue.Count <= 0 ? Option<Vector2>.None() : _nextPositionsQueue.Peek().Some();
        
        public void PushPosition(Vector2 nextPosition)
        {
            _nextPositionsQueue.Enqueue(nextPosition);
            if (!_isFirst && _nextPositionsQueue.Count <= _maxQueueSize) return;
            
            var topNextPosition = _nextPositionsQueue.Dequeue();
            GetComponent<RectTransform>().anchoredPosition = topNextPosition;
                
            foreach (var nextBody in _nextBody)
                nextBody.PushPosition(topNextPosition);
        }
    }
}