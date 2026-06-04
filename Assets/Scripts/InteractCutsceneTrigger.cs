using UnityEngine;
using UnityEngine.Events; // Tambahan jika ingin letupan partikel sekalian

[RequireComponent(typeof(Collider2D))] // Wajib pasang Collider di objek ini
public class InteractCutsceneTrigger : MonoBehaviour
{
    [Header("Manager yang akan Dimainkan")]
    [Tooltip("Seret CutsceneManager (di objek yang sama atau beda) ke kotak ini")]
    public CutsceneManager targetCutscene;

    [Header("Opsi Lanjutan")]
    public bool hanyaBisaDiputarSekali = true;
    public UnityEvent eventTambahanSaatDipencet; // Misal mau putar SFX khusus

    private bool pemainDekat = false;
    private bool sudahTerpakai = false;

    private void OnEnable() => PlayerControllerScript.OnInteractPressed += Eksekusi;
    private void OnDisable() => PlayerControllerScript.OnInteractPressed -= Eksekusi;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player")) pemainDekat = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player")) pemainDekat = false;
    }

    private void Eksekusi()
    {
        // Cegah spam/error jika sudah terpakai
        if (!pemainDekat || targetCutscene == null || (hanyaBisaDiputarSekali && sudahTerpakai))
            return;

        sudahTerpakai = true;

        // Picu Cutscene-nya!
        targetCutscene.PutarManual();
        
        // Picu event tambahan di Inspector (jika ada seperti matikan gameObject)
        eventTambahanSaatDipencet?.Invoke(); 
    }
}