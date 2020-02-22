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

	public StrokeGraphic targetGlyphGraphic, castedGlyphGraphic, currentGlyphGraphic, currentStrokeGraphic;


	public PlayerBehaviour player;

	void Start () {
        glyphInput.OnGlyphCast.AddListener(this.OnGlyphCast);

		if (glyphInput.OnStrokeDraw!=this.OnStrokeDraw) glyphInput.OnStrokeDraw+=this.OnStrokeDraw;
		if (glyphInput.OnPointDraw!=this.OnPointDraw) glyphInput.OnPointDraw+=this.OnPointDraw;

		StartCoroutine(CleanScreen());
	}

	public void InitCleanScreen(){
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

	Stroke Clone(Stroke stroke) {
		Vector2[] points = new Vector2[stroke.Length];
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

	public void ClearAll(){
		if (targetGlyphGraphic != null) targetGlyphGraphic.ClearStrokes();
		if (castedGlyphGraphic != null) castedGlyphGraphic.ClearStrokes();
		if (currentGlyphGraphic != null) currentGlyphGraphic.ClearStrokes();
		if (currentStrokeGraphic != null) currentStrokeGraphic.ClearStrokes();
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
				player.CastFireballRight();
				break;
			case "WaterGlyph":
				StartCoroutine(Morph (match));
				player.CastShieldBack();
				break;
			case "AirGlyph":
				StartCoroutine(Morph (match));
				player.CastWindForward();
				break;
			case "LightningGlyph":
				StartCoroutine(Morph (match));
				player.CastLightningNeutral();
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
			Clear(currentGlyphGraphic);
			return;
		}
		Stroke[] latestStroke = new Stroke[1];
		latestStroke[0] = Clone(strokes[strokes.Length - 1]);
		GlyphMatch castGlyph = Match(latestStroke);
		// Debug.Log(castGlyph.target.ToString());
		// Debug.Log(castGlyph.Cost);
		if (castGlyph.target.ToString() == "UpStroke" && castGlyph.Cost < 0.06) {
			Glyph newGlyph=Glyph.CreateGlyph(new List<Stroke>(strokes).GetRange(0, strokes.Length - 1).ToArray(), glyphInput.sampleDistance);
			newGlyph.name="NewGlyph ["+this.name+"]";
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

