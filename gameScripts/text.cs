using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.IO;

public class text : MonoBehaviour {

	//text data:
	private int			activeLines = 0;
	private int			maxLines = 25;
	public	Text		fullData;
	public	Text		Title;
	public	Text		text_time;
	public	Text		text_heartRate;
	public	Text		text_bodyTemp;
	public	Text		text_glucose;
	public	Text		text_hydration;
	public	Text		text_oxygen;
	public	Text		text_flexions;

	void Start () {
		Title.text = "Time\t\tHeart Rate\t\tBody Temp\t\tGlucose\t\tHydration\t\tOxygen\t\tFlexions";
	}

	public String[] handleUIText(captors.BioSensors s, float currTime) {
		String[] lowPrecsTable = {	helpers.lowerPrecision(currTime),
									helpers.lowerPrecision(s.heartBPM),
									helpers.lowerPrecision(s.bodyTemp),
									helpers.lowerPrecision(s.glucose),
									helpers.lowerPrecision(s.hydration),
									helpers.lowerPrecision(s.oxygen),
		};

		text_time.text = text_time.text.Insert(0, "\n" + lowPrecsTable[0]);
		text_heartRate.text = text_heartRate.text.Insert (0, "\n" + lowPrecsTable[1]);
		text_bodyTemp.text = text_bodyTemp.text.Insert(0, "\n" + lowPrecsTable[2]);
		text_glucose.text = text_glucose.text.Insert(0, "\n" + lowPrecsTable[3]);
		text_hydration.text = text_hydration.text.Insert (0, "\n" + lowPrecsTable[4]);
		text_oxygen.text = text_oxygen.text.Insert(0, "\n" + lowPrecsTable[4]);
		text_flexions.text = text_flexions.text.Insert(0, "\n" + lowPrecsTable[5]);
		activeLines++;
		if (activeLines > maxLines) {
			text_time.text = text_time.text.Remove(text_time.text.TrimEnd().LastIndexOf(Environment.NewLine));
			text_heartRate.text = text_heartRate.text.Remove(text_heartRate.text.TrimEnd().LastIndexOf(Environment.NewLine));
			text_bodyTemp.text = text_bodyTemp.text.Remove(text_bodyTemp.text.TrimEnd().LastIndexOf(Environment.NewLine));
			text_glucose.text = text_glucose.text.Remove(text_glucose.text.TrimEnd().LastIndexOf(Environment.NewLine));
			text_hydration.text = text_hydration.text.Remove(text_hydration.text.TrimEnd().LastIndexOf(Environment.NewLine));
			text_oxygen.text = text_oxygen.text.Remove(text_oxygen.text.TrimEnd().LastIndexOf(Environment.NewLine));
			text_flexions.text = text_flexions.text.Remove(text_flexions.text.TrimEnd().LastIndexOf(Environment.NewLine));
		}
		return (lowPrecsTable);
	}
}