using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AdVd.GlyphRecognition;

/// <summary>
/// Utility monobehaviour to draw glyphs and strokes. The user may re-implement this class
/// in order to draw the strokes in a custom way.
/// </summary>
public class GlyphRecognition : MonoBehaviour {

    public GlyphDrawInput glyphInput;

	public StrokeGraphic targetGlyphGraphic, castedGlyphGraphic, currentGlyphGraphic, currentStrokeGraphic, storedGlyphGraphic;

	public Glyph storedGlyph;

	public PlayerBehaviour player;

	private enum CastDirection {Right, Left};

	private CastDirection currentCast;

	void Start () {
        glyphInput.OnGlyphCast.AddListener(this.OnGlyphCast);

		if (glyphInput.OnStrokeDraw!=this.OnStrokeDraw) glyphInput.OnStrokeDraw+=this.OnStrokeDraw;
		if (glyphInput.OnPointDraw!=this.OnPointDraw) glyphInput.OnPointDraw+=this.OnPointDraw;

		StartCoroutine(CleanScreen());
	}

	IEnumerator CleanScreen() {
		UnityEngine.UI.Image img = GetComponent<UnityEngine.UI.Image>();
		while (true) {
			if (img.color.a > 0f) {
				float newAlpha = (img.color.a - 0.05f);
				img.color = new Color(img.color.r, img.color.g, img.color.b, newAlpha);
			}
			yield return new WaitForFixedUpdate();
		}
	}

	GlyphMatch Match(Stroke[] strokes) {
		Glyph drawnGlyph = Glyph.CreateGlyph(strokes, glyphInput.sampleDistance);
		if (glyphInput.Method!=null && glyphInput.targetGlyphSet!=null){
			GlyphMatch match;
			int index = glyphInput.method.MultiMatch(drawnGlyph, glyphInput.targetGlyphSet, out match);
			return match;
		}
		return null;

	}

	Stroke Clone(Stroke stroke, out Vector2[] points) {
		points = new Vector2[stroke.Length];
		for(int i = 0; i < stroke.Length; i ++) {
			points[i] = stroke[i];
		}
		Stroke clone = new Stroke(points);
		return clone;
	}

	void Set(StrokeGraphic strokeGraphic, Glyph glyph)
    {
		if (strokeGraphic != null) strokeGraphic.SetStrokes(glyph);
	}
	void Set(StrokeGraphic strokeGraphic, Stroke[] strokes)
    {
		if (strokeGraphic != null) strokeGraphic.SetStrokes(strokes);
	}
	void Clear(StrokeGraphic strokeGraphic)
    {
		if (strokeGraphic != null) strokeGraphic.ClearStrokes();
	}
	bool IsClear(StrokeGraphic strokeGraphic)
    {
		return strokeGraphic == null || strokeGraphic.IsClear;
	}


	void OnGlyphCast(int index, GlyphMatch match){
		Clear(currentGlyphGraphic);
		if (match == null) {
			Clear(targetGlyphGraphic);
			Clear(castedGlyphGraphic);
			return;
		}

		// Debug.Log(match.target.ToString());
		// Debug.Log(match.Cost);
		switch (match.target.ToString()) {
			case "FireGlyph":
				StartCoroutine(Morph (match));
				if (currentCast == CastDirection.Right) {
					player.CastFireball(25, 1f);
				} else {
					player.CastFireball(-25, 1f);
				}
				break;
			case "WaterGlyph":
				StartCoroutine(Morph (match));
				player.CastShieldBack();
				break;
			case "AirGlyph":
				StartCoroutine(Morph (match));
				player.CastWindForward();
				break;
			default:
				Clear(targetGlyphGraphic);
				Clear(castedGlyphGraphic);
				break;

		}
	}

	const float step=0.01f;

	IEnumerator Morph(GlyphMatch match){
		Clear(castedGlyphGraphic);
		Stroke[] strokes = null;
		/*
		for (float t=0;t<1;t+=0.05f){
			match.SetLerpStrokes(t, ref strokes);
			Set(targetGlyphGraphic,strokes);
			yield return new WaitForSeconds(step);
		}
		*/
		float t = 0f;
		while (t < 0.99f) {
			match.SetLerpStrokes(t, ref strokes);
			Set(targetGlyphGraphic, strokes);
			t += (1 - t) * 0.1f;
			yield return new WaitForSeconds(step);
		}

		Set(targetGlyphGraphic,match.target);
		if (IsClear(currentStrokeGraphic) && IsClear(currentGlyphGraphic)){
			Set(castedGlyphGraphic,match.source);
		}
	}

	void OnStrokeDraw(Stroke[] strokes){
		Clear(currentStrokeGraphic);
		if (strokes == null) {
			return;
		}
		Stroke[] latestStroke = new Stroke[1];
		Vector2[] points;
		latestStroke[0] = Clone(strokes[strokes.Length - 1], out points);
		Vector2 vectorStroke = points[points.Length-1] - points[0];
		GlyphMatch castGlyph = Match(latestStroke);

		Debug.Log(castGlyph.target.ToString());
        Debug.Log(castGlyph.Cost);
		if (castGlyph.target.ToString() == "UpStroke" && castGlyph.Cost < 0.06) {
			float direction = Vector2.Dot(Vector2.up, vectorStroke);

			if (direction > 0) {
				if (storedGlyph && IsClear(currentGlyphGraphic)) {
					Set(currentGlyphGraphic, storedGlyph);
					Clear(storedGlyphGraphic);
					storedGlyph = null;
				}
			} else {
				Glyph newGlyph=Glyph.CreateGlyph(
					new List<Stroke>(strokes).GetRange(0, strokes.Length - 1).ToArray(),
					glyphInput.sampleDistance
				);
				storedGlyph = newGlyph;
				Clear(currentGlyphGraphic);
				Set(storedGlyphGraphic, newGlyph);
				glyphInput.ClearInput();
				Debug.Log("Store");
			}

		} else if (castGlyph.target.ToString() == "CastLeft" && castGlyph.Cost < 0.1) {
			Debug.Log("CastLeft");
			Glyph newGlyph=Glyph.CreateGlyph(
				new List<Stroke>(strokes).GetRange(0, strokes.Length - 1).ToArray(),
				glyphInput.sampleDistance
			);
			currentCast = CastDirection.Left;
			glyphInput.Cast(newGlyph);
			glyphInput.ClearInput();

		} else if (castGlyph.target.ToString() == "CastRight" && castGlyph.Cost < 0.1) {
			Debug.Log("CastRight");
			Glyph newGlyph=Glyph.CreateGlyph(
				new List<Stroke>(strokes).GetRange(0, strokes.Length - 1).ToArray(),
				glyphInput.sampleDistance
			);
			currentCast = CastDirection.Right;
			glyphInput.Cast(newGlyph);
			glyphInput.ClearInput();
		} else {
			Set(currentGlyphGraphic,strokes);
		}
	}

	void OnPointDraw(Vector2[] points){
		Clear(castedGlyphGraphic);
		if (points!=null) Set(currentStrokeGraphic,new Stroke[]{ new Stroke(points) });
		else Clear(currentStrokeGraphic);
	}

	public void ChangePlayer(GameObject p) {
		player = p.GetComponent<PlayerBehaviour>();
	}
}

