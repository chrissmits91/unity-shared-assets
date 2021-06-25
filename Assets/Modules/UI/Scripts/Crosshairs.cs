using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    [SerializeField] private RectTransform selectedCrosshairs;

    private static Crosshairs _instance;

    public static Crosshairs Instance {
        get {
            return _instance;
        }
    }

    public RectTransform SelectedCrosshairs {
        get {
            return selectedCrosshairs;
        }
    }

    private void Awake() {
        if (_instance == null) {
            _instance = this;
        } else {
            Destroy(gameObject);
        }
    }
}
