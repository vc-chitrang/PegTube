using static Modules.Utility.Utility;

namespace UnityEngine.UI.ProceduralImage {
	public class MaterialHelper {
		/// <summary>
		/// Sets the material values.
		/// </summary>
		/// <returns>The material values.</returns>
		/// <param name="info">Values to set.</param>
		/// <param name="baseMaterial">Base material.</param>
		public static Material SetMaterialValues(ProceduralImageMaterialInfo info, Material baseMaterial) {
			if (baseMaterial == null) {
				throw new System.ArgumentNullException("baseMaterial");
			}

			if (baseMaterial.shader.name != "UI/Procedural UI Image") {
				LogWarning($"Parameter 'baseMaterial' does not use shader 'UI/Procedural UI Image'. Method returns baseMaterial. @ {baseMaterial.shader.name}");
				return baseMaterial;
			}

			Material m;
			m = baseMaterial;
			m.SetFloat("_Width", info.width);
			m.SetFloat("_Height", info.height);
			m.SetFloat("_PixelWorldScale", info.pixelWorldScale);
			m.SetVector("_Radius", info.radius);
			m.SetFloat("_LineWeight", info.borderWidth);
			m.SetColor("_TopColor", info.topColor);
			m.SetColor("_BotColor", info.botColor);
			m.SetInt("_ReverseGradient", info.reverseGradient ? 1 : 0);
			m.SetInt("_UseAlphaClip",1); // optimisation* to discard fragments with 0 alpha
			return m;
		}

	}

	/// <summary>
	/// Material info. Contains values of "UI/Procedural UI Image" shader
	/// </summary>
	public struct ProceduralImageMaterialInfo {
		public float width;
		public float height;
		public float pixelWorldScale;
		public Vector4 radius;
		public float borderWidth;
		public Color topColor;
		public Color botColor;
		public bool reverseGradient;

		public ProceduralImageMaterialInfo(float width,
										   float height,
										   float pixelWorldScale,
										   Vector4 radius,
										   float borderWidth,
										   Color topColor,
										   Color botColor,
										   bool reverseGradient) {
			this.width = width;
			this.height = height;
			this.pixelWorldScale = pixelWorldScale;
			this.radius = radius;
			this.borderWidth = borderWidth;
			this.topColor = topColor;
			this.botColor = botColor;
			this.reverseGradient = reverseGradient;
		}
		public override string ToString() {
			return string.Format("width:{0},height:{1},pws:{2},radius:{3},bw:{4}", width, height, pixelWorldScale, radius, borderWidth);
		}
	}
}
