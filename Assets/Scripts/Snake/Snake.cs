using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace Yuuta.Snake
{
    public class Snake : MonoBehaviour
    {
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        private static readonly IDictionary<Direction, Direction> REVERSE_DIRECTION_MAP =
            new Dictionary<Direction, Direction>
            {
                {Direction.Up, Direction.Down},
                {Direction.Down, Direction.Up},
                {Direction.Left, Direction.Right},
                {Direction.Right, Direction.Left}
            };

        [SerializeField] private Vector2 _bodySize;
        [SerializeField] private Vector2 _startPosition;
        [SerializeField] private Direction _direction;
        [SerializeField] private float _speed;
        [SerializeField] private float _rushSpeed;
        [SerializeField] private float _accumulatedSpeed;

        [SerializeField] private RectTransform _groundRectTransform;
        [SerializeField] private Transform _bodyRootTransform;
        [SerializeField] private Body _bodyPrefab;
        [SerializeField] private Food _food;

        public bool IsDead { private set; get; }
        public bool IsRushed { private set; get; }
        public int Length => _bodies.Count;
        
        private readonly List<Body> _bodies = new();
        private Direction _currentDirection = Direction.Up;

        public void Initialize()
        {
            _ClearBodies();
            IsDead = false;
            IsRushed = false;
            _currentDirection = _direction;
            
            var firstBody = _CreateBody(_startPosition, _bodySize);
            firstBody.SetFirst();
            _bodies.Add(firstBody);
        }

        private void _ClearBodies()
        {
            foreach (var body in _bodies)
            {
                Destroy(body.gameObject);
            }
            _bodies.Clear();
        }

        private Body _CreateBody(Vector2 position, Vector2 size)
        {
            var body = Instantiate(_bodyPrefab, _bodyRootTransform);
            var bodyRectTransform = body.GetComponent<RectTransform>();
            bodyRectTransform.anchoredPosition = position;
            bodyRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            bodyRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            body.gameObject.SetActive(true);

            return body;
        }

        public void ChangeDirection(Direction direction)
            => _currentDirection = REVERSE_DIRECTION_MAP[_currentDirection] == direction ? _currentDirection : direction;

        public void ChangeToRushMode()
            => IsRushed = true;

        public void ChangeToNormalMode()
            => IsRushed = false;
        
        public void UpdateOneStep()
        {
            var firstBody = _bodies.First();
            var firstBodyPosition = firstBody.GetTopNextPosition()
                .ValueOr(firstBody.GetComponent<RectTransform>().anchoredPosition);
            
            if (_IsHitBody(_bodies.Skip(3).ToArray(), firstBodyPosition))
                IsDead = true;
            
            if (IsDead)
                return;

            if (_IsEatingFood(_food, firstBodyPosition))
            {
                var currentLastBody = _bodies.Last();
                var newBody = _CreateBody(
                    currentLastBody.GetComponent<RectTransform>().anchoredPosition, _bodySize);
                currentLastBody.SetNextBody(newBody);
                
                _bodies.Add(newBody);
                _food.RandomPut();
            }
            
            var nextPosition = _CheckOverLimit(firstBodyPosition 
                    + _GetSpeedVector((IsRushed ? _rushSpeed : _speed) 
                                      + _accumulatedSpeed * _bodies.Count) * Time.deltaTime);
            firstBody.PushPosition(nextPosition);
        }

        private Vector2 _GetSpeedVector(float speed)
            => _currentDirection switch
            {
                Direction.Up => new Vector2(0, speed),
                Direction.Down => new Vector2(0, -speed),
                Direction.Left => new Vector2(-speed, 0),
                Direction.Right => new Vector2(speed, 0),
                _ => throw new ArgumentOutOfRangeException()
            };

        private Vector2 _CheckOverLimit(Vector2 position)
        {
            Rect rect = _groundRectTransform.rect;
            return new Vector2(
                position.x > rect.width
                    ? 0
                    : position.x < 0
                        ? rect.width
                        : position.x,
                position.y > rect.height
                    ? 0
                    : position.y < 0
                        ? rect.height
                        : position.y);
        }

        private bool _IsEatingFood(Food food, Vector2 headPosition)
        {
            var foodRectTransform = food.GetComponent<RectTransform>();
            return (new Rect(foodRectTransform.anchoredPosition, foodRectTransform.rect.size))
                .Overlaps(new Rect(headPosition, _bodySize));
        }

        private bool _IsHitBody(Body[] bodies, Vector2 headPosition) 
            => bodies.Any(body =>
            {
                var bodyRectTransform = body.GetComponent<RectTransform>();
                return new Rect(bodyRectTransform.anchoredPosition, bodyRectTransform.rect.size)
                    .Overlaps(new Rect(headPosition, _bodySize));
            });
    }
}

