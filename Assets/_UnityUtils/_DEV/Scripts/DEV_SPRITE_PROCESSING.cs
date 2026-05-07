using UnityEngine;

public class DEV_SPRITE_PROCESSING : MonoBehaviour
{
	#region SPRITE DEBUG

	[SerializeField] private SpriteRenderer _spriteRenderer;


	[Button]
	public void DEV_DEBUG_SIZE_BOUND_SPRITE_RENDERER()
	{
		if (_spriteRenderer == null)
		{
			_spriteRenderer = transform.GetComponent<SpriteRenderer>();
		}
		
		Debug.Log(_spriteRenderer.sprite.bounds);
	}

	[Button]
	public void DEV_RESIZE_SPRITE_BOUND()
	{
		if (_spriteRenderer == null)
		{
			_spriteRenderer = transform.GetComponent<SpriteRenderer>();
		}

		Vector2 currentExtents = _spriteRenderer.bounds.extents;
		
		float scaleFactor = .7f / Mathf.Max(currentExtents.x, currentExtents.y);
		
		_spriteRenderer.transform.localScale *= scaleFactor;
	}

	#endregion
	
	
	
}
