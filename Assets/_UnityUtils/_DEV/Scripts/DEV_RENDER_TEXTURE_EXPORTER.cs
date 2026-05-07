#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DEV_RENDER_TEXTURE_EXPORTER : MonoBehaviour
{
	public RenderTexture renderTexture;
	public string fileName = "RenderTextureOutput.png";

	[Button]
	public void ExportRenderTexture()
	{
		if (renderTexture == null) return;

		Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = renderTexture;

		texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		texture.Apply();

		RenderTexture.active = currentRT;

		if (QualitySettings.activeColorSpace == ColorSpace.Linear)
		{
			Color[] pixels = texture.GetPixels();
			for (int i = 0; i < pixels.Length; i++) pixels[i] = pixels[i].gamma;
			texture.SetPixels(pixels);
			texture.Apply();
		}

		SaveTextureToFile(texture, fileName);
		Destroy(texture);
		
		Debug.Log($"RenderTexture exported to: {Path.Combine(Application.dataPath, fileName)}");
	}

	private void SaveTextureToFile(Texture2D texture, string fileName)
	{
		byte[] bytes = texture.EncodeToPNG();
		string filePath = Path.Combine(Application.dataPath, fileName);
		File.WriteAllBytes(filePath, bytes);
	}
	
}
#endif