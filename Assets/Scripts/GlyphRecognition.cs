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

	float costThreshold = 0.6f;
	public float CostThreshold {get{ return costThreshold; } set{
		costThreshold = value;
		Debug.Log(value);
	}}

	public Stroke[] storedGlyph;

	public PlayerBehaviour player;

	private enum CastDirection {Right, Left, Forward};

	private CastDirection currentCast;

	private bool stopStoredMorph = false;

    // This should potentially be in a constants file, though I'm not sure how to do importing in c#
    private Dictionary<string, Color> glyphColours = new Dictionary<string, Color>();

	void Start () {
        glyphInput.OnGlyphCast.AddListener(this.OnGlyphCast);

		if (glyphInput.OnStrokeDraw!=this.OnStrokeDraw) glyphInput.OnStrokeDraw+=this.OnStrokeDraw;
		if (glyphInput.OnPointDraw!=this.OnPointDraw) glyphInput.OnPointDraw+=this.OnPointDraw;

		StartCoroutine(CleanScreen());

        // Add Glyph Colour
        glyphColours.Add("fireball", new Color(1f, 0, 0));
		glyphColours.Add("shield", new Color(113/255f, 199/255f, 1f));
		glyphColours.Add("windslash", new Color(26/255f, 1f, 0));
		glyphColours.Add("finalSpark", new Color(1f, 247/255f, 103/255f));
		glyphColours.Add("arcanePulse", new Color(214/255f, 135/255f, 1f));
		glyphColours.Add("iceSpikes", new Color(127/255f, 126/255f, 253/255f));
		glyphColours.Add("royalFire", new Color(143/255f, 111/255f, 1f));
		glyphColours.Add("default", new Color(191 / 255f, 110 / 255f, 54 / 255f, 64 / 255f));

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
        if (glyphInput.Method != null && glyphInput.targetGlyphSet != null)
        {
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

	public void ClearAll(){
		if (targetGlyphGraphic != null) targetGlyphGraphic.ClearStrokes();
		if (castedGlyphGraphic != null) castedGlyphGraphic.ClearStrokes();
		if (currentGlyphGraphic != null) currentGlyphGraphic.ClearStrokes();
		if (currentStrokeGraphic != null) currentStrokeGraphic.ClearStrokes();
	}


	void OnGlyphCast(int index, GlyphMatch match){
		// Reset casted glyph transparency
		targetGlyphGraphic.color = new Color(1f, 1f, 1f, 1f);

		Clear(currentGlyphGraphic);
		if (match != null) {
			Debug.Log(match.Cost);
		}
		if (match == null || match.Cost > costThreshold) {
			Clear(targetGlyphGraphic);
			Clear(castedGlyphGraphic);
			return;
		}

		// Debug.Log(match.target.ToString());
		// Debug.Log(match.Cost);
		
		// Make sure glyph recognition finishes and clears the stroke list
		// through any possible errors.
		try {
			switch (match.target.ToString()) {
				case "Fireball":
				case "Fireball2":
				case "Fireball3":
				case "Fireball4":
					StartCoroutine(Morph (match, glyphColours["fireball"]));
					if (currentCast == CastDirection.Right) {
						player.CastFireball(25, 1f);
					} else if (currentCast == CastDirection.Left) {
						player.CastFireball(-25, 1f);
					} else {
						player.CastFireball(0, 0f);
					}
					break;
				case "Shield":
				case "Shield2":
				case "Shield3":
					StartCoroutine(Morph (match, glyphColours["shield"]));
					player.CastShieldBack();
					break;
				case "Windslash":
				case "Windslash2":
				case "Windslash3":
				case "Windslash4":
					StartCoroutine(Morph (match, glyphColours["windslash"]));
					player.CastWindForward();
					break;
				case "Lightning":
				case "Lightning2":
					StartCoroutine(Morph (match, glyphColours["finalSpark"]));
					player.CastLightningNeutral();
					break;
				case "ArcanePulse":
					StartCoroutine(Morph (match, glyphColours["arcanePulse"]));
					player.CastArcanePulse();
					break;
				// case "Arcanopulse":
				// case "Arcanopulse2":
				// case "Arcanopulse3":
				// 	StartCoroutine(Morph (match));
				// 	player.CastLightningNeutral();
				// 	break;
				// case "Icespike":
				// case "Icespike2":
				// case "Icespike3":
				// 	StartCoroutine(Morph (match));
				// 	player.CastLightningNeutral();
				// 	break;
				// case "Royalfire":
				// case "Royalfire":
				// case "Royalfire":
				// 	StartCoroutine(Morph (match));
				// 	player.CastLightningNeutral();
				// 	break;
				default:
					Debug.Log("Fizzle");
					player.CastFizzle();
					ClearAll();
					//Clear(targetGlyphGraphic);
					//Clear(castedGlyphGraphic);
					//Clear(currentGlyphGraphic);
					//Clear(currentStrokeGraphic);
					//glyphInput.strokeList.Clear();
					break;
			}
		}
		catch (System.Exception e) {
			Debug.LogError("Glyph recognition " + e + " occured. Clearing strokes.");
			ClearAll();
		}

	}

	const float step=0.01f;

	IEnumerator Morph(GlyphMatch match, Color color){
		targetGlyphGraphic.color = new Color(1f, 1f, 1f, 1f);

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
			targetGlyphGraphic.material.color = Color.Lerp(glyphColours["default"], color, t);
         	t += (1 - t) * 0.1f;
			yield return new WaitForSeconds(step);
		}

		Set(targetGlyphGraphic,match.target);
		if (IsClear(currentStrokeGraphic) && IsClear(currentGlyphGraphic)){
			Set(castedGlyphGraphic,match.source);
		}
	}

	IEnumerator MorphStored(GlyphMatch match) {
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
		while (t < 0.99f && !stopStoredMorph) {
			match.SetLerpStrokes(t, ref strokes);
			Set(storedGlyphGraphic, strokes);
			storedGlyph = strokes;
			t += (1 - t) * 0.1f;
			yield return new WaitForSeconds(step);
		}
		if(!stopStoredMorph) Set(storedGlyphGraphic,match.target);
		stopStoredMorph = false;
	}

	void Cast(Stroke[] strokes, CastDirection dir) {
		Glyph newGlyph=Glyph.CreateGlyph(
			strokes,
			glyphInput.sampleDistance
		);
		currentCast = dir;
		glyphInput.Cast(newGlyph);
		glyphInput.ClearInput();
	}

	void getStored() {
		Set(currentGlyphGraphic, storedGlyph);
		Clear(storedGlyphGraphic);
		glyphInput.strokeList = new List<Stroke>(storedGlyph);
		storedGlyph = null;
		stopStoredMorph = true;
	}

	void OnStrokeDraw(Stroke[] strokes){
		Clear(currentStrokeGraphic);
		if (strokes == null) {
			return;
		}
		Stroke[] latestStroke = new Stroke[1];
		Stroke[] previousGlyph = new List<Stroke>(strokes).GetRange(0, strokes.Length - 1).ToArray();
		Vector2[] points;
		latestStroke[0] = Clone(strokes[strokes.Length - 1], out points);
		Vector2 vectorStroke = points[points.Length-1] - points[0];
		GlyphMatch castGlyph = Match(latestStroke);

		//Debug.Log(castGlyph.target.ToString());
        //Debug.Log(castGlyph.Cost);
		if (castGlyph.target.ToString() == "UpStroke" && castGlyph.Cost < 0.06) {
			float direction = Vector2.Dot(Vector2.up, vectorStroke);

			if (direction > 0) {
				if (storedGlyph != null && IsClear(currentGlyphGraphic)) {
					getStored();
				} else {
					Cast(previousGlyph, CastDirection.Forward);
				}
			} else {
				Glyph newGlyph=Glyph.CreateGlyph(
					previousGlyph,
					glyphInput.sampleDistance
				);
				GlyphMatch match = Match(previousGlyph);
				Clear(currentGlyphGraphic);

				if (match != null && match.target.ToString() != "UpStroke" ) {
					storedGlyph = previousGlyph;
					Set(storedGlyphGraphic, newGlyph);
					StartCoroutine(MorphStored (match));
				} else
                {
					player.CastFizzle();
                }

				glyphInput.ClearInput();
				Debug.Log("Store");
			}

		} else if (castGlyph.target.ToString() == "CastLeft" && castGlyph.Cost < 0.1) {
			if (storedGlyph != null && IsClear(currentGlyphGraphic)) {
				getStored();
			} else {
				Cast(previousGlyph, CastDirection.Left);
			}
		} else if (castGlyph.target.ToString() == "CastRight" && castGlyph.Cost < 0.1) {
			if (storedGlyph != null && IsClear(currentGlyphGraphic)) {
				getStored();
			} else {
				Cast(previousGlyph, CastDirection.Right);
			}
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

