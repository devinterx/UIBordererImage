using System;
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
                    GenerateTiledSprite(toFill);
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

        private void GenerateTiledSprite(VertexHelper vh)
        {
            Vector4 vector1;
            Vector4 vector2;
            Vector4 vector3;
            Vector2 vector4;
            if (overrideSprite != null)
            {
                vector1 = DataUtility.GetOuterUV(overrideSprite);
                vector2 = DataUtility.GetInnerUV(overrideSprite);
                vector3 = overrideSprite.border;
                vector4 = overrideSprite.rect.size;
            }
            else
            {
                vector1 = Vector4.zero;
                vector2 = Vector4.zero;
                vector3 = Vector4.zero;
                vector4 = Vector2.one * 100f;
            }

            var pixelAdjustedRect = GetPixelAdjustedRect();
            var num1 = (vector4.x - vector3.x - vector3.z) / pixelsPerUnit;
            var num2 = (vector4.y - vector3.y - vector3.w) / pixelsPerUnit;
            vector3 = GetAdjustedBorders(vector3 / pixelsPerUnit, pixelAdjustedRect);
            var uvMin = new Vector2(vector2.x, vector2.y);
            var vector5 = new Vector2(vector2.z, vector2.w);
            UIVertex.simpleVert.color = color;
            var x1 = vector3.x;
            var x2 = pixelAdjustedRect.width - vector3.z;
            var y1 = vector3.y;
            var y2 = pixelAdjustedRect.height - vector3.w;
            vh.Clear();
            var uvMax = vector5;
            if (Math.Abs(num1) < 0.1f)
                num1 = x2 - x1;
            if (Math.Abs(num2) < 0.1f)
                num2 = y2 - y1;
            if (fillCenter)
            {
                var y3 = y1;
                while (y3 < (double) y2)
                {
                    var y4 = y3 + num2;
                    if (y4 > (double) y2)
                    {
                        uvMax.y = uvMin.y + (float) ((vector5.y - (double) uvMin.y) *
                                                     (y2 - (double) y3) / (y4 - (double) y3));
                        y4 = y2;
                    }

                    uvMax.x = vector5.x;
                    var x3 = x1;
                    while (x3 < (double) x2)
                    {
                        var x4 = x3 + num1;
                        if (x4 > (double) x2)
                        {
                            uvMax.x = uvMin.x + (float) ((vector5.x - (double) uvMin.x) *
                                                         (x2 - (double) x3) / (x4 - (double) x3));
                            x4 = x2;
                        }

                        AddQuad(vh, new Vector2(x3, y3) + pixelAdjustedRect.position,
                            new Vector2(x4, y4) + pixelAdjustedRect.position, color, uvMin, uvMax);
                        x3 += num1;
                    }

                    y3 += num2;
                }
            }

            if (!hasBorder) return;

            var vector6 = vector5;
            var y5 = y1;
            while (y5 < (double) y2)
            {
                var y3 = y5 + num2;
                if (y3 > (double) y2)
                {
                    vector6.y = uvMin.y + (float) ((vector5.y - (double) uvMin.y) *
                                                   (y2 - (double) y5) / (y3 - (double) y5));
                    y3 = y2;
                }

                AddQuad(vh, new Vector2(0.0f, y5) + pixelAdjustedRect.position,
                    new Vector2(x1, y3) + pixelAdjustedRect.position, color,
                    new Vector2(vector1.x, uvMin.y), new Vector2(uvMin.x, vector6.y));
                AddQuad(vh, new Vector2(x2, y5) + pixelAdjustedRect.position,
                    new Vector2(pixelAdjustedRect.width, y3) + pixelAdjustedRect.position, color,
                    new Vector2(vector5.x, uvMin.y), new Vector2(vector1.z, vector6.y));
                y5 += num2;
            }

            vector6 = vector5;
            var x5 = x1;
            while (x5 < (double) x2)
            {
                var x3 = x5 + num1;
                if (x3 > (double) x2)
                {
                    vector6.x = uvMin.x + (float) ((vector5.x - (double) uvMin.x) *
                                                   (x2 - (double) x5) / (x3 - (double) x5));
                    x3 = x2;
                }

                AddQuad(vh, new Vector2(x5, 0.0f) + pixelAdjustedRect.position,
                    new Vector2(x3, y1) + pixelAdjustedRect.position, color,
                    new Vector2(uvMin.x, vector1.y), new Vector2(vector6.x, uvMin.y));
                AddQuad(vh, new Vector2(x5, y2) + pixelAdjustedRect.position,
                    new Vector2(x3, pixelAdjustedRect.height) + pixelAdjustedRect.position, color,
                    new Vector2(uvMin.x, vector5.y), new Vector2(vector6.x, vector1.w));
                x5 += num1;
            }

            AddQuad(vh, new Vector2(0.0f, 0.0f) + pixelAdjustedRect.position,
                new Vector2(x1, y1) + pixelAdjustedRect.position, color,
                new Vector2(vector1.x, vector1.y), new Vector2(uvMin.x, uvMin.y));
            AddQuad(vh, new Vector2(x2, 0.0f) + pixelAdjustedRect.position,
                new Vector2(pixelAdjustedRect.width, y1) + pixelAdjustedRect.position, color,
                new Vector2(vector5.x, vector1.y), new Vector2(vector1.z, uvMin.y));
            AddQuad(vh, new Vector2(0.0f, y2) + pixelAdjustedRect.position,
                new Vector2(x1, pixelAdjustedRect.height) + pixelAdjustedRect.position, color,
                new Vector2(vector1.x, vector5.y), new Vector2(uvMin.x, vector1.w));
            AddQuad(vh, new Vector2(x2, y2) + pixelAdjustedRect.position,
                new Vector2(pixelAdjustedRect.width, pixelAdjustedRect.height) + pixelAdjustedRect.position,
                color, new Vector2(vector5.x, vector5.y), new Vector2(vector1.z, vector1.w));
        }

        private static Vector4 GetAdjustedBorders(Vector4 border, Rect rect)
        {
            for (var index = 0; index <= 1; ++index)
            {
                var num1 = border[index] + border[index + 2];
                if (!(rect.size[index] < (double) num1) || !(Math.Abs(num1) > 0.1f)) continue;

                var num2 = rect.size[index] / num1;
                border[index] *= num2;
                border[index + 2] *= num2;
            }

            return border;
        }

        private static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color,
            Vector2 uvMin, Vector2 uvMax)
        {
            var currentVerticesCount = vertexHelper.currentVertCount;
            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0.0f), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0.0f), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0.0f), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0.0f), color, new Vector2(uvMax.x, uvMin.y));
            vertexHelper.AddTriangle(currentVerticesCount, currentVerticesCount + 1, currentVerticesCount + 2);
            vertexHelper.AddTriangle(currentVerticesCount + 2, currentVerticesCount + 3, currentVerticesCount);
        }

        private static Material SetMaterialValues(ImageMaterialInfo info, Material baseMaterial)
        {
            if (baseMaterial == null)
            {
                throw new ArgumentNullException("baseMaterial");
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
