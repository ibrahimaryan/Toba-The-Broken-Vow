# Brainstorming: Auto-Play Prologue Dinamis dengan SFX Panjang

Saat ini, fitur *Auto-Play* Prologue Anda menggunakan *delay* statis (menunggu persis `2.5` detik setelah teks selesai diketik, lalu otomatis lanjut ke teks berikutnya). 

Masalahnya: Jika sebuah baris dialog memiliki SFX/Voice Over berdurasi **5 detik** (seperti suara petir panjang atau narator yang bicara lambat), teks akan berganti di saat audionya masih memutar keras, yang akan merusak *timing* sinematik Anda.

Berikut adalah hasil *brainstorming* 3 solusi yang bisa kita terapkan:

---

## Solusi 1: Pengaturan Delay Manual per-Baris (Manual & Presisi)
Kita menambahkan variabel baru di `VNDialogueLine.cs`, misalnya `public float customAutoPlayDelay = 0f;`.
- Jika Anda isi `0`, ia menggunakan delay bawaan (2.5 detik).
- Jika Anda isi `5`, ia akan menunggu 5 detik (sesuai panjang SFX Anda).

**Kelebihan:** Anda pegang kendali penuh 100%. Sangat cocok jika Anda ingin memberikan efek jeda dramatis (meskipun tidak ada SFX).
**Kekurangan:** Agak repot karena Anda harus mengecek durasi tiap file audio secara manual lalu mengetikkan angkanya di Unity Inspector.

## Solusi 2: Penghitungan Sisa Waktu Audio (Otomatis)
Sistem akan membaca properti `length` (durasi) dari `sfxClip` secara otomatis saat dialog dimunculkan.
Setelah teks selesai diketik, sistem akan menghitung: *Apakah audio ini masih berbunyi?* Jika iya, sistem akan memperpanjang waktu tunggunya sesuai sisa waktu audio.

**Kelebihan:** Sangat praktis. Anda tinggal masukkan SFX, dan Prologue akan selalu selaras dengan audio.
**Kekurangan:** Jika Anda menggunakan *ambient SFX* (misal: suara desiran angin 10 detik yang tujuannya dibiarkan menyala melintasi 3 baris teks), sistem akan menahan teks tersebut selama 10 detik padahal seharusnya sudah pindah ke baris teks selanjutnya.

## Solusi 3: Fitur "Wait For Audio" (Hybrid / Rekomendasi)
Kita memberikan satu *Checkbox* baru di `VNDialogueLine.cs` bernama `[x] Wait For Audio To Finish`.

1. Jika **tidak dicentang**, Auto-Play berjalan dengan delay biasa (2.5 detik). Cocok untuk efek suara pendek atau *ambient* panjang.
2. Jika **dicentang**, maka Auto-Play **baru akan berlanjut ketika audio tersebut benar-benar berhenti**.
3. Agar deteksi audionya akurat, kita akan mengubah logika `sfxSource.PlayOneShot(clip)` menjadi `sfxSource.clip = clip; sfxSource.Play();` khusus untuk Prologue.
   **(Kelebihan ekstra: Jika pemain mengeklik Skip, suara narator lama akan otomatis terpotong dan diganti suara narator baru, tidak tumpang tindih berisik seperti PlayOneShot).*

---

## Contoh Modifikasi Kode (Untuk Solusi 3)

**Pada VNDialogueLine.cs:**
```csharp
[Header("Audio")]
public AudioClip sfxClip;
[Tooltip("Centang ini jika Anda ingin Auto-Play menunggu hingga audio ini selesai dimainkan sebelum pindah ke teks selanjutnya.")]
public bool waitForAudio = false;
```

**Pada DialogueManagerCS.cs:**
```csharp
private IEnumerator AutoAdvanceDelay(VNDialogueLine line)
{
    // 1. Tunggu dulu sesuai delay minimal bawaan (2.5 detik)
    yield return new WaitForSeconds(prologueAutoPlayDelay);

    // 2. Jika baris ini minta ditunggu sampai audionya selesai
    if (line.waitForAudio && sfxSource != null && sfxSource.isPlaying)
    {
        // Tahan Coroutine di sini SELAMA audio masih berbunyi
        while (sfxSource.isPlaying)
        {
            yield return null; 
        }
    }

    // 3. Audio beres, lanjut!
    DisplayNextLine();
}
```

---

## Kesimpulan
Untuk game bertema naratif seperti visual novel, **Solusi 3** adalah "Best Practice" yang dipakai standar industri karena memberikan fleksibilitas tertinggi bagi desainer level/penulis naskah.

Bagaimana menurut Anda? Silakan balas di obrolan jika Anda ingin saya langsung mengimplementasikan salah satu dari solusi di atas ke dalam skrip Anda!
