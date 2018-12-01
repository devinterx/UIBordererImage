using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace BorderedImage
{
    [ExecuteInEditMode]
    [AddComponentMenu("UI/BorderedImage", 11), DisallowMultipleComponent]
    public class BorderedImage : Image
    {
        [SerializeField]
        private float _borderSize;

        [SerializeField]
        private float _falloffDistance = 1;

        [SerializeField]
        private Vector4 _borderRadius = Vector4.zero;

        private static Sprite _simpleSprite;

        public float BorderWidth
        {
            set
            {
                _borderSize = value;
                SetMaterialDirty();
            }
        }

        public float FallOffDistance
        {
            set
            {
                _falloffDistance = value;
                SetMaterialDirty();
            }
        }
        
        public Vector4 BorderRadius
        {
            set
            {
                _borderRadius = value;
                SetMaterialDirty();
            }
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            OnEnable();
        }
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            if (sprite == null) sprite = GenerateSimpleSprite();
            material = new Material(Shader.Find("UI/Bordered Image"));
        }

        public void Update()
        {
            UpdateMaterial();
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (overrideSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }

            switch (type)
            {
                case Type.Simple:
                    GenerateSimpleSprite(toFill);
                    break;
                case Type.Sliced:
                    GenerateSimpleSprite(toFill);
                    break;
                case Type.Tiled:
                    GenerateSimpleSprite(toFill);
                    break;
                case Type.Filled:
                    base.OnPopulateMesh(toFill);
                    break;
            }
        }

        public override Material GetModifiedMaterial(Material baseMaterial)
        {
            var rect = GetComponent<RectTransform>().rect;

            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            var pixelSize = Vector3.Distance(corners[1], corners[2]) / rect.width;
            pixelSize = pixelSize / _falloffDistance;

            var radius = ReCalculateRadius(_borderRadius);
            var tempMaterial = SetMaterialValues(
                new ImageMaterialInfo(
                    rect.width + _falloffDistance,
                    rect.height + _falloffDistance,
                    Mathf.Max(pixelSize, 0),
                    radius,
                    Mathf.Max(_borderSize, 0)),
                baseMaterial
            );
            return base.GetModifiedMaterial(tempMaterial);
        }

        private Vector4 ReCalculateRadius(Vector4 vector)
        {
            var rect = rectTransform.rect;
            vector = new Vector4(Mathf.Max(vector.x, 0), Mathf.Max(vector.y, 0), Mathf.Max(vector.z, 0),
                Mathf.Max(vector.w, 0));
            var scaleFactor = Mathf.Min(rect.width / (vector.x + vector.y), rect.width / (vector.z + vector.w),
                rect.height / (vector.x + vector.w), rect.height / (vector.z + vector.y), 1);
            return vector * scaleFactor;
        }

        private Sprite GenerateSimpleSprite()
        {
            if (_simpleSprite != null) return _simpleSprite;

            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return _simpleSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero);
        }

        private void GenerateSimpleSprite(VertexHelper vh)
        {
            var r = GetPixelAdjustedRect();
            var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);
            var uv = !(overrideSprite != null) ? Vector4.zero : DataUtility.GetOuterUV(overrideSprite);

            var aa = _falloffDistance / 2f;

            var color32 = color;
            vh.Clear();
            vh.AddVert(new Vector3(v.x - aa, v.y - aa), color32, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(v.x - aa, v.w + aa), color32, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(v.z + aa, v.w + aa), color32, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(v.z + aa, v.y - aa), color32, new Vector2(uv.z, uv.y));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        private Material SetMaterialValues(ImageMaterialInfo info, Material baseMaterial)
        {
            if (baseMaterial == null)
            {
                throw new System.ArgumentNullException("baseMaterial");
            }

            if (baseMaterial.shader.name != "UI/Bordered Image")
            {
                return baseMaterial;
            }

            var tempMaterial = baseMaterial;

            tempMaterial.SetFloat("_ImageWidth", info.ImageWidth);
            tempMaterial.SetFloat("_ImageHeight", info.ImageHeight);
            tempMaterial.SetVector("_BorderRadius", info.BorderRadius);
            tempMaterial.SetFloat("_BorderSize", info.BorderSize);
            tempMaterial.SetFloat("_PixelWorldScale", info.PixelWorldScale);
            return tempMaterial;
        }

        private struct ImageMaterialInfo
        {
            public readonly float ImageWidth;
            public readonly float ImageHeight;
            public readonly float BorderSize;
            public readonly Vector4 BorderRadius;
            public readonly float PixelWorldScale;

            public ImageMaterialInfo(float imageWidth, float imageHeight, float pixelWorldScale,
                Vector4 borderRadius, float borderSize)
            {
                ImageWidth = imageWidth;
                ImageHeight = imageHeight;
                BorderRadius = borderRadius;
                BorderSize = borderSize;
                PixelWorldScale = pixelWorldScale;
            }
        }
    }
}
