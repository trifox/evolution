using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;


[RequireComponent(typeof(Button))]
public class LoadLegacySimulationButton : MonoBehaviour, IPointerDownHandler {

	[DllImport("__Internal")]
	private static extern void OpenFile(string gameObjectName, string methodName);

	[SerializeField] CreatureBuilder creatureBuilder;
	[SerializeField] Evolution evolution;

	[SerializeField] UIFade fileTooNewMessage;
	[SerializeField] UIFade invalidFileMessage;

	public void OnPointerDown(PointerEventData eventData) {
		OpenFile(gameObject.name, "OnFileChosen");
	}

	public void OnFileChosen(string url) {
		StartCoroutine(LoadFileContents(url));
	}

	private IEnumerator LoadFileContents(string url) {
		var loader = new WWW(url);
		yield return loader;

		if (loader.text.StartsWith("{")) {
			fileTooNewMessage.FadeInOut(5f);
		} else {
			try {
				SimulationSerializer.LoadSimulationFromContents(Path.GetFileNameWithoutExtension(url), loader.text, creatureBuilder, evolution);
			} catch {
				invalidFileMessage.FadeInOut(3f);
			}
		}
	}
}
