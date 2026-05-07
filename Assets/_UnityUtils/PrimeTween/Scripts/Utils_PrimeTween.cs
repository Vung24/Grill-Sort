using System;
using System.Collections.Generic;
#if PRIME_TWEEN
using PrimeTween;
#endif
using UnityEngine;

public class Utils_PrimeTween : MonoBehaviour
{
#if PRIME_TWEEN
	[Flags]
	public enum TweenType
	{
		None     = 0,
		Position = 1 << 0,
		Rotation = 1 << 1,
		Scale    = 1 << 2,
	}

	public enum SpaceType
	{
		World,
		Local
	}
	public enum PlayType
	{
		Manual,
		Automatic
	}
	
	public enum TweenStart
	{
		All,
		Once,
	}

	[Serializable]
	private struct TransformState
	{
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;
	}
	
	public enum ValueMode
	{
		Absolute,
		Relative
	}

	[SerializeField] private List<Transform> targets = new();
	[SerializeField] private TweenType tweenType;
	[SerializeField] private SpaceType spaceType = SpaceType.Local;
	[SerializeField] private PlayType playType = PlayType.Automatic;
	[SerializeField] private TweenStart tweenStart = TweenStart.All;
	[SerializeField] private ValueMode valueMode = ValueMode.Absolute;

	[SerializeField] private TweenSettings<Vector3> position;
	[SerializeField] private TweenSettings<Vector3> rotation;
	[SerializeField] private TweenSettings<Vector3> scale;

	[SerializeField] private float delay;
	[SerializeField] private bool loopYoYo;
	[SerializeField] private bool reverseOrder;
	
	

	private readonly Dictionary<Transform, TransformState> _cached = new();
	private readonly List<Tween> _tweens = new();

	private void Start()
	{
		if (playType == PlayType.Automatic)
		{
			Play();
		}
	}

	private void Cache()
	{
		_cached.Clear();

		foreach (var t in targets)
		{
			if (!t) continue;

			_cached[t] = new TransformState
			{
				position = spaceType == SpaceType.Local ? t.localPosition : t.position,
				rotation = spaceType == SpaceType.Local ? t.localRotation : t.rotation,
				scale    = t.localScale
			};
		}
	}

	public void Play()
	{
		Stop();
		Cache();

		for (int i = 0; i < targets.Count; i++)
		{
			var t = targets[i];
			if (!t) continue;

			float delayWave = tweenStart == TweenStart.Once
				? delay * (reverseOrder ? targets.Count - 1 - i : i)
				: 0f;

			if (tweenType.HasFlag(TweenType.Position))
			{
				var s = position;
				TweenSettings<Vector3> revS = default;
				s.settings.startDelay += delayWave;

				if (valueMode == ValueMode.Relative)
				{
					var pos = spaceType == SpaceType.Local ? t.localPosition : t.position;
					s.startValue = pos + s.startValue;
					s.endValue = pos + s.endValue;
				}

				if (s.settings.cycleMode == CycleMode.Yoyo && loopYoYo)
				{
					revS = s;	
					s.settings.cycles = 1;
					revS.startValue = s.endValue;
					revS.endValue = s.startValue;
				}
				
				PosTween();

				void PosTween()
				{
					_tweens.Add(
						spaceType == SpaceType.Local
							? Tween.LocalPosition(t, s).OnComplete(() =>
							{
								if (loopYoYo)
								{
									_tweens.Add(Tween.LocalPosition(t, revS).OnComplete(PosTween));;
								}
							})
							: Tween.Position(t, s));
				} ;
				
			}

			if (tweenType.HasFlag(TweenType.Rotation))
			{
				var s = rotation;
				s.settings.startDelay += delayWave;
				
				if (valueMode == ValueMode.Relative)
				{
					var rot = spaceType == SpaceType.Local ? t.localEulerAngles : t.eulerAngles;
					s.startValue = rot + s.startValue;
					s.endValue = rot + s.endValue;
				}

				_tweens.Add(
					spaceType == SpaceType.Local
						? Tween.LocalRotation(t, s)
						: Tween.Rotation(t, s)
				);
			}

			if (tweenType.HasFlag(TweenType.Scale))
			{
				var s = scale;
				s.settings.startDelay += delayWave;
				
				if (valueMode == ValueMode.Relative)
				{
					var sca = t.localScale;
					s.startValue = sca + s.startValue;
					s.endValue = sca + s.endValue;
				}
				
				_tweens.Add(Tween.Scale(t, s));
			}
		}
	}

	public void Stop()
	{
		foreach (var tw in _tweens)
			if (tw.isAlive) tw.Stop();

		_tweens.Clear();
	}

	public void ResetTransform()
	{
		Stop();

		foreach (var pair in _cached)
		{
			if (!pair.Key) continue;

			if (spaceType == SpaceType.Local)
			{
				pair.Key.localPosition = pair.Value.position;
				pair.Key.localRotation = pair.Value.rotation;
			}
			else
			{
				pair.Key.position = pair.Value.position;
				pair.Key.rotation = pair.Value.rotation;
			}

			pair.Key.localScale = pair.Value.scale;
		}
	}
#endif
	
	#if UNITY_EDITOR

	/*
	private void OnValidate()
	{
		if (tweenStart == TweenStart.Once)
		{
			if (tweenType.HasFlag(TweenType.Position))
			{
				if (position.settings.cycleMode == CycleMode.Yoyo && position.settings.cycles == -1)
				{
					this.LogError("Prime Tween non support delay per Loop YoYo mode - Change Custom YoYo Loop");
					return;
				}
			}
			if (tweenType.HasFlag(TweenType.Rotation))
			{
				if (rotation.settings.cycleMode == CycleMode.Yoyo && rotation.settings.cycles == -1)
				{
					this.LogError("Prime Tween non support delay per Loop YoYo mode - Change Custom YoYo Loop");
					return;
				}
			}
			if (tweenType.HasFlag(TweenType.Scale))
			{
				if (scale.settings.cycleMode == CycleMode.Yoyo && scale.settings.cycles == -1)
				{
					this.LogError("Prime Tween non support delay per Loop YoYo mode - Change Custom YoYo Loop");
					return;
				}
			}
		}
	}
*/
	#endif
}
